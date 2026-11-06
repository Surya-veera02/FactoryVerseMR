
using UnityEngine;

public class ProductQuality : MonoBehaviour
{
    [Header("Quality Settings")]
    [SerializeField, Range(0f, 1f)]
    private float defectProbability = 0.2f;

    public bool IsDefective { get; private set; }

    private bool qualityAssigned;

    public void AssignRandomQuality()
    {
        if (qualityAssigned)
            return;

        IsDefective =
            Random.value < defectProbability;

        qualityAssigned = true;

        Debug.Log(
            $"[Quality] {name}: " +
            (IsDefective ? "DEFECTIVE" : "NORMAL")
        );
    }
}
