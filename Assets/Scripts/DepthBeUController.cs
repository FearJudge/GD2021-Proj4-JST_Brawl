using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthBeUController : MonoBehaviour
{
    public Transform feet;
    public GameObject body;
    public float speed;
    private float feetCollSize = 0.11f;
    private float feetEdgeSize = 0.02f;
    private float speedX = 0;
    private float speedY = 0;
    private float moveThreshold = 0.3f;
    private bool feetXPosOn = false;
    private bool feetXNegOn = false;
    private bool feetYPosOn = false;
    private bool feetYNegOn = false;
    private bool airborne = false;
    private float feetOffset = 0f;

    SpriteRenderer sr;
    Rigidbody rb;
    Animator animator;
    [SerializeField] LayerMask groundMask;

    // Start is called before the first frame update
    void Start()
    {
        feetOffset = feet.localPosition.y;
        rb = body.GetComponent<Rigidbody>();
        sr = body.GetComponent<SpriteRenderer>();
        animator = body.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveCharacter();
        AnimateCharacter();
        CharacterJump();
    }

    void MoveCharacter()
    {
        float xMove = Input.GetAxis("Horizontal");
        float yMove = Input.GetAxis("Vertical");
        Collider[] pX = Physics.OverlapBox(new Vector3((feet.position.x + feetCollSize), feet.position.y, feet.position.z), (Vector3.one * feetEdgeSize), feet.rotation, groundMask);
        Collider[] nX = Physics.OverlapBox(new Vector3((feet.position.x - feetCollSize), feet.position.y, feet.position.z), (Vector3.one * feetEdgeSize), feet.rotation, groundMask);
        Collider[] pY = Physics.OverlapBox(new Vector3(feet.position.x, feet.position.y, (feet.position.z + feetCollSize)), (Vector3.one * feetEdgeSize), feet.rotation, groundMask);
        Collider[] nY = Physics.OverlapBox(new Vector3(feet.position.x, feet.position.y, (feet.position.z - feetCollSize)), (Vector3.one * feetEdgeSize), feet.rotation, groundMask);
        speedX = 0;
        speedY = 0;

        if ((pX.Length > 0 && xMove > moveThreshold) || (nX.Length > 0 && xMove < -moveThreshold))
        {
            speedX = xMove;
        }
        if ((pY.Length > 0 && yMove > moveThreshold) || (nY.Length > 0 && yMove < -moveThreshold))
        {
            speedY = yMove;
        }
        transform.position += new Vector3(speedX, 0, speedY) * speed * Time.deltaTime;
    }

    void CharacterJump()
    {
        Collider[] fD = Physics.OverlapBox(new Vector3(feet.position.x, body.transform.position.y - 1f, feet.position.z), (Vector3.one * feetEdgeSize), feet.rotation, groundMask);
        if (fD.Length > 0 && rb.velocity.y <= 0f)
        {
            rb.velocity = new Vector3(0, 0, 0);
            rb.isKinematic = true;
            airborne = false;
        }
        if (Input.GetButtonDown("Submit") && fD.Length > 0 && !airborne)
        {
            rb.isKinematic = false;
            rb.velocity = new Vector3(0, 7f, 0);
            airborne = true;
        }
    }

    void AnimateCharacter()
    {
        if (Mathf.Abs(speedX) > 0.001f || Mathf.Abs(speedY) > 0.001f) { animator.SetBool("running", true); }
        else { animator.SetBool("running", false); }
        if (speedX < 0) { sr.flipX = true; }
        else if (speedX > 0) { sr.flipX = false; }
    }
}
