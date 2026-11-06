
using UnityEngine;

public class ProductQuality : MonoBehaviour
{
    [Header("Fallback Probability")]
    [SerializeField, Range(0f, 1f)]
    private float defectProbability = 0.2f;

    public bool IsDefective { get; private set; }

    private bool qualityAssigned;

    public void AssignRandomQuality(float probability)
    {
        if (qualityAssigned)
            return;

        probability = Mathf.Clamp01(probability);

        IsDefective = Random.value < probability;
        qualityAssigned = true;

        Debug.Log(
            $"[Quality] {name}: " +
            (IsDefective ? "DEFECTIVE" : "NORMAL") +
            $" | Probability: {probability:P0}"
        );
    }

    // Retains compatibility with existing calls.
    public void AssignRandomQuality()
    {
        AssignRandomQuality(defectProbability);
    }
}
