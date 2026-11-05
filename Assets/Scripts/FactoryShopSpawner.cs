
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

        // Conveyor_Shop prefab is saved inactive,
        // so its Awake/OnEnable haven't run yet.
        instance.SetActive(false);

        ConveyorModule conveyor =
            instance.GetComponent<ConveyorModule>();

        if (conveyor != null)
        {
            conveyor.SetAutoSnapOnStart(false);
        }

        instance.SetActive(true);

        pendingModule = instance;

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
            return;

        pendingModule = null;

        Debug.Log("[Shop] Module placement confirmed.");
    }
}
