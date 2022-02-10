using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DepthBeUController : MonoBehaviour
{
    public bool playerFacingRight = true;
    public Transform feet;
    public GameObject body;
    public float speed;
    public float hangTime = 0.2f;
    private float airborneTimer = 0f;
    public int baseJumpsAvailable = 1;
    private int jumpsAvailable = 1;
    private float feetCollSize = 0.14f;
    private float bodyCollSize = 0.32f;
    private float feetEdgeSize = 0.02f;
    private float bodyEdgeSize = 0.09f;
    private float bodyHeight = 0.9f;
    private float speedX = 0;
    private float speedY = 0;
    private float moveThreshold = 0.3f;
    private bool feetXPosOn = false;
    private bool feetXNegOn = false;
    private bool feetYPosOn = false;
    private bool feetYNegOn = false;
    public bool airborne = false;
    public bool frozen = false;
    private float feetOffset = 0f;
    private Vector3 force = Vector3.zero;

    SpriteRenderer sr;
    [HideInInspector]public Rigidbody rb;
    [SerializeField] GameObject hurtBoxObject;
    [HideInInspector] public HurtBox hb;
    [HideInInspector] public Animator animator;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask playerBodyMask;
    [SerializeField] LayerMask playerFeetMask;
    private LayerMask hitBoxLayer = new LayerMask();

    Collider[] feetCheckPosX;
    Collider[] feetCheckNegX;
    Collider[] feetCheckPosY;
    Collider[] feetCheckNegY;
    Collider[] bodyCheckPosX;
    Collider[] bodyCheckNegX;
    Collider[] bodyCheckPosY;
    Collider[] bodyCheckNegY;
    Collider[] feetCheckUnder;

    // Start is called before the first frame update
    void Start()
    {
        SetUP();
    }

    protected void SetUP()
    {
        feetOffset = body.transform.localPosition.y - feet.transform.localPosition.y;
        hb = hurtBoxObject.GetComponent<HurtBox>();
        hitBoxLayer.value = (1 << hurtBoxObject.layer);
        rb = body.GetComponent<Rigidbody>();
        sr = body.GetComponentInChildren<SpriteRenderer>();
        animator = body.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        GetCollisionsAroundCharacter();
        MoveCharacter();
        ProjectLanding();
        AnimateCharacter();
        CharacterJump();
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
        feetCheckUnder = Physics.OverlapBox(new Vector3(feet.position.x, body.transform.position.y - 1f, feet.position.z), (Vector3.one * bodyEdgeSize), feet.rotation, groundMask.value + 1);
    }

    protected bool CheckCollision(Collider[] listedCollisions, bool isFeet = true)
    {
        bool hasGround = false;
        bool hasObstacle = false;
        if (listedCollisions.Length == 0 && isFeet) { return false; }
        foreach (Collider c in listedCollisions)
        {
            int maskForC = (1 << c.gameObject.layer);
            if (maskForC == groundMask.value) { hasGround = true; }
            else if (!isFeet && maskForC != playerBodyMask && maskForC != playerFeetMask && maskForC != hitBoxLayer) { hasObstacle = true; }
        }
        if (!hasGround && isFeet) { return hasGround; }
        return !hasObstacle;
    }

    protected void MoveCharacter()
    {
        if (frozen) { RealizeInvoluntaryMovement(); return; }
        float xMove = Input.GetAxis("Horizontal");
        float yMove = Input.GetAxis("Vertical");
        if (!MenuPauser.paused && Input.GetKeyDown(KeyCode.Escape)) { SceneManager.LoadSceneAsync("PauseMenuScene", LoadSceneMode.Additive); }

        RealizeMovement(xMove, yMove);
    }

    protected void RealizeMovement(float xMove, float yMove)
    {
        speedX = 0;
        speedY = 0;
        if ((CheckCollision(feetCheckPosX) && CheckCollision(bodyCheckPosX, false) && xMove > moveThreshold) || (CheckCollision(feetCheckNegX) && CheckCollision(bodyCheckNegX, false) && xMove < -moveThreshold))
        {
            speedX = xMove;
        }
        if ((CheckCollision(feetCheckPosY) && CheckCollision(bodyCheckPosY, false) && yMove > moveThreshold) || (CheckCollision(feetCheckNegY) && CheckCollision(bodyCheckNegY, false) && yMove < -moveThreshold))
        {
            speedY = yMove;
        }
        RealizeInvoluntaryMovement();
        transform.position += new Vector3(speedX, 0, speedY) * speed * Time.deltaTime;
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        body.transform.localPosition = new Vector3(0, body.transform.localPosition.y, 0);
    }

    protected void RealizeInvoluntaryMovement()
    {
        if (force.magnitude < 0.0005f) { force = Vector3.zero; return; }
        float X = 0;
        float Y = 0;
        if ((CheckCollision(feetCheckPosX) && CheckCollision(bodyCheckPosX, false) && force.x > 0) || (CheckCollision(feetCheckNegX) && CheckCollision(bodyCheckNegX, false) && force.x < 0))
        {
            X = force.x;
        }
        if ((CheckCollision(feetCheckPosY) && CheckCollision(bodyCheckPosY, false) && force.z > 0) || (CheckCollision(feetCheckNegY) && CheckCollision(bodyCheckNegY, false) && force.z < 0))
        {
            Y = force.z;
        }
        transform.position += new Vector3(X, 0, Y) * 5f * Time.deltaTime;
        force *= 0.95f;
    }

    protected void ProjectLanding()
    {
        LayerMask lm = new LayerMask
        {
            value = groundMask.value + 1
        };
        Ray landingGround = new Ray(body.transform.position, Vector3.down);
        Physics.Raycast(landingGround, out RaycastHit info, 25f, lm);
        feet.position = info.point;
    }

    protected void CharacterJump()
    {
        AirborneChecks();
        InputJump();
        SetOffGroundIfDropped();
    }

    private void InputJump()
    {
        if ((Input.GetButtonDown("Submit") && jumpsAvailable > 0 && !airborne && !frozen) || (Input.GetButtonDown("Submit") && jumpsAvailable > 0 && airborneTimer <= 0f && rb.velocity.y >= 0.15f && !frozen))
        {
            animator.SetBool("grounded", false);
            rb.velocity = new Vector3(0, 7f, 0);
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
        if (feetCheckUnder.Length > 0 && rb.velocity.y <= 0f)
        {
            animator.SetBool("grounded", true);
            rb.velocity = new Vector3(0, 0, 0);
            airborne = false;
            jumpsAvailable = baseJumpsAvailable;
            airborneTimer = hangTime;
        }
        if (feetCheckUnder.Length == 0)
        {
            airborneTimer -= Time.deltaTime;
        }
    }

    protected void AnimateCharacter()
    {
        if (Mathf.Abs(speedX) > 0.001f || Mathf.Abs(speedY) > 0.001f) { animator.SetBool("running", true); }
        else { animator.SetBool("running", false); }
        if (speedX < 0) { body.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); playerFacingRight = false; }
        else if (speedX > 0) { body.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); playerFacingRight = true; }
    }

    public virtual void GetHit(int dmg, float stun, bool knockBack, Vector3 knockBackV, bool fromLeft)
    {
        frozen = true;
        Invoke("UnFreeze", stun);
        int dir = 1;
        if (!fromLeft) { dir = -1; knockBackV.x *= dir; }
        force = knockBackV;
        rb.velocity += Vector3.up * knockBackV.y;
        if (knockBack) { rb.velocity += knockBackV; }
        else { rb.velocity += new Vector3(1f * dir, 2.6f, 0f); }
    }

    protected void UnFreeze()
    {
        frozen = false;
    }
}
