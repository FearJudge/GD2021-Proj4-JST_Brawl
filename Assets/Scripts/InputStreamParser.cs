using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputStreamParser : MonoBehaviour
{
    public static int moveErrorOnIgnoredInput = 2;
    public static int moveErrorOnCloseMissInput = 1;
    public static int moveErrorOnFarMissInput = 3;
    public static bool invertX = false;

    public PlayerInput p_input;
    private InputActionAsset actions;
    private InputAction moveBtns;
    private InputAction[] attackBtns = new InputAction[3];

    public enum ErrorType
    {
        IgnoredInput,
        AdditionalInputClose,
        AdditionalInputFar,
        FailedInput
    }

    [System.Serializable]
    public class MoveDetails
    {
        public MoveDetails(MoveDetails copy)
        {
            moveDefinition = new List<string>(copy.moveDefinition);
            moveAllowedError = copy.moveAllowedError;
            moveAllowedDuration = copy.moveAllowedDuration;
            movePriority = copy.movePriority;
            if (moveAllowedDuration == 0) { moveAllowedDuration = 800; }
            moveName = copy.moveName;
            followUpTo = copy.followUpTo;
            followUpAllowedOnlyOnHit = copy.followUpAllowedOnlyOnHit;
            cancelFrom = copy.cancelFrom;
            properties = copy.properties;
            moveBloodCost = copy.moveBloodCost;
            moveAmmoCost = copy.moveAmmoCost;
            currentError = 0;
            currentDuration = 0;
        }

        public void AddError(ErrorType error)
        {
            switch (error)
            {
                case ErrorType.IgnoredInput:
                    currentError += moveErrorOnIgnoredInput;
                    break;
                case ErrorType.AdditionalInputClose:
                    currentError += moveErrorOnCloseMissInput;
                    break;
                case ErrorType.AdditionalInputFar:
                    currentError += moveErrorOnFarMissInput;
                    break;
                case ErrorType.FailedInput:
                    currentError += 999;
                    break;
                default:
                    break;
            }
        }

        public void AddDuration(int val)
        {
            currentDuration += val;
        }

        public int GetDuration()
        {
            return currentDuration;
        }

        public int GetError()
        {
            return currentError;
        }

        public void AddUpgrade(UpgradeLibrary.MoveUpgrade moveUpgrade)
        {
            if (moveUpgrade.onMove != moveName) { return; }
            if (moveUpgrade.damageModifier != 0f) { properties.hurtBoxDamage = Mathf.FloorToInt(properties.hurtBoxDamage * moveUpgrade.damageModifier); }
            if (moveUpgrade.stunModifier != 0f) { properties.hitStunDuration *= moveUpgrade.stunModifier; }
            if (moveUpgrade.playerVelChange != Vector3.zero) { properties.characterVelocity += moveUpgrade.playerVelChange; }
            if (moveUpgrade.knockdownChange != Vector3.zero) { properties.knockDownVelocity += moveUpgrade.knockdownChange; }
            if (moveUpgrade.knockBackModifier != 0f) { properties.knockDownVelocity *= moveUpgrade.knockBackModifier; }
            if (moveUpgrade.changeAttackPrevention != 0) { properties.allowNextMove += moveUpgrade.changeAttackPrevention; if (properties.allowNextMove < 0) { properties.allowNextMove = 0; } }
            if (moveUpgrade.changeFollowUpTiming != 0) { properties.followUpAllowFrom += moveUpgrade.changeFollowUpTiming; if (properties.followUpAllowFrom < 0) { properties.followUpAllowFrom = 0; } }
            if (moveUpgrade.changeMovementPrevention != 0) { properties.preventMovement += moveUpgrade.changeMovementPrevention; }
            if (moveUpgrade.addLifeSteal != 0) { properties.lifeSteal += moveUpgrade.addLifeSteal; }
            if (moveUpgrade.changeSpeedOfAnimation != 0f) { properties.animationSpeedMod += moveUpgrade.changeSpeedOfAnimation; if (properties.animationSpeedMod <= 0f) { properties.animationSpeedMod = 0.1f; } }
            if (moveUpgrade.addFollowUps != "") { string prefix = ""; if (followUpTo != "") { prefix = "|"; } followUpTo += prefix + moveUpgrade.addFollowUps; }
            if (moveUpgrade.removeFollowUps != "") { followUpTo.Replace(moveUpgrade.removeFollowUps, ""); }
        }

        public string moveName = "";
        public string followUpTo = "";
        public string cancelFrom = "";
        public bool followUpAllowedOnlyOnHit = false;
        public List<string> moveDefinition = new List<string>();
        public int moveAllowedError;
        public int moveAllowedDuration;
        public int movePriority;
        public int moveBloodCost;
        public int moveAmmoCost;
        public bool allowOnGround = true;
        public bool allowOnAir = false;
        protected int currentError = 0;
        protected int currentDuration = 0;
        public MoveProp properties;
    }

    [System.Serializable]
    public struct StreamedIn
    {
        public string Input;
        public int framesFromLast;
    }

    public DepthBeUController player;
    protected PlayerController pcont;
    public MoveListCurator curator;
    public const int INPUTMEMORY = 13;
    public const int TURNAROUNDDUR = 9;
    public string savedMoveName = "";
    int followUpDelay = 0;
    int followUpValue = 0;
    public int followUpAllow = 0;
    public int movePrevention = 0;
    public int dirPrevention = 0;
    public const int MOVEBUFFDUR = 9;
    public const int HELD = 30;
    public const int RECHECK = 10;
    public int buffer = 0;
    bool A = false;
    bool B = false;
    int heldA = 0;
    int heldB = 0;

    [SerializeField] public List<StreamedIn> StreamingInputList = new List<StreamedIn>();
    int framesFrom = 0;
    public TMPro.TextMeshProUGUI stream;
    public TMPro.TextMeshProUGUI move;
    string defin = "";

    private void Awake()
    {
        pcont = (PlayerController)player;
        actions = p_input.actions;
        moveBtns = actions.FindAction("Move");
        attackBtns = new InputAction[4] { actions.FindAction("LightAttack"), actions.FindAction("HeavyAttack"), actions.FindAction("WeaponSwap"), actions.FindAction("Macro_LightHeavy") };
        attackBtns[0].started += ADown => { A = true; };
        attackBtns[1].started += BDown => { B = true; };
        attackBtns[0].canceled += AUp => { A = false; };
        attackBtns[1].canceled += BUp => { B = false; };
        ChangeMoveList(0);
    }

    void Update()
    {
        GetInput();
    }

    private void OnDestroy()
    {
        attackBtns[0].started -= ADown => { A = true; };
        attackBtns[1].started -= BDown => { B = true; };
        attackBtns[0].canceled -= AUp => { A = false; };
        attackBtns[1].canceled -= BUp => { B = false; };
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CaptureInput();
    }

    public void AddUpgrade(UpgradeLibrary.MoveUpgrade up)
    {
        curator.Upgrade(up);
        curator.ChangeList(0);
    }

    void GetInput()
    {
        void CheckFacing()
        {
            if (!player.playerFacingRight)
            {
                invertX = true;
            }
            else
            {
                invertX = false;
            }
        }

        int res = 0;
        Vector2 mv = moveBtns.ReadValue<Vector2>();
        if (mv.y >= 0.3f) { res = 6; }
        else if (mv.y <= -0.3f) { res = 0; }
        else { res = 3; }
        if (mv.x >= 0.3f) { res += 3; if (invertX) { res -= 2; } }
        else if (mv.x <= -0.3f) { res += 1; if (invertX) { res += 2; } }
        else { res += 2; }
        if (framesFrom >= TURNAROUNDDUR)
        {
            CheckFacing();
        }
        if (attackBtns[2].triggered) { ChangeMoveList(); }
        if (attackBtns[3].triggered) { A = true; B = true; }
        string definB = res.ToString();
        if (A) { if (heldA > HELD) { definB += "/A"; } else { definB += "A"; heldA++; } } else { if (heldA > HELD) { definB += "-A"; } else { heldA = 0; } }
        if (B) { if (heldB > HELD) { definB += "/B"; } else { definB += "B"; heldB++; } } else { if (heldB > HELD) { definB += "-B"; } else { heldB = 0; } }
        if (StreamingInputList.Count == 0) { defin = definB; return; }
        if (attackBtns[3].triggered) { A = false; B = false; }
        if (definB != StreamingInputList[StreamingInputList.Count - 1].Input) { defin = definB; }
    }

    void ChangeMoveList(int count = 1)
    {
        curator.ChangeList(count);
        UI_HealthBarBrain.PlayerIconChange(player.hpScript, curator.currentList);
    }

    void CaptureInput()
    {
        void AddInList(string definition)
        {
            if (buffer > 0) { ParseInput(); return; }
            StreamingInputList.Add(new StreamedIn { Input = definition, framesFromLast = framesFrom });
            framesFrom = 0;
            if (stream != null)
            {
                stream.text = "";
                foreach (StreamedIn inp in StreamingInputList)
                {
                    stream.text += inp.Input;
                }
            }
            if (definition.Contains("-A")) { heldA = 0; } else if (definition.Contains("-B")) { heldB = 0; }
            if (definition.Contains("A") || definition.Contains("B")) { ParseInput(); }
            else { ParseInput(true); }
        }
        if (pcont.death) { return; }
        if (followUpDelay > 0) { followUpDelay--; if (followUpDelay == 0) { followUpAllow = followUpValue; followUpValue = 0; } }
        else if (followUpAllow > 0) { followUpAllow--; if (followUpAllow == 0) { savedMoveName = ""; } }
        if (movePrevention > 0) { movePrevention--; if (movePrevention == 0 && move != null) { move.text = ""; } }
        if (dirPrevention > 0) { dirPrevention--; player.halted = true; if (dirPrevention == 0) { player.halted = false; player.animator.speed = 1f; } }
        if (StreamingInputList.Count == 0 || buffer > 0)
        {
            AddInList(defin);
        }
        else if (defin != StreamingInputList[StreamingInputList.Count - 1].Input)
        {
            AddInList(defin);
        }
        else if (framesFrom > RECHECK)
        {
            AddInList(defin);
        }
        if (StreamingInputList.Count >= INPUTMEMORY) { StreamingInputList.RemoveAt(0); }
        framesFrom++;
    }

    void ParseInput(bool isMovement=false)
    {
        List<MoveDetails> iterationList = new List<MoveDetails>();
        List<string> allowedToFollowUp = new List<string>();


        bool PopulateAllows(MoveDetails moveToExamine, string toCompare)
        {
            string[] moves = moveToExamine.followUpTo.Split('|');
            List<string> movesFound = new List<string>();
            for (int z = 0; z < moves.Length; z++)
            {
                string trim = moves[z].Trim(' ');
                if (trim == "*") { return true; }
                else if (trim == "-" && toCompare == "") { return true; }
                movesFound.Add(trim);
            }
            if (movesFound.Count <= 0) { movesFound.Add(moveToExamine.followUpTo); }
            if (movesFound.Contains(toCompare)) { return true; }
            return false;
        }
        void FindAllWithStartInput(string start, bool nonActionMoves)
        {
            MoveDetails[] moveList = new MoveDetails[0];
            if (!nonActionMoves) { moveList = curator.ReturnCurrentMoves(); }
            else { moveList = curator.ReturnMovement(); }
            for (int j = 0; j < moveList.Length; j++)
            {
                if (CheckAgainstInput(moveList[j].moveDefinition[0], start) &&
                    PopulateAllows(moveList[j], savedMoveName) &&
                    moveList[j].moveBloodCost <= pcont.special.Value &&
                    pcont.projectilespawner.ammo >= moveList[j].moveAmmoCost)
                {
                    if ((!player.airborne && moveList[j].allowOnGround) || (player.airborne && moveList[j].allowOnAir))
                    {
                        AddToIterationList(moveList[j]);
                    }
                }
            }
        }
        void AddToIterationList(MoveDetails move)
        {
            iterationList.Add(new MoveDetails(move));
            iterationList[iterationList.Count - 1].moveDefinition.RemoveAt(0);
        }
        int DetermineCloseness(string input, string result)
        {
            string det = input.Trim(new char[2] { 'A', 'B' });
            string res = result.Trim(new char[2] { 'A', 'B' });
            int.TryParse(det, out int dirA);
            int.TryParse(res, out int dirB);

            if (dirA - 3 == dirB || dirA + 3 == dirB) { return 1; }
            switch (dirA % 3)
            {
                case 0:
                    // Right side of NUMPAD
                    if (dirA - 1 == dirB) { return 1; }
                    break;
                case 1:
                    // Left side of NUMPAD
                    if (dirA + 1 == dirB) { return 1; }
                    break;
                case 2:
                    // Middle of NUMPAD
                    if (dirA - 1 == dirB || dirA + 1 == dirB) { return 1; }
                    break;
                default:
                    break;
            }
            return 2;
        }
        void DeleteExcessIterations()
        {
            if (iterationList.Count == 0) { return; }
            for (int k = iterationList.Count - 1; k >= 0; k--)
            {
                if (iterationList[k].moveAllowedError < iterationList[k].GetError() || iterationList[k].moveAllowedDuration < iterationList[k].GetDuration())
                {
                    iterationList.RemoveAt(k);
                }
            }
        }
        bool CheckAgainstInput(string definition, string input)
        {
            if (definition == "*") { return true; }
            if (definition.Contains("A")) { if (!input.Contains("A")) { return false; } }
            if (definition.Contains("B")) { if (!input.Contains("B")) { return false; } }
            if (definition.Contains("/A")) { if (!input.Contains("/A")) { return false; } }
            if (definition.Contains("/B")) { if (!input.Contains("/B")) { return false; } }
            if (definition.Contains("-A")) { if (!input.Contains("-A")) { return false; } }
            if (definition.Contains("-B")) { if (!input.Contains("-B")) { return false; } }
            string dirInput = input.Trim('A', 'B', '/', '-');
            if (definition.Contains("(") && definition.Contains(")"))
            {
                string definStart = definition.Trim('(', ')', 'A', 'B', '/');
                char[] definedDirs = definStart.ToCharArray();
                for (int a = 0; a < definedDirs.Length; a++)
                {
                    if (dirInput == definedDirs[a].ToString()) { return true; }
                }
            }
            return dirInput == definition;
        }
        void IterateErrors(string input, int durFromLast)
        {
            for (int k = 0; k < iterationList.Count; k++)
            {
                iterationList[k].AddDuration(durFromLast);
                for (int m = 0; m < 1; m++)
                {
                    if (iterationList[k].moveDefinition.Count <= m) { break; }
                    if (CheckAgainstInput(iterationList[k].moveDefinition[m], input))
                    {
                        if (m == 1) { iterationList[k].AddError(ErrorType.IgnoredInput); }
                        if (m == 2) { iterationList[k].AddError(ErrorType.IgnoredInput); iterationList[k].AddError(ErrorType.IgnoredInput); }
                        iterationList[k].moveDefinition.RemoveAt(0);
                        break;
                    }
                    int close = DetermineCloseness(input, iterationList[k].moveDefinition[m]);
                    if (close == 1) { iterationList[k].AddError(ErrorType.AdditionalInputClose); }
                    else { iterationList[k].AddError(ErrorType.AdditionalInputFar); }
                    if (m == 2) { iterationList[k].AddError(ErrorType.FailedInput); }
                }
            }
            DeleteExcessIterations();
        }
        void IterateMissedEnds()
        {
            for (int y = 0; y < iterationList.Count; y++)
            {
                int toFail = 3;
                foreach (string s in iterationList[y].moveDefinition)
                {
                    iterationList[y].AddError(ErrorType.IgnoredInput);
                    toFail--;
                    if (toFail == 0) { iterationList[y].AddError(ErrorType.FailedInput); }
                }
            }
            DeleteExcessIterations();
        }
        int FindHighestPriority()
        {
            int highest = -1;
            int highestInd = -1;
            for (int a = 0; a < iterationList.Count; a++)
            {
                if (iterationList[a].movePriority > highest) { highest = iterationList[a].movePriority; highestInd = a; }
            }
            return highestInd;
        }

        if ((movePrevention > 0 && followUpAllow == 0) || player.frozen) { if (buffer == 0) { buffer = MOVEBUFFDUR; } else { buffer--; } if (buffer == 0) { return; } return; }
        buffer = 0;

        for (int i = 0; i < StreamingInputList.Count; i++)
        {
            string watchedInput = StreamingInputList[i].Input;
            IterateErrors(watchedInput, StreamingInputList[i].framesFromLast);
            FindAllWithStartInput(watchedInput, isMovement);
        }
        IterateMissedEnds();

        int prior = FindHighestPriority();
        if (prior == -1) { return; }
        MoveDetails found = iterationList[prior];
        if (movePrevention > 0 && followUpAllow > 0)
        {
            if (!PopulateAllows(found, savedMoveName))
            {
                return;
            }
        }

        pcont.special.Value -= found.moveBloodCost;
        if (found.followUpAllowedOnlyOnHit) { found.moveName = "!" + found.moveName; }
        if (prior >= 0) { ActivateMove(found.properties, found.moveName); }
    }

    void ActivateMove(MoveProp mp, string name)
    {
        if (move != null) { move.text = name; }
        mp.ActivateMove(player, pcont.gv, curator.currentList);
        followUpAllow = 0;
        dirPrevention = mp.preventMovement;
        movePrevention = mp.allowNextMove;
        if (mp.allowNextMove <= mp.followUpAllowFrom) { return; }
        savedMoveName = name;
        if (mp.followUpAllowFrom == 0) { followUpDelay = mp.allowNextMove; }
        else
        {
            followUpDelay = mp.followUpAllowFrom;
        }
        followUpValue = mp.allowNextMove - mp.followUpAllowFrom;
    }

    public void HitConfirmed()
    {
        savedMoveName = savedMoveName.Trim('!');
        ParseInput();
    }
}
