
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
        if (!Application.isPlaying)
            return;

        if (currentState == ProductionState.Running ||
            currentState == ProductionState.Stopping ||
            currentState == ProductionState.Completed)
            return;

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
        ConveyorModule[] conveyors =
            GetComponentsInChildren<ConveyorModule>();

        foreach (ConveyorModule conveyor in conveyors)
        {
            XRGrabInteractable grab =
                conveyor.GetComponent<XRGrabInteractable>();

            if (grab != null)
            {
                grab.enabled = !locked;
            }
        }

        Debug.Log(
            locked
                ? "[Factory] Conveyors locked."
                : "[Factory] Conveyors unlocked."
        );
    }

}
