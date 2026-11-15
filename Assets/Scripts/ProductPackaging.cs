
using UnityEngine;

public class ProductPackaging : MonoBehaviour
{
    [Header("Product Visuals")]
    [SerializeField] private GameObject unpackedModel;
    [SerializeField] private GameObject packedBoxModel;
    [SerializeField] private GameObject defectiveBoxModel;

    [Header("Difficulty")]
    [SerializeField]
    private FactoryDifficultyManager difficultyManager;

    private ProductQuality productQuality;

    public bool IsPacked { get; private set; }

    public bool IsUnpackedFailure { get; private set; }

    private bool processingComplete;

    private void Awake()
    {
        productQuality = GetComponent<ProductQuality>();

        if (difficultyManager == null)
        {
            difficultyManager =
                FindFirstObjectByType<FactoryDifficultyManager>();
        }

        SetPackedState(false);
        processingComplete = false;
        IsUnpackedFailure = false;
    }

    public void Pack()
    {
        // Prevent repeated packing attempts,
        // including when packing fails.
        if (processingComplete)
            return;

        processingComplete = true;

        float unpackedChance =
            difficultyManager != null
                ? difficultyManager.CurrentUnpackedProbability
                : 0f;

        // Level 3 packing failure
        if (Random.value < unpackedChance)
        {
            IsPacked = false;
            IsUnpackedFailure = true;

            if (unpackedModel != null)
                unpackedModel.SetActive(true);

            if (packedBoxModel != null)
                packedBoxModel.SetActive(false);

            if (defectiveBoxModel != null)
                defectiveBoxModel.SetActive(false);

            Debug.Log(
                $"[Packaging] {name} left UNPACKED."
            );

            return;
        }

        // Existing packaging behaviour
        bool isDefective =
            productQuality != null &&
            productQuality.IsDefective;

        IsPacked = true;
        IsUnpackedFailure = false;

        if (unpackedModel != null)
            unpackedModel.SetActive(false);

        if (packedBoxModel != null)
            packedBoxModel.SetActive(!isDefective);

        if (defectiveBoxModel != null)
            defectiveBoxModel.SetActive(isDefective);

        Debug.Log(
            $"[Packaging] {name} packed as " +
            (isDefective
                ? "DEFECTIVE BOX"
                : "NORMAL BOX")
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
