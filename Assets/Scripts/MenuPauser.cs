using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPauser : MonoBehaviour
{
    public static bool paused = false;
    public bool pauseGame = true;
    public PlayerInput p_input;
    public GameObject lc;
    public string resumelevel;

    // Start is called before the first frame update
    void Awake()
    {
        if (pauseGame) { paused = true; Time.timeScale = 0; }
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
        GameObject changer = Instantiate(lc);
        LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
        transitionscript.StartTransition("MainMenuScene", 0.1f);
    }

    public void Resume()
    {
        SceneManager.UnloadSceneAsync("PauseMenuScene");
    }

    public void ResumeAlt()
    {
        SceneManager.UnloadSceneAsync("FinishMenuScene");
    }

    public void Retry()
    {
        GameObject changer = Instantiate(lc);
        LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
        transitionscript.StartTransition(resumelevel, 0.1f);
    }
}
