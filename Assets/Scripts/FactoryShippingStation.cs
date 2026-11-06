
using UnityEngine;

public class FactoryShippingStation : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField] private int targetDeliveries = 10;

    [Header("Credit Economy")]
    [SerializeField] private FactoryCreditManager creditManager;

    private int deliveredCount;
    private int rejectedCount;
    private int defectiveDeliveredCount;
    private bool objectiveCompleted;

    public int DeliveredCount => deliveredCount;
    public int RejectedCount => rejectedCount;
    public int DefectiveDeliveredCount => defectiveDeliveredCount;
    public int TargetDeliveries => targetDeliveries;
    public bool ObjectiveCompleted => objectiveCompleted;

    private void Awake()
    {
        if (creditManager == null)
            creditManager =
                GetComponentInParent<FactoryCreditManager>();
    }

    public void ReceiveProduct(GameObject product)
    {
        if (product == null)
            return;

        ProductPackaging packaging =
            product.GetComponent<ProductPackaging>();

        ProductQuality quality =
            product.GetComponent<ProductQuality>();

        bool isPacked =
            packaging != null && packaging.IsPacked;

        bool isDefective =
            quality != null && quality.IsDefective;

        ProductMover mover =
            product.GetComponent<ProductMover>();

        if (mover != null)
            mover.UnregisterProduct();

        // 1. Defective products take priority.
        if (isDefective)
        {
            defectiveDeliveredCount++;

            if (creditManager != null)
            {
                int penalty =
                    creditManager.defectiveDeliveryPenalty;

                // Deduct penalty without making balance negative.
                int amountToDeduct = Mathf.Min(
                    penalty,
                    creditManager.currentCredits
                );

                if (amountToDeduct > 0)
                    creditManager.TrySpend(amountToDeduct);
            }

            Debug.LogWarning(
                "[Shipping] DEFECTIVE PRODUCT ARRIVED! " +
                $"Total defective: {defectiveDeliveredCount}"
            );
        }
        // 2. Accept normal packed products.
        else if (isPacked)
        {
            deliveredCount++;

            if (creditManager != null)
            {
                creditManager.AddCredits(
                    creditManager.successfulDeliveryReward
                );
            }

            Debug.Log(
                $"[Shipping] GOOD DELIVERY! " +
                $"{deliveredCount}/{targetDeliveries}. " +
                $"Reward: +20 CR"
            );

            if (!objectiveCompleted &&
                deliveredCount >= targetDeliveries)
            {
                objectiveCompleted = true;

                Debug.Log(
                    "[FactoryVerse] OBJECTIVE COMPLETE!"
                );
            }
        }
        // 3. Reject unpacked products.
        else
        {
            rejectedCount++;

            Debug.LogWarning(
                "[Shipping] UNPACKED PRODUCT REJECTED! " +
                $"Total rejected: {rejectedCount}"
            );
        }

        Destroy(product);
    }

    public void ResetShipping()
    {
        deliveredCount = 0;
        rejectedCount = 0;
        defectiveDeliveredCount = 0;
        objectiveCompleted = false;
    }
}
