
using UnityEngine;

public class FactorySourceStation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject productPrefab;
    [SerializeField] private ConveyorModule sourceConveyor;
    [SerializeField] private FactoryGrid grid;
    [SerializeField] private ConveyorConnectionManager connectionManager;

    [Header("Production")]
    //[SerializeField] private int maxProducts = 10;
    [SerializeField] private float pokeCooldown = 0.5f;
    //[SerializeField] private int maxActiveProducts = 3;

    [Header("Credit Economy")]
    [SerializeField] private FactoryCreditManager creditManager;

    private int productsSpawned;
    private bool productionEnabled;
    private float nextSpawnTime;

    public int ProductsSpawned => productsSpawned;
    //public int MaxProducts => maxProducts;
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
        if (creditManager == null)
            creditManager = GetComponentInParent<FactoryCreditManager>();
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
        if (!Application.isPlaying || !productionEnabled)
            return false;

        // Prevent excessively rapid spawning.
        if (Time.time < nextSpawnTime)
            return false;

        if (productPrefab == null ||
            sourceConveyor == null ||
            grid == null ||
            connectionManager == null ||
            creditManager == null ||
            !sourceConveyor.CurrentCell.HasValue)
        {
            Debug.LogWarning(
                "[Source] Cannot spawn: missing references."
            );
            return false;
        }

        int cost = creditManager.productionCost;

        if (!creditManager.CanAfford(cost))
        {
            Debug.LogWarning(
                "[Source] Insufficient credits to produce."
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

        // Charge only after successful initialization.
        if (!creditManager.TrySpend(cost))
        {
            mover.UnregisterProduct();
            Destroy(product);
            return false;
        }

        productsSpawned++;
        nextSpawnTime = Time.time + pokeCooldown;

        Debug.Log(
            $"[Source] Spawned product #{productsSpawned}. " +
            $"Cost: {cost} CR. " +
            $"Balance: {creditManager.currentCredits}"
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
        return creditManager != null &&
               creditManager.CanAfford(
                   creditManager.productionCost
               );
    }

}
