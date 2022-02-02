using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    public float speed = 4f;
    public Vector3 mousePosition = Vector3.zero;
    public Rigidbody2D rb2;
    Animator animator;
    SpriteRenderer sr;
    public float jumpheight = 6f;
    public bool isGrounded = true;
    public Transform feet;
    public LayerMask layerMask;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        sr = gameObject.GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        //MoveCharacter();
        
    }

    private void FixedUpdate()
    {
        moveRB();
        jumpRB();
        PointToMouse();
    }

    void moveRB()
    {
        float x = Input.GetAxis("Horizontal") * speed;
        if (Mathf.Abs(x) > 0.001f) { animator.SetBool("running", true); }
        else { animator.SetBool("running", false); }
        if (x < 0) { sr.flipX = true; }
        else if (x > 0) { sr.flipX = false; }
        rb2.velocity = new Vector2(x, rb2.velocity.y);
    }

    void jumpRB()
    {
        isGrounded = Physics2D.OverlapCircle(feet.position, 0.2f, layerMask);
        if (Input.GetButton("Submit") && isGrounded)
        {
            rb2.velocity = new Vector2(rb2.velocity.x, jumpheight);
        }
    }

    void PointToMouse()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z - transform.position.z)));
        mousePosition -= transform.position;
    }
}
