using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Universe;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public LayerMask groundMask;
    public float downRayDist = 0.5f;

    [Header("Camera:")]
    public float sensitivityHorizontal = 1.0f;
    public float sensitivityVertical = 1.0f;
    public float angleMin = -80f, angleMax = 90f;

    [Header("Movement:")]
    public float speed = 10f;
    public float stickToGroundForce = 0.1f;

    private Vector3 desiredVelocity;
    private Vector3 gravity;

    private Vector2 rotation;

    private SolarSystem ss;
    private Rigidbody rb;
    private Camera cam;

    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        ss = FindObjectOfType<SolarSystem>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        gravity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate
        Vector3 up = (-gravity).normalized;

        rotation.y += Input.GetAxis("Mouse X");
        rotation.x -= Input.GetAxis("Mouse Y");
        rotation.x = Mathf.Clamp(rotation.x, angleMin, angleMax);

        transform.rotation = Quaternion.AngleAxis(rotation.y * sensitivityHorizontal, up);
        cam.transform.localEulerAngles = new Vector2(rotation.x * sensitivityVertical, 0);

        //Move
        var input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        desiredVelocity = transform.up * input.z + transform.right * input.x;

        var velocity = desiredVelocity * speed + (grounded ? gravity.normalized * stickToGroundForce : gravity);
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        gravity = ss.CalculateVelocity(rb.position, gravity, Time.fixedDeltaTime);

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
