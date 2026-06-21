using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -19.62f;
    [SerializeField] private float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        // Lock the cursor while testing
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(x, 0f, z);

        if (input.magnitude > 0.01f)
        {
            input = Vector3.ProjectOnPlane(input, Vector3.up).normalized;

            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
                input = camForward * input.z + camRight * input.x;
            }
            else
            {
                input = transform.forward * input.z + transform.right * input.x;
            }

            input.y = 0f;
            controller.Move(input * moveSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}