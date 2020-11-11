using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCameraController : MonoBehaviour {

    public float movemont_speed = 5f;
    public float mouse_sensitivity_x = 900f;
    public float mouse_sensitivity_y = 900f;

    private float x_rotation = .0f;
    private float y_rotation = .0f;

    private void OnValidate() {
        if (movemont_speed < 1f) movemont_speed = 1f;
        if (mouse_sensitivity_x < 1f) mouse_sensitivity_x = 1f;
        if (mouse_sensitivity_y < 1f) mouse_sensitivity_y = 1f;
    }

    // Start is called before the first frame update
    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update() {
        // move
        bool pressed_w = Input.GetKey(KeyCode.W);
        bool pressed_a = Input.GetKey(KeyCode.A);
        bool pressed_s = Input.GetKey(KeyCode.S);
        bool pressed_d = Input.GetKey(KeyCode.D);
        bool pressed_shift = Input.GetKey(KeyCode.LeftShift);
        bool pressed_ctrl = Input.GetKey(KeyCode.LeftControl);

        Vector3 move_to = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) move_to += transform.forward;
        if (Input.GetKey(KeyCode.S)) move_to -= transform.forward;
        if (Input.GetKey(KeyCode.A)) move_to -= transform.right;
        if (Input.GetKey(KeyCode.D)) move_to += transform.right;
        if (Input.GetKey(KeyCode.LeftShift)) move_to += transform.up;
        if (Input.GetKey(KeyCode.LeftControl)) move_to -= transform.up;

        transform.localPosition += move_to.normalized * movemont_speed * Time.deltaTime;

        // look around
        float mouse_x = Input.GetAxis("Mouse X") * mouse_sensitivity_x * Time.deltaTime;
        float mouse_y = Input.GetAxis("Mouse Y") * mouse_sensitivity_y * Time.deltaTime;

        x_rotation -= mouse_y;
        y_rotation += mouse_x;

        transform.localRotation = Quaternion.Euler(x_rotation, y_rotation, 0);
    }
}
