//////////////////////////////////////////////////////////////////////////
//  Central game coordinator.
//
//  Design patterns used here:
//  • STATE PATTERN   – delegates phase logic to IGameState classes
//  • COMMAND PATTERN – all bet actions go through BetCommandInvoker
//  • OBSERVER        – publishes/subscribes via RouletteEventBus
//  • SINGLETON       – one instance per scene
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteGameManager : MonoBehaviour
{
    // Singleton instance
    public static RouletteGameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializeStateMachine();
        SubscribeToEventBus();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEventBus();
        RouletteEventBus.ClearAllListeners();
    }

    // Inspector refs
    [Header("Settings")]
    public int playerBalance = 1000;
    public int minBet = 1;
    public int maxBet = 500;

    [Header("References")]
    public RouletteTableLayout tableLayout;
    public ChipTray chipTray;
    public RouletteController RouletteController;

    // Holds current game state
    private IGameState currentState;

    // Pre-allocated states to avoid GC
    private readonly WaitingToStartState stateWaiting = new WaitingToStartState();
    private readonly PlacingBetsState stateBetting = new PlacingBetsState();
    private readonly SpinningWheelState stateSpinning = new SpinningWheelState();
    private readonly ShowingResultState stateResult = new ShowingResultState();
    private readonly PayoutsState statePayouts = new PayoutsState();

    // Command invoker
    private readonly BetCommandInvoker invoker = new BetCommandInvoker();

    // Round data
    private int pendingWinningNumber = -1;
    private readonly Dictionary<BetSpot, List<RouletteChip>> activeBets = new();
    private readonly List<(BetSpot spot, int value)> lastRoundBets = new();
    private readonly List<int> history = new();

    // Public readonly accessors
    public int PendingWinningNumber => pendingWinningNumber;
    public int Balance => playerBalance;
    public List<int> History => history;
    public bool IsBettingAllowed() => currentState is PlacingBetsState;

    // Initialization
    private void InitializeStateMachine()
        => TransitionTo(stateWaiting);

    private void SubscribeToEventBus()
    {
        RouletteEventBus.OnChipPlaced += HandleChipPlaced;
        RouletteEventBus.OnChipRemoved += HandleChipRemoved;
        RouletteEventBus.OnSpinFinished += HandleSpinFinished;
    }

    private void UnsubscribeFromEventBus()
    {
        RouletteEventBus.OnChipPlaced -= HandleChipPlaced;
        RouletteEventBus.OnChipRemoved -= HandleChipRemoved;
        RouletteEventBus.OnSpinFinished -= HandleSpinFinished;
    }

    //  State machine
    private void TransitionTo(IGameState next)
    {
        currentState?.Exit(this);
        currentState = next;
        currentState.Enter(this);
    }

    private void Update() => currentState?.Tick(this);


    //  Public phase triggers (called by UI buttons.)

    // Starts betting phase.
    public void StartBettingPhase()
    {
        if (currentState is SpinningWheelState) return;
        TransitionTo(stateBetting);
    }

    // Closes betting, spins the wheel towards winningNumber.
    // Pass -1 to generate a random number.
    public void StartSpin(int winningNumber = -1)
    {
        if (!(currentState is PlacingBetsState)) return;

        pendingWinningNumber = winningNumber < 0
            ? Random.Range(0, 37)
            : winningNumber;

        TransitionTo(stateSpinning);
    }


    //  Payout (called by PayoutsState)
    public void ProcessPayouts()
    {
        int totalWin = 0;
        int totalBet = 0;

        foreach (var kvp in activeBets)
        {
            BetSpot spot = kvp.Key;
            int betAmount = spot.GetTotalBet();
            totalBet += betAmount;

            // Strategy pattern: spot delegates to its IPayoutStrategy
            int result = spot.CalculatePayout(pendingWinningNumber);

            if (result > 0)
            {
                totalWin += betAmount + result;
                Debug.Log($"[Payout] {spot.spotLabel}: +{result}");
            }
            else
            {
                Debug.Log($"[Payout] {spot.spotLabel}: -{betAmount}");
            }
        }

        int net = totalWin - totalBet;
        playerBalance += net;

        history.Add(pendingWinningNumber);

        RouletteEventBus.RaiseRoundResult(net);
        RouletteEventBus.RaiseBalanceChanged(playerBalance);

        StartCoroutine(CleanupAndRestart());
    }

    //  Command Pattern wrappers (UI calls these)

    // Place a chip on a spot via command (supports Undo).
    public void ExecutePlaceChip(RouletteChip chip, BetSpot spot)
    {
        if (!IsBettingAllowed()) return;
        invoker.Execute(new PlaceChipCommand(chip, spot));
    }

    // Remove a chip via command (supports Undo).
    public void ExecuteRemoveChip(RouletteChip chip, BetSpot spot)
    {
        if (!IsBettingAllowed()) return;
        invoker.Execute(new RemoveChipCommand(chip, spot));
    }

    // Clear table via command (supports Undo).
    public void ExecuteClearTable()
    {
        if (!IsBettingAllowed()) return;
        invoker.Execute(new ClearTableCommand(tableLayout, chipTray.Pool));
    }

    // Undo the last bet action.
    public void UndoLastBet()
    {
        if (!IsBettingAllowed()) return;
        invoker.Undo();
    }

    // Repeat last round's bets.
    public void RepeatLastBets()
    {
        if (!IsBettingAllowed()) return;
        if (lastRoundBets.Count == 0)
        {
            Debug.Log("[GameManager] No last-round bets to repeat.");
            return;
        }
        invoker.Execute(new RepeatBetsCommand(lastRoundBets, chipTray.Pool));
    }


    // EventBus handlers
    private void HandleChipPlaced(RouletteChip chip, BetSpot spot)
    {
        if (!activeBets.ContainsKey(spot))
            activeBets[spot] = new List<RouletteChip>();

        if (!activeBets[spot].Contains(chip))
            activeBets[spot].Add(chip);

        playerBalance -= chip.Value;
        RouletteEventBus.RaiseBalanceChanged(playerBalance);
    }

    private void HandleChipRemoved(RouletteChip chip, BetSpot spot)
    {
        if (activeBets.TryGetValue(spot, out var list))
            list.Remove(chip);

        playerBalance += chip.Value;
        RouletteEventBus.RaiseBalanceChanged(playerBalance);
    }

    private void HandleSpinFinished()
    {
        TransitionTo(stateResult);
        // Brief pause then payouts
        StartCoroutine(DelayedTransition(statePayouts, 1.5f));
    }

   
    //  Helpers
    public int GetTotalBetAmount()
    {
        int total = 0;
        foreach (var kvp in activeBets)
            total += kvp.Key.GetTotalBet();
        return total;
    }

    public bool IsValidNumber(int n) => n >= 0 && n <= 36;

    private IEnumerator CleanupAndRestart()
    {
        // Snapshot bets for Repeat feature
        lastRoundBets.Clear();
        foreach (var kvp in activeBets)
        {
            foreach (var chip in kvp.Key.GetPlacedChips())
                lastRoundBets.Add((kvp.Key, chip.Value));
        }

        yield return new WaitForSeconds(2f);

        ClearAllBets();
        invoker.ClearHistory();

        TransitionTo(stateBetting);
    }

    private IEnumerator DelayedTransition(IGameState next, float delay)
    {
        yield return new WaitForSeconds(delay);
        TransitionTo(next);
    }

    public void ClearAllBets()
    {
        if (tableLayout == null) return;
        foreach (var spot in tableLayout.AllSpots)
            spot.ClearAllChips();
        activeBets.Clear();
    }
}