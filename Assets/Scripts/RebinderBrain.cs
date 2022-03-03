using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebinderBrain : MonoBehaviour
{
    public InputActionAsset inputActions;
    public int rebindType = 0;

    private List<RebinderUnit> rebinderChilds = new List<RebinderUnit>();

    private string[] inputDisplayNames = { "Keyboard", "Gamepad" };
    [SerializeField] TMPro.TextMeshProUGUI currentBindType;

    [SerializeField] GameObject conflictMessage;
    [SerializeField] RectTransform messageSpawnPoint;
    private List<GameObject> messages = new List<GameObject>();
    public Vector2 messageOffset;

    public void SwapInputType()
    {
        rebindType++;
        if (rebindType > inputDisplayNames.Length - 1) { rebindType = 0; }
        currentBindType.text = GetControlType();
        for (int a = 0; a < rebinderChilds.Count; a++)
        {
            rebinderChilds[a].SetStringValueText();
        }
    }

    public void Subscribe(RebinderUnit unit)
    {
        rebinderChilds.Add(unit);
    }

    public string GetControlType()
    {
        return inputDisplayNames[rebindType];
    }

    public List<InputBinding> CheckConflictingBindings()
    {
        List<string> binds = new List<string>();
        List<InputBinding> conflicts = new List<InputBinding>();

        for (int a = 0; a < inputActions.actionMaps.Count; a++)
        {
            if (inputActions.actionMaps[a].name == "UI") { continue; }
            for (int b = 0; b < inputActions.actionMaps[a].bindings.Count; b++)
            {
                if (!inputActions.actionMaps[a].bindings[b].groups.Contains(GetControlType())) { continue; }
                string bindStr = inputActions.actionMaps[a].bindings[b].ToDisplayString();
                if (binds.Contains(bindStr)) { conflicts.Add(inputActions.actionMaps[a].bindings[b]); }
                else { binds.Add(bindStr); }
            }
        }

        return conflicts;
    }

    public void CreateConflictMessages(List<InputBinding> conflicting)
    {
        for (int y = 0; y < messages.Count; y++) { Destroy(messages[y]); }
        messages.Clear();
        for (int a = 0; a < conflicting.Count; a++)
        {
            GameObject msg = Instantiate(conflictMessage, transform);
            RectTransform msgRect = msg.GetComponent<RectTransform>();
            RebinderMessage msgScr = msg.GetComponent<RebinderMessage>();

            msgScr.SetMessage(conflicting[a]);
            msgRect.anchoredPosition = messageSpawnPoint.anchoredPosition + (messageOffset * a);

            messages.Add(msg);
        }
    }

    public void SaveUserRebinds()
    {
        List<string> rebinded = new List<string>();
        for (int a = 0; a < inputActions.actionMaps.Count; a++)
        {
            if (inputActions.actionMaps[a].name == "UI") { continue; }
            for (int b = 0; b < inputActions.actionMaps[a].bindings.Count; b++)
            {
                string bindStr = inputActions.actionMaps[a].bindings[b].effectivePath;
                rebinded.Add(bindStr);
            }
        }
        string path = Application.dataPath + string.Format("/../bindings.sav");

        StreamWriter writer = new StreamWriter(path, false);

        foreach (var st in rebinded)
        {
            writer.Write(st + "\n");
        }

        writer.Close();
    }

    public void LoadUserRebinds()
    {
        List<string> rebinding = new List<string>();
        if (!File.Exists(Application.dataPath + string.Format("/../bindings.sav"))) { return; }
        Debug.Log("Loadin!");
        string path = Application.dataPath + string.Format("/../bindings.sav");

        StreamReader reader = new StreamReader(path, true);
        
        while (!reader.EndOfStream)
        {
            rebinding.Add(reader.ReadLine());
        }
        for (int a = 0; a < inputActions.actionMaps.Count; a++)
        {
            if (inputActions.actionMaps[a].name == "UI") { continue; }
            for (int b = 0; b < inputActions.actionMaps[a].bindings.Count; b++)
            {
                InputBinding inb = inputActions.actionMaps[a].bindings[b];
                Debug.Log(inb.effectivePath + " -> " + rebinding[0]);
                inb.overridePath = rebinding[0];
                inputActions.actionMaps[a].ApplyBindingOverride(inb);
                rebinding.RemoveAt(0);
            }
        }
    }
}
