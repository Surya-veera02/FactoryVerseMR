
using System.Collections.Generic;
using UnityEngine;

public class ConveyorConnectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FactoryPlacementManager placementManager;

    [Header("Debug")]
    [SerializeField]
    private bool logConnections = true;

    private readonly Dictionary<ConveyorModule, ConveyorModule>
        outputConnections = new();

    private void Awake()
    {
        if (placementManager == null)
            placementManager =
                GetComponent<FactoryPlacementManager>();
    }

    public void RebuildConnections()
    {
        outputConnections.Clear();

        if (placementManager == null)
            return;

        ConveyorModule[] modules =
            GetComponentsInChildren<ConveyorModule>();

        foreach (ConveyorModule module in modules)
        {
            if (!module.isActiveAndEnabled ||
                !module.CurrentCell.HasValue)
                continue;

            Vector2Int targetCell =
                module.CurrentCell.Value +
                module.OutputDirection;

            ConveyorModule neighbour =
                placementManager.GetModuleAtCell(targetCell);

            if (neighbour == null || neighbour == module)
                continue;

            // The neighbour must accept products
            // from this conveyor's output direction.
            bool validConnection =
                neighbour.InputDirection ==
                -module.OutputDirection;

            if (!validConnection)
                continue;

            outputConnections[module] = neighbour;

            if (logConnections)
            {
                Debug.Log(
                    $"[Factory Connection] " +
                    $"{module.name} -> {neighbour.name}"
                );
            }
        }

        if (logConnections)
        {
            Debug.Log(
                $"[Factory Connection] Rebuilt: " +
                $"{outputConnections.Count} valid connection(s)."
            );
        }
    }

    public ConveyorModule GetNextModule(
        ConveyorModule module)
    {
        if (module == null)
            return null;

        return outputConnections.TryGetValue(
            module,
            out ConveyorModule next)
            ? next
            : null;
    }


    public bool HasValidRoute(
        ConveyorModule source,
        ConveyorModule destination)
    {
        if (source == null || destination == null)
            return false;

        // Use the latest conveyor placement information.
        RebuildConnections();

        HashSet<ConveyorModule> visited = new();

        ConveyorModule current = source;

        while (current != null)
        {
            // Prevent infinite loops.
            if (!visited.Add(current))
            {
                Debug.LogWarning(
                    "[Factory Connection] Route contains a loop."
                );
                return false;
            }

            if (current == destination)
                return true;

            current = GetNextModule(current);
        }

        return false;
    }


}
