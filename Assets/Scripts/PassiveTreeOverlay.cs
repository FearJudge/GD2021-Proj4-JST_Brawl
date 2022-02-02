using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PassiveTreeOverlay : MonoBehaviour
{
    public bool isOn = false;

    public bool useButton = false;
    public string buttonName = "";
    public KeyCode passiveTreeKey = KeyCode.P;

    public string treeSceneName;

    // Update is called once per frame
    void Update()
    {
        if (useButton) { CheckButton(); } else { CheckKey(); }
    }

    void CheckButton()
    {
        if (Input.GetButtonDown(buttonName)) { ToggleTree(); };
    }

    void CheckKey()
    {
        if (Input.GetKeyDown(passiveTreeKey)) { ToggleTree(); };
    }

    void ToggleTree()
    {
        if (isOn) { SceneManager.UnloadSceneAsync(treeSceneName); isOn = false; }
        else { SceneManager.LoadSceneAsync(treeSceneName, LoadSceneMode.Additive); isOn = true; }
    }
}
