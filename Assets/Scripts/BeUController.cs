using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeUController : MonoBehaviour
{
    public Transform feet;
    public GameObject body;
    public float speed;
    private float feetCollSize = 0.11f;
    private float feetEdgeSize = 0.02f;
    private float speedX = 0;
    private float speedY = 0;
    private float groundLevel = 0;
    private float moveThreshold = 0.3f;
    private bool feetXPosOn = false;
    private bool feetXNegOn = false;
    private bool feetYPosOn = false;
    private bool feetYNegOn = false;
    private bool airborne = false;
    private float feetOffset = 0f;

    SpriteRenderer sr;
    Rigidbody2D rb2;
    Animator animator;
    [SerializeField] LayerMask groundMask;

    // Start is called before the first frame update
    void Start()
    {
        feetOffset = feet.localPosition.y;
        rb2 = body.GetComponent<Rigidbody2D>();
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
        feetXPosOn = Physics2D.OverlapCircle(new Vector2(feet.position.x + feetCollSize, feet.position.y), feetEdgeSize, groundMask);
        feetXNegOn = Physics2D.OverlapCircle(new Vector2(feet.position.x - feetCollSize, feet.position.y), feetEdgeSize, groundMask);
        feetYPosOn = Physics2D.OverlapCircle(new Vector2(feet.position.x, feet.position.y + feetCollSize), feetEdgeSize, groundMask);
        feetYNegOn = Physics2D.OverlapCircle(new Vector2(feet.position.x, feet.position.y - feetCollSize), feetEdgeSize, groundMask);
        speedX = 0;
        speedY = 0;

        if ((feetXPosOn && xMove > moveThreshold) || (feetXNegOn && xMove < -moveThreshold))
        {
            speedX = xMove;
        }
        if ((feetYPosOn && yMove > moveThreshold) || (feetYNegOn && yMove < -moveThreshold))
        {
            speedY = yMove;
        }
        transform.position += new Vector3(speedX, speedY, 0) * speed * Time.deltaTime;
    }

    void CharacterJump()
    {
        if (body.transform.localPosition.y <= groundLevel && rb2.velocity.y <= 0f)
        {
            rb2.velocity = new Vector2(0, 0);
            rb2.isKinematic = true;
            airborne = false;
        }
        if (Input.GetButtonDown("Submit") && body.transform.localPosition.y <= groundLevel && !airborne)
        {
            rb2.isKinematic = false;
            rb2.velocity = new Vector2(0, 7f);
            airborne = true;
        }
    }

    public void Drop(float newFloorHeight)
    {
        if (newFloorHeight > groundLevel) { return; }
        groundLevel = newFloorHeight;
        feet.position = new Vector2(0, feetOffset + groundLevel);
    }

    void AnimateCharacter()
    {
        if (Mathf.Abs(speedX) > 0.001f || Mathf.Abs(speedY) > 0.001f) { animator.SetBool("running", true); }
        else { animator.SetBool("running", false); }
        if (speedX < 0) { sr.flipX = true; }
        else if (speedX > 0) { sr.flipX = false; }
    }
}
