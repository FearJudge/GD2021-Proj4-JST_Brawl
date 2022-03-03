using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : DepthBeUController
{
    public enum State
    {
        Standing,
        Moving,
        Attacking,
        Hit,
        Dodging,
        Dead
    }
    public enum Condition
    {
        Always,
        MoveRecoveredFrom,
        GotHit,
        MoveInterrupted,
        PlayerAttacking,
        PlayerInRange,
        PlayerOutOfRange,
        PlayerHit,
        ReachedDestination,
        WaitLimitReached
    }

    [System.Serializable]
    public class AIBehaviour
    {
        [System.Serializable]
        public class AITransition
        {
            public int behaviourIndexToTransitionTo = 0;
            public Condition conditionUsed;
            [Range(0f, 1f)]
            public float chance = 1f;
        }

        [Header("Action To Perform")]
        public bool performAMove = false;
        [Tooltip("Add a Move.")]
        public MoveProp useMove;
        [Tooltip("Set a custom location to Move to. Y component is ignored. 0,0,0 is ignored.")]
        public Vector3 moveToSpot;
        [Tooltip("Instead of world position, the vector is an offset of the current position.")]
        public bool relateSpotToSelf;
        [Tooltip("Instead of world position, the vector is an offset of a random player.")]
        public bool relateSpotToPlayer;
        [Tooltip("Wait Behaviour, Randomly chosen between two values.")]
        public Vector2 waitAction;

        [Header("Additional Effects and Rules. Performed After Behaviour")]
        public int modifyLife = 0;
        public float waitAfter = 0f;
        public bool turnToFacePlayer = false;
        public bool turnToFaceAwayFromPlayer = false;

        [Header("Conditions to Next Action")]
        public AITransition[] transitions;
    }

    public AIBehaviour[] behaviour = new AIBehaviour[0];

    private uint upgradeIdentifier = 0;
    private State memoryAct = State.Standing;
    public State currentAct = State.Standing;
    public Condition exitReason = Condition.Always;
    public float maximumWaitTime = 5f;
    public Vector3 destination = Vector3.zero;
    public Transform movingDestination = null;
    public Vector3 destinationOffset = Vector3.zero;
    protected Vector3 prevDestination = Vector3.zero;
    public PlayerController currentTarget;
    public float visionRadius = 30f;
    public float inRangeRadius = 0.6f;
    int behaviourInd = 0;
    bool inProgress = false;
    bool transitioning = false;
    bool cutAwayInRange = false;
    int moveDelay = 0;
    const int BASESTUCK = 68;
    int gotStuckX = 0;
    float idle = 0f;
    float waitIdle = 0f;
    float idleAfter = 0f;


    public override void Update()
    {
        if (currentAct == State.Dead) { return; }
        base.Update();
        ActAccordingToRules();
    }

    public override void FixedUpdate()
    {
        if (currentAct == State.Dead) { return; }
        base.FixedUpdate();
        ForwardRules();
        TransitionBetweenRules(behaviourInd);
    }

    protected override void MoveCharacter()
    {
        ControlledKnockBack();
    }

    void ActAccordingToRules()
    {
        if (behaviour.Length == 0 || inProgress) { return; }
        transitioning = false;
        gotStuckX = BASESTUCK;
        if (behaviour.Length <= behaviourInd) { behaviourInd = 0; }
        AIBehaviour curBehaviour = behaviour[behaviourInd];
        if (curBehaviour.performAMove)
        {
            currentAct = State.Attacking;
            curBehaviour.useMove.ActivateMove(this);
            moveDelay = curBehaviour.useMove.allowNextMove;
        }
        else if (curBehaviour.moveToSpot != Vector3.zero || curBehaviour.relateSpotToSelf || curBehaviour.relateSpotToPlayer)
        {
            currentAct = State.Moving;
            movingDestination = null;
            if (curBehaviour.relateSpotToPlayer) { movingDestination = PlayerController.GetRandomPlayer().transform; destination = Vector3.Scale(movingDestination.position, new Vector3(1, 0, 1)); destinationOffset = new Vector3(curBehaviour.moveToSpot.x, 0, curBehaviour.moveToSpot.z); }
            else if (curBehaviour.relateSpotToSelf) { destination = Vector3.Scale(transform.position + curBehaviour.moveToSpot, new Vector3(1, 0, 1)); }
            else { destination = new Vector3(curBehaviour.moveToSpot.x, 0, curBehaviour.moveToSpot.z); }
        }
        else if (curBehaviour.waitAction != Vector2.zero)
        {
            currentAct = State.Standing;
            idle = Random.Range(curBehaviour.waitAction.x, curBehaviour.waitAction.y);
        }
        else
        {
            currentAct = State.Standing;
            idle = Random.Range(8f, 12f);
        }
        inProgress = true;
        cutAwayInRange = HasCutAwayTransitionInRange();
        idleAfter = curBehaviour.waitAfter;
    }

    void ForwardRules()
    {
        if (!inProgress || transitioning) { return; }
        if (waitIdle > 0f)
        {
            if (InDistanceToPlayer() && cutAwayInRange) { waitIdle = 0; NextRule(Condition.PlayerInRange); return; }
            waitIdle -= Time.fixedDeltaTime;
            if (waitIdle <= 0f) { NextRule(); }
            return;
        }
        switch (currentAct)
        {
            case State.Standing:
                if (idle <= 0f) { NextRule(Condition.WaitLimitReached); idle = 0f; return; }
                idle -= Time.fixedDeltaTime;
                AnimateCharacterBool("running", false);
                break;
            case State.Moving:
                float range = (destination - new Vector3(transform.position.x, 0, transform.position.z)).magnitude;
                if (range < inRangeRadius) { NextRule(Condition.ReachedDestination); return;  }
                MoveToDestinationSpot();
                break;
            case State.Attacking:
                if (moveDelay == 0) { NextRule(Condition.MoveRecoveredFrom); return; }
                moveDelay--;
                break;
            case State.Hit:
                break;
            case State.Dodging:
                break;
            default:
                break;
        }
    }

    void NextRule(Condition reason)
    {
        ControlledCharacterMovement(0f, 0f);
        exitReason = reason;
        NextRule();
    }

    void NextRule()
    {
        if (idleAfter > 0f) { waitIdle = idleAfter; idleAfter = 0f; return; }
        if (behaviour[behaviourInd].modifyLife != 0) { hpScript.Hp += behaviour[behaviourInd].modifyLife; }
        if ((behaviour[behaviourInd].turnToFacePlayer || behaviour[behaviourInd].turnToFaceAwayFromPlayer) && (currentTarget != null || movingDestination != null))
        {
            int reverse = 1;
            if (behaviour[behaviourInd].turnToFaceAwayFromPlayer) { reverse = -1; }
            if (movingDestination != null) { ControlledCharacterMovement(Mathf.Clamp((movingDestination.position.x - transform.position.x) * reverse, -1f, 1f), 0, Time.fixedDeltaTime); }
            if (currentTarget != null) { ControlledCharacterMovement(Mathf.Clamp((currentTarget.transform.position.x - transform.position.x) * reverse, -1f, 1f), 0, Time.fixedDeltaTime); }
        }
        transitioning = true;
    }

    void TransitionBetweenRules(int failsafeIndex = 0)
    {
        if (!transitioning) { return; }
        float ranChance = Random.Range(0f, 1f);
        inProgress = false;
        for (int a = 0; a < behaviour[behaviourInd].transitions.Length; a++)
        {
            AIBehaviour.AITransition transition = behaviour[behaviourInd].transitions[a];
            if (transition.chance < ranChance && transition.chance != 0) { continue; }
            if (transition.conditionUsed != Condition.Always)
            {
                if (transition.conditionUsed != exitReason) { continue; }
            }
            behaviourInd = transition.behaviourIndexToTransitionTo;
            return;
        }
        behaviourInd = failsafeIndex;
    }

    bool HasCutAwayTransitionInRange()
    {
        bool found = false;
        for (int a = 0; a < behaviour[behaviourInd].transitions.Length; a++)
        {
            if (behaviour[behaviourInd].transitions[a].conditionUsed == Condition.PlayerInRange)
            { found = true; break; }
        }
        return found;
    }

    void MoveToDestinationSpot()
    {
        if (InDistanceToPlayer() && cutAwayInRange) { NextRule(Condition.PlayerInRange); return; }
        if (frozen) { return; }
        if (movingDestination != null) { destination = Vector3.Scale(movingDestination.position + destinationOffset, new Vector3(1, 0, 1)); }
        prevDestination = transform.position;
        float destStepX = Mathf.Clamp(destination.x - transform.position.x, -1, 1);
        float destStepZ = Mathf.Clamp(destination.z - transform.position.z, -1, 1);
        ControlledCharacterMovement(destStepX, destStepZ, Time.fixedDeltaTime);
        if (Mathf.Abs(prevDestination.x - transform.position.x) <= 0.005f && Mathf.Abs(destStepX) >= 0.15f)
        { gotStuckX--; if (gotStuckX == 0) { gotStuckX = BASESTUCK; NextRule(Condition.ReachedDestination); } }
    }

    public override void GetHit(int dmg, float stun, bool knockBack, Vector3 knockback, bool fromLeft, bool isCrit=false)
    {
        if (currentAct != State.Hit)
        {
            AnimateCharacterBool("stunned", true);
            AnimateCharacter(0f, 0f);
            currentAct = State.Hit;
            NextRule(Condition.GotHit);
        }
        base.GetHit(dmg, stun, knockBack, knockback, fromLeft);
    }

    protected bool InDistanceToPlayer()
    {
        if (currentTarget == null)
        {
            foreach (PlayerController p in PlayerController.players)
            {
                if (Mathf.Abs((p.transform.position - transform.position).magnitude) < visionRadius) { currentTarget = p; return true; }
            }
            return false;
        }
        else
        {
            if (Mathf.Abs((currentTarget.transform.position - transform.position).magnitude) > (visionRadius * 2)) { currentTarget = null; return false; }
        }
        return true;
    }

    protected override void UnFreeze()
    {
        AnimateCharacterBool("stunned", false);
        base.UnFreeze();
    }

    public override void Kill()
    {
        AnimateCharacterBool("stunned", false);
        currentAct = State.Dead;
        EncounterManager.IDied(gameObject, this);
        if (upgradeIdentifier != 0)
        {
            EncounterManager.EncounterCleared += Dissolve;
        }
        else
        {
            Invoke("Dissolve", 2f);
        }
        base.Kill();
    }

    public void SetUpgradeId(uint id)
    {
        upgradeIdentifier = id;
    }

    public uint GetUpgradeId()
    {
        return upgradeIdentifier;
    }
}
