using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : DepthBeUController
{
    public Health hpScript;
    public State currentAct = State.Standing;
    public enum State
    {
        Standing
    }

    // Start is called before the first frame update
    void Start()
    {
        SetUP();
    }

    public override void Update()
    {
        GetCollisionsAroundCharacter();
        ActAccordingToRules();
        ProjectLanding();
        AnimateCharacter();
        Gravity();
    }

    void Gravity()
    {
        AirborneChecks();
        SetOffGroundIfDropped();
    }

    void ActAccordingToRules()
    {
        if (currentAct == State.Standing) { RealizeMovement(0f, 0f); }
    }

    public override void GetHit(int dmg, float stun, bool knockBack, Vector3 knockback, bool fromLeft)
    {
        hpScript.Hp -= dmg;
        base.GetHit(dmg, stun, knockBack, knockback, fromLeft);
    }
}
