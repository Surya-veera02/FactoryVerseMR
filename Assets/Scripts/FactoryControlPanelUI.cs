
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactoryControlPanelUI : MonoBehaviour
{
    [Header("Factory References")]
    [SerializeField]
    private FactoryProductionController controller;

    [SerializeField]
    private FactoryShippingStation shipping;

    [Header("UI References")]
    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private TMP_Text deliveredText;

    [SerializeField]
    private TMP_Text activeProductsText;

    [Header("Credits")]
    [SerializeField]
    private FactoryCreditManager creditManager;

    [SerializeField]
    private TMP_Text creditText;

    [Header("Production Buttons")]
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private Button stopButton;

    [SerializeField]
    private Button clearLineButton;

    [SerializeField]
    private Button resetButton;

    private void Awake()
    {
        // Find references if not assigned.
        if (controller == null)
            controller =
                FindFirstObjectByType<FactoryProductionController>();

        if (shipping == null)
            shipping =
                FindFirstObjectByType<FactoryShippingStation>();

        if (creditManager == null)
            creditManager =
                GetComponentInParent<FactoryCreditManager>();

        // Register button listeners.
        if (startButton != null)
            startButton.onClick.AddListener(OnStartPressed);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopPressed);

        if (clearLineButton != null)
            clearLineButton.onClick.AddListener(OnClearLinePressed);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetPressed);
    }

    private void Update()
    {
        if (controller == null || shipping == null)
            return;

        // Factory status.
        if (statusText != null)
        {
            statusText.text =
                "STATUS: " + controller.CurrentState;
        }

        // Total products reaching Shipping:
        // good + defective + unpacked.
        if (deliveredText != null)
        {
            deliveredText.text =
                "DELIVERED: " + shipping.TotalReachedShipping;
        }

        // Currently active products.
        if (activeProductsText != null)
        {
            activeProductsText.text =
                "ACTIVE: " + ProductMover.ActiveProductCount;
        }

        bool running =
            controller.CurrentState ==
            FactoryProductionController.ProductionState.Running;

        bool stopping =
            controller.CurrentState ==
            FactoryProductionController.ProductionState.Stopping;

        bool completed =
            controller.CurrentState ==
            FactoryProductionController.ProductionState.Completed;

        if (startButton != null)
        {
            startButton.interactable =
                !running && !stopping && !completed;
        }

        if (stopButton != null)
        {
            stopButton.interactable = running;
        }

        RefreshCreditText();
    }

    private void RefreshCreditText()
    {
        if (creditManager == null || creditText == null)
            return;

        creditText.text =
            $"CREDITS: {creditManager.currentCredits}";
    }

    private void OnStartPressed()
    {
        if (controller != null)
            controller.StartFactory();
    }

    private void OnStopPressed()
    {
        if (controller != null)
            controller.StopFactory();
    }

    private void OnClearLinePressed()
    {
        if (controller != null)
            controller.ClearLine();
    }

    private void OnResetPressed()
    {
        if (controller != null)
            controller.ResetFactory();
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartPressed);

        if (stopButton != null)
            stopButton.onClick.RemoveListener(OnStopPressed);

        if (clearLineButton != null)
            clearLineButton.onClick.RemoveListener(OnClearLinePressed);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnResetPressed);
    }
}
