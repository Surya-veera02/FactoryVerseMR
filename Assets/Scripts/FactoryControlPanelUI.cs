
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactoryControlPanelUI : MonoBehaviour
{
    [Header("Factory References")]
    [SerializeField] private FactoryProductionController controller;
    [SerializeField] private FactoryShippingStation shipping;

    [Header("UI References")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text deliveredText;
    [SerializeField] private TMP_Text activeProductsText;

    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;

    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartPressed);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopPressed);
    }

    private void Update()
    {
        if (controller == null || shipping == null)
            return;

        if (statusText != null)
            statusText.text =
                "STATUS: " + controller.CurrentState;

        if (deliveredText != null)
            deliveredText.text =
                "DELIVERED: " + shipping.DeliveredCount +
                "/" + shipping.TargetDeliveries;

        if (activeProductsText != null)
            activeProductsText.text =
                "ACTIVE: " + ProductMover.ActiveProductCount;

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
            startButton.interactable =
                !running && !stopping && !completed;

        if (stopButton != null)
            stopButton.interactable = running;
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

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartPressed);

        if (stopButton != null)
            stopButton.onClick.RemoveListener(OnStopPressed);
    }
}
