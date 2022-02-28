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

    public Button rebindBtn;
    public TMPro.TextMeshProUGUI valString;

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    public bool useCompositeOnKeyboard = false;
    public bool useCompositeOnPad = false;

    void Awake()
    {
        SetStringValueText();
        master.Subscribe(this);
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
            .WithBindingMask(InputBinding.MaskByGroup(master.GetControlType()))
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(func => RebindDone())
            .Start();
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
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "up");
                break;
            case 2:
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "down");
                break;
            case 3:
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "left");
                break;
            case 4:
                bindIndex = inputActions.FindAction(actionname).bindings.IndexOf(x => x.isPartOfComposite && x.name.ToLower() == "right");
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
        Debug.Log(inputActions.FindAction(actionname).GetBindingIndex(InputBinding.MaskByGroup(master.GetControlType())));
        return inputActions.FindAction(actionname).GetBindingIndex(InputBinding.MaskByGroup(master.GetControlType()));
        
    }

    public void SetStringValueText()
    {
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
    }

    void WaitingText(string optionalMadeUpText = "")
    {
        if (optionalMadeUpText == "") { valString.text = "Waiting for Input..."; }
        else { valString.text = optionalMadeUpText; }
    }
}
