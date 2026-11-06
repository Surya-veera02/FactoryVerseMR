
using UnityEngine;

public class FactoryPurchaseInfo : MonoBehaviour
{
    public int PurchasePrice { get; private set; }

    private bool refunded;

    public void Initialize(int price)
    {
        PurchasePrice = Mathf.Max(0, price);
        refunded = false;
    }

    public bool TryRefund(FactoryCreditManager credits)
    {
        if (refunded || credits == null)
            return false;

        refunded = true;
        credits.AddCredits(PurchasePrice);

        Debug.Log(
            $"[Shop] Refunded {PurchasePrice} credits for {name}."
        );

        return true;
    }
}
