
using UnityEngine;

public class FactoryShopSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject conveyorPrefab;
    [SerializeField] private GameObject packingMachinePrefab;

    [Header("Corner Conveyor Prefabs")]
    [SerializeField] private GameObject cornerRightPrefab;
    [SerializeField] private GameObject cornerLeftPrefab;

    [Header("Placement")]
    [SerializeField] private Transform factoryBoard;
    [SerializeField] private Transform pickupPoint;


    [Header("Credit Economy")]
    [SerializeField] private FactoryCreditManager creditManager;


    [Header("Debug")]
    [SerializeField] private bool logSpawning = true;

    private GameObject pendingModule;

    public bool HasPendingModule => pendingModule != null;

    public bool TrySpawnConveyor()
    {
        return TrySpawn(
            conveyorPrefab,
            "Conveyor",
            creditManager != null
                ? creditManager.straightConveyorCost : 0
        );
    }

    public bool TrySpawnPackingMachine()
    {
        return TrySpawn(
            packingMachinePrefab,
            "Packing Machine",
            creditManager != null
                ? creditManager.packingMachineCost : 0
        );
    }

    public bool TrySpawnCornerRight()
    {
        return TrySpawn(
            cornerRightPrefab,
            "Right Corner Conveyor",
            creditManager != null
                ? creditManager.cornerConveyorCost : 0
        );
    }

    public bool TrySpawnCornerLeft()
    {
        return TrySpawn(
            cornerLeftPrefab,
            "Left Corner Conveyor",
            creditManager != null
                ? creditManager.cornerConveyorCost : 0
        );
    }



    private bool TrySpawn(
        GameObject prefab,
        string moduleName,
        int price)
    {
        if (prefab == null ||
            factoryBoard == null ||
            pickupPoint == null ||
            creditManager == null)
        {
            Debug.LogError(
                "[Shop] Missing prefab, board, pickup point " +
                "or credit manager."
            );
            return false;
        }

        if (pendingModule != null)
        {
            Debug.LogWarning(
                "[Shop] Finish placing the current module first."
            );
            return false;
        }

        if (!creditManager.CanAfford(price))
        {
            Debug.LogWarning(
                $"[Shop] Not enough credits for {moduleName}. " +
                $"Required: {price}, " +
                $"Available: {creditManager.currentCredits}"
            );
            return false;
        }

        GameObject instance = Instantiate(
            prefab,
            pickupPoint.position,
            pickupPoint.rotation,
            factoryBoard
        );

        instance.SetActive(false);

        ConveyorModule conveyor =
            instance.GetComponent<ConveyorModule>();

        if (conveyor != null)
            conveyor.SetAutoSnapOnStart(false);

        FactoryPurchaseInfo purchase =
            instance.GetComponent<FactoryPurchaseInfo>();

        if (purchase == null)
            purchase = instance.AddComponent<FactoryPurchaseInfo>();

        purchase.Initialize(price);

        ShopPendingPlacement pendingPlacement =
            instance.GetComponent<ShopPendingPlacement>();

        if (pendingPlacement == null)
            pendingPlacement =
                instance.AddComponent<ShopPendingPlacement>();

        pendingPlacement.Initialize(this);

        // Charge only after the instance has been created.
        if (!creditManager.TrySpend(price))
        {
            Destroy(instance);
            return false;
        }

        pendingModule = instance;
        instance.SetActive(true);

        if (logSpawning)
        {
            Debug.Log(
                $"[Shop] Purchased {moduleName} " +
                $"for {price} credits. " +
                $"Balance: {creditManager.currentCredits}"
            );
        }

        return true;
    }

    public void ClearPendingModule()
    {
        if (pendingModule == null)
            return;

        FactoryPurchaseInfo purchase =
            pendingModule.GetComponent<FactoryPurchaseInfo>();

        if (purchase != null)
            purchase.TryRefund(creditManager);

        Destroy(pendingModule);
        pendingModule = null;

        Debug.Log(
            "[Shop] Pending module cancelled and refunded."
        );
    }


    public void ConfirmPendingModule(GameObject instance)
    {
        if (instance != pendingModule)
        {
            Debug.LogWarning(
                "[Shop] Confirmation rejected: " +
                "instance is not the pending module."
            );
            return;
        }

        PackingMachinePlacement packing =
            instance.GetComponent<PackingMachinePlacement>();

        if (packing != null)
        {
            FactoryBuildShopUI shopUI =
                FindFirstObjectByType<FactoryBuildShopUI>();

            if (shopUI != null)
            {
                shopUI.RegisterPackingMachinePlacement();
            }
            else
            {
                Debug.LogError(
                    "[Shop] FactoryBuildShopUI not found!"
                );
            }
        }

        pendingModule = null;

        Debug.Log("[Shop] Module placement confirmed.");
    }

}
