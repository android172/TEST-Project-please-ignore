using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControler : MonoBehaviour
{   
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        transform.position = Camera.main.transform.position;
        transform.up = Camera.main.transform.up;

        /*
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
        */
        
        //rb.AddForce(0,0,5000.0f);

        
    }
}
