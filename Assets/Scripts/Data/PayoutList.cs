using RouletteGame.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteGame.Data
{
    //////////////////////////////////////////////////////////////////////////
    //  ScriptableObject that stores payout multipliers per BetType.
    //  Used by PayoutStrategyFactory
    //////////////////////////////////////////////////////////////////////////

    [CreateAssetMenu(fileName = "PayoutList", menuName = "Roulette/Payout List")]
    public class PayoutList : ScriptableObject
    {
        // Serializable mapping between BetType and its payout multiplier.
        [Serializable]
        private struct PayoutEntry
        {
            public BetType BetType;
            public int PayoutMultiplier;
        }
       
        //////////////////////////////////////////////////////////////////////////
        // Inspector-defined payout configuration list.
        [SerializeField] private List<PayoutEntry> payouts = new List<PayoutEntry>();

        //////////////////////////////////////////////////////////////////////////
        private int[] payoutTable;

        //////////////////////////////////////////////////////////////////////////
        // Builds a lookup table for fast BetType -> multiplier access.
        // Converts serialized list into an indexed array for performance.
        private void OnEnable()
        {
            int count = Enum.GetValues(typeof(BetType)).Length;
            payoutTable = new int[count];

            foreach (var p in payouts)
                payoutTable[(int)p.BetType] = p.PayoutMultiplier;
        }
        private void EnsureInitialized()
        {
            if (payoutTable != null) return;
            OnEnable();
        }

        // Returns payout multiplier for given BetType.
        // Rebuilds table if needed.
        public int GetMultiplier(BetType betType)
        {
            EnsureInitialized();
            return payoutTable[(int)betType];
        }
    }
}