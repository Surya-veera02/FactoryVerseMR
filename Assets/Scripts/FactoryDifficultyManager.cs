
using System;
using UnityEngine;

public class FactoryDifficultyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FactoryCreditManager creditManager;

    [Header("Difficulty Settings")]
    [SerializeField] private int level2Threshold = 600;

    [SerializeField, Range(0f, 1f)]
    private float level1DefectProbability = 0.2f;

    [SerializeField, Range(0f, 1f)]
    private float level2DefectProbability = 0.4f;

    [SerializeField]
    private int level2WrongRejectionPenalty = 10;

    public int CurrentLevel { get; private set; } = 1;

    public float CurrentDefectProbability =>
        CurrentLevel == 1
            ? level1DefectProbability
            : level2DefectProbability;

    public int WrongRejectionPenalty =>
        CurrentLevel == 1
            ? 0
            : level2WrongRejectionPenalty;

    public event Action<int> OnLevelChanged;

    private void Awake()
    {
        if (creditManager == null)
            creditManager =
                GetComponentInParent<FactoryCreditManager>();
    }

    private void OnEnable()
    {
        if (creditManager != null)
            creditManager.OnCreditsChanged += CheckLevel;
    }

    private void Start()
    {
        if (creditManager != null)
            CheckLevel(creditManager.currentCredits);
    }

    private void OnDisable()
    {
        if (creditManager != null)
            creditManager.OnCreditsChanged -= CheckLevel;
    }

    private void CheckLevel(int credits)
    {
        if (CurrentLevel >= 2)
            return;

        if (credits < level2Threshold)
            return;

        CurrentLevel = 2;

        Debug.Log(
            "[Difficulty] LEVEL 2 UNLOCKED! " +
            "Defect probability increased to 40%."
        );

        OnLevelChanged?.Invoke(CurrentLevel);
    }

    public void ResetDifficulty()
    {
        CurrentLevel = 1;
        OnLevelChanged?.Invoke(CurrentLevel);
    }
}
