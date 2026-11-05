
using UnityEngine;

public class ConveyorModule : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField]
    private FactoryPlacementManager placementManager;

    [Header("Rotation")]
    [SerializeField, Range(0, 3)]
    private int quarterTurns = 0;

    [Header("Shop Placement")]
    [SerializeField]
    private bool autoSnapOnStart = true;

    public Vector2Int? CurrentCell { get; private set; }

    public int QuarterTurns => quarterTurns;

    public enum ConveyorShape
    {
        Straight,
        Corner
    }

    public enum CornerTurn
    {
        Right,
        Left
    }

    [Header("Conveyor Shape")]
    [SerializeField]
    private ConveyorShape conveyorShape =
        ConveyorShape.Straight;

    [SerializeField]
    private CornerTurn cornerTurn =
        CornerTurn.Right;

    public ConveyorShape Shape => conveyorShape;
    public bool IsCorner => conveyorShape == ConveyorShape.Corner;

    public Vector2Int OutputDirection
    {
        get
        {
            switch (quarterTurns)
            {
                case 0: return Vector2Int.up;
                case 1: return Vector2Int.right;
                case 2: return Vector2Int.down;
                default: return Vector2Int.left;
            }
        }
    }

    public Vector2Int InputDirection
    {
        get
        {
            if (conveyorShape == ConveyorShape.Straight)
                return -OutputDirection;

            Vector2Int output = OutputDirection;

            // Right turn: incoming neighbour is
            // 90 degrees clockwise from the output.
            if (cornerTurn == CornerTurn.Right)
            {
                return new Vector2Int(
                    output.y,
                    -output.x
                );
            }

            // Left turn: incoming neighbour is
            // 90 degrees counterclockwise from the output.
            return new Vector2Int(
                -output.y,
                output.x
            );
        }
    }


    public Vector2Int? OutputCell
    {
        get
        {
            if (!CurrentCell.HasValue)
                return null;

            return CurrentCell.Value + OutputDirection;
        }
    }

    public Vector2Int? InputCell
    {
        get
        {
            if (!CurrentCell.HasValue)
                return null;

            return CurrentCell.Value + InputDirection;
        }
    }


    internal void SetCell(Vector2Int cell)
    {
        CurrentCell = cell;
    }

    private void Start()
    {
        if (placementManager == null)
        {
            placementManager =
                GetComponentInParent<FactoryPlacementManager>();
        }

        if (placementManager == null)
        {
            Debug.LogError(
                $"{name}: FactoryPlacementManager not found!"
            );
            return;
        }


        // Existing scene conveyors register automatically.
        // Shop-spawned conveyors wait until the player places them.
        if (!autoSnapOnStart)
        {
            Debug.Log(
                $"{name}: Waiting for player placement."
            );
            return;
        }

        if (!placementManager.TrySnap(
            this,
            transform.position))
        {
            Debug.LogWarning(
                $"{name}: Starting cell unavailable. " +
                "Disabling this conveyor instance."
            );

            gameObject.SetActive(false);
        }

    }


    [ContextMenu("Snap To Nearest Cell")]
    public void SnapToNearestCell()
    {
        TrySnapToNearestCell();
    }

    // Returns true only when placement succeeds.
    public bool TrySnapToNearestCell()
    {
        if (!Application.isPlaying || placementManager == null)
            return false;

        bool success = placementManager.TrySnap(
            this,
            transform.position
        );

        if (!success)
        {
            Debug.LogWarning(
                $"{name}: Cell occupied or outside grid."
            );

            // Return existing conveyors to their last valid cell.
            if (CurrentCell.HasValue)
                placementManager.AlignModule(this);
        }

        return success;
    }


    [ContextMenu("Rotate 90 Degrees")]
    public void Rotate90()
    {
        if (!Application.isPlaying || placementManager == null)
            return;

        quarterTurns = (quarterTurns + 1) % 4;
        placementManager.AlignModule(this);

        // Refresh logical connections after rotation.
        GetComponentInParent<ConveyorConnectionManager>()
            ?.RebuildConnections();
    }


    public void SetQuarterTurnsFromWorldRotation(
        Quaternion worldRotation)
    {
        if (placementManager == null)
            return;

        // Convert the rotation into the board's local space.
        Quaternion localRotation =
            Quaternion.Inverse(placementManager.transform.rotation)
            * worldRotation;

        Vector3 forward = localRotation * Vector3.forward;

        float angle;

        // Handle cases where the conveyor is tilted vertically.
        if (new Vector2(forward.x, forward.z).sqrMagnitude < 0.001f)
        {
            Vector3 right = localRotation * Vector3.right;

            angle = Mathf.Atan2(
                -right.z,
                right.x
            ) * Mathf.Rad2Deg;
        }
        else
        {
            angle = Mathf.Atan2(
                forward.x,
                forward.z
            ) * Mathf.Rad2Deg;
        }

        // Snap the angle to 0, 90, 180 or 270 degrees.
        quarterTurns =
            ((Mathf.RoundToInt(angle / 90f) % 4) + 4) % 4;

        Debug.Log(
            $"{name} rotation snapped to {quarterTurns * 90} degrees"
        );
    }

    public void SetAutoSnapOnStart(bool enabled)
    {
        autoSnapOnStart = enabled;
    }

    private void OnDestroy()
    {
        if (placementManager != null)
            placementManager.Release(this);
    }
}
