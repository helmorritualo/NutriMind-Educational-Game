using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, -5f);
    [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.3f, 0f);
    [SerializeField] private float followSmoothTime = 0.2f;
    [SerializeField] private float rotationSmoothTime = 0.15f;

    private Vector3 _smoothVelocity;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            transform.position = GetDesiredPosition();
            transform.rotation = GetDesiredRotation();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            GetDesiredPosition(),
            ref _smoothVelocity,
            followSmoothTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            GetDesiredRotation(),
            Time.deltaTime / rotationSmoothTime);
    }

    private Vector3 GetDesiredPosition()
    {
        return target.position
            + target.right * offset.x
            + Vector3.up * offset.y
            + target.forward * offset.z;
    }

    private Quaternion GetDesiredRotation()
    {
        Vector3 lookDirection = (target.position + lookAtOffset) - transform.position;
        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            return transform.rotation;
        }

        return Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}
