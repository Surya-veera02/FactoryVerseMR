
using System.Collections.Generic;
using UnityEngine;

public class ProductMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 0.12f;
    [SerializeField] private float heightAboveBoard = 0.06f;

    [Header("Corner Movement")]
    [SerializeField] private bool smoothCorners = true;
    [SerializeField, Range(0.1f, 0.5f)]
    private float cornerRadiusFactor = 0.35f;
    [SerializeField] private bool rotateWithPath = true;
    [SerializeField] private float productRotationOffset = 0f;

    private FactoryGrid grid;
    private ConveyorConnectionManager connectionManager;
    private ConveyorModule currentModule;
    private ConveyorModule targetModule;

    private bool isMoving;
    private bool registeredAsActive;


    [Header("Packing Detection")]
    [SerializeField] private float packingDetectionInterval = 0.05f;

    private float nextPackingCheckTime;
    private PackingMachinePlacement[] packingMachines;
    private ProductPackaging packaging;


    private struct PathSegment
    {
        public Vector3 start;
        public Vector3 control;
        public Vector3 end;
        public bool curved;
        public float length;
    }

    private readonly List<PathSegment> path = new();

    private int pathIndex;
    private float pathProgress;

    public static int ActiveProductCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetActiveProductCount()
    {
        ActiveProductCount = 0;
    }

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

        transform.SetParent(grid.transform, true);

        Vector2Int cell = currentModule.CurrentCell.Value;

        transform.position = grid.GetCellCenterWorld(
            cell.x,
            cell.y,
            heightAboveBoard
        );

        FindNextModule();
        
packaging = GetComponent<ProductPackaging>();

packingMachines =
    grid.GetComponentsInChildren<PackingMachinePlacement>();

