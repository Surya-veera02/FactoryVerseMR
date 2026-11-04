
using UnityEngine;

public class FactoryShippingStation : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField] private int targetDeliveries = 10;

    private int deliveredCount;
    private bool objectiveCompleted;

    public int DeliveredCount => deliveredCount;
    public bool ObjectiveCompleted => objectiveCompleted;


    public void ReceiveProduct(GameObject product)
    {
        if (product == null)
            return;

        ProductMover mover = product.GetComponent<ProductMover>();

        if (mover != null)
            mover.UnregisterProduct();

        deliveredCount++;

        Debug.Log(
            $"[Shipping] Delivered: " +
            $"{deliveredCount}/{targetDeliveries}"
        );

        Destroy(product);

        if (!objectiveCompleted &&
            deliveredCount >= targetDeliveries)
        {
            objectiveCompleted = true;

            Debug.Log(
                "[FactoryVerse] OBJECTIVE COMPLETE! " +
                "All products delivered."
            );
        }
    }

}
