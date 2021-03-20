using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCameraController : MonoBehaviour 
{
    public float sensitivity = 50.0f;
    public float runMultiplier = 2;


    private float xRotation = 0.0f;
    private float minY = -80.0f;
    private float maxY = 80.0f;
    private Transform player;
    private Camera mainCam;
    private bool running;

    class CameraState 
    {
        public Vector3 position = new Vector3(0, 0, 0);
        public Vector3 forwards_direction = new Vector3(1, 0, 0);
        public Vector3 up_direction = new Vector3(0, 1, 0);
        public Vector3 right_direction = new Vector3(0, 0, 1);
        public Vector3 global_up = new Vector3(0, 1, 0);

        Quaternion get_rotation() 
        {
            return Quaternion.LookRotation(forwards_direction, up_direction);
        }

        public void update_transform(Transform t) 
        {
            t.position = position;
            t.rotation = get_rotation();
        }

        public void lerp_towards(CameraState target, float position_lerp_factor, float rotation_lerp_factor)
        {
            position = Vector3.Lerp(position, target.position, position_lerp_factor);
            forwards_direction = Vector3.Lerp(forwards_direction, target.forwards_direction, rotation_lerp_factor);
            up_direction = Vector3.Lerp(up_direction, target.up_direction, rotation_lerp_factor);
            right_direction = Vector3.Lerp(right_direction, target.right_direction, rotation_lerp_factor);
            global_up = Vector3.Lerp(global_up, target.global_up, rotation_lerp_factor);
        }
    }

    CameraState current_state;
    CameraState target_state;

    bool cursor_locked = false;

    GameObject planet;
    bool on_planet = false;
    float planet_effect;

    // Speeds
    public float movement_speed = 10f;
    public float tilt_speed = 1f;

    public AnimationCurve mouse_sensitivity_curve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));    
    
    Vector3 GetInputTranslationDirection() 
    {
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W)) 
            direction += target_state.forwards_direction;
        if (Input.GetKey(KeyCode.S)) 
            direction += -target_state.forwards_direction;
        if (Input.GetKey(KeyCode.A)) 
            direction += target_state.right_direction;
        if (Input.GetKey(KeyCode.D)) 
           direction += -target_state.right_direction;
        //if (Input.GetKey(KeyCode.LeftControl)) 
        //    direction += -target_state.up_direction;
        //if(Input.GetKey(KeyCode.Space))
        //    direction += target_state.up_direction;

        return direction;
    }

    void OnEnable() 
    {
        // camera and player init
        current_state = new CameraState();
        target_state = new CameraState();
        player = GameObject.FindWithTag("Player").transform;
        mainCam = Camera.main;
        current_state.position = player.position;
        target_state.position = player.position;
        current_state.position.y += 0.5f;
        target_state.position.y += 0.5f;
        //player.transform.forward = current_state.forwards_direction;
        //player.transform.rotation = Quaternion.FromToRotation(player.transform.up, current_state.global_up) * player.transform.rotation;

        // unlock mouse
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        cursor_locked = !cursor_locked;
    }

    // Update is called once per frame
    void Update() 
    {
        // check if we are near a planets gravitational field
        if (on_planet) 
        {
            float distance = Vector3.Distance(transform.position, planet.transform.position);
            if (distance > planet_effect) 
            {
                on_planet = false;
            }
        }
        else 
        {
            foreach (var surface in GameObject.FindGameObjectsWithTag("Surface")) {
                float gravitational_effect_radius = surface.GetComponent<Planet>().shape_settings.radius * 2f;
                float distance = Vector3.Distance(transform.position, surface.transform.position);
                Debug.Log("D: " + surface.transform.position);
                if (distance < gravitational_effect_radius) {
                    planet = surface;
                    planet_effect = gravitational_effect_radius;
                    on_planet = true;
                    break;
                }
            }
        }

        // Lock and unlock cursor
        if (Input.GetMouseButtonDown(1)) 
        {
            if (cursor_locked)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
                Cursor.lockState = CursorLockMode.Locked;
            cursor_locked = !cursor_locked;
        }

        // Rotation
        // Tilt
        if (on_planet) 
        {
            Vector3 new_global_up = transform.position - planet.transform.position;
            Quaternion rot = Quaternion.FromToRotation(target_state.global_up, new_global_up);
            target_state.global_up = new_global_up;
            target_state.forwards_direction = rot * target_state.forwards_direction;
            target_state.right_direction = rot * target_state.right_direction;
        }
        else 
        {
            if (Input.GetKey(KeyCode.Q)) 
            {
                Vector3 global_forwards = Vector3.Cross(target_state.global_up, target_state.right_direction).normalized;
                target_state.right_direction = Quaternion.AngleAxis(tilt_speed * Time.deltaTime, target_state.forwards_direction) * target_state.right_direction;
                target_state.global_up = Vector3.Cross(target_state.right_direction, global_forwards).normalized;
            }
            else if (Input.GetKey(KeyCode.E)) 
            {
                Vector3 global_forwards = Vector3.Cross(target_state.global_up, target_state.right_direction).normalized;
                target_state.right_direction = Quaternion.AngleAxis(-tilt_speed * Time.deltaTime, target_state.forwards_direction) * target_state.right_direction;
                target_state.global_up = Vector3.Cross(target_state.right_direction, global_forwards).normalized;
            }
        }
        // Mouse movement
        if (cursor_locked) 
        {
            var mouse_movement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            var mouseSensitivityFactor = mouse_sensitivity_curve.Evaluate(mouse_movement.magnitude);

            // left-right
            target_state.forwards_direction = Quaternion.AngleAxis(mouse_movement.x * mouseSensitivityFactor, target_state.global_up) * current_state.forwards_direction;
            target_state.right_direction = Vector3.Cross(target_state.forwards_direction, target_state.global_up).normalized;
            // up-down
            Vector3 new_forwards = Quaternion.AngleAxis(mouse_movement.y * mouseSensitivityFactor, target_state.right_direction) * target_state.forwards_direction;
            float up_down_angle = Vector3.Angle(new_forwards, target_state.global_up);
            if (up_down_angle > 5 && up_down_angle < 170)
                target_state.forwards_direction = new_forwards;
            // update up direction
            target_state.up_direction = Vector3.Cross(target_state.right_direction, target_state.forwards_direction).normalized;
            
        }

        // Translation
        Vector3 translation;

        if(Input.GetKey(KeyCode.LeftShift))
            translation = GetInputTranslationDirection().normalized * movement_speed * runMultiplier * Time.deltaTime;

        else
            translation = GetInputTranslationDirection().normalized * movement_speed * Time.deltaTime;
        
        target_state.position = current_state.position + translation;
        

        // interpolate between target state and current state
        var position_lerp_factor = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / 0.2f) * Time.deltaTime);
        var rotation_lerp_factor = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / 0.01f) * Time.deltaTime);
        current_state.lerp_towards(target_state, position_lerp_factor, rotation_lerp_factor);

        current_state.update_transform(transform);
        /*
        // camera/player rotation
        float rotX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float rotY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xRotation -= rotY;
        xRotation = MoveByAngle(xRotation, minY, maxY);
        transform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);
        //player.transform.rotation = Quaternion.AngleAxis(rotX, current_state.global_up) * player.transform.rotation;
        */
    }

    private static float MoveByAngle(float angle, float min, float max)
    {
        return Mathf.Clamp(angle, min, max);
    }
}
