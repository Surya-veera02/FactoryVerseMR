
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

    [Header("Credit Feedback")]
    [SerializeField]
    private FloatingCreditFeedback feedbackPrefab;

    [SerializeField]
    private Transform feedbackSpawnPoint;

    [Header("Shipping Audio")]
    [SerializeField] private AudioSource shippingAudio;
    [SerializeField] private AudioClip correctPackageSound;
    [SerializeField] private AudioClip wrongPackageSound;

    private void Awake()
    {
        if (creditManager == null)
            creditManager =
                GetComponentInParent<FactoryCreditManager>();
    }

    private void ShowCreditFeedback(int amount)
    {
        if (feedbackPrefab == null ||
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

            PlayWrongPackageSound();

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
                {
                    ShowCreditFeedback(-amountToDeduct);
                }

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

            PlayCorrectPackageSound();

            if (creditManager != null)
            {
                creditManager.AddCredits(
                    creditManager.successfulDeliveryReward
                );
                ShowCreditFeedback(
    creditManager.successfulDeliveryReward);
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

            PlayWrongPackageSound();

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
    private void PlayCorrectPackageSound()
    {
        if (shippingAudio != null && correctPackageSound != null)
            shippingAudio.PlayOneShot(correctPackageSound);
    }

    private void PlayWrongPackageSound()
    {
        if (shippingAudio != null && wrongPackageSound != null)
            shippingAudio.PlayOneShot(wrongPackageSound);
    }
}
