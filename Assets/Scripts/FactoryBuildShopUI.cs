
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactoryBuildShopUI : MonoBehaviour
{
    public enum BuildModuleType
    {
        None,
        Conveyor,
        PackingMachine
    }

    [Header("Factory")]
    [SerializeField] private FactoryProductionController controller;
    [SerializeField] private Transform factoryBoard;

    [Header("Level 1 Inventory Limits")]
    [SerializeField] private int maxConveyors = 5;
    [SerializeField] private int maxPackingMachines = 1;

    [Header("UI")]
    [SerializeField] private TMP_Text conveyorCountText;
    [SerializeField] private TMP_Text packingCountText;
    [SerializeField] private TMP_Text selectionText;

    [SerializeField] private Button conveyorSelectButton;
    [SerializeField] private Button packingSelectButton;

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
    }

    private void Update()
    {
        if (controller == null || factoryBoard == null)
            return;

        int placedConveyors =
            factoryBoard.GetComponentsInChildren
                <ConveyorModule>(true).Length;

        bool canBuild = CanBuild();

        if (conveyorCountText != null)
        {
            conveyorCountText.text =
                $"CONVEYORS: {placedConveyors}/{maxConveyors}";
        }

        if (packingCountText != null)
        {
            packingCountText.text =
                $"PACKING MACHINES: " +
                $"{placedPackingMachines}/{maxPackingMachines}";
        }

        if (conveyorSelectButton != null)
        {
            conveyorSelectButton.interactable =
                canBuild && placedConveyors < maxConveyors;
        }

        if (packingSelectButton != null)
        {
            packingSelectButton.interactable =
                canBuild &&
                placedPackingMachines < maxPackingMachines;
        }

        if (!canBuild &&
            SelectedModule != BuildModuleType.None)
        {
            SelectedModule = BuildModuleType.None;
            SetSelectionText("BUILD MODE UNAVAILABLE");
        }
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
        if (!CanBuild())
            return;

        SelectedModule = BuildModuleType.Conveyor;

        SetSelectionText("SELECTED: CONVEYOR");

        Debug.Log("[Build Shop] Conveyor selected.");
    }

    private void SelectPackingMachine()
    {
        if (!CanBuild())
            return;

        SelectedModule = BuildModuleType.PackingMachine;

        SetSelectionText("SELECTED: PACKING MACHINE");

        Debug.Log("[Build Shop] Packing machine selected.");
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

    private void OnDestroy()
    {
        if (conveyorSelectButton != null)
            conveyorSelectButton.onClick.RemoveListener(
                SelectConveyor);

        if (packingSelectButton != null)
            packingSelectButton.onClick.RemoveListener(
                SelectPackingMachine);
    }
}
