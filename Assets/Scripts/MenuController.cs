using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [System.Serializable]
    public struct MenuScreen
    {
        public GameObject root;
        public Button[] buttons;
        public int level;
        public int variant;
    }

    public MenuScreen[] screens;
    public int currentLevel = 0;

    public void GoBackScreen(int toVariant=0)
    {
        currentLevel--;
        SetActiveScreen(currentLevel, toVariant);
    }

    public void GoForwardScreen(int variant=0)
    {
        currentLevel++;
        SetActiveScreen(currentLevel, variant);
    }

    public void SetActiveScreen(int currentLevel, int variant)
    {
        for (int a = 0; a < screens.Length; a++)
        {
            if (screens[a].level < currentLevel)
            {
                for (int b = 0; b < screens[a].buttons.Length; b++)
                {
                    screens[a].buttons[b].interactable = false;
                }
            }
            else if (screens[a].level == currentLevel && screens[a].variant == variant)
            {
                screens[a].root.gameObject.SetActive(true);
                for (int b = 0; b < screens[a].buttons.Length; b++)
                {
                    screens[a].buttons[b].interactable = true;
                }
            }
            else
            {
                screens[a].root.gameObject.SetActive(false);
            }
        }
    }

    public void GameQuit()
    {
        Application.Quit();
    }

    public void GoToScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
