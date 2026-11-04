
using UnityEngine;

public class FactoryGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int columns = 6;
    [SerializeField] private int rows = 4;
    [SerializeField] private float cellSize = 0.12f;

    [Header("Visual Settings")]
    [SerializeField] private float boardTopY = 0.0175f;
    [SerializeField] private Material gridLineMaterial;

    private const float lineThickness = 0.003f;

    public int Columns => columns;
    public int Rows => rows;

    private void Awake()
    {
        CreateGridVisuals();
    }

    private void CreateGridVisuals()
    {
        GameObject gridRoot = new GameObject("GridVisuals");
        gridRoot.transform.SetParent(transform, false);

        float width = columns * cellSize;
        float depth = rows * cellSize;
        float lineY = boardTopY + 0.0025f;

        // Lines parallel to the Z axis
        for (int x = 0; x <= columns; x++)
        {
            float xPosition = -width / 2f + x * cellSize;

            CreateLine(
                gridRoot.transform,
                "ColumnLine_" + x,
                new Vector3(xPosition, lineY, 0f),
                new Vector3(lineThickness, lineThickness, depth)
            );
        }

        // Lines parallel to the X axis
        for (int z = 0; z <= rows; z++)
        {
            float zPosition = -depth / 2f + z * cellSize;

            CreateLine(
                gridRoot.transform,
                "RowLine_" + z,
                new Vector3(0f, lineY, zPosition),
                new Vector3(width, lineThickness, lineThickness)
            );
        }
    }

    private void CreateLine(
        Transform parent,
        string lineName,
        Vector3 localPosition,
        Vector3 localScale)
    {
        GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);

        line.name = lineName;
        line.transform.SetParent(parent, false);
        line.transform.localPosition = localPosition;
        line.transform.localScale = localScale;

        // Grid lines are visual only.
        Destroy(line.GetComponent<Collider>());

        if (gridLineMaterial != null)
        {
            line.GetComponent<Renderer>().sharedMaterial =
                gridLineMaterial;
        }
    }

    // Returns the world position of a cell's center.
    public Vector3 GetCellCenterWorld(
        int column,
        int row,
        float heightAboveBoard = 0f)
    {
        float width = columns * cellSize;
        float depth = rows * cellSize;

        Vector3 localPosition = new Vector3(
            -width / 2f + (column + 0.5f) * cellSize,
            boardTopY + heightAboveBoard,
            -depth / 2f + (row + 0.5f) * cellSize
        );

        return transform.TransformPoint(localPosition);
    }

    // Converts a world position into grid coordinates.
    public bool TryGetCell(
        Vector3 worldPosition,
        out int column,
        out int row)
    {
        Vector3 local = transform.InverseTransformPoint(worldPosition);

        float width = columns * cellSize;
        float depth = rows * cellSize;

        column = Mathf.FloorToInt((local.x + width / 2f) / cellSize);
        row = Mathf.FloorToInt((local.z + depth / 2f) / cellSize);

        return column >= 0 && column < columns &&
               row >= 0 && row < rows;
    }
}
