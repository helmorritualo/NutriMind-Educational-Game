using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float acceleration = 14f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.4f;
    [SerializeField] private float groundedStickForce = -2f;
    [SerializeField] private Transform visualModel;
    [SerializeField] private Joystick movementJoystick;
    [SerializeField] private Button jumpButton;
    [SerializeField] private float groundProbeHeight = 10f;
    [SerializeField] private float groundProbeDistance = 100f;

    private CharacterController _controller;
    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        ResolveMovementJoystick();
        WireJumpButton();
        AlignVisualModel();
        ApplySpawnPointIfPresent();
        SnapFeetToGround();
    }

    private void Update()
    {
        Vector2 rawInput = ReadMoveInput();
        float inputMagnitude = Mathf.Clamp01(rawInput.magnitude);

        Vector3 targetHorizontal = Vector3.zero;
        if (inputMagnitude > 0.01f)
        {
            Vector3 inputDirection = new Vector3(rawInput.x, 0f, rawInput.y).normalized;
            inputDirection = ToCameraRelativeDirection(inputDirection);
            float targetSpeed = Mathf.Lerp(walkSpeed, runSpeed, inputMagnitude);
            targetHorizontal = inputDirection * targetSpeed;
        }

        _horizontalVelocity = Vector3.MoveTowards(
            _horizontalVelocity,
            targetHorizontal,
            acceleration * Time.deltaTime);

        if (_horizontalVelocity.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_horizontalVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        if (_controller.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = groundedStickForce;
        }

        if (ReadJumpPressed())
        {
            Jump();
        }

        _verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion = _horizontalVelocity;
        motion.y = _verticalVelocity;
        _controller.Move(motion * Time.deltaTime);
    }

    private static Vector3 ToCameraRelativeDirection(Vector3 inputDirection)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return inputDirection;
        }

        Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        Vector3 worldDirection = camForward * inputDirection.z + camRight * inputDirection.x;
        return worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : inputDirection;
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 joystickInput = ReadJoystickInput();
        if (joystickInput.sqrMagnitude > 0.0001f)
        {
            return joystickInput;
        }

        return ReadKeyboardInput();
    }

    private Vector2 ReadJoystickInput()
    {
        if (movementJoystick == null)
        {
            return Vector2.zero;
        }

        return Vector2.ClampMagnitude(movementJoystick.Direction, 1f);
    }

    private Vector2 ReadKeyboardInput()
    {
        Vector2 input = Vector2.zero;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return input;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;

        return Vector2.ClampMagnitude(input, 1f);
    }

    public void Jump()
    {
        if (!_controller.isGrounded)
        {
            return;
        }

        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void ResolveMovementJoystick()
    {
        if (movementJoystick != null)
        {
            return;
        }

        GameObject joystickObject = GameObject.Find("MovementJoystick");
        if (joystickObject != null)
        {
            movementJoystick = joystickObject.GetComponent<Joystick>();
        }
    }

    private void WireJumpButton()
    {
        if (jumpButton == null)
        {
            GameObject jumpButtonObject = GameObject.Find("JumpButton");
            if (jumpButtonObject == null)
            {
                jumpButtonObject = GameObject.Find("JumpButton ");
            }

            if (jumpButtonObject != null)
            {
                jumpButton = jumpButtonObject.GetComponent<Button>();
            }
        }

        if (jumpButton == null)
        {
            return;
        }

        jumpButton.onClick.AddListener(Jump);
    }

    private static bool ReadJumpPressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
    }

    private void AlignVisualModel()
    {
        Transform model = visualModel != null ? visualModel : transform.Find("PlayerModel");
        if (model == null)
        {
            return;
        }

        // Unity capsule mesh pivot is at center; CharacterController feet are at transform.y.
        Vector3 localPosition = model.localPosition;
        localPosition.y = 1f;
        model.localPosition = localPosition;
    }

    private void ApplySpawnPointIfPresent()
    {
        GameObject spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint == null)
        {
            return;
        }

        _controller.enabled = false;
        transform.SetPositionAndRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
        _controller.enabled = true;
    }

    private void SnapFeetToGround()
    {
        if (!TryGetGroundHeight(transform.position, out float groundY))
        {
            return;
        }

        _controller.enabled = false;
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        _controller.enabled = true;
        _verticalVelocity = groundedStickForce;
    }

    private bool TryGetGroundHeight(Vector3 feetPosition, out float groundY)
    {
        Vector3 origin = feetPosition + Vector3.up * groundProbeHeight;
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, groundProbeDistance, ~0, QueryTriggerInteraction.Ignore);
        float bestY = float.MinValue;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (IsSelfCollider(hit.collider))
            {
                continue;
            }

            if (hit.point.y > bestY)
            {
                bestY = hit.point.y;
                found = true;
            }
        }

        groundY = bestY;
        return found;
    }

    private bool IsSelfCollider(Collider collider)
    {
        Transform hitTransform = collider.transform;
        return hitTransform == transform || hitTransform.IsChildOf(transform);
    }
}
