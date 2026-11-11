
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class SourceMachinePoke : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FactorySourceStation sourceStation;

    private XRSimpleInteractable interactable;
    [Header("Audio")]
    [SerializeField]
    private FactorySoundEffect productionSound;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();

        if (sourceStation == null)
        {
            sourceStation =
                GetComponentInParent<FactorySourceStation>();
        }
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnPoked);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnPoked);
    }

    private void OnPoked(SelectEnterEventArgs args)
    {
        Debug.Log(
            "[Source Poke] Interaction detected by: " +
            args.interactorObject.transform.name
        );

        if (sourceStation == null)
        {
            Debug.LogError(
                "[Source Poke] Source station missing."
            );
            return;
        }

        bool spawned = sourceStation.TrySpawnProduct();

        Debug.Log(
            "[Source Poke] Spawn result: " + spawned
        );

        if (spawned)
        {
            Debug.Log(
                "[Source Poke] Product successfully spawned!"
            );

            if (productionSound != null)
                productionSound.PlaySound();
        }

        // Future milestone:
        // Play machine jerk animation.
    }
}
