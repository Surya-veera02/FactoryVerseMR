
using UnityEngine;

public class FactoryProductSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject productPrefab;
    [SerializeField] private ConveyorModule startingConveyor;
    [SerializeField] private FactoryGrid grid;
    [SerializeField]
    private ConveyorConnectionManager connectionManager;

    private void Awake()
    {
        if (grid == null)
            grid = GetComponent<FactoryGrid>();

        if (connectionManager == null)
            connectionManager =
                GetComponent<ConveyorConnectionManager>();
    }

    [ContextMenu("Spawn Test Product")]
    public void SpawnTestProduct()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Enter Play Mode before spawning products."
            );
            return;
        }

        if (productPrefab == null ||
            startingConveyor == null ||
            grid == null ||
            connectionManager == null)
        {
            Debug.LogError(
                "FactoryProductSpawner: Missing references."
            );
            return;
        }

        GameObject product = Instantiate(
            productPrefab,
            transform.position,
            Quaternion.identity
        );

        ProductMover mover =
            product.GetComponent<ProductMover>();

        if (mover == null)
            mover = product.AddComponent<ProductMover>();

        mover.Initialize(
            grid,
            connectionManager,
            startingConveyor
        );

        Debug.Log("Test product spawned.");
    }
}
