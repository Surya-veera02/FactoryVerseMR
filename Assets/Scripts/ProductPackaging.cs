
using UnityEngine;

public class ProductPackaging : MonoBehaviour
{
    [Header("Product Visuals")]
    [SerializeField] private GameObject unpackedModel;
    [SerializeField] private GameObject packedBoxModel;

    public bool IsPacked { get; private set; }

    private void Awake()
    {
        SetPackedState(false);
    }

    public void Pack()
    {
        if (IsPacked)
            return;

        SetPackedState(true);

        Debug.Log(
            $"[Packaging] Product packed: {name}"
        );
    }

    private void SetPackedState(bool packed)
    {
        IsPacked = packed;

        if (unpackedModel != null)
            unpackedModel.SetActive(!packed);

        if (packedBoxModel != null)
            packedBoxModel.SetActive(packed);
    }
}
