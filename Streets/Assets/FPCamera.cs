using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCamera : MonoBehaviour
{
    public float speed = 0.5f;
    public float jump = 0.5f;
    public float sensitivity = 1f;
    public Transform camera;
    public Camera mainCamera;
    public Camera fpCamera;
    private Camera activeCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool grounded = true;

    // Start is called before the first frame update
    void Start()
    {
        activeCamera = mainCamera;
        SetActiveCamera(activeCamera);  
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (camera == null)
            camera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
		{
			// Switch cameras
			if (activeCamera == fpCamera)
				SetActiveCamera(mainCamera);
			else
				SetActiveCamera(fpCamera);
		}
        if (activeCamera == fpCamera)
        {
            HandleMovement();
            HandleMouseLook();
            HandleJump();
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        camera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 velocity = move * speed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.C) && grounded)
        {
            rb.AddForce(Vector3.up * jump, ForceMode.Impulse);
            grounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            if (Vector3.Dot(collision.contacts[0].normal, Vector3.up) > 0.5f)
                grounded = true;
        }
    }

    void SetActiveCamera(Camera cam)
    {

        fpCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(false);

        cam.gameObject.SetActive(true);
        activeCamera = cam;

        if (activeCamera == fpCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Use WASD to move, mouse to look, and C to jump");
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
