//////////////////////////////////////////////////////////////////////////
//  STRATEGY PATTERN – Each BetType has its own payout strategy.
//  Add new bet types by adding a new class; zero existing code
//  changes required.
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

// Payout Strategy Interface
public interface IPayoutStrategy
{
    // Returns net result for this bet spot.
    // Positive  = player profit (stake already taken at bet time).
    // Negative  = loss amount (magnitude = total stake on spot).
    int Calculate(BetSpot spot, int winningNumber);
}

//  Concrete strategies

// Generic strategy: covers a fixed set of numbers.
public class StandardPayoutStrategy : IPayoutStrategy
{
    public int Calculate(BetSpot spot, int winningNumber)
    {
        bool win = false;
        foreach (int n in spot.CoveredNumbers)
        {
            if (n == winningNumber) { win = true; break; }
        }

        int stake = spot.GetTotalBet();
        return win ? stake * spot.Payout : -stake;
    }
}

// Factory that maps BetType to IPayoutStrategy 
public static class PayoutStrategyFactory
{
    private static readonly Dictionary<BetType, IPayoutStrategy> strategies
        = new Dictionary<BetType, IPayoutStrategy>();

    static PayoutStrategyFactory()
    {
        // All standard bet types share the same algorithm;
        // override specific ones here if rules ever differ.
        var standard = new StandardPayoutStrategy();

        foreach (BetType bt in System.Enum.GetValues(typeof(BetType)))
            strategies[bt] = standard;
    }

    public static IPayoutStrategy Get(BetType betType)
    {
        if (strategies.TryGetValue(betType, out var s)) return s;
        
        Debug.LogWarning($"[PayoutStrategyFactory] No strategy for {betType}, using standard.");
        
        return new StandardPayoutStrategy();
    }
}