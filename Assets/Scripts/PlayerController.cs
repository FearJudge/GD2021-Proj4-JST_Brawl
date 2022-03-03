using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : DepthBeUController
{
    public Special special;
    public PlayerInput p_input;
    private InputActionAsset inasset;
    private InputAction moveBtns;
    private InputAction escBtn;
    private InputAction jmpBtn;
    public static List<PlayerController> players = new List<PlayerController>();
    public bool controlOn = true;
    public bool death = false;
    public InputStreamParser myStream;
    bool madeMove = false;
    public MoveProp.GlobalVariables gv = new MoveProp.GlobalVariables()
    {
        lifestealArray = new int[3],
        damageModArray = new float[3],
        damagePlusArray = new int[3],
        hitstunArray = new float[3],
        critArray = new int[3]
    };

    private void Awake()
    {
        players.Add(this);
        SetUP();
        special = GetComponent<Special>();
        inasset = p_input.actions;
        moveBtns = inasset.FindAction("Move");
        escBtn = inasset.FindAction("Pause");
        jmpBtn = inasset.FindAction("Jump");
        EncounterManager.EncounterEnded += SwapToChoose;
    }

    protected override void MoveCharacter()
    {
        if (death) { return; }
        if (!controlOn) { UpgradeInput(); return; }
        if (frozen || halted) { ControlledCharacterMovement(0, 0); return; }
        Vector2 inputMoves = moveBtns.ReadValue<Vector2>();
        float X = inputMoves.x;
        float Z = inputMoves.y;
        jumpRequested = jmpBtn.triggered;
        if (escBtn.triggered && !MenuPauser.paused) { SceneManager.LoadSceneAsync("PauseMenuScene", LoadSceneMode.Additive); }

        ControlledCharacterMovement(X, Z);
    }

    private void UpgradeInput()
    {
        frozen = true;
        ControlledCharacterMovement(0, 0);
        Vector2 inputMoves = moveBtns.ReadValue<Vector2>();
        float X = inputMoves.x;
        if (X > 0.1f && !madeMove) { madeMove = true; UpgradeLink.Scroll(1); }
        else if (X < -0.1f && !madeMove) { madeMove = true; UpgradeLink.Scroll(-1); }
        else if (X == 0f) { madeMove = false; }
        if (jmpBtn.triggered) { UpgradeLink.SelectCurrent(); }
    }

    protected override void ParseUpgrade(UpgradeLibrary.MoveUpgrade up)
    {
        myStream.AddUpgrade(up);
    }

    protected override void ParseUpgrade(UpgradeLibrary.PlayerUpgrade up)
    {
        if (up.addHealth != 0) { hpScript.AddMaxHealth(up.addHealth); hpScript.FullRecover(); }
        if (up.healthModifier != 0f && up.healthModifier != 1f) { hpScript.ModMaxHealth(up.healthModifier); hpScript.FullRecover(); }
        if (up.addLifeSteal != 0) { gv.lifestealArray = MoveProp.DetermineIntArrayValues(gv.lifestealArray, up.addLifeSteal, up); }
        if (up.damageModifier != 0f) { gv.damageModArray = MoveProp.DetermineFloatArrayValues(gv.damageModArray, up.damageModifier, up); }
        if (up.movementModifier != 0f) { speed *= up.movementModifier; }
        if (up.stunModifier != 0f) { gv.hitstunArray = MoveProp.DetermineFloatArrayValues(gv.hitstunArray, up.stunModifier, up); }
        if (up.addCriticalChance != 0) { gv.critArray = MoveProp.DetermineIntArrayValues(gv.critArray, up.addCriticalChance, up); }
    }

    private void OnDestroy()
    {
        players.Remove(this);
    }

    public static PlayerController GetRandomPlayer()
    {
        if (players.Count == 0) { return null; }
        return players[Random.Range(0, players.Count - 1)];
    }

    public override void Kill()
    {
        base.Kill();
        SceneManager.LoadSceneAsync("DeathMenuScene", LoadSceneMode.Additive);
    }

    void SwapToChoose()
    {
        controlOn = false;
        EncounterManager.EncounterCleared += Chosen;
    }

    void Chosen()
    {
        frozen = false;
        controlOn = true;
    }
}
