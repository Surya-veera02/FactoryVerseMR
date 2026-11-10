
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FactoryGridSizeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FactoryGrid grid;
    [SerializeField] private FactoryProductionController production;
    [SerializeField] private FactoryShopSpawner shopSpawner;
    [SerializeField] private FactoryPlacementManager placementManager;
    [SerializeField] private ConveyorConnectionManager connections;
    [SerializeField] private Transform boardSurface;

    [Header("UI")]
    [SerializeField] private TMP_Text gridSizeText;

    [Header("Limits")]
    [SerializeField] private int minSize = 4;
    [SerializeField] private int maxSize = 10;

    private Vector3 initialSurfaceScale;
    private Vector3 initialMeshSize;

    private void Awake()
    {
        if (grid == null)
            grid = GetComponent<FactoryGrid>();

        if (production == null)
            production = GetComponent<FactoryProductionController>();

        if (placementManager == null)
            placementManager = GetComponent<FactoryPlacementManager>();

        if (connections == null)
            connections = GetComponent<ConveyorConnectionManager>();

        if (shopSpawner == null)
            shopSpawner = GetComponentInChildren<FactoryShopSpawner>(true);

        if (boardSurface != null)
        {
            initialSurfaceScale = boardSurface.localScale;

            MeshFilter mesh = boardSurface.GetComponent<MeshFilter>();

            if (mesh != null && mesh.sharedMesh != null)
                initialMeshSize = mesh.sharedMesh.bounds.size;
        }
    }

    private void Start()
    {
        ResizeSurface();
        UpdateText();
    }

    public void IncreaseColumns() => TryResize(grid.Columns + 1, grid.Rows);
    public void DecreaseColumns() => TryResize(grid.Columns - 1, grid.Rows);
    public void IncreaseRows() => TryResize(grid.Columns, grid.Rows + 1);
    public void DecreaseRows() => TryResize(grid.Columns, grid.Rows - 1);

    [Header("Perimeter Stations")]
    [SerializeField]
    private FactoryPerimeterAnchor shopAnchor;
    [SerializeField]
    private FactoryPerimeterAnchor returnBinAnchor;
    [SerializeField]
    private FactoryPerimeterAnchor boardGrabHandleAnchor;

    private void TryResize(int newColumns, int newRows)
    {
        if (grid == null || production == null ||
            placementManager == null || connections == null)
            return;

        if (newColumns < minSize || newColumns > maxSize ||
            newRows < minSize || newRows > maxSize)
            return;

        var state = production.CurrentState;

        bool canResize =
            state == FactoryProductionController.ProductionState.Ready ||
            state == FactoryProductionController.ProductionState.Stopped ||
            state == FactoryProductionController.ProductionState.InvalidRoute;

        if (!canResize ||
            ProductMover.ActiveProductCount > 0 ||
            (shopSpawner != null && shopSpawner.HasPendingModule))
        {
            Debug.LogWarning(
                "[Grid Resize] Finish production and pending placements first."
            );
            return;
        }

        ConveyorModule[] conveyors =
            GetComponentsInChildren<ConveyorModule>();

        // Validate all occupied cells before changing anything.
        foreach (ConveyorModule conveyor in conveyors)
        {
            if (!conveyor.CurrentCell.HasValue)
                continue;

            if (!grid.IsCellWithinBounds(
                conveyor.CurrentCell.Value, newColumns, newRows))
            {
                Debug.LogWarning(
                    $"[Grid Resize] Cannot shrink. " +
                    $"{conveyor.name} would be outside the grid."
                );
                return;
            }
        }

        // Remember which conveyor each installed machine occupies.
        Dictionary<PackingMachinePlacement, ConveyorModule>
            packingConnections = new();

        foreach (PackingMachinePlacement packing in
            GetComponentsInChildren<PackingMachinePlacement>())
        {
            if (!packing.IsPlaced)
                continue;

            if (grid.TryGetCell(packing.transform.position,
                out int col, out int row))
            {
                ConveyorModule conveyor =
                    placementManager.GetModuleAtCell(
                        new Vector2Int(col, row));

                if (conveyor != null)
                    packingConnections[packing] = conveyor;
            }
        }

        // Resize the grid.
        grid.SetGridSize(newColumns, newRows);

        // Realign conveyors to their existing cell coordinates.
        foreach (ConveyorModule conveyor in conveyors)
        {
            if (conveyor.CurrentCell.HasValue)
                placementManager.AlignModule(conveyor);
        }

        // Realign installed packing machines.
        foreach (var entry in packingConnections)
        {
            if (entry.Key != null && entry.Value != null)
                entry.Key.RealignToConveyor(entry.Value);
        }

        ResizeSurface();

        // Find all perimeter anchors without repositioning them.
        FactoryPerimeterAnchor[] anchors =
            GetComponentsInChildren<FactoryPerimeterAnchor>(true);

        foreach (FactoryPerimeterAnchor anchor in anchors)
        {
            Debug.Log(
                $"[Anchor Check] Object: {anchor.name}, " +
                $"Parent: {anchor.transform.parent?.name}"
            );

            // Reposition perimeter objects after grid resizing.

            if (shopAnchor != null)
                shopAnchor.UpdateAnchorPosition();

            if (returnBinAnchor != null)
                returnBinAnchor.UpdateAnchorPosition();

            if (boardGrabHandleAnchor != null)
                boardGrabHandleAnchor.UpdateAnchorPosition();

            // Temporarily disabled for testing.
            // anchor.UpdateAnchorPosition();
        }

        connections.RebuildConnections();
        production.ValidateRoute();
        UpdateText();

        Debug.Log(
            $"[Grid Resize] Success: {newColumns}x{newRows}"
        );
    }

    public void RefreshAfterLayoutReset()
    {
        // Update the physical board dimensions.
        ResizeSurface();

        // Reposition perimeter objects.
        if (shopAnchor != null)
            shopAnchor.UpdateAnchorPosition();

        if (returnBinAnchor != null)
            returnBinAnchor.UpdateAnchorPosition();

        if (boardGrabHandleAnchor != null)
            boardGrabHandleAnchor.UpdateAnchorPosition();

        // Refresh the displayed grid size.
        UpdateText();

        Debug.Log(
            "[Grid Resize] Board surface and perimeter objects refreshed."
        );
    }

    private void ResizeSurface()
    {
        if (grid == null || boardSurface == null)
            return;

        // Assumes a mesh-based surface, such as a Unity Cube.
        if (initialMeshSize.x <= 0f || initialMeshSize.z <= 0f)
        {
            Debug.LogWarning(
                "[Grid Resize] BoardSurface requires a MeshFilter."
            );
            return;
        }

        float width = grid.Columns * grid.CellSize;
        float depth = grid.Rows * grid.CellSize;

        // Compensate for the original mesh dimensions.
        // The surface should be a direct child of FactoryBoard.
        boardSurface.localScale = new Vector3(
            width / initialMeshSize.x,
            initialSurfaceScale.y,
            depth / initialMeshSize.z
        );

        // Keep the surface centred under the grid.
        Vector3 position = boardSurface.localPosition;
        position.x = 0f;
        position.z = 0f;
        boardSurface.localPosition = position;
    }

    private void UpdateText()
    {
        if (gridSizeText != null && grid != null)
            gridSizeText.text =
                $"GRID: {grid.Columns} x {grid.Rows}";
    }
}
