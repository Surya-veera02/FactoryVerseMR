
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class FactoryShippingTrigger : MonoBehaviour
{
    [SerializeField]
    private FactoryShippingStation shippingStation;

    private void Awake()
    {
        if (shippingStation == null)
        {
            shippingStation =
                GetComponentInParent<FactoryShippingStation>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (shippingStation == null)
            return;

        ProductMover mover =
            other.GetComponentInParent<ProductMover>();

        if (mover == null)
            return;

        // ReceiveProduct handles credits, counters,
        // audio, unregistering and destruction.
        shippingStation.ReceiveProduct(mover.gameObject);
    }
}
