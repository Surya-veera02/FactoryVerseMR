
using UnityEngine;

public class FactorySourceStation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject productPrefab;
    [SerializeField] private ConveyorModule sourceConveyor;
    [SerializeField] private FactoryGrid grid;
    [SerializeField] private ConveyorConnectionManager connectionManager;

    [Header("Production")]
    [SerializeField] private int maxProducts = 10;
    [SerializeField] private float pokeCooldown = 0.5f;
    [SerializeField] private int maxActiveProducts = 3;

    private int productsSpawned;
    private bool productionEnabled;
    private float nextSpawnTime;

    public int ProductsSpawned => productsSpawned;
    public int MaxProducts => maxProducts;
    public bool IsProducing => productionEnabled;

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

    [ContextMenu("Start Production")]
    public void StartProduction()
    {
        if (!Application.isPlaying || productionEnabled)
            return;

        if (productPrefab == null ||
            sourceConveyor == null ||
            grid == null ||
            connectionManager == null)
        {
            Debug.LogError(
                "[Source] Missing production references."
            );
            return;
        }

        productionEnabled = true;

        Debug.Log(
            "[Source] Manual production enabled. " +
            "Poke the machine to spawn a product."
        );
    }

    // Called by the source machine's poke interaction.
    public bool TrySpawnProduct()
    {

        Debug.Log(
            $"[SOURCE DEBUG] " +
            $"Enabled={productionEnabled}, " +
            $"Spawned={productsSpawned}/{maxProducts}, " +
            $"Active={ProductMover.ActiveProductCount}/{maxActiveProducts}, " +
            $"CooldownRemaining={Mathf.Max(0f, nextSpawnTime - Time.time):F2}, " +
            $"HasSourceCell={sourceConveyor != null && sourceConveyor.CurrentCell.HasValue}, " +
            $"PrefabAssigned={productPrefab != null}, " +
            $"GridAssigned={grid != null}, " +
            $"ConnectionsAssigned={connectionManager != null}"
        );

        if (!Application.isPlaying || !productionEnabled)
            return false;

        if (!HasProductsRemaining())
        {
            Debug.Log("[Source] Production quantity reached.");
            return false;
        }

        if (Time.time < nextSpawnTime)
            return false;

        if (ProductMover.ActiveProductCount >= maxActiveProducts)
        {
            Debug.Log(
                "[Source] Too many active products. " +
                "Wait for products to leave the conveyor."
            );
            return false;
        }

        if (productPrefab == null ||
            sourceConveyor == null ||
            grid == null ||
            connectionManager == null ||
            !sourceConveyor.CurrentCell.HasValue)
        {
            Debug.LogWarning(
                "[Source] Product cannot spawn: invalid setup."
            );
            return false;
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
            sourceConveyor
        );

        if (!mover.isActiveAndEnabled)
        {
            Destroy(product);
            return false;
        }

        productsSpawned++;
        nextSpawnTime = Time.time + pokeCooldown;

        Debug.Log(
            $"[Source] Product spawned by poke: " +
            $"{productsSpawned}/{maxProducts}"
        );

        return true;
    }

    [ContextMenu("Stop Production")]
    public void StopProduction()
    {
        if (!productionEnabled)
            return;

        productionEnabled = false;

        Debug.Log("[Source] Manual production disabled.");
    }

    public void ResetProduction()
    {
        StopProduction();
        productsSpawned = 0;
        nextSpawnTime = 0f;
    }

    public bool HasProductsRemaining()
    {
        return productsSpawned < maxProducts;
    }
}
