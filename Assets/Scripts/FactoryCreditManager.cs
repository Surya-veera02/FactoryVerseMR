
using System;
using UnityEngine;

public class FactoryCreditManager : MonoBehaviour
{
    [Header("Starting Credits")]
    [Min(0)]
    public int startingCredits = 500;

    [Header("Current Credits - Testing")]
    [Min(0)]
    public int currentCredits;

    [Header("Shop Prices")]
    [Min(0)] public int straightConveyorCost = 50;
    [Min(0)] public int cornerConveyorCost = 60;
    [Min(0)] public int packingMachineCost = 150;

    [Header("Package Economy")]
    [Min(0)] public int productionCost = 5;
    [Min(0)] public int successfulDeliveryReward = 20;
    [Min(0)] public int defectiveDeliveryPenalty = 40;

    public event Action<int> OnCreditsChanged;

    private void Awake()
    {
        currentCredits = startingCredits;
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && currentCredits >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (!CanAfford(amount))
        {
            Debug.LogWarning(
                $"[Credits] Cannot afford {amount} CR. " +
                $"Balance: {currentCredits} CR"
            );

            return false;
        }

        currentCredits -= amount;
        NotifyChanged();

        Debug.Log(
            $"[Credits] Spent {amount} CR. " +
            $"Balance: {currentCredits} CR"
        );

        return true;
    }

    public void AddCredits(int amount)
    {
        if (amount <= 0)
            return;

        currentCredits += amount;
        NotifyChanged();

        Debug.Log(
            $"[Credits] Added {amount} CR. " +
            $"Balance: {currentCredits} CR"
        );
    }

    [ContextMenu("Reset Credits To Starting Balance")]
    public void ResetCredits()
    {
        currentCredits = startingCredits;
        NotifyChanged();

        Debug.Log("[Credits] Economy reset.");
    }

    [ContextMenu("Test: Add 100 Credits")]
    private void TestAddCredits()
    {
        AddCredits(100);
    }

    [ContextMenu("Test: Spend 50 Credits")]
    private void TestSpendCredits()
    {
        TrySpend(50);
    }

    private void NotifyChanged()
    {
        OnCreditsChanged?.Invoke(currentCredits);
    }
}
