
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactoryBuildShopUI : MonoBehaviour
{
    public enum BuildModuleType
    {
        None,
        Conveyor,
        CornerRight,
        CornerLeft,
        PackingMachine
    }

    [Header("Factory")]
    [SerializeField] private FactoryProductionController controller;
    [SerializeField] private Transform factoryBoard;

    [Header("Level 1 Inventory Limits")]
    [SerializeField] private int maxConveyors = 5;
    [SerializeField] private int maxPackingMachines = 1;

    [Header("Shop Spawning")]
    [SerializeField] private FactoryShopSpawner shopSpawner;

    [Header("Credit Economy")]
    [SerializeField] private FactoryCreditManager creditManager;
    [SerializeField] private TMP_Text creditBalanceText;


    [Header("UI")]
    [SerializeField] private TMP_Text conveyorCountText;
    [SerializeField] private TMP_Text packingCountText;
    [SerializeField] private TMP_Text selectionText;

    [SerializeField] private Button conveyorSelectButton;
    [SerializeField] private Button packingSelectButton;

    [Header("Corner Conveyor Buttons")]
    [SerializeField] private Button cornerRightSelectButton;
    [SerializeField] private Button cornerLeftSelectButton;


    [Header("Cancel Purchase")]
    [SerializeField] private Button cancelPurchaseButton;


    public BuildModuleType SelectedModule { get; private set; }
        = BuildModuleType.None;

    // Updated by the future placement system.
    private int placedPackingMachines = 0;

    private void Awake()
    {
        if (conveyorSelectButton != null)
            conveyorSelectButton.onClick.AddListener(
                SelectConveyor);

        if (packingSelectButton != null)
            packingSelectButton.onClick.AddListener(
                SelectPackingMachine);

        if (cornerRightSelectButton != null)
            cornerRightSelectButton.onClick.AddListener(SelectCornerRight);

        if (cornerLeftSelectButton != null)
            cornerLeftSelectButton.onClick.AddListener(SelectCornerLeft);

        if (cancelPurchaseButton != null)
            cancelPurchaseButton.onClick.AddListener(CancelPurchase);

    }


    private void Update()
    {
        if (controller == null || factoryBoard == null)
            return;

        // Display the available factory credits.
        if (creditBalanceText != null && creditManager != null)
        {
            creditBalanceText.text =
                $"CREDITS: {creditManager.currentCredits}";
        }


        int placedConveyors = 0;

        ConveyorModule[] conveyors =
            factoryBoard.GetComponentsInChildren<ConveyorModule>();


        foreach (ConveyorModule conveyor in conveyors)
        {
            if (!conveyor.CurrentCell.HasValue)
                continue;

            // Fixed stations do not consume shop inventory.
            if (conveyor.GetComponent<FactorySourceStation>() != null ||
                conveyor.GetComponent<FactoryShippingStation>() != null)
                continue;

            placedConveyors++;
        }


        bool canBuild = CanBuild();

        // Update conveyor inventory text.
        if (conveyorCountText != null)
        {
            conveyorCountText.text =
                $"CONVEYORS: {placedConveyors}/{maxConveyors}";
        }

        // Update packing machine inventory text.
        if (packingCountText != null)
        {
            packingCountText.text =
                $"PACKING MACHINES: " +
                $"{placedPackingMachines}/{maxPackingMachines}";
        }


        bool canAddConveyor =
            canBuild &&
            placedConveyors < maxConveyors &&
            shopSpawner != null &&
            !shopSpawner.HasPendingModule;


        // Straight conveyor button.
        if (conveyorSelectButton != null)
        {
            conveyorSelectButton.interactable =
    canAddConveyor &&
    creditManager != null &&
    creditManager.CanAfford(
        creditManager.straightConveyorCost);
        }

        // Right corner conveyor button.
        if (cornerRightSelectButton != null)
        {
            cornerRightSelectButton.interactable =
    canAddConveyor &&
    creditManager != null &&
    creditManager.CanAfford(
        creditManager.cornerConveyorCost);
        }

        // Left corner conveyor button.
        if (cornerLeftSelectButton != null)
        {
            cornerLeftSelectButton.interactable =
    canAddConveyor &&
    creditManager != null &&
    creditManager.CanAfford(
        creditManager.cornerConveyorCost);
        }

        // Packing machine button.
        if (packingSelectButton != null)
        {

            packingSelectButton.interactable =
                canBuild &&
                shopSpawner != null &&
                !shopSpawner.HasPendingModule &&
                placedPackingMachines < maxPackingMachines &&
                creditManager != null &&
                creditManager.CanAfford(
                    creditManager.packingMachineCost);

        }

        // Disable build selection during production.
        if (!canBuild &&
            SelectedModule != BuildModuleType.None)
        {
            SelectedModule = BuildModuleType.None;
            SetSelectionText("BUILD MODE UNAVAILABLE");
        }


        if (cancelPurchaseButton != null)
            cancelPurchaseButton.interactable =
                shopSpawner != null &&
                shopSpawner.HasPendingModule;

    }


    private void CancelPurchase()
    {
        if (shopSpawner == null ||
            !shopSpawner.HasPendingModule)
            return;

        shopSpawner.ClearPendingModule();

        SelectedModule = BuildModuleType.None;
        SetSelectionText("PURCHASE CANCELLED");
    }


    public void ResetLayoutInventory()
    {
        placedPackingMachines = 0;
        SelectedModule = BuildModuleType.None;

        SetSelectionText("FACTORY LAYOUT RESET");

        Debug.Log("[Build Shop] Layout inventory reset.");
    }


    private bool CanBuild()
    {
        var state = controller.CurrentState;

        return state ==
            FactoryProductionController.ProductionState.Ready
            || state ==
            FactoryProductionController.ProductionState.Stopped
            || state ==
            FactoryProductionController.ProductionState.InvalidRoute;
    }


    private void SelectConveyor()
    {
        if (!CanBuild() || shopSpawner == null)
            return;

        if (!shopSpawner.TrySpawnConveyor())
        {
            SetSelectionText("FINISH CURRENT PLACEMENT");
            return;
        }

        SelectedModule = BuildModuleType.Conveyor;
        SetSelectionText("PICK UP CONVEYOR");

        Debug.Log("[Build Shop] Conveyor spawned.");
    }



    private void SelectPackingMachine()
    {
        if (!CanBuild() || shopSpawner == null)
            return;

        if (!shopSpawner.TrySpawnPackingMachine())
        {
            SetSelectionText("FINISH CURRENT PLACEMENT");
            return;
        }

        SelectedModule = BuildModuleType.PackingMachine;
        SetSelectionText("PICK UP PACKING MACHINE");

        Debug.Log("[Build Shop] Packing machine spawned.");
    }


    private void SetSelectionText(string message)
    {
        if (selectionText != null)
            selectionText.text = message;
    }

    // Called once a packing machine has been successfully placed.
    public void RegisterPackingMachinePlacement()
    {
        placedPackingMachines =
            Mathf.Min(maxPackingMachines,
                      placedPackingMachines + 1);

        SelectedModule = BuildModuleType.None;
        SetSelectionText("PACKING MACHINE PLACED");
    }


    private void SelectCornerRight()
    {
        if (!CanBuild() || shopSpawner == null)
            return;

        if (!shopSpawner.TrySpawnCornerRight())
        {
            SetSelectionText("FINISH CURRENT PLACEMENT");
            return;
        }

        SelectedModule = BuildModuleType.CornerRight;
        SetSelectionText("PICK UP RIGHT CORNER");
    }

    private void SelectCornerLeft()
    {
        if (!CanBuild() || shopSpawner == null)
            return;

        if (!shopSpawner.TrySpawnCornerLeft())
        {
            SetSelectionText("FINISH CURRENT PLACEMENT");
            return;
        }

        SelectedModule = BuildModuleType.CornerLeft;
        SetSelectionText("PICK UP LEFT CORNER");
    }


    private void OnDestroy()
    {
        if (conveyorSelectButton != null)
            conveyorSelectButton.onClick.RemoveListener(
                SelectConveyor);

        if (packingSelectButton != null)
            packingSelectButton.onClick.RemoveListener(
                SelectPackingMachine);

        if (cornerRightSelectButton != null)
            cornerRightSelectButton.onClick.RemoveListener(SelectCornerRight);

        if (cornerLeftSelectButton != null)
            cornerLeftSelectButton.onClick.RemoveListener(SelectCornerLeft);

        if (cancelPurchaseButton != null)
            cancelPurchaseButton.onClick.RemoveListener(CancelPurchase);

    }
}
