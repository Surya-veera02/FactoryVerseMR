
using UnityEngine;

public class ConveyorModule : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField]
    private FactoryPlacementManager placementManager;

    [Header("Rotation")]
    [SerializeField, Range(0, 3)]
    private int quarterTurns = 0;

    public Vector2Int? CurrentCell { get; private set; }

    public int QuarterTurns => quarterTurns;

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

        // Snap to the starting cell.
        if (!placementManager.TrySnap(this, transform.position))
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
        if (!Application.isPlaying || placementManager == null)
            return;

        bool success = placementManager.TrySnap(
            this,
            transform.position
        );

        if (!success)
        {
            Debug.LogWarning(
                $"{name}: Cell occupied or outside grid."
            );

            // Return to the last valid position.
            placementManager.AlignModule(this);
        }
    }

    [ContextMenu("Rotate 90 Degrees")]
    public void Rotate90()
    {
        if (!Application.isPlaying || placementManager == null)
            return;

        quarterTurns = (quarterTurns + 1) % 4;

        placementManager.AlignModule(this);
    }

    private void OnDestroy()
    {
        if (placementManager != null)
            placementManager.Release(this);
    }
}
