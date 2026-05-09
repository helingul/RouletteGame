using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PayoutList", menuName = "Data/Payout List")]
public class PayoutList : ScriptableObject
{
    [Serializable]
    private struct Payout
    {
        public BetType BetType;
        public int PayoutMultiplier;
    }

    [SerializeField] private List<Payout> payouts;

    // Cached list for accessing payout multipliers
    private int[] payoutTable;

    private void OnEnable()
    {
        int enumCount = Enum.GetValues(typeof(BetType)).Length;

        payoutTable = new int[enumCount];

        foreach (var payout in payouts)
        {
            payoutTable[(int)payout.BetType] = payout.PayoutMultiplier;
        }
    }

    public int GetMultiplier(BetType betType)
    {
        return payoutTable[(int)betType];
    }
}