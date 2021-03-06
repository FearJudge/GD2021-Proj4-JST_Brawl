using System.Collections;
using System.Collections.Generic;
using ToonBoom.Harmony;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DepthBeUController : MonoBehaviour
{
    protected struct AllowedMovementAroundChar
    {
        public bool feetXplus;
        public bool feetXneg;
        public bool feetZplus;
        public bool feetZneg;
        public bool bodyXplus;
        public bool bodyXneg;
        public bool bodyZplus;
        public bool bodyZneg;
    }

    protected List<UpgradeLibrary.IUpgrade> addedUpgrades = new List<UpgradeLibrary.IUpgrade>();

    public Health hpScript;
    [HideInInspector] public bool playerFacingRight = true;
    [HideInInspector] public Transform feet;
    public GameObject body;
    public float speed;
    [HideInInspector] public bool isAlive = true;
    float maxResistance = 10f;
    float resistanceRegen = 0f;
    public float resistance = 10f;
    public float hangTime = 0.2f;
    public float knockBackRes = 1f;
    public float stunnedRes = 1f;
    private float airborneTimer = 0f;
    public int baseJumpsAvailable = 1;
    private int jumpsAvailable = 1;
    [SerializeField] private float feetCollSize = 0.14f;
    [SerializeField] private float bodyCollSize = 0.32f;
    [SerializeField] private float feetEdgeSize = 0.02f;
    [SerializeField] private float bodyEdgeSize = 0.09f;
    [SerializeField] private float bodyLandingPad = 0.25f;
    [SerializeField] private float bodyHeight = 0.9f;
    private readonly float moveThreshold = 0.3f;
    public bool airborne = false;
    public bool frozen = false;
    public bool halted = false;
    public bool Invulnerable
    {
        set
        {
            invulnerable = value;
            if (value == true) { if (outlineBrain != null) { SetInvulnerableOutline(false); } }
            else { if (outlineBrain != null && resistance > 0f) { outlineBrain.DeactivateOutlines(); } }
        }
        get
        {
            return invulnerable;
        }
    }
    private bool invulnerable = false;
    
    protected float stunnedFor = 0f;
    protected bool jumpRequested = false;
    private float feetOffset = 0f;
    private Vector3 force = Vector3.zero;
    [SerializeField] private Transform cameraFocus;
    private CameraLock cameraLocking;

    public HackyOutline outlineBrain;
    [HideInInspector] public HarmonyRenderer sr;
    [HideInInspector] public Rigidbody rb_root;
    [HideInInspector] public Rigidbody rb_body;
    [HideInInspector] public BoxCollider playerCollisionBox;
    [SerializeField] GameObject hurtBoxObject;
    [HideInInspector] public HurtBox hb;
    public ProjectileSpawner[] projectilespawners;
    [HideInInspector] public Animator animator;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask collideWith;
    private LayerMask hitBoxLayer = new LayerMask();
    private Color baseCol = Color.white;

    Collider[] feetCheckPosX;
    Collider[] feetCheckNegX;
    Collider[] feetCheckPosY;
    Collider[] feetCheckNegY;
    Collider[] bodyCheckPosX;
    Collider[] bodyCheckNegX;
    Collider[] bodyCheckPosY;
    Collider[] bodyCheckNegY;
    Collider[] feetCheckUnder;
    AllowedMovementAroundChar movementChecks = new AllowedMovementAroundChar();

    // Start is called before the first frame update
    void Awake()
    {
        SetUP();
    }

    protected void SetUP()
    {
        Debug.Log("Triggered!");
        feet = transform;
        maxResistance = resistance;
        if (cameraFocus == null) { cameraFocus = Camera.main.transform.parent; }
        cameraLocking = cameraFocus.GetComponent<CameraLock>();
        feetOffset = body.transform.localPosition.y - feet.transform.localPosition.y;
        hb = hurtBoxObject.GetComponent<HurtBox>();
        hitBoxLayer.value = (1 << hurtBoxObject.layer);
        rb_root = feet.GetComponent<Rigidbody>();
        rb_body = body.GetComponent<Rigidbody>();
        playerCollisionBox = body.GetComponent<BoxCollider>();
        sr = body.GetComponentInChildren<HarmonyRenderer>();
        animator = body.GetComponentInChildren<Animator>();
        baseCol = sr.Color;
        GetCollisionsAroundCharacter();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        MoveCharacter();
        CharacterAirborne();
        RegainResistance();
    }

    public virtual void FixedUpdate()
    {
        GetCollisionsAroundCharacter();
        PreventRigidBodyCollisions();
        TickDownStun();
        CheckForOoB();
    }

    protected void GetCollisionsAroundCharacter()
    {
        Vector3 feetBox = new Vector3(feetEdgeSize, 2000f, feetEdgeSize);
        Vector3 bodyBox = new Vector3(bodyEdgeSize, bodyHeight, bodyEdgeSize);
        feetCheckPosX = Physics.OverlapBox(new Vector3((feet.position.x + feetCollSize), feet.position.y, feet.position.z), feetBox, feet.rotation);
        feetCheckNegX = Physics.OverlapBox(new Vector3((feet.position.x - feetCollSize), feet.position.y, feet.position.z), feetBox, feet.rotation);
        feetCheckPosY = Physics.OverlapBox(new Vector3(feet.position.x, feet.position.y, (feet.position.z + feetCollSize)), feetBox, feet.rotation);
        feetCheckNegY = Physics.OverlapBox(new Vector3(feet.position.x, feet.position.y, (feet.position.z - feetCollSize)), feetBox, feet.rotation);
        bodyCheckPosX = Physics.OverlapBox(new Vector3((body.transform.position.x + bodyCollSize), body.transform.position.y, body.transform.position.z), bodyBox, feet.rotation);
        bodyCheckNegX = Physics.OverlapBox(new Vector3((body.transform.position.x - bodyCollSize), body.transform.position.y, body.transform.position.z), bodyBox, feet.rotation);
        bodyCheckPosY = Physics.OverlapBox(new Vector3(body.transform.position.x, body.transform.position.y, (body.transform.position.z + bodyCollSize)), bodyBox, feet.rotation);
        bodyCheckNegY = Physics.OverlapBox(new Vector3(body.transform.position.x, body.transform.position.y, (body.transform.position.z - bodyCollSize)), bodyBox, feet.rotation);
        feetCheckUnder = Physics.OverlapBox(new Vector3(feet.position.x, body.transform.position.y - bodyHeight, feet.position.z), new Vector3(bodyLandingPad, bodyEdgeSize, bodyLandingPad / 2), feet.rotation, groundMask.value + 1);
        movementChecks.feetXplus = CheckCollision(feetCheckPosX, groundMask, 0);
        movementChecks.feetXneg = CheckCollision(feetCheckNegX, groundMask, 0);
        movementChecks.feetZplus = CheckCollision(feetCheckPosY, groundMask, 0);
        movementChecks.feetZneg = CheckCollision(feetCheckNegY, groundMask, 0);
        movementChecks.bodyXplus = CheckCollision(bodyCheckPosX, 0, collideWith);
        movementChecks.bodyXneg = CheckCollision(bodyCheckNegX, 0, collideWith);
        movementChecks.bodyZplus = CheckCollision(bodyCheckPosY, 0, collideWith);
        movementChecks.bodyZneg = CheckCollision(bodyCheckNegY, 0, collideWith);
    }

    protected void CheckForOoB()
    {
        if (frozen) { return; }
        if (transform.position.y <= -30) { Kill(); frozen = true; }
    }

    protected void RegainResistance()
    {
        if (resistanceRegen == 0f && resistance > 0f) { return; }
        resistanceRegen -= Time.deltaTime;
        if (resistanceRegen > 0f) { return; }
        resistanceRegen = 0f;
        resistance = maxResistance;
        if (outlineBrain != null) { outlineBrain.DeactivateOutlines(); }
    }

    protected virtual void MoveCharacter() { }

    protected bool CheckCollision(Collider[] listedCollisions, LayerMask require, LayerMask collide)
    {
        bool hasRequired = false;
        bool hasCollision = false;
        if (require.value == 0) { hasRequired = true; }
        if (listedCollisions.Length == 0 && require.value != 0) { return false; }
        foreach (Collider c in listedCollisions)
        {
            int maskForC = (1 << c.gameObject.layer);
            if (maskForC == require.value) { hasRequired = true; }
            else if ((maskForC & collide) != 0) { hasCollision = true; }
        }
        return (hasRequired && !hasCollision);
    }

    protected void ControlledCharacterMovement(float x, float z, float delta = 0f)
    {
        float forceX = 0f;
        float forceZ = 0f;

        if ( (x > moveThreshold && movementChecks.feetXplus && movementChecks.bodyXplus) || (x < -moveThreshold && movementChecks.feetXneg && movementChecks.bodyXneg) )
        {
            if ((cameraFocus.position.x + cameraLocking.cameraLockArea.x > transform.position.x && x > 0) || (cameraFocus.position.x - cameraLocking.cameraLockArea.x < transform.position.x && x < 0))
            { forceX = x; }
        }
        else if ((x < -moveThreshold && movementChecks.feetXneg && movementChecks.bodyXneg) || (x > moveThreshold && movementChecks.feetXplus && movementChecks.bodyXplus)) { forceX = x; }
        if ( (z > moveThreshold && movementChecks.feetZplus && movementChecks.bodyZplus) || (z < -moveThreshold && movementChecks.feetZneg && movementChecks.bodyZneg) )
        {
            forceZ = z;
        }
        else if ((z < -moveThreshold && movementChecks.feetZneg && movementChecks.bodyZneg) || (z > moveThreshold && movementChecks.feetZplus && movementChecks.bodyZplus)) { forceZ = z; }
        ControlledKnockBack();

        if (delta == 0f) { delta = Time.deltaTime; }
        transform.position += new Vector3(forceX, 0, forceZ) * delta * speed;
        AnimateCharacter(forceX, forceZ);
    }

    protected void ControlledKnockBack()
    {
        if ((rb_root.velocity.x > 0f && cameraFocus.position.x + cameraLocking.cameraLockArea.x < transform.position.x) || (rb_root.velocity.x < 0f && cameraFocus.position.x - cameraLocking.cameraLockArea.x > transform.position.x))
        {
            rb_root.velocity = new Vector3(0, rb_root.velocity.y, rb_root.velocity.z);
        }
    }

    protected void PreventRigidBodyCollisions()
    {
        float velX = rb_root.velocity.x;
        float velZ = rb_root.velocity.z;

        if ((velX > 0 && (!movementChecks.feetXplus || !movementChecks.bodyXplus)) || (velX < 0 && (!movementChecks.feetXneg || !movementChecks.bodyXneg))) { velX = 0f; }
        if ((velZ > 0 && (!movementChecks.feetZplus || !movementChecks.bodyZplus)) || (velZ < 0 && (!movementChecks.feetZneg || !movementChecks.bodyZneg))) { velZ = 0f; }

        rb_root.velocity = new Vector3(velX, rb_root.velocity.y, velZ);
    }

    protected void TickDownStun()
    {
        if (stunnedFor == 0f && !frozen) { if (animator.GetBool("stunned")) { animator.SetBool("stunned", false); } return; }
        stunnedFor -= Time.fixedDeltaTime;
        if (stunnedFor <= 0f) { stunnedFor = 0f; UnFreeze(); }
    }

    protected void CharacterAirborne()
    {
        AirborneChecks();
        JumpLogic();
        SetOffGroundIfDropped();
    }

    private void JumpLogic()
    {
        if ((jumpRequested && jumpsAvailable > 0 && !airborne && !frozen) || (jumpRequested && jumpsAvailable > 0 && airborneTimer <= 0f && rb_body.velocity.y >= 0.15f && !frozen))
        {
            jumpRequested = false;
            animator.SetBool("grounded", false);
            rb_body.velocity = new Vector3(0, 7f, 0);
            airborne = true;
            jumpsAvailable--;
            airborneTimer = hangTime;
        }
    }

    protected void SetOffGroundIfDropped()
    {
        if (airborneTimer <= 0f && !airborne)
        {
            airborneTimer = 0f;
            airborne = true;
            jumpsAvailable--;
        }
    }

    protected void AirborneChecks()
    {
        if (feetCheckUnder.Length > 0 && rb_body.velocity.y <= 0f)
        {
            animator.SetBool("grounded", true);
            rb_body.velocity = new Vector3(0, 0, 0);
            airborne = false;
            jumpsAvailable = baseJumpsAvailable;
            airborneTimer = hangTime;
        }
        if (feetCheckUnder.Length == 0)
        {
            airborneTimer -= Time.deltaTime;
        }
    }

    protected void AnimateCharacter(float speedX, float speedY)
    {
        if (frozen) { speedX = 0f; speedY = 0f; }
        if (Mathf.Abs(speedX) > 0.001f || Mathf.Abs(speedY) > 0.001f) { animator.SetBool("running", true); }
        else { animator.SetBool("running", false); }
        if (speedX < 0) { body.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); playerFacingRight = false; }
        else if (speedX > 0) { body.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); playerFacingRight = true; }
    }

    public void ChangeFacing()
    {
        if (playerFacingRight) { body.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); playerFacingRight = false; }
        else { body.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); playerFacingRight = true; }
    }

    protected void AnimateCharacter(string trigger)
    {
        for (int a = 0; a < animator.parameters.Length; a++)
        {
            if (animator.parameters[a].name == trigger) { animator.SetTrigger(trigger); return; }
        }
    }

    protected void AnimateCharacterBool(string boolean, bool boolvalue)
    {
        for (int a = 0; a < animator.parameters.Length; a++)
        {
            if (animator.parameters[a].name == boolean) { animator.SetBool(boolean, boolvalue); return; }
        }
    }

    public virtual void GetHit(int dmg, float stun, bool knockBack, Vector3 knockBackV, bool fromLeft, bool isCrit=false, float resistanceChange = 1f)
    {
        if (invulnerable || resistance <= 0f) { return; }
        if (isCrit) { dmg *= 2; }
        int dir = 1;
        if (!fromLeft) { dir = -1; knockBackV.x *= dir; }
        float res = ResistanceMod();
        hpScript.Hp -= Mathf.CeilToInt(dmg * res);
        stunnedFor = stun * res * stunnedRes;
        SetVelocity(knockBackV * res);
        if (stunnedFor > 0.01f) {
            frozen = true;
            sr.Color = Color.red;
            resistance -= stun * resistanceChange;
            AnimateCharacterBool("stunned", true);
            AnimateCharacter(0f, 0f);
        }
        if (outlineBrain != null && resistance <= 0f) { SetInvulnerableOutline(true); }
    }

    protected float ResistanceMod()
    {
        resistanceRegen = 3f;
        switch (resistance)
        {
            case float n when n >= maxResistance * 0.75f:
                return 1f;
            case float n when n >= maxResistance * 0.5f && n < maxResistance * 0.75f:
                return 0.9f;
            case float n when n >= maxResistance * 0.25f && n < maxResistance * 0.5f:
                return 0.7f;
            case float n when n > maxResistance * 0f && n < maxResistance * 0.25f:
                return 0.35f;
            case float n when n <= maxResistance * 0f:
                return 0f;
            default:
                return 1f;
        }
    }

    protected virtual void SetInvulnerableOutline(bool isResisted)
    {
        outlineBrain.ActivateOutlines();
    }

    public void Resistance(float val)
    {
        resistance -= val;
        if (outlineBrain != null && resistance <= 0f) { outlineBrain.ActivateOutlines(); }
    }

    public virtual void SetVelocity(Vector3 direction, bool flipX = false)
    {
        if (direction == Vector3.zero) { return; }
        if (flipX && !playerFacingRight) { direction.x *= -1; }
        Vector3 bodyDir = direction * knockBackRes;
        Vector3 rootDir = direction * knockBackRes;
        rb_body.velocity = bodyDir;
        if (rb_root.velocity.magnitude > rootDir.magnitude) { rootDir = rb_root.velocity + (0.1f * direction); rb_root.velocity = new Vector3(rootDir.x, 0, rootDir.z); }
        else { rb_root.velocity = new Vector3(rootDir.x, 0, rootDir.z); }
    }

    public virtual void AddUpgradeToCharacter(UpgradeLibrary.IUpgrade up, int linkLength=0)
    {
        if (linkLength >= 10) { return; }
        addedUpgrades.Add(up);
        if (up.upgradeId >= UpgradeLibrary.BREAKPOINT)
        {
            UpgradeLibrary.MoveUpgrade movePlus = (UpgradeLibrary.MoveUpgrade)up;
            ParseUpgrade(movePlus);
        }
        else
        {
            UpgradeLibrary.PlayerUpgrade playerPlus = (UpgradeLibrary.PlayerUpgrade)up;
            ParseUpgrade(playerPlus);
        }
        for (int a = 0; a < up.upgradeChain.Length; a++)
        {
            AddUpgradeToCharacter(UpgradeLibrary.GetUpgrade(up.upgradeChain[a]), linkLength++);
        }
    }

    protected virtual void ParseUpgrade(UpgradeLibrary.PlayerUpgrade up) { }
    protected virtual void ParseUpgrade(UpgradeLibrary.MoveUpgrade up) { }

    protected virtual void UnFreeze()
    {
        sr.Color = baseCol;
        frozen = false;
    }

    public virtual void Kill()
    {
        isAlive = false;
        AnimateCharacterBool("stunned", false);
        animator.SetTrigger("Die");
        sr.Color = baseCol;
        rb_root.velocity = Vector3.zero;
        playerCollisionBox.gameObject.layer = 11;
        stunnedFor = 0f;
        frozen = true;
    }

    public virtual void Dissolve()
    {
        Destroy(gameObject);
    }
}
