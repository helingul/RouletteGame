using RouletteGame.Common;
using System.Collections.Generic;
using UnityEngine;
namespace RouletteGame.Bets
{
    //////////////////////////////////////////////////////////////////////////
    // Defines payout calculation behavior for roulette bets using
    // the Strategy Pattern. Each bet type can provide its own
    // payout logic without modifying existing systems. 
    //////////////////////////////////////////////////////////////////////////

    // Payout Strategy Interface
    public interface IPayoutStrategy
    {
        // Returns net result for this bet spot.
        // Positive  = player profit (stake already taken at bet time).
        // Negative  = loss amount (magnitude = total stake on spot).
        int Calculate(BetSpot spot, int winningNumber);
    }

    //////////////////////////////////////////////////////////////////////////
    //  Concrete strategies
    // Generic strategy: covers a fixed set of numbers.
    public class StandardPayoutStrategy : IPayoutStrategy
    {
        public int Calculate(BetSpot spot, int winningNumber)
        {
            bool win = false;
            foreach (int number in spot.CoveredNumbers)
            {
                if (number == winningNumber) 
                {
                    win = true;
                    break; 
                }
            }

            int stake = spot.GetTotalBet();

            // Return profit on win, otherwise return lost stake as negative.
            return win ? stake * spot.Payout : -stake;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // Factory that maps BetType to IPayoutStrategy 
    public static class PayoutStrategyFactory
    {
        private static readonly Dictionary<BetType, IPayoutStrategy> strategies
            = new Dictionary<BetType, IPayoutStrategy>();

        static PayoutStrategyFactory()
        {
            // All standard bet types share the same algorithm,
            // override specific ones here if rules ever differ.
            var standard = new StandardPayoutStrategy();

            foreach (BetType betType in System.Enum.GetValues(typeof(BetType)))
            {
                strategies[betType] = standard;
            }    
        }
        public static IPayoutStrategy Get(BetType betType)
        {
            if (strategies.TryGetValue(betType, out var strategy)) return strategy;

            Debug.LogWarning($"[PayoutStrategyFactory] No strategy for {betType}, using standard.");
           
            // Fallback prevents null strategy usage if a type was not registered.
            return new StandardPayoutStrategy();
        }
    }
}