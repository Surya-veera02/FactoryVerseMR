
using UnityEngine;

public class HomeCanvasHeadFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform head;

    [Header("Position")]
    [SerializeField] private float distance = 1.5f;
    [SerializeField] private float verticalOffset = -0.05f;
    [SerializeField] private float followSpeed = 4f;
    [SerializeField] private float recenterDistance = 0.35f;
    [SerializeField] private float recenterAngle = 20f;

    private bool initialized;

    private void LateUpdate()
    {
        if (head == null)
            return;

        Vector3 forward = Vector3.ProjectOnPlane(
            head.forward,
            Vector3.up
        ).normalized;

        if (forward.sqrMagnitude < 0.001f)
            forward = transform.forward;

        Vector3 targetPosition =
            head.position +
            forward * distance +
            Vector3.up * verticalOffset;

        Quaternion targetRotation =
            Quaternion.LookRotation(forward, Vector3.up);

        if (!initialized)
        {
            transform.SetPositionAndRotation(
                targetPosition,
                targetRotation
            );

            initialized = true;
            return;
        }

        float distanceFromTarget =
            Vector3.Distance(transform.position, targetPosition);

        float angleFromTarget =
            Quaternion.Angle(transform.rotation, targetRotation);

        if (distanceFromTarget > recenterDistance ||
            angleFromTarget > recenterAngle)
        {
            float t = 1f - Mathf.Exp(
                -followSpeed * Time.deltaTime
            );

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                t
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                t
            );
        }
    }
}
