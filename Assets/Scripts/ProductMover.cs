
using UnityEngine;

public class ProductMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 0.12f;
    [SerializeField] private float heightAboveBoard = 0.045f;

    [Header("Corner Movement")]
    [SerializeField] private bool smoothCorners = true;
    [SerializeField, Range(0.1f, 0.5f)]
    private float cornerRadiusFactor = 0.35f;

    [SerializeField] private bool rotateWithPath = true;

    private FactoryGrid grid;
    private ConveyorConnectionManager connectionManager;
    private ConveyorModule currentModule;
    private ConveyorModule targetModule;

    private bool isMoving;
    private bool registeredAsActive;

    private Vector3 segmentStart;
    private Vector3 segmentEnd;
    private Vector3 curveControl;

    private bool curvedSegment;
    private float segmentProgress;
    private float segmentLength;

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

        Vector2Int cell = currentModule.CurrentCell.Value;

        transform.SetParent(grid.transform, true);

        transform.position = grid.GetCellCenterWorld(
            cell.x,
            cell.y,
            heightAboveBoard
        );

        FindNextModule();
    }

    private void Update()
    {
        if (!isMoving || targetModule == null)
            return;

        float distanceThisFrame = speed * Time.deltaTime;

        segmentProgress +=
            distanceThisFrame / Mathf.Max(segmentLength, 0.0001f);

        segmentProgress = Mathf.Clamp01(segmentProgress);

        Vector3 position = EvaluateSegment(segmentProgress);

        transform.localPosition = position;

        if (rotateWithPath)
        {
            Vector3 direction =
                EvaluateDirection(segmentProgress);

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.localRotation =
                    Quaternion.LookRotation(
                        direction.normalized,
                        Vector3.up
                    );
            }
        }

        if (segmentProgress >= 1f)
        {
            currentModule = targetModule;
            FindNextModule();
        }
    }

    private void FindNextModule()
    {
        targetModule =
            connectionManager.GetNextModule(currentModule);

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

        SetupMovementSegment();

        isMoving = true;

        Debug.Log(
            $"Product moving: {currentModule.name} " +
            $"-> {targetModule.name}"
        );
    }

    private void SetupMovementSegment()
    {
        segmentStart = transform.localPosition;

        Vector2Int targetCell = targetModule.CurrentCell.Value;

        segmentEnd = grid.transform.InverseTransformPoint(
            grid.GetCellCenterWorld(
                targetCell.x,
                targetCell.y,
                heightAboveBoard
            )
        );

        curvedSegment = false;
        curveControl = (segmentStart + segmentEnd) * 0.5f;

        // Apply a curved entry path when moving
        // into a corner conveyor.
        if (smoothCorners && targetModule.IsCorner)
        {
            Vector3 incoming = new Vector3(
                -targetModule.InputDirection.x,
                0f,
                -targetModule.InputDirection.y
            );

            Vector3 outgoing = new Vector3(
                targetModule.OutputDirection.x,
                0f,
                targetModule.OutputDirection.y
            );

            float cellSize =
                Vector3.Distance(segmentStart, segmentEnd);

            Vector3 cornerCenter = segmentEnd;

            // Bend the final portion of the approach
            // toward the corner's outgoing direction.
            curveControl =
                cornerCenter -
                incoming * cellSize * cornerRadiusFactor -
                outgoing * cellSize * cornerRadiusFactor;

            curvedSegment = true;
        }

        segmentProgress = 0f;

        segmentLength = EstimateSegmentLength();
    }

    private Vector3 EvaluateSegment(float t)
    {
        if (!curvedSegment)
        {
            return Vector3.Lerp(
                segmentStart,
                segmentEnd,
                t
            );
        }

        float oneMinusT = 1f - t;

        return oneMinusT * oneMinusT * segmentStart +
               2f * oneMinusT * t * curveControl +
               t * t * segmentEnd;
    }

    private Vector3 EvaluateDirection(float t)
    {
        if (!curvedSegment)
            return segmentEnd - segmentStart;

        return
            2f * (1f - t) * (curveControl - segmentStart) +
            2f * t * (segmentEnd - curveControl);
    }

    private float EstimateSegmentLength()
    {
        if (!curvedSegment)
        {
            return Vector3.Distance(
                segmentStart,
                segmentEnd
            );
        }

        float length = 0f;
        Vector3 previous = EvaluateSegment(0f);

        const int samples = 12;

        for (int i = 1; i <= samples; i++)
        {
            float t = i / (float)samples;

            Vector3 current = EvaluateSegment(t);

            length += Vector3.Distance(
                previous,
                current
            );

            previous = current;
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
