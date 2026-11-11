
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class FactoryShopToggle : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private GameObject shopPanel;

    [Header("Animation")]
    [SerializeField]
    private FactoryMachineJerkAnimation shopAnimation;
    [SerializeField] private FactorySoundEffect shopSound;

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        // Hide shop UI initially.
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnShopSelected);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnShopSelected);
    }

    private void OnShopSelected(SelectEnterEventArgs args)
    {
        if (shopPanel == null)
            return;

        bool openingShop = !shopPanel.activeSelf;

        // Toggle shop visibility.
        shopPanel.SetActive(openingShop);

        // Animate only when opening the shop.
        if (openingShop)
        {
            if (shopAnimation != null)
                shopAnimation.PlayAnimation();

            if (shopSound != null)
                shopSound.PlaySound();
        }
    }
}
