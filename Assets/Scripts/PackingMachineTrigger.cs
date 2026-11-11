
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PackingMachineTrigger : MonoBehaviour
{

    [SerializeField]
    private FactoryMachineJerkAnimation machineAnimation;
    [SerializeField] private FactorySoundEffect packingSound;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Product"))
            return;

        ProductPackaging packaging =
            other.GetComponentInParent<ProductPackaging>();

        if (packaging == null)
        {
            Debug.LogWarning(
                "[Packing] ProductPackaging not found."
            );
            return;
        }

        if (packaging.IsPacked)
            return;

        packaging.Pack();
        if (packingSound != null)
            packingSound.PlaySound();
        if (machineAnimation != null)
            machineAnimation.PlayAnimation();

        Debug.Log(
            "[Packing Machine] Product successfully packed!"
        );
    }
}
