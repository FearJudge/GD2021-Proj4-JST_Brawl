using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebinderUnit : MonoBehaviour
{
    public RebinderBrain master;

    public InputActionAsset inputActions;
    public string actionname;
    string displayname;
    string displaynamealt;

    public Button rebindBtn;
    public TMPro.TextMeshProUGUI valString;
    public TMPro.TextMeshProUGUI nameString;

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    public bool useCompositeOnKeyboard = false;
    public bool useCompositeOnPad = false;
    public Toggle checkmark;

    void Awake()
    {
        displayname = nameString.text;
        displaynamealt = displayname + " (4-Directions)";
        SetStringValueText();
        master.Subscribe(this);
        if (useCompositeOnKeyboard) { nameString.text = displaynamealt; } else { nameString.text = displayname; }
    }

    public void RebindBegin()
    {
        bool comp = false;
        if (master.rebindType == 0) { comp = useCompositeOnKeyboard; }
        else { comp = useCompositeOnPad; }
        if (comp) { BeginCompositeRebind(); }
        else { BeginSimpleRebind(); }
    }

    public void BeginSimpleRebind()
    {
        WaitingText();

        rebindOperation = inputActions.FindAction(actionname).PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithTargetBinding(FindRebindIndexOfType())
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(func => RebindDone())
            .Start();
    }

    public void SwapComposite()
    {
        if (master.rebindType == 0) { useCompositeOnKeyboard = !useCompositeOnKeyboard; }
        else { useCompositeOnPad = !useCompositeOnPad; }
        SetStringValueText();
    }

    public void BeginCompositeRebind()
    {
        RebindNext(0);
    }

    void RebindDone()
    {
        try
        {
            SetStringValueText();
        }
        catch (System.Exception)
        {
            rebindOperation.Dispose();
            throw;
        }
        rebindOperation.Dispose();
    }

    void RebindNext(int comp)
    {
        if (rebindOperation != null) { rebindOperation.Dispose(); }
        int id = comp + 1;
        int bindIndex = 0;
        switch (id)
        {
            case 1:
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "up" && x.groups.Contains(master.GetControlType()));
                break;
            case 2:
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "down" && x.groups.Contains(master.GetControlType()));
                break;
            case 3:
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "left" && x.groups.Contains(master.GetControlType()));
                break;
            case 4:
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "right" && x.groups.Contains(master.GetControlType()));
                break;
            default:
                break;
        }
        if (id >= 5 || bindIndex == -1) { RebindDone(); return; }
        WaitingText("Input Button for: " + inputActions.FindAction(actionname).bindings[bindIndex].name);

        rebindOperation = inputActions.FindAction(actionname).PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .WithBindingMask(InputBinding.MaskByGroup(master.GetControlType()))
            .WithTargetBinding(bindIndex)
            .WithExpectedControlType("Button")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(func => RebindNext(id))
            .Start();
    }

    void CheckConflictingBindings()
    {

    }

    int FindRebindIndexOfType()
    {
        Debug.Log(inputActions.FindAction(actionname).GetBindingIndex(InputBinding.MaskByGroups(IsItComposite())));
        return inputActions.FindAction(actionname).GetBindingIndex(InputBinding.MaskByGroups(IsItComposite()));
        
    }

    string[] IsItComposite()
    {
        if (master.rebindType == 0 && useCompositeOnKeyboard) { return new string[] { "Keyboard", "CompositeKeys" }; }
        else if (master.rebindType != 0 && useCompositeOnPad) { return new string[] { "Composite" }; }
        else { return new string[] { master.GetControlType() }; }
    }

    public void SetStringValueText()
    {
        if (master.rebindType == 0 && !useCompositeOnKeyboard && checkmark != null) { useCompositeOnKeyboard = true; }
        string GetInputName(int idOffset=0)
        {
            return InputControlPath.ToHumanReadableString(
                inputActions.FindAction(actionname).bindings[FindRebindIndexOfType() + idOffset].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        if (inputActions.FindAction(actionname).bindings[FindRebindIndexOfType()].isPartOfComposite)
        {
            valString.text = GetInputName(0) + ", " + GetInputName(1) + ", " + GetInputName(2) + ", " + GetInputName(3);
        }
        else
        {
            valString.text = GetInputName();
        }

        master.CreateConflictMessages(master.CheckConflictingBindings());
        if (master.rebindType == 0 && useCompositeOnKeyboard || master.rebindType != 0 && useCompositeOnPad) { checkmark?.SetIsOnWithoutNotify(true); nameString.text = displaynamealt; }
        else { checkmark?.SetIsOnWithoutNotify(false); nameString.text = displayname; }
    }

    void WaitingText(string optionalMadeUpText = "")
    {
        if (optionalMadeUpText == "") { valString.text = "Waiting for Input..."; }
        else { valString.text = optionalMadeUpText; }
    }
}
