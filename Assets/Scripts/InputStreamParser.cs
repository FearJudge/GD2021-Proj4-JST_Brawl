using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputStreamParser : MonoBehaviour
{
    public static int moveErrorOnIgnoredInput = 2;
    public static int moveErrorOnCloseMissInput = 1;
    public static int moveErrorOnFarMissInput = 3;
    public static bool invertX = false;

    public enum ErrorType
    {
        IgnoredInput,
        AdditionalInputClose,
        AdditionalInputFar,
        FailedInput
    }

    [System.Serializable]
    public class MoveProperties
    {
        public string animationTrigger = "";
        public string enemyAnimationTrigger = "";
        public int allowNextMove = 50;
        public int preventMovement = 10;
        public Vector3 characterVelocity = Vector3.zero;
        public int hurtBoxDamage = 10;
        public float hitStunDuration = 0.25f;
        public bool knockDown = false;
        public Vector3 knockDownVelocity = Vector3.zero;
    }

    [System.Serializable]
    public class MoveDetails
    {
        public MoveDetails(MoveDetails copy)
        {
            moveDefinition = new List<string>(copy.moveDefinition);
            moveAllowedError = copy.moveAllowedError;
            moveAllowedDuration = copy.moveAllowedDuration;
            if (moveAllowedDuration == 0) { moveAllowedDuration = 800; }
            moveName = copy.moveName;
            properties = copy.properties;
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

        public string moveName = "";
        public List<string> moveDefinition = new List<string>();
        public int moveAllowedError;
        public int moveAllowedDuration;
        public int movePriority;
        public bool allowOnGround = true;
        public bool allowOnAir = false;
        protected int currentError = 0;
        protected int currentDuration = 0;
        public MoveProperties properties;
    }

    [System.Serializable]
    public struct StreamedIn
    {
        public string Input;
        public int framesFromLast;
    }

    public const int INPUTMEMORY = 13;
    public const int TURNAROUNDDUR = 16;
    public int movePrevention = 0;
    public int dirPrevention = 0;

    [SerializeField] public MoveDetails[] moveList = new MoveDetails[0];
    [SerializeField] public List<StreamedIn> StreamingInputList = new List<StreamedIn>();
    int framesFrom = 0;
    public DepthBeUController playerController;
    public TMPro.TextMeshProUGUI stream;
    string defin = "";

    void Update()
    {
        GetInput();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CaptureInput();
    }

    void GetInput()
    {
        void CheckFacing()
        {
            if (!DepthBeUController.playerFacingRight)
            {
                invertX = true;
            }
            else
            {
                invertX = false;
            }
        }

        int res = 0;
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        if (y >= 0.3f) { res = 6; }
        else if (y <= -0.3f) { res = 0; }
        else { res = 3; }
        if (x >= 0.3f) { res += 3; if (invertX) { res -= 2; } }
        else if (x <= -0.3f) { res += 1; if (invertX) { res += 2; } }
        else { res += 2; }
        if ((res == 5 || res == 4 || res == 6) && framesFrom >= TURNAROUNDDUR)
        {
            CheckFacing();
        }
        bool A = Input.GetKey(KeyCode.Z);
        bool B = Input.GetKey(KeyCode.X);
        string definB = res.ToString();
        if (A) { definB += "A"; }
        if (B) { definB += "B"; }
        if (StreamingInputList.Count == 0) { defin = definB; return; }
        if (definB != StreamingInputList[StreamingInputList.Count - 1].Input) { defin = definB; }
    }

    void CaptureInput()
    {
        void AddInList(string definition)
        {
            StreamingInputList.Add(new StreamedIn { Input = definition, framesFromLast = framesFrom });
            stream.text = "";
            framesFrom = 0;
            foreach (StreamedIn inp in StreamingInputList)
            {
                stream.text += inp.Input;
            }
            if (definition.Contains("A")) { ParseInput(); }
        }
        if (movePrevention > 0) { movePrevention--; }
        if (dirPrevention > 0) { dirPrevention--; playerController.frozen = true; if (dirPrevention == 0) { playerController.frozen = false; } }
        if (StreamingInputList.Count == 0)
        {
            AddInList(defin);
        }
        else if (defin != StreamingInputList[StreamingInputList.Count - 1].Input)
        {
            AddInList(defin);
        }
        if (StreamingInputList.Count >= INPUTMEMORY) { StreamingInputList.RemoveAt(0); }
        framesFrom++;
    }

    void ParseInput()
    {
        List<MoveDetails> iterationList = new List<MoveDetails>();

        void FindAllWithStartInput(string start)
        {
            for (int j = 0; j < moveList.Length; j++)
            {
                if (moveList[j].moveDefinition[0] == start)
                {
                    if ((!playerController.airborne && moveList[j].allowOnGround) || (playerController.airborne && moveList[j].allowOnAir))
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
            for (int k = iterationList.Count - 1; k >= 0; k--)
            {
                if (iterationList[k].moveAllowedError < iterationList[k].GetError() || iterationList[k].moveAllowedDuration < iterationList[k].GetDuration())
                {
                    iterationList.RemoveAt(k);
                }
            }
        }
        void IterateErrors(string input, int durFromLast)
        {
            for (int k = 0; k < iterationList.Count; k++)
            {
                iterationList[k].AddDuration(durFromLast);
                for (int m = 0; m < 1; m++)
                {
                    if (iterationList[k].moveDefinition.Count < m) { break; }
                    if (iterationList[k].moveDefinition[m] == input)
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

        if (movePrevention > 0) { StreamingInputList.Clear(); return; }

        for (int i = 0; i < StreamingInputList.Count; i++)
        {
            string watchedInput = StreamingInputList[i].Input;
            IterateErrors(watchedInput, StreamingInputList[i].framesFromLast);
            FindAllWithStartInput(watchedInput);
        }
        IterateMissedEnds();

        int prior = FindHighestPriority();
        if (prior >= 0) { Debug.Log(iterationList[prior].moveName); ActivateMove(iterationList[prior].properties); }

        StreamingInputList.Clear();
    }

    void ActivateMove(MoveProperties mp)
    {
        if (playerController == null) { Debug.Log("No Controller!"); return; }

        if (mp.animationTrigger != "") { playerController.animator.SetTrigger(mp.animationTrigger); }
        playerController.rb.velocity += mp.characterVelocity;
        if (mp.characterVelocity.y > 1f) { playerController.airborne = true; }
        playerController.hb.damage = mp.hurtBoxDamage;
        playerController.hb.hitStun = mp.hitStunDuration;
        playerController.hb.knockDown = mp.knockDown;
        playerController.hb.knockDownVelocity = mp.knockDownVelocity;
        dirPrevention = mp.preventMovement;
        movePrevention = mp.allowNextMove;
    }
}
