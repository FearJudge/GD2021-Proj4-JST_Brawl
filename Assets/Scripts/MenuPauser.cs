using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPauser : MonoBehaviour
{
    public static bool paused = false;
    public PlayerInput p_input;

    // Start is called before the first frame update
    void Awake()
    {
        paused = true;
        Time.timeScale = 0;
    }

    private void OnDestroy()
    {
        paused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (p_input.actions.FindAction("Pause").triggered) { Resume(); }
    }

    public void QuitCommand()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void Resume()
    {
        SceneManager.UnloadSceneAsync("PauseMenuScene");
    }
}
