
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

    [Header("Difficulty References")]
    [SerializeField]
    private FactoryDifficultyManager difficultyManager;

    [Header("Difficulty UI")]
    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text defectRateText;

    private void Awake()
    {
        if (shippingStation == null)
        {
            shippingStation =
                FindFirstObjectByType<FactoryShippingStation>();
        }

        if (difficultyManager == null)
        {
            difficultyManager =
                FindFirstObjectByType<FactoryDifficultyManager>();
        }
    }

    private void Start()
    {
        RefreshHUD();
        RefreshDifficultyHUD();
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

    private void OnEnable()
    {
        if (difficultyManager != null)
            difficultyManager.OnLevelChanged += OnDifficultyChanged;
    }

    private void OnDisable()
    {
        if (difficultyManager != null)
            difficultyManager.OnLevelChanged -= OnDifficultyChanged;
    }

    private void RefreshDifficultyHUD()
    {
        if (difficultyManager == null)
            return;

        if (levelText != null)
        {
            levelText.text =
                $"LEVEL {difficultyManager.CurrentLevel:00}";
        }

        if (defectRateText != null)
        {
            float percentage =
                difficultyManager.CurrentDefectProbability * 100f;

            defectRateText.text =
                $"DEFECT RATE: {percentage:0}%";
        }
    }

    private void OnDifficultyChanged(int newLevel)
    {
        RefreshDifficultyHUD();
    }
}
