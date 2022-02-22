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
    
}
