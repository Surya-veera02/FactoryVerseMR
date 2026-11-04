
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FactoryProductionController : MonoBehaviour
{
    public enum ProductionState
    {
        Ready,
        Running,
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

        if (currentState == ProductionState.Completed)
        {
            Debug.Log("[Factory] Objective already completed.");
            return;
        }

        if (!source.HasProductsRemaining())
        {
            Debug.Log("[Factory] No products remaining.");
            return;
        }

        if (!ValidateRoute())
            return;

        source.StartProduction();

        SetConveyorsLocked(true);

        currentState = ProductionState.Running;

        Debug.Log("[Factory] Production started.");
    }

    [ContextMenu("Stop Factory")]
    public void StopFactory()
    {
        if (!Application.isPlaying)
            return;

        source.StopProduction();

        SetConveyorsLocked(false);

        currentState = ProductionState.Stopped;

        Debug.Log("[Factory] Production stopped.");
    }

    private void Update()
    {
        if (shipping == null)
            return;

        if (shipping.ObjectiveCompleted &&
            currentState != ProductionState.Completed)
        {
            source.StopProduction();

            currentState = ProductionState.Completed;

            Debug.Log(
                "[Factory] Production objective completed!"
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
