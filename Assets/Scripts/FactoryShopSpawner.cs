
using UnityEngine;

public class FactoryShopSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject conveyorPrefab;
    [SerializeField] private GameObject packingMachinePrefab;

    [Header("Placement")]
    [SerializeField] private Transform factoryBoard;
    [SerializeField] private Transform pickupPoint;

    [Header("Debug")]
    [SerializeField] private bool logSpawning = true;

    private GameObject pendingModule;

    public bool HasPendingModule => pendingModule != null;

    public bool TrySpawnConveyor()
    {
        return TrySpawn(conveyorPrefab, "Conveyor");
    }

    public bool TrySpawnPackingMachine()
    {
        return TrySpawn(packingMachinePrefab, "Packing Machine");
    }

    private bool TrySpawn(
        GameObject prefab,
        string moduleName)
    {
        if (prefab == null ||
            factoryBoard == null ||
            pickupPoint == null)
        {
            Debug.LogError(
                "[Shop] Missing prefab, board, or pickup point."
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

        // Instantiate inactive so registration and XR components
        // cannot run before we configure the module.
        bool prefabWasActive = prefab.activeSelf;

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
        {
            conveyor.SetAutoSnapOnStart(false);
        }

        // Add and initialize the placement component.
        ShopPendingPlacement pendingPlacement =
            instance.GetComponent<ShopPendingPlacement>();

        if (pendingPlacement == null)
        {
            pendingPlacement =
                instance.AddComponent<ShopPendingPlacement>();
        }

        pendingPlacement.Initialize(this);

        // Register pending instance before activation.
        pendingModule = instance;

        instance.SetActive(true);

        if (logSpawning)
        {
            Debug.Log(
                $"[Shop] Spawned pending {moduleName}: " +
                $"{instance.name}"
            );
        }

        return true;
    }

    public void ClearPendingModule()
    {
        if (pendingModule == null)
            return;

        Destroy(pendingModule);
        pendingModule = null;

        Debug.Log("[Shop] Pending module cancelled.");
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
