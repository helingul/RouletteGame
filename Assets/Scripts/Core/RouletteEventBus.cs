using System;

public static class RouletteEventBus
{
    // Fired whenever the player's balance changes. Sends new balance.
    public static event Action<int> OnBalanceChanged;

    // Fired when the betting phase opens.
    public static event Action OnBettingStarted;

    // Fired when betting is closed (wheel starts).
    public static event Action OnBettingEnded;

    // Fired when a chip is placed on a bet spot.
    public static event Action<RouletteChip, BetSpot> OnChipPlaced;

    // Fired when a chip is removed from a bet spot.
    public static event Action<RouletteChip, BetSpot> OnChipRemoved;

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

    //  Raise helpers

    public static void RaiseBalanceChanged(int newBalance) => OnBalanceChanged?.Invoke(newBalance);
    public static void RaiseBettingStarted() => OnBettingStarted?.Invoke();
    public static void RaiseBettingEnded() => OnBettingEnded?.Invoke();
    public static void RaiseChipPlaced(RouletteChip c, BetSpot s) => OnChipPlaced?.Invoke(c, s);
    public static void RaiseChipRemoved(RouletteChip c, BetSpot s) => OnChipRemoved?.Invoke(c, s);
    public static void RaiseWinningNumber(int n) => OnWinningNumberDetermined?.Invoke(n);
    public static void RaiseRoundResult(int net) => OnRoundResult?.Invoke(net);
    public static void RaiseGameStateChanged(string name) => OnGameStateChanged?.Invoke(name);
    public static void RaiseSpinStarted() => OnSpinStarted?.Invoke();
    public static void RaiseSpinFinished() => OnSpinFinished?.Invoke();

    // Cleanup (call on scene unload)
    public static void ClearAllListeners()
    {
        OnBalanceChanged = null;
        OnBettingStarted = null;
        OnBettingEnded = null;
        OnChipPlaced = null;
        OnChipRemoved = null;
        OnWinningNumberDetermined = null;
        OnRoundResult = null;
        OnGameStateChanged = null;
        OnSpinStarted = null;
        OnSpinFinished = null;
    }
}