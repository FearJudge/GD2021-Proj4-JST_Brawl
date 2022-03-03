using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_OnScreenEffectBrain : MonoBehaviour
{
    public enum HintType
    {
        None,
        Moving,
        Jumping,
        AttackingBasics,
        WeaponSwap,
        Blood,
        Upgrades,
        CommandMoves
    }

    public static UI_OnScreenEffectBrain instance;
    public Animator effectAnimator;
    public TMPro.TextMeshProUGUI hintDisplay;
    public InputActionMap iam;
    public PlayerInput pi;

    void Start()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    public static void GoGo()
    {
        instance.effectAnimator.SetTrigger("ArrowFlash");
    }

    public static void EncounterStart()
    {
        instance.effectAnimator.SetBool("Encounter", true);
    }

    public static void EncounterEnd()
    {
        instance.effectAnimator.SetBool("Encounter", false);
    }

    public static void UnDisplayHint()
    {
       instance.effectAnimator.SetBool("HintScreenUp", false);
    }

    public static void DisplayHint(HintType hint)
    {
        string hintText = "Hello!";
        switch (hint)
        {
            case HintType.Moving:
                hintText = "Moving:\nUse " + 
                    instance.pi.currentActionMap.FindAction("Move").
                    GetBindingDisplayString(InputBinding.MaskByGroup(instance.pi.currentControlScheme), InputBinding.DisplayStringOptions.DontIncludeInteractions) + 
                    " to move your character.";
                break;
            case HintType.Jumping:
                hintText = "Jump:\nUse " + 
                    instance.pi.currentActionMap.FindAction("Jump").
                    GetBindingDisplayString(InputBinding.MaskByGroup(instance.pi.currentControlScheme), InputBinding.DisplayStringOptions.DontIncludeInteractions) + 
                    " to jump.";
                break;
            case HintType.AttackingBasics:
                hintText = "Attacking:\nUse " +
                    instance.pi.currentActionMap.FindAction("LightAttack").
                    GetBindingDisplayString(InputBinding.MaskByGroup(instance.pi.currentControlScheme), InputBinding.DisplayStringOptions.DontIncludeInteractions) +
                    " for a Light Attack and " + instance.pi.currentActionMap.FindAction("HeavyAttack").
                    GetBindingDisplayString(InputBinding.MaskByGroup(instance.pi.currentControlScheme), InputBinding.DisplayStringOptions.DontIncludeInteractions) + 
                    " for a Heavy Attack.";
                break;
            case HintType.WeaponSwap:
                hintText = "Stances:\nUse " +
                    instance.pi.currentActionMap.FindAction("WeaponSwap").
                    GetBindingDisplayString(InputBinding.MaskByGroup(instance.pi.currentControlScheme), InputBinding.DisplayStringOptions.DontIncludeInteractions) +
                    " to swap between: Melee, Ranged and Spell stances.";
                break;
            case HintType.Blood:
                hintText = "Blood:\n" +
                    "When you hit enemies, your blood gauge rises. Using certain actions will drain blood to empower them.";
                break;
            case HintType.Upgrades:
                hintText = "Upgrades:\n" +
                    "After defeating enemies in an encounter. You may drain blood from 1 corpse for an upgrade to keep. Choose wisely.";
                break;
            case HintType.CommandMoves:
                hintText = "Attacking 2:\n" +
                    "Not everything happens just by mashing buttons.\nTry to load an empowered bolt by pressing DOWN, DOWN and HeavyAttack while in Ranged stance.";
                break;
            default:
                break;
        }
        instance.hintDisplay.text = hintText;
        instance.effectAnimator.SetBool("HintScreenUp", true);
    }
}
