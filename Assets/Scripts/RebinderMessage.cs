using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebinderMessage : MonoBehaviour
{
    public TMPro.TextMeshProUGUI target;
    public TMPro.TextMeshProUGUI errorInput;

    public void SetMessage(InputBinding bind)
    {
        target.text = bind.action;
        errorInput.text = InputControlPath.ToHumanReadableString(bind.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}
