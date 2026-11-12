
using UnityEngine;
using TMPro;

public class FactoryGameplayHUD : MonoBehaviour
{
    [Header("Factory References")]
    [SerializeField]
    private FactoryShippingStation shippingStation;

    [Header("Performance Counters")]
    [SerializeField]
    private TMP_Text deliveredValue;

    [SerializeField]
    private TMP_Text defectiveValue;

    [SerializeField]
    private TMP_Text unpackedValue;

    private int previousDelivered = -1;
    private int previousDefective = -1;
    private int previousUnpacked = -1;

    private void Awake()
    {
        if (shippingStation == null)
        {
            shippingStation =
                FindFirstObjectByType<FactoryShippingStation>();
        }
    }

    private void Start()
    {
        RefreshHUD();
    }

    private void Update()
    {
        if (shippingStation == null)
            return;

        // Refresh only when a counter changes.
        if (previousDelivered != shippingStation.DeliveredCount ||
            previousDefective != shippingStation.DefectiveDeliveredCount ||
            previousUnpacked != shippingStation.RejectedCount)
        {
            RefreshHUD();
        }
    }

    private void RefreshHUD()
    {
        if (shippingStation == null)
            return;

        previousDelivered = shippingStation.DeliveredCount;
        previousDefective = shippingStation.DefectiveDeliveredCount;
        previousUnpacked = shippingStation.RejectedCount;

        if (deliveredValue != null)
            deliveredValue.text = previousDelivered.ToString();

        if (defectiveValue != null)
            defectiveValue.text = previousDefective.ToString();

        if (unpackedValue != null)
            unpackedValue.text = previousUnpacked.ToString();
    }
}
