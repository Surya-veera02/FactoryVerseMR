
using UnityEngine;

public class FactoryPerimeterAnchor : MonoBehaviour
{
    public enum AnchorSide
    {
        Front,
        Back,
        Left,
        Right,
        FrontLeft,
        FrontRight,
        BackLeft,
        BackRight
    }

    [Header("References")]
    [SerializeField] private FactoryGrid grid;

    [Header("Position")]
    [SerializeField] private AnchorSide anchorSide = AnchorSide.Front;
    [SerializeField] private float edgeGap = 0.025f;
    [SerializeField] private float alongEdgeOffset = 0f;
    [SerializeField] private float heightOffset = 0f;

    [Header("Options")]
    [SerializeField] private bool updateContinuously = false;

    private void Awake()
    {
        if (grid == null)
            grid = GetComponentInParent<FactoryGrid>();
    }

    private void Start()
    {
        UpdateAnchorPosition();
    }

    private void LateUpdate()
    {
        if (updateContinuously)
            UpdateAnchorPosition();
    }

    public void UpdateAnchorPosition()
    {
        if (grid == null)
            return;

        float halfWidth = grid.Columns * grid.CellSize * 0.5f;
        float halfDepth = grid.Rows * grid.CellSize * 0.5f;

        Vector3 position = transform.localPosition;

        switch (anchorSide)
        {
            case AnchorSide.Front:
                position.x = alongEdgeOffset;
                position.z = -halfDepth - edgeGap;
                break;

            case AnchorSide.Back:
                position.x = alongEdgeOffset;
                position.z = halfDepth + edgeGap;
                break;

            case AnchorSide.Left:
                position.x = -halfWidth - edgeGap;
                position.z = alongEdgeOffset;
                break;

            case AnchorSide.Right:
                position.x = halfWidth + edgeGap;
                position.z = alongEdgeOffset;
                break;

            case AnchorSide.FrontLeft:
                position.x = -halfWidth - edgeGap;
                position.z = -halfDepth - edgeGap;
                break;

            case AnchorSide.FrontRight:
                position.x = halfWidth + edgeGap;
                position.z = -halfDepth - edgeGap;
                break;

            case AnchorSide.BackLeft:
                position.x = -halfWidth - edgeGap;
                position.z = halfDepth + edgeGap;
                break;

            case AnchorSide.BackRight:
                position.x = halfWidth + edgeGap;
                position.z = halfDepth + edgeGap;
                break;
        }

        position.y = heightOffset;
        transform.localPosition = position;
    }
}
