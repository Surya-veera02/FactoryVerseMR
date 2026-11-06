
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FactoryProductionController : MonoBehaviour
{
    public enum ProductionState
    {
        Ready,
        Running,
        Stopping,
        Stopped,
        InvalidRoute,
        Completed
    }

    [Header("References")]
    [SerializeField] private FactorySourceStation source;
    [SerializeField] private FactoryShippingStation shipping;
    [SerializeField] private ConveyorConnectionManager connections;

    [Header("Build Shop")]
    [SerializeField] private FactoryShopSpawner shopSpawner;

    [Header("Status")]
    [SerializeField]
    private ProductionState currentState =
        ProductionState.Ready;

    public ProductionState CurrentState => currentState;


    private void Awake()
    {
        if (connections == null)
            connections =
                GetComponent<ConveyorConnectionManager>();

        if (shopSpawner == null)
            shopSpawner =
                GetComponentInChildren<FactoryShopSpawner>(true);
    }


    private void Start()
    {
        ValidateRoute();
    }

    [ContextMenu("Validate Route")]
    public bool ValidateRoute()
    {
        if (source == null ||
            shipping == null ||
            connections == null)
        {
            Debug.LogError(
                "[Factory] Missing production references."
            );

            currentState = ProductionState.InvalidRoute;
            return false;
        }

        ConveyorModule sourceModule =
            source.GetComponent<ConveyorModule>();

        ConveyorModule shippingModule =
            shipping.GetComponent<ConveyorModule>();

        bool valid = connections.HasValidRoute(
            sourceModule,
            shippingModule
        );

        if (!valid)
        {
            currentState = ProductionState.InvalidRoute;
            Debug.LogWarning("[Factory] Invalid production route.");
        }
        else if (currentState != ProductionState.Running &&
                 currentState != ProductionState.Completed)
        {
            currentState = ProductionState.Ready;
            Debug.Log("[Factory] Route valid. Ready to produce.");
        }

        return valid;
    }


    [ContextMenu("Start Factory")]
    public void StartFactory()
    {
        Debug.Log(
    $"[START DEBUG] State={currentState}, " +
    $"Source={(source != null ? source.name : "NULL")}, " +
    $"Shipping={(shipping != null ? shipping.name : "NULL")}, " +
    $"Pending={(shopSpawner != null && shopSpawner.HasPendingModule)}, " +
    $"Active={ProductMover.ActiveProductCount}"
);

        if (!Application.isPlaying)
            return;

        if (currentState == ProductionState.Running ||
            currentState == ProductionState.Stopping ||
            currentState == ProductionState.Completed)
            return;

        if (shopSpawner != null && shopSpawner.HasPendingModule)
        {
            Debug.LogWarning(
                "[Factory] Finish placing or cancel the pending " +
                "shop module before starting production."
            );
            return;
        }

        if (source == null || !source.HasProductsRemaining())
        {
            Debug.LogWarning(
                "[Factory] No products remaining to spawn."
            );
            return;
        }

        Debug.Log(
            $"[Factory] Restart check: " +
            $"State={currentState}, " +
            $"ActiveProducts={ProductMover.ActiveProductCount}, " +
            $"Spawned={source.ProductsSpawned}, " +
            $"Delivered={shipping.DeliveredCount}"
        );

        if (ProductMover.ActiveProductCount > 0)
        {
            Debug.LogWarning(
                "[Factory] Clear existing products before restarting."
            );
            return;
        }

        Debug.Log($"[Factory] Active products: {ProductMover.ActiveProductCount}");

        if (!ValidateRoute())
            return;

        SetConveyorsLocked(true);

        source.StartProduction();

        currentState = ProductionState.Running;

        Debug.Log("[Factory] Production started.");
    }



    [ContextMenu("Stop Factory")]
    public void StopFactory()
    {
        if (!Application.isPlaying)
            return;

        if (currentState != ProductionState.Running)
            return;

        source.StopProduction();

        currentState = ProductionState.Stopping;

        Debug.Log(
            "[Factory] Stopping production. " +
            "Waiting for active products to finish."
        );
    }



    private void Update()
    {
        if (shipping == null)
            return;

        if (currentState == ProductionState.Running &&
    !source.IsProducing)
        {
            Debug.LogWarning(
                "[Factory DEBUG] Controller is Running, " +
                "but source production is disabled. " +
                $"ShippingObjectiveCompleted={shipping.ObjectiveCompleted}"
            );
        }

        // Check whether the delivery objective is complete.
        if (shipping.ObjectiveCompleted &&
            currentState != ProductionState.Completed)
        {
            source.StopProduction();

            currentState = ProductionState.Completed;

            SetConveyorsLocked(false);

            Debug.Log(
                "[Factory] Production objective completed!"
            );

            return;
        }

        // Finish a graceful stop once all products have left.
        if (currentState == ProductionState.Stopping &&
            ProductMover.ActiveProductCount == 0)
        {
            currentState = ProductionState.Stopped;

            SetConveyorsLocked(false);

            Debug.Log(
                "[Factory] Line empty. Build mode enabled."
            );
        }
    }




    private void SetConveyorsLocked(bool locked)
    {
        // Lock or unlock conveyor belts.
        ConveyorModule[] conveyors =
            GetComponentsInChildren<ConveyorModule>();

        foreach (ConveyorModule conveyor in conveyors)
        {
            XRGrabInteractable grab =
                conveyor.GetComponent<XRGrabInteractable>();

            if (grab != null)
            {
                // Do not unlock an unfinished shop placement.
                ShopPendingPlacement pending =
                    conveyor.GetComponent<ShopPendingPlacement>();

                bool isPending =
                    pending != null && pending.IsPending;

                grab.enabled = !locked || isPending;
            }
        }

        // Lock packing machines during production.
        // Installed machines remain fixed in Level 1.
        PackingMachinePlacement[] packingMachines =
            GetComponentsInChildren<PackingMachinePlacement>();

        foreach (PackingMachinePlacement packing in packingMachines)
        {
            XRGrabInteractable grab =
                packing.GetComponent<XRGrabInteractable>();

            if (grab != null)
            {
                grab.enabled = !locked; //&& !packing.IsPlaced;
            }
        }

        Debug.Log(
            locked
                ? "[Factory] Build modules locked."
                : "[Factory] Build mode enabled."
        );
    }



    [ContextMenu("Clear Line")]
    public void ClearLine()
    {
        if (currentState == ProductionState.Running)
        {
            Debug.LogWarning(
                "[Factory] Stop production before clearing the line."
            );
            return;
        }

        ProductMover[] products =
            GetComponentsInChildren<ProductMover>(true);

        foreach (ProductMover product in products)
        {
            if (product == null)
                continue;

            product.UnregisterProduct();
            Destroy(product.gameObject);
        }

        Debug.Log("[Factory] Line cleared.");
    }

    [ContextMenu("Reset Factory")]
    public void ResetFactory()
    {

        if (currentState == ProductionState.Running ||
            currentState == ProductionState.Stopping)
        {
            Debug.LogWarning(
                "[Factory] Wait for production to stop " +
                "before resetting the factory."
            );
            return;
        }


        source.StopProduction();

        ProductMover[] products =
            GetComponentsInChildren<ProductMover>(true);

        foreach (ProductMover product in products)
        {
            if (product == null)
                continue;

            product.UnregisterProduct();
            Destroy(product.gameObject);
        }

        source.ResetProduction();
        shipping.ResetShipping();

        SetConveyorsLocked(false);

        currentState = ProductionState.Stopped;

        Debug.Log("[Factory] Factory reset. Ready for a new round.");
    }

}
