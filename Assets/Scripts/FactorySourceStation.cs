
using System.Collections;
using UnityEngine;

public class FactorySourceStation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject productPrefab;
    [SerializeField] private ConveyorModule sourceConveyor;
    [SerializeField] private FactoryGrid grid;
    [SerializeField] private ConveyorConnectionManager connectionManager;

    [Header("Production")]
    [SerializeField] private float spawnInterval = 2.5f;
    [SerializeField] private int maxProducts = 10;
    [SerializeField] private bool autoStart = false;

    private int productsSpawned;
    private Coroutine productionRoutine;

    public int ProductsSpawned => productsSpawned;

    public bool IsProducing => productionRoutine != null;
    public int MaxProducts => maxProducts;

    private void Awake()
    {
        if (sourceConveyor == null)
            sourceConveyor = GetComponent<ConveyorModule>();

        if (grid == null)
            grid = GetComponentInParent<FactoryGrid>();

        if (connectionManager == null)
            connectionManager =
                GetComponentInParent<ConveyorConnectionManager>();
    }

    private void Start()
    {
        if (autoStart)
            StartProduction();
    }

    [ContextMenu("Start Production")]
    public void StartProduction()
    {
        if (!Application.isPlaying ||
            productionRoutine != null)
            return;

        if (productPrefab == null ||
            sourceConveyor == null ||
            grid == null ||
            connectionManager == null)
        {
            Debug.LogError(
                "FactorySourceStation: Missing references."
            );
            return;
        }

        productionRoutine =
            StartCoroutine(ProductionLoop());
    }

    private IEnumerator ProductionLoop()
    {
        // Allow conveyor registration to finish.
        yield return null;

        while (productsSpawned < maxProducts)
        {
            if (sourceConveyor.CurrentCell.HasValue)
            {
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
                    sourceConveyor
                );

                productsSpawned++;

                Debug.Log(
                    $"[Source] Product spawned: " +
                    $"{productsSpawned}/{maxProducts}"
                );
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        productionRoutine = null;

        Debug.Log("[Source] Production completed.");
    }


    [ContextMenu("Stop Production")]
    public void StopProduction()
    {
        if (productionRoutine == null)
            return;

        StopCoroutine(productionRoutine);
        productionRoutine = null;

        Debug.Log("[Source] Production stopped.");
    }

    public bool HasProductsRemaining()
    {
        return productsSpawned < maxProducts;
    }

}
