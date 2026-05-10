//////////////////////////////////////////////////////////////////////////
//  ScriptableObject – stores payout multipliers per BetType.
//  Used by PayoutStrategyFactory
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PayoutList", menuName = "Roulette/Payout List")]
public class PayoutList : ScriptableObject
{
    [Serializable]
    private struct PayoutEntry
    {
        public BetType BetType;
        public int PayoutMultiplier;
    }

    [SerializeField] private List<PayoutEntry> payouts = new List<PayoutEntry>();

    private int[] payoutTable;

    private void OnEnable()
    {
        int count = Enum.GetValues(typeof(BetType)).Length;
        payoutTable = new int[count];

        foreach (var p in payouts)
            payoutTable[(int)p.BetType] = p.PayoutMultiplier;
    }

    public int GetMultiplier(BetType betType)
    {
        if (payoutTable == null) OnEnable();
        return payoutTable[(int)betType];
    }
}