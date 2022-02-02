using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{

    public float scrollSpeed = 1f;

    public Transform cameraTarget;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float camX = Mathf.Lerp(transform.position.x, cameraTarget.position.x, scrollSpeed * Time.deltaTime);
        transform.position = new Vector2(camX, transform.position.y);
    }
}
