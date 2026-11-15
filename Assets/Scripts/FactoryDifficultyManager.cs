
using System;
using UnityEngine;

public class FactoryDifficultyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FactoryCreditManager creditManager;

    [Header("Level Thresholds")]
    [SerializeField] private int level2Threshold = 600;
    [SerializeField] private int level3Threshold = 750;

    [Header("Defect Probabilities")]
    [SerializeField, Range(0f, 1f)]
    private float level1DefectProbability = 0.2f;

    [SerializeField, Range(0f, 1f)]
    private float level2DefectProbability = 0.4f;

    [Header("Level 3 Packing Failures")]
    [SerializeField, Range(0f, 1f)]
    private float level3UnpackedProbability = 0.2f;

    [Header("Penalties")]
    [SerializeField]
    private int level2WrongRejectionPenalty = 10;

    public int CurrentLevel { get; private set; } = 1;

    public float CurrentDefectProbability =>
        CurrentLevel == 1
            ? level1DefectProbability
            : level2DefectProbability;

    public float CurrentUnpackedProbability =>
        CurrentLevel >= 3
            ? level3UnpackedProbability
            : 0f;

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
        int targetLevel = CurrentLevel;

        if (credits >= level3Threshold)
            targetLevel = 3;
        else if (credits >= level2Threshold)
            targetLevel = Mathf.Max(targetLevel, 2);

        if (targetLevel <= CurrentLevel)
            return;

        // Trigger each newly unlocked level.
        while (CurrentLevel < targetLevel)
        {
            CurrentLevel++;

            Debug.Log(
                $"[Difficulty] LEVEL {CurrentLevel} UNLOCKED!"
            );

            OnLevelChanged?.Invoke(CurrentLevel);
        }
    }

    public void ResetDifficulty()
    {
        CurrentLevel = 1;

        Debug.Log(
            "[Difficulty] Reset to Level 1."
        );

        OnLevelChanged?.Invoke(CurrentLevel);
    }
}
