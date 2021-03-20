using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControler : MonoBehaviour
{    public float gravity;

    private bool isGrounded;
    private float gravForce;
    private float jumpSpeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        gravForce = gravity;
        isGrounded = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(isGrounded && Input.GetKey(KeyCode.Space))
        {
            jumpSpeed += 20.0f;
            transform.position = transform.up * jumpSpeed;
            gravForce = 0;
        }

        if(gravForce < 1)
            gravForce += Time.deltaTime * 10;
        if(gravForce > 1)
            gravForce = 1;

        jumpSpeed -= gravForce;

        

        
    }
}