nextPackingCheckTime = 0f;

    }

    private void Update()
    {
        if (!isMoving || pathIndex >= path.Count)
            return;

        float remainingDistance = speed * Time.deltaTime;

        // Consume movement distance across path segments.
        while (remainingDistance > 0f &&
               isMoving &&
               pathIndex < path.Count)
        {
            PathSegment segment = path[pathIndex];

            float remainingOnSegment =
                segment.length * (1f - pathProgress);

            float movement =
                Mathf.Min(remainingDistance, remainingOnSegment);

            pathProgress +=
                movement / Mathf.Max(segment.length, 0.0001f);

            pathProgress = Mathf.Clamp01(pathProgress);

            transform.localPosition =
                Evaluate(segment, pathProgress);

            if (rotateWithPath)
            {
                Vector3 direction =
                    EvaluateDirection(segment, pathProgress);

                if (direction.sqrMagnitude > 0.0001f)
                {
                    Quaternion pathRotation = Quaternion.LookRotation(
                        direction.normalized,
                        Vector3.up
                    );

                    transform.localRotation =
                        pathRotation * Quaternion.Euler(0f, 90f, 0f);
                }
            }

            remainingDistance -= movement;

            if (pathProgress >= 0.9999f)
            {
                pathIndex++;
                pathProgress = 0f;

                if (pathIndex >= path.Count)
                {
                    currentModule = targetModule;
                    FindNextModule();
                }
            }
        }
        CheckForPackingMachine();
    }


    private void CheckForPackingMachine()
    {
        if (packaging == null || packaging.IsPacked)
            return;

        if (Time.time < nextPackingCheckTime)
            return;

        nextPackingCheckTime =
            Time.time + packingDetectionInterval;

        if (grid == null ||
            !grid.TryGetCell(
                transform.position,
                out int column,
                out int row))
            return;

        Vector2Int productCell =
            new Vector2Int(column, row);

        if (packingMachines == null)
            return;

        foreach (PackingMachinePlacement machine in packingMachines)
        {
            if (machine == null ||
                !machine.gameObject.activeInHierarchy ||
                !machine.IsPlaced)
                continue;

            if (!grid.TryGetCell(
                machine.transform.position,
                out int machineColumn,
                out int machineRow))
                continue;

            Vector2Int machineCell =
                new Vector2Int(machineColumn, machineRow);

            if (productCell != machineCell)
                continue;

            packaging.Pack();

            Debug.Log(
                $"[Packing Machine] Packed product at {productCell}"
            );

            break;
        }
    }

    private void FindNextModule()
    {
        ConveyorModule next =
            connectionManager.GetNextModule(currentModule);

        ConveyorModule corner = null;

        // Look ahead through a corner so we can
        // build the complete entry-to-exit curve.
        if (smoothCorners && next != null && next.IsCorner)
        {
            ConveyorModule afterCorner =
                connectionManager.GetNextModule(next);

            if (afterCorner != null &&
                next.CurrentCell.HasValue &&
                afterCorner.CurrentCell.HasValue)
            {
                corner = next;
                next = afterCorner;
            }
        }

        targetModule = next;

        if (targetModule == null)
        {
            isMoving = false;

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

            return;
        }

        if (!targetModule.CurrentCell.HasValue)
        {
            isMoving = false;
            return;
        }

        BuildPathToNextModule(corner);

        isMoving = true;

        Debug.Log(
            $"Product moving: {currentModule.name} " +
            $"-> {targetModule.name}"
        );
    }




    private void BuildPathToNextModule(ConveyorModule corner)
    {
        path.Clear();
        pathIndex = 0;
        pathProgress = 0f;

        Vector3 start = transform.localPosition;

        Vector2Int targetCell = targetModule.CurrentCell.Value;

        Vector3 end = grid.transform.InverseTransformPoint(
            grid.GetCellCenterWorld(
                targetCell.x,
                targetCell.y,
                heightAboveBoard
            )
        );

        // Normal straight movement.
        if (corner == null)
        {
            AddStraight(start, end);
            return;
        }

        Vector2Int cornerCell = corner.CurrentCell.Value;

        Vector3 center = grid.transform.InverseTransformPoint(
            grid.GetCellCenterWorld(
                cornerCell.x,
                cornerCell.y,
                heightAboveBoard
            )
        );

        float halfCell = grid.CellSize * 0.5f;

        Vector3 input = ToVector3(corner.InputDirection);
        Vector3 output = ToVector3(corner.OutputDirection);

        // Actual entry and exit edges of the corner.
        Vector3 entry = center + input * halfCell;
        Vector3 exit = center + output * halfCell;

        // 1. Approach the corner in a straight line.
        AddStraight(start, entry);

        // 2. Follow the complete 90-degree turn.
        // The centre acts as the Bezier control point,
        // giving the curve correct entry/exit tangents.
        AddCurve(entry, center, exit);

        // 3. Leave the corner toward the next conveyor.
        AddStraight(exit, end);
    }



    private Vector3 ToVector3(Vector2Int direction)
    {
        return new Vector3(
            direction.x,
            0f,
            direction.y
        );
    }

    private void AddStraight(Vector3 start, Vector3 end)
    {
        float length = Vector3.Distance(start, end);

        if (length < 0.0001f)
            return;

        path.Add(new PathSegment
        {
            start = start,
            control = (start + end) * 0.5f,
            end = end,
            curved = false,
            length = length
        });
    }

    private void AddCurve(
        Vector3 start,
        Vector3 control,
        Vector3 end)
    {
        PathSegment segment = new PathSegment
        {
            start = start,
            control = control,
            end = end,
            curved = true
        };

        segment.length = EstimateLength(segment);

        path.Add(segment);
    }

    private Vector3 Evaluate(PathSegment segment, float t)
    {
        if (!segment.curved)
        {
            return Vector3.Lerp(
                segment.start,
                segment.end,
                t
            );
        }

        float u = 1f - t;

        return u * u * segment.start +
               2f * u * t * segment.control +
               t * t * segment.end;
    }

    private Vector3 EvaluateDirection(
        PathSegment segment,
        float t)
    {
        if (!segment.curved)
            return segment.end - segment.start;

        return
            2f * (1f - t) *
            (segment.control - segment.start) +
            2f * t *
            (segment.end - segment.control);
    }

    private float EstimateLength(PathSegment segment)
    {
        float length = 0f;
        Vector3 previous = segment.start;

        const int samples = 16;

        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;

            Vector3 next = Evaluate(segment, t);

            length += Vector3.Distance(previous, next);
            previous = next;
        }

        return Mathf.Max(length, 0.0001f);
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
