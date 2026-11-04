
using System.Collections;
using UnityEngine;

public class PlaceFactoryBoard : MonoBehaviour
{
    [Header("XR Camera")]
    [SerializeField] private Transform headTransform;

    [Header("Placement Settings")]
    [SerializeField] private float distanceFromPlayer = 0.85f;
    [SerializeField] private float heightBelowEyes = 0.65f;

    private IEnumerator Start()
    {
        // Allow XR tracking to initialize.
        yield return new WaitForSeconds(1f);

        if (headTransform == null)
        {
            Debug.LogError("XR Camera reference is missing!");
            yield break;
        }

        // Get the player's horizontal viewing direction.
        Vector3 forward = Vector3.ProjectOnPlane(
            headTransform.forward,
            Vector3.up
        );

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        // Position the board in front of the player.
        Vector3 boardPosition =
            headTransform.position +
            forward * distanceFromPlayer -
            Vector3.up * heightBelowEyes;

        transform.position = boardPosition;

        // Rotate the board to face the player's direction.
        // Only yaw is applied, keeping the board horizontal.
        transform.rotation = Quaternion.LookRotation(
            forward,
            Vector3.up
        );

        Debug.Log("Factory board positioned successfully.");
    }
}
