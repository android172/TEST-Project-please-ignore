using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Universe;

[RequireComponent(typeof(Rigidbody))]
public class PlayerGravity : MonoBehaviour
{
    public LayerMask groundMask;
    public float downRayDist = 0.5f;

    public float stickToGroundForce = 0.1f;

    private Vector3 gravity;

    private SolarSystem ss;
    private Rigidbody rb;

    private bool grounded;

    public float jetPackSpeed;
    public float jumpPower;

    // Start is called before the first frame update
    void Start()
    {
        ss = FindObjectOfType<SolarSystem>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        gravity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //rb.MovePosition(rb.position + (grounded ? gravity.normalized * stickToGroundForce : gravity) * Time.deltaTime);

        Vector3 up = (-(gravity.normalized));

        if (Input.GetKey(KeyCode.Space))
        {
            rb.velocity += up * jetPackSpeed * Time.deltaTime;
        }

        if (grounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Hop");
                rb.AddForce(up * jumpPower);
                grounded = false;
            }
            else
            {
                rb.velocity += gravity.normalized * stickToGroundForce * Time.deltaTime;
            }
        }
        else
        {
            rb.velocity += gravity * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        gravity = ss.CalculateVelocity(rb.position, gravity, Time.fixedDeltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Check ground
        Ray downRay = new Ray(rb.position, gravity);
        Vector3 dRay = gravity.normalized * downRayDist;

        if (Physics.Raycast(downRay, maxDistance: downRayDist, layerMask: groundMask))
        {
            gravity = Vector3.zero;
            Debug.DrawRay(rb.position, dRay, Color.red);
            grounded = true;
        }
        else
        {
            Debug.DrawRay(rb.position, dRay, Color.blue);
            grounded = false;
        }
    }
}
