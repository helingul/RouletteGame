using RouletteGame.Bets;
using System;

namespace RouletteGame.Core
{
    using Chip = Chip.Chip;

    //////////////////////////////////////////////////////////////////////////
    // Global event bus for all gameplay.
    // Used for loose coupling between gameplay systems.
    //////////////////////////////////////////////////////////////////////////
    public static class RouletteEventBus
    {
        // Fired whenever the player's balance changes. Sends new balance.
        public static event Action<int> OnBalanceChanged;

        // Fired when the betting phase opens.
        public static event Action OnBettingStarted;

        // Fired when betting is closed (wheel starts).
        public static event Action OnBettingEnded;

        // Fired when a chip is placed on a bet spot.
        public static event Action<Chip, BetSpot> OnChipPlaced;

        // Fired when a chip is removed from a bet spot.
        public static event Action<Chip, BetSpot> OnChipRemoved;

        // Fired when a chip is added to tray. Sends chip value.
        public static event Action<int> OnChipAdded;

        // Fired when the winning number is determined.
        public static event Action<int> OnWinningNumberDetermined;

        //Fired after payouts. Sends net result (positive = win, negative = loss).
        public static event Action<int> OnRoundResult;

        // Fired when the game state changes. Sends new state name.
        public static event Action<string> OnGameStateChanged;

        // Fired when the wheel/ball spin begins.
        public static event Action OnSpinStarted;

        // Fired when the spin animation finishes.
        public static event Action OnSpinFinished;

        // Fired when a player tries to place a chip on a bet spot with insufficient balance
        public static event Action OnBetExceedsBalance;

        // Fired when chip tray capacity is reached.
        public static event Action OnChipTrayFull;


        //  Raise helpers
        public static void RaiseBalanceChanged(int newBalance) => OnBalanceChanged?.Invoke(newBalance);
        public static void RaiseBettingStarted() => OnBettingStarted?.Invoke();
        public static void RaiseBettingEnded() => OnBettingEnded?.Invoke();
        public static void RaiseChipPlaced(Chip c, BetSpot s) => OnChipPlaced?.Invoke(c, s);
        public static void RaiseChipRemoved(Chip c, BetSpot s) => OnChipRemoved?.Invoke(c, s);
        public static void RaiseChipAdded(int v) => OnChipAdded?.Invoke(v);
        public static void RaiseWinningNumber(int n) => OnWinningNumberDetermined?.Invoke(n);
        public static void RaiseRoundResult(int net) => OnRoundResult?.Invoke(net);
        public static void RaiseGameStateChanged(string name) => OnGameStateChanged?.Invoke(name);
        public static void RaiseSpinStarted() => OnSpinStarted?.Invoke();
        public static void RaiseSpinFinished() => OnSpinFinished?.Invoke();
        public static void RaiseBetExceedsBalance() => OnBetExceedsBalance?.Invoke();
        public static void RaiseChipTrayFull() => OnChipTrayFull?.Invoke();

        // Cleanup (called on scene unload)
        public static void ClearAllListeners()
        {
            OnBalanceChanged = null;
            OnBettingStarted = null;
            OnBettingEnded = null;
            OnChipPlaced = null;
            OnChipRemoved = null;
            OnChipAdded = null;
            OnWinningNumberDetermined = null;
            OnRoundResult = null;
            OnGameStateChanged = null;
            OnSpinStarted = null;
            OnSpinFinished = null;
            OnBetExceedsBalance = null;
            OnChipTrayFull = null;
        }
    }
}