
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FactoryReturnZone : MonoBehaviour
{

    [Header("Credit Feedback")]
    [SerializeField]
    private FloatingCreditFeedback feedbackPrefab;

    [SerializeField]
    private Transform feedbackSpawnPoint;
    private BoxCollider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();
        zoneCollider.isTrigger = true;
    }

    public void ShowCreditFeedback(int amount)
    {
        if (amount == 0 ||
            feedbackPrefab == null ||
            feedbackSpawnPoint == null)
            return;

        FloatingCreditFeedback feedback =
            Instantiate(
                feedbackPrefab,
                feedbackSpawnPoint.position,
                Quaternion.identity
            );

        feedback.Show(amount);
    }

    public bool ContainsPoint(Vector3 worldPosition)
    {
        if (zoneCollider == null)
            return false;

        Vector3 localPoint =
            transform.InverseTransformPoint(worldPosition);

        Vector3 offset =
            localPoint - zoneCollider.center;

        Vector3 halfSize =
            zoneCollider.size * 0.5f;

        return Mathf.Abs(offset.x) <= halfSize.x &&
               Mathf.Abs(offset.y) <= halfSize.y &&
               Mathf.Abs(offset.z) <= halfSize.z;
    }
}
