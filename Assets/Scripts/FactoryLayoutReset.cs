
using UnityEngine;

public class FactoryLayoutReset : MonoBehaviour
{
    [Header("Factory References")]
    [SerializeField] private FactoryGrid grid;
    [SerializeField] private FactoryPlacementManager placementManager;
    [SerializeField] private ConveyorConnectionManager connections;
    [SerializeField] private FactoryProductionController controller;

    [Header("Shop References")]
    [SerializeField] private FactoryBuildShopUI shopUI;
    [SerializeField] private FactoryShopSpawner shopSpawner;

    [Header("Fixed Stations")]
    [SerializeField] private ConveyorModule sourceModule;
    [SerializeField] private ConveyorModule shippingModule;

    [Header("Default Layout")]
    [SerializeField] private int defaultColumns = 6;
    [SerializeField] private int defaultRows = 4;

    // Set these to the original source/shipping
    // cells in your default 6x4 factory layout.
    [SerializeField] private Vector2Int defaultSourceCell;
    [SerializeField] private Vector2Int defaultShippingCell;

    public void ResetLayout()
    {
        if (grid == null ||
            placementManager == null ||
            connections == null ||
            controller == null ||
            shopUI == null ||
            shopSpawner == null ||
            sourceModule == null ||
            shippingModule == null)
        {
            Debug.LogError(
                "[Layout Reset] Missing Inspector references."
            );
            return;
        }

        var state = controller.CurrentState;

        if (state ==
                FactoryProductionController.ProductionState.Running ||
            state ==
                FactoryProductionController.ProductionState.Stopping)
        {
            Debug.LogWarning(
                "[Layout Reset] Wait for production to finish."
            );
            return;
        }

        if (shopSpawner.HasPendingModule)
        {
            Debug.LogWarning(
                "[Layout Reset] Finish or cancel pending placement."
            );
            return;
        }

        // Validate before modifying the factory.
        if (!grid.IsCellWithinBounds(
                defaultSourceCell, defaultColumns, defaultRows) ||
            !grid.IsCellWithinBounds(
                defaultShippingCell, defaultColumns, defaultRows) ||
            defaultSourceCell == defaultShippingCell)
        {
            Debug.LogError(
                "[Layout Reset] Invalid default station cells."
            );
            return;
        }

        Debug.Log("[Layout Reset] Starting layout reset.");

        // 1. Reset products, production and delivery counters.
        controller.ResetFactory();

        // 2. Remove player-placed conveyor modules.
        ConveyorModule[] modules =
            GetComponentsInChildren<ConveyorModule>(true);

        foreach (ConveyorModule module in modules)
        {
            if (module == null)
                continue;

            // Never remove fixed stations.
            if (module == sourceModule ||
                module == shippingModule)
                continue;

            // Only remove installed conveyor modules.
            if (!module.CurrentCell.HasValue)
                continue;

            module.gameObject.SetActive(false);
            placementManager.Release(module);

            Destroy(module.gameObject);
        }

        // 3. Remove installed packing machines.
        PackingMachinePlacement[] machines =
            GetComponentsInChildren<PackingMachinePlacement>(true);

        foreach (PackingMachinePlacement machine in machines)
        {
            if (machine == null || !machine.IsPlaced)
                continue;

            machine.gameObject.SetActive(false);
            Destroy(machine.gameObject);
        }

        // 4. Clear the old cell registry.
        placementManager.ClearOccupancyForLayoutReset();

        // 5. Restore the default grid size.
        grid.SetGridSize(defaultColumns, defaultRows);

        // 6. Reposition and register fixed stations.
        bool sourcePlaced = placementManager.TrySnap(
            sourceModule,
            grid.GetCellCenterWorld(
                defaultSourceCell.x,
                defaultSourceCell.y
            )
        );

        bool shippingPlaced = placementManager.TrySnap(
            shippingModule,
            grid.GetCellCenterWorld(
                defaultShippingCell.x,
                defaultShippingCell.y
            )
        );

        if (!sourcePlaced || !shippingPlaced)
        {
            Debug.LogError(
                "[Layout Reset] Failed to restore fixed stations."
            );
            return;
        }

        // 7. Reset shop counters.
        shopUI.ResetLayoutInventory();

        // 8. Refresh the connection graph and route status.
        connections.RebuildConnections();
        controller.ValidateRoute();

        Debug.Log(
            "[Layout Reset] Complete. " +
            $"Grid restored to {defaultColumns}x{defaultRows}."
        );
    }
}
