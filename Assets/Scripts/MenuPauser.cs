using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPauser : MonoBehaviour
{
    public static bool paused = false;

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
        if (Input.GetKeyDown(KeyCode.Escape)) { Resume(); }
    }

    public void QuitCommand()
    {
        Application.Quit();
    }

    public void Resume()
    {
        SceneManager.UnloadSceneAsync("PauseMenuScene");
    }
}
