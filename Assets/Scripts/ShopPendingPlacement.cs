
using UnityEngine;

public class ShopPendingPlacement : MonoBehaviour
{
    private FactoryShopSpawner shopSpawner;
    private bool placementConfirmed;

    public bool IsPending => !placementConfirmed;

    public void Initialize(FactoryShopSpawner spawner)
    {
        shopSpawner = spawner;
    }

    public void ConfirmPlacement()
    {
        if (placementConfirmed || shopSpawner == null)
            return;

        placementConfirmed = true;

        shopSpawner.ConfirmPendingModule(gameObject);

        Debug.Log(
            $"[Shop Placement] {name} placed successfully."
        );
    }
}
