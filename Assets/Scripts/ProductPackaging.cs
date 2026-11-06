
using UnityEngine;

public class ProductPackaging : MonoBehaviour
{
    [Header("Product Visuals")]
    [SerializeField] private GameObject unpackedModel;
    [SerializeField] private GameObject packedBoxModel;
    [SerializeField] private GameObject defectiveBoxModel;

    private ProductQuality productQuality;

    public bool IsPacked { get; private set; }

    private void Awake()
    {
        productQuality = GetComponent<ProductQuality>();

        SetPackedState(false);
    }

    public void Pack()
    {
        if (IsPacked)
            return;

        bool isDefective =
            productQuality != null &&
            productQuality.IsDefective;

        IsPacked = true;

        if (unpackedModel != null)
            unpackedModel.SetActive(false);

        if (packedBoxModel != null)
            packedBoxModel.SetActive(!isDefective);

        if (defectiveBoxModel != null)
            defectiveBoxModel.SetActive(isDefective);

        Debug.Log(
            $"[Packaging] {name} packed as " +
            (isDefective ? "DEFECTIVE BOX" : "NORMAL BOX")
        );
    }

    private void SetPackedState(bool packed)
    {
        IsPacked = packed;

        if (unpackedModel != null)
            unpackedModel.SetActive(!packed);

        if (packedBoxModel != null)
            packedBoxModel.SetActive(false);

        if (defectiveBoxModel != null)
            defectiveBoxModel.SetActive(false);
    }
}
