using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public float scrollingSpeed = 1f;
    public Transform targetTransform;
    private Rigidbody2D rb2;
    public float playerMovement;
    public Vector2 resistance = Vector2.one;

    // Start is called before the first frame update
    void Start()
    {
        rb2 = targetTransform.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //playerMovement = rb2.velocity.x;
        //transform.Translate(-Vector2.right*playerMovement*scrollingSpeed*resistance.x*Time.deltaTime, 0);
        transform.position = targetTransform.position * resistance * -scrollingSpeed;
    }
}
