using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : DepthBeUController
{
    public static List<PlayerController> players = new List<PlayerController>();
    public bool controlOn = true;
    public InputStreamParser myStream;
    bool madeMove = false;
    public MoveProp.GlobalVariables gv = new MoveProp.GlobalVariables()
    {
        lifestealArray = new int[3],
        damageModArray = new float[3],
        damagePlusArray = new int[3],
        hitstunArray = new float[3]
    };

    private void Start()
    {
        players.Add(this);
        EncounterManager.EncounterEnded += SwapToChoose;
    }

    protected override void MoveCharacter()
    {
        if (!controlOn) { UpgradeInput(); return; }
        if (frozen) { return; }
        float X = Input.GetAxis("Horizontal");
        float Z = Input.GetAxis("Vertical");
        jumpRequested = Input.GetButtonDown("Submit");
        if (Input.GetButtonDown("Cancel") && !MenuPauser.paused) { SceneManager.LoadSceneAsync("PauseMenuScene", LoadSceneMode.Additive); }

        ControlledCharacterMovement(X, Z);
    }

    private void UpgradeInput()
    {
        frozen = true;
        ControlledCharacterMovement(0, 0);
        float X = Input.GetAxis("Horizontal");
        if (X > 0.1f && !madeMove) { madeMove = true; UpgradeLink.Scroll(1); }
        else if (X < -0.1f && !madeMove) { madeMove = true; UpgradeLink.Scroll(-1); }
        else if (X == 0f) { madeMove = false; }
        if (Input.GetButtonDown("Submit")) { UpgradeLink.SelectCurrent(); }
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
