
using System.Collections.Generic;
using UnityEngine;

public class FactoryPlacementManager : MonoBehaviour
{
    [Header("Grid Reference")]
    [SerializeField] private FactoryGrid grid;

    private ConveyorConnectionManager connectionManager;

    private readonly Dictionary<Vector2Int, ConveyorModule> occupiedCells
        = new Dictionary<Vector2Int, ConveyorModule>();

    private void Awake()
    {
        if (grid == null)
            grid = GetComponent<FactoryGrid>();

        connectionManager =
        GetComponent<ConveyorConnectionManager>();
    }

    public bool TrySnap(ConveyorModule module, Vector3 worldPosition)
    {
        if (grid == null || module == null)
            return false;

        // Find the cell beneath the conveyor.
        if (!grid.TryGetCell(
            worldPosition,
            out int column,
            out int row))
        {
            return false;
        }

        Vector2Int targetCell = new Vector2Int(column, row);

        // Reject placement if another conveyor occupies the cell.
        if (occupiedCells.TryGetValue(
            targetCell, out ConveyorModule existing))
        {
            if (existing != null && existing != module)
                return false;
        }

        // Release the module's previous cell.
        if (module.CurrentCell.HasValue)
        {
            Vector2Int previousCell = module.CurrentCell.Value;

            if (occupiedCells.TryGetValue(
                previousCell, out ConveyorModule previous)
                && previous == module)
            {
                occupiedCells.Remove(previousCell);
            }
        }

        // Reserve the new cell.
        occupiedCells[targetCell] = module;
        module.SetCell(targetCell);

        AlignModule(module);

        if (connectionManager != null)
            connectionManager.RebuildConnections();

        Debug.Log(
            $"{module.name} snapped to ({column}, {row})"
        );

        return true;
    }


    // Check whether a conveyor can occupy a grid cell
    // without changing the existing placement.
    public bool CanPlaceConveyor(
        ConveyorModule module,
        Vector3 worldPosition)
    {
        if (grid == null || module == null)
            return false;

        if (!grid.TryGetCell(
            worldPosition,
            out int column,
            out int row))
        {
            return false;
        }

        Vector2Int targetCell =
            new Vector2Int(column, row);

        // Check the occupancy dictionary directly.
        if (occupiedCells.TryGetValue(
            targetCell,
            out ConveyorModule existing))
        {
            if (existing != null && existing != module)
                return false;
        }

        return true;
    }


    public void AlignModule(ConveyorModule module)
    {
        if (grid == null || !module.CurrentCell.HasValue)
            return;

        Vector2Int cell = module.CurrentCell.Value;

        Vector3 position = grid.GetCellCenterWorld(
            cell.x,
            cell.y
        );

        Quaternion rotation =
            grid.transform.rotation *
            Quaternion.Euler(
                0f,
                module.QuarterTurns * 90f,
                0f
            );

        module.transform.SetPositionAndRotation(
            position,
            rotation
        );
    }

    public void Release(ConveyorModule module)
    {
        if (!module.CurrentCell.HasValue)
            return;

        Vector2Int cell = module.CurrentCell.Value;

        if (occupiedCells.TryGetValue(
            cell, out ConveyorModule existing)
            && existing == module)
        {
            occupiedCells.Remove(cell);
        }
        if (connectionManager != null)
            connectionManager.RebuildConnections();
    }

    public ConveyorModule GetModuleAtCell(Vector2Int cell)
    {
        if (occupiedCells.TryGetValue(
            cell,
            out ConveyorModule module))
        {
            if (module != null && module.isActiveAndEnabled)
                return module;
        }

        return null;
    }

}
