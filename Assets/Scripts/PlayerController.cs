using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : DepthBeUController
{
    public static List<PlayerController> players = new List<PlayerController>();

    private void Start()
    {
        players.Add(this);
    }

    protected override void MoveCharacter()
    {
        if (frozen) { return; }
        float X = Input.GetAxis("Horizontal");
        float Z = Input.GetAxis("Vertical");
        jumpRequested = Input.GetButtonDown("Submit");
        if (Input.GetButtonDown("Cancel") && !MenuPauser.paused) { SceneManager.LoadSceneAsync("PauseMenuScene", LoadSceneMode.Additive); }

        ControlledCharacterMovement(X, Z);
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
}
