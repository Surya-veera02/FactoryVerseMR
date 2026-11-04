
using UnityEngine;

public class ProductMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 0.12f;
    [SerializeField] private float heightAboveBoard = 0.045f;

    private FactoryGrid grid;
    private ConveyorConnectionManager connectionManager;
    private ConveyorModule currentModule;

    private ConveyorModule targetModule;

    private bool isMoving;

    public static int ActiveProductCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(
    RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveProductCount()
    {
        ActiveProductCount = 0;
    }

    private bool registeredAsActive;

    public void Initialize(
        FactoryGrid factoryGrid,
        ConveyorConnectionManager connections,
        ConveyorModule startingModule)
    {
        grid = factoryGrid;
        connectionManager = connections;
        currentModule = startingModule;

        if (grid == null ||
            connectionManager == null ||
            currentModule == null ||
            !currentModule.CurrentCell.HasValue)
        {
            Debug.LogError("ProductMover: Invalid setup.");
            enabled = false;
            return;
        }

        if (!registeredAsActive)
        {
            ActiveProductCount++;
            registeredAsActive = true;
        }

        Vector2Int cell = currentModule.CurrentCell.Value;

        transform.position = grid.GetCellCenterWorld(
            cell.x,
            cell.y,
            heightAboveBoard
        );

        // Parent to the board so it follows board movement.
        transform.SetParent(grid.transform, true);

        FindNextModule();
    }

    private void Update()
    {
        if (!isMoving ||
            targetModule == null ||
            !targetModule.CurrentCell.HasValue)
            return;

        Vector2Int targetCell = targetModule.CurrentCell.Value;

        Vector3 targetPosition = grid.GetCellCenterWorld(
            targetCell.x,
            targetCell.y,
            heightAboveBoard
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * grid.transform.lossyScale.x * Time.deltaTime
        );

        if (Vector3.Distance(
            transform.position,
            targetPosition) < 0.001f)
        {
            currentModule = targetModule;
            FindNextModule();
        }
    }


    private void FindNextModule()
    {
        targetModule =
            connectionManager.GetNextModule(currentModule);

        if (targetModule == null)
        {
            isMoving = false;

            // Check whether this conveyor is a shipping station.
            FactoryShippingStation shipping =
                currentModule.GetComponent<FactoryShippingStation>();

            if (shipping != null)
            {
                shipping.ReceiveProduct(gameObject);
            }
            else
            {
                Debug.Log(
                    $"Product stopped at {currentModule.name}"
                );
            }
        }
        else
        {
            isMoving = true;

            Debug.Log(
                $"Product moving: {currentModule.name} " +
                $"-> {targetModule.name}"
            );
        }
    }

    public void UnregisterProduct()
    {
        if (!registeredAsActive)
            return;

        registeredAsActive = false;

        ActiveProductCount = Mathf.Max(
            0,
            ActiveProductCount - 1
        );

        Debug.Log(
            $"[Product] Unregistered {name}. " +
            $"Active products: {ActiveProductCount}"
        );
    }

    private void OnDestroy()
    {
        UnregisterProduct();
    }


}
