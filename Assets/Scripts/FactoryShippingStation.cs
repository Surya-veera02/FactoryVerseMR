
using UnityEngine;

public class FactoryShippingStation : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField] private int targetDeliveries = 10;

    [Header("Credit Economy")]
    [SerializeField] private FactoryCreditManager creditManager;

    private readonly System.Collections.Generic.HashSet<int>
    receivedProductIds = new();

    private int deliveredCount;
    private int rejectedCount;
    private int defectiveDeliveredCount;
    private bool objectiveCompleted;

    public int DeliveredCount => deliveredCount;
    public int RejectedCount => rejectedCount;
    public int DefectiveDeliveredCount => defectiveDeliveredCount;

    // Total products reaching Shipping, regardless of outcome.
    public int TotalReachedShipping =>
        deliveredCount + defectiveDeliveredCount + rejectedCount;

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

        int productId = product.GetInstanceID();

        if (!receivedProductIds.Add(productId))
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

        // 1. Unpacked products take priority.
        if (!isPacked)
        {
            rejectedCount++;

            PlayWrongPackageSound();

            Debug.LogWarning(
                "[Shipping] UNPACKED PRODUCT REJECTED! " +
                $"Total unpacked: {rejectedCount}"
            );
        }
        // 2. Defective packed products.
        else if (isDefective)
        {
            defectiveDeliveredCount++;

            PlayWrongPackageSound();

            if (creditManager != null)
            {
                int penalty =
                    creditManager.defectiveDeliveryPenalty;

                int amountToDeduct = Mathf.Min(
                    penalty,
                    creditManager.currentCredits
                );

                if (amountToDeduct > 0)
                {
                    ShowCreditFeedback(-amountToDeduct);
                    creditManager.TrySpend(amountToDeduct);
                }
            }

            Debug.LogWarning(
                "[Shipping] DEFECTIVE BOX ARRIVED! " +
                $"Total defective: {defectiveDeliveredCount}"
            );
        }
        // 3. Successfully packed products.
        else
        {
            deliveredCount++;

            PlayCorrectPackageSound();

            if (creditManager != null)
            {
                int reward =
                    creditManager.successfulDeliveryReward;

                creditManager.AddCredits(reward);
                ShowCreditFeedback(reward);
            }

            Debug.Log(
                "[Shipping] GOOD DELIVERY! " +
                $"{deliveredCount}/{targetDeliveries}."
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

        Destroy(product);
    }

    public void ResetShipping()
    {
        deliveredCount = 0;
        rejectedCount = 0;
        defectiveDeliveredCount = 0;
        objectiveCompleted = false;
        receivedProductIds.Clear();
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
