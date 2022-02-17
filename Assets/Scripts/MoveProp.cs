using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoveProp
{
    [SerializeField] public readonly string moveName = "";
    public string animationTrigger = "";
    public float animationSpeedMod = 1f;
    public int allowNextMove = 50;
    public int preventMovement = 10;
    public int followUpAllowFrom = 0;
    public Vector3 characterVelocity = Vector3.zero;
    public int hurtBoxDamage = 10;
    public int lifeSteal = 0;
    public float hitStunDuration = 0.25f;
    public bool knockDown = false;
    public Vector3 knockDownVelocity = Vector3.zero;

    public void ActivateMove(DepthBeUController player)
    {
        if (player == null) { Debug.Log("No Controller!"); return; }
        if (animationTrigger != "") { player.animator.SetTrigger(animationTrigger); }
        player.rb_body.velocity += characterVelocity;
        if (characterVelocity.y > 1f) { player.airborne = true; }
        player.hb.damage = hurtBoxDamage;
        player.hb.hitStun = hitStunDuration;
        player.hb.knockDown = knockDown;
        player.hb.knockDownVelocity = knockDownVelocity;
    }
}

public class MoveUpgrade : MonoBehaviour
{
    public string upgradeName = "";
    public string onMove = "";
    public float changeSpeedOfAnimation = 0f;
    public int changeAttackPrevention = 0;
    public int changeMovementPrevention = 0;
    public int changeFollowUpTiming = 0;
    public Vector3 playerVelChange = Vector3.zero;
    public int damageChange = 0;
    public int lifeStealChange = 0;
    public float hitStunChange = 0f;
    // knockdown: 0 = no change, 1 = change to FALSE, 2 = change to TRUE, 3 = SWAP
    public int changeKnockDown = 0;
    public Vector3 knockdownChange = Vector3.zero;
}
