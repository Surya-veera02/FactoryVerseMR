
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class FactoryShopToggle : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private GameObject shopPanel;

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        // Hide only the shop's visual content.
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

        shopPanel.SetActive(!shopPanel.activeSelf);
    }
}
