using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthBeUController : MonoBehaviour
{
    public static bool playerFacingRight = true;
    public Transform feet;
    public GameObject body;
    public float speed;
    public float hangTime = 0.2f;
    private float airborneTimer = 0f;
    public int baseJumpsAvailable = 1;
    private int jumpsAvailable = 1;
    private float feetCollSize = 0.06f;
    private float bodyCollSize = 0.22f;
    private float feetEdgeSize = 0.02f;
    private float bodyEdgeSize = 0.05f;
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

    SpriteRenderer sr;
    [HideInInspector]public Rigidbody rb;
    [SerializeField] GameObject hurtBoxObject;
    [HideInInspector] public HurtBox hb;
    [HideInInspector] public Animator animator;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask playerBodyMask;
    [SerializeField] LayerMask playerFeetMask;

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
        feetOffset = body.transform.localPosition.y - feet.transform.localPosition.y;
        hb = hurtBoxObject.GetComponent<HurtBox>();
        rb = body.GetComponent<Rigidbody>();
        sr = body.GetComponent<SpriteRenderer>();
        animator = body.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetCollisionsAroundCharacter();
        MoveCharacter();
        ProjectLanding();
        AnimateCharacter();
        CharacterJump();
    }

    void GetCollisionsAroundCharacter()
    {
        Vector3 feetBox = new Vector3(feetEdgeSize, 2000f, feetEdgeSize);
        Vector3 bodyBox = new Vector3(bodyEdgeSize, bodyHeight, bodyEdgeSize);
        feetCheckPosX = Physics.OverlapBox(new Vector3((feet.position.x + feetCollSize), feet.position.y, feet.position.z), feetBox, feet.rotation);
        feetCheckNegX = Physics.OverlapBox(new Vector3((feet.position.x - feetCollSize), feet.position.y, feet.position.z), feetBox, feet.rotation);
        feetCheckPosY = Physics.OverlapBox(new Vector3(feet.position.x, feet.position.y, (feet.position.z + feetCollSize)), feetBox, feet.rotation);
        feetCheckNegY = Physics.OverlapBox(new Vector3(feet.position.x, feet.position.y, (feet.position.z - feetCollSize)), feetBox, feet.rotation);
        bodyCheckPosX = Physics.OverlapBox(new Vector3((feet.position.x + bodyCollSize), body.transform.position.y, feet.position.z), bodyBox, feet.rotation);
        bodyCheckNegX = Physics.OverlapBox(new Vector3((feet.position.x - bodyCollSize), body.transform.position.y, feet.position.z), bodyBox, feet.rotation);
        bodyCheckPosY = Physics.OverlapBox(new Vector3(feet.position.x, body.transform.position.y, (feet.position.z + bodyCollSize)), bodyBox, feet.rotation);
        bodyCheckNegY = Physics.OverlapBox(new Vector3(feet.position.x, body.transform.position.y, (feet.position.z - bodyCollSize)), bodyBox, feet.rotation);
        feetCheckUnder = Physics.OverlapBox(new Vector3(feet.position.x, body.transform.position.y - 1f, feet.position.z), (Vector3.one * bodyEdgeSize), feet.rotation, groundMask.value + 1);
    }

    bool CheckCollision(Collider[] listedCollisions, bool isFeet = true)
    {
        bool hasGround = false;
        bool hasObstacle = false;
        if (listedCollisions.Length == 0 && isFeet) { return false; }
        foreach (Collider c in listedCollisions)
        {
            int maskForC = (1 << c.gameObject.layer);
            if (maskForC == groundMask.value) { hasGround = true; }
            else if (!isFeet && maskForC != playerBodyMask && maskForC != playerFeetMask) { hasObstacle = true; }
        }
        if (!hasGround && isFeet) { return hasGround; }
        return !hasObstacle;
    }

    void MoveCharacter()
    {
        if (frozen) { return; }
        float xMove = Input.GetAxis("Horizontal");
        float yMove = Input.GetAxis("Vertical");

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
        transform.position += new Vector3(speedX, 0, speedY) * speed * Time.deltaTime;
    }

    void ProjectLanding()
    {
        LayerMask lm = new LayerMask();
        lm.value = groundMask.value + 1;
        Ray landingGround = new Ray(body.transform.position, Vector3.down);
        Physics.Raycast(landingGround, out RaycastHit info, 25f, lm);
        feet.position = info.point;
    }

    void CharacterJump()
    {
        if (feetCheckUnder.Length > 0 && rb.velocity.y <= 0f)
        {
            rb.velocity = new Vector3(0, 0, 0);
            airborne = false;
            jumpsAvailable = baseJumpsAvailable;
            airborneTimer = hangTime;
        }
        if (feetCheckUnder.Length == 0)
        {
            airborneTimer -= Time.deltaTime;
        }
        if ((Input.GetButtonDown("Submit") && jumpsAvailable > 0 && !airborne && !frozen) || (Input.GetButtonDown("Submit") && jumpsAvailable > 0 && airborneTimer <= 0f && rb.velocity.y >= 0.15f && !frozen))
        {
            rb.velocity = new Vector3(0, 7f, 0);
            airborne = true;
            jumpsAvailable--;
            airborneTimer = hangTime;
        }
        if (airborneTimer <= 0f && !airborne)
        {
            airborneTimer = 0f;
            airborne = true;
            jumpsAvailable--;
        }
    }

    void AnimateCharacter()
    {
        if (Mathf.Abs(speedX) > 0.001f || Mathf.Abs(speedY) > 0.001f) { animator.SetBool("running", true); }
        else { animator.SetBool("running", false); }
        if (speedX < 0) { sr.flipX = true; playerFacingRight = false; }
        else if (speedX > 0) { sr.flipX = false; playerFacingRight = true; }
    }
}
