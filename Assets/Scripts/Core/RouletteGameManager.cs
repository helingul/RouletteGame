using RouletteGame.Bets;
using RouletteGame.Chip;
using RouletteGame.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteGame.Core
{
    using Chip = Chip.Chip;

    //////////////////////////////////////////////////////////////////////////
    /// Central coordinator for the roulette game.
    /// Manages game state transitions, betting logic, payout processing,
    /// and event coordination using multiple design patterns (State,
    /// Command, Observer, Singleton).
    //////////////////////////////////////////////////////////////////////////
    public class RouletteGameManager : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////
        // Singleton instance
        public static RouletteGameManager Instance { get; private set; }

        //////////////////////////////////////////////////////////////////////////

        // Inspector refs
        [Header("Settings")]
        [SerializeField] private int playerBalance = 1000;

        [Header("References")]
        [SerializeField] private RouletteTableLayout tableLayout;
        [SerializeField] private ChipTray chipTray;
        [SerializeField] private RouletteController rouletteController;

        //////////////////////////////////////////////////////////////////////////
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
        private readonly Dictionary<BetSpot, List<Chip>> activeBets = new();
        private readonly List<(BetSpot spot, int value)> lastRoundBets = new();
        private readonly List<int> history = new();

        //////////////////////////////////////////////////////////////////////////
        // Public properties
        public RouletteController RouletteController => rouletteController;
        public int PendingWinningNumber => pendingWinningNumber;
        public int Balance => playerBalance;
        public List<int> History => history;
        public int TotalWin { get; private set; } = 0;
       
        //////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
                return; 
            }

            Instance = this;

            InitializeStateMachine();
            
            SubscribeToEventBus();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEventBus();
            RouletteEventBus.ClearAllListeners();
        }

        private void Update() => currentState?.Tick(this);
        public bool IsBettingAllowed() => currentState is PlacingBetsState;

        public void SetWinningNumber(int number) => pendingWinningNumber = number;
        public bool IsValidNumber(int n) => n >= 0 && n <= 36;


        // Start game in betting state as default phase.
        private void InitializeStateMachine()  => TransitionTo(stateBetting);

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

        // Central state transition handler ensuring proper exit/enter calls.
        private void TransitionTo(IGameState next)
        {
            currentState?.Exit(this);
            currentState = next;
            currentState.Enter(this);
        }

        // Starts betting phase.
        public void StartBettingPhase()
        {
            if (currentState is SpinningWheelState) return;

            pendingWinningNumber = -1;

            TransitionTo(stateBetting);
        }

        // Closes betting, spins the wheel towards winningNumber.
        // Resolve winning number or generate random fallback before spinning.
        public void StartSpin()
        {
            if (!(currentState is PlacingBetsState)) return;

            pendingWinningNumber = IsValidNumber(pendingWinningNumber)
                ? pendingWinningNumber
                : Random.Range(0, 37);

            TransitionTo(stateSpinning);
        }

        // Calculate total win/loss for all active bets.
        public void ProcessPayouts()
        {
            int totalWin = 0;
            int totalBet = 0;

            // Iterate all bet spots and evaluate payout individually.
            foreach (var activeBetPair in activeBets)
            {
                BetSpot spot = activeBetPair.Key;
                int betAmount = spot.GetTotalBet();
                totalBet += betAmount;

                int result = spot.CalculatePayout(pendingWinningNumber);

                if (result > 0)
                {
                    totalWin += betAmount + result;
                    Debug.Log($"[Payout] {spot.SpotLabel}: +{result}");
                }
                else
                {
                    Debug.Log($"[Payout] {spot.SpotLabel}: -{betAmount}");
                }
            }

            int net = totalWin - totalBet;
            playerBalance += net;
            TotalWin += totalWin;

            history.Add(pendingWinningNumber);

            RouletteEventBus.RaiseRoundResult(net);
            RouletteEventBus.RaiseBalanceChanged(playerBalance);

            StartCoroutine(CleanupAndRestart());
        }

        // Command Pattern wrappers
        // Place a chip on a spot via command (supports Undo).
        public Result ExecutePlaceChip(Chip chip, BetSpot spot)
        {
            if (!IsBettingAllowed()) return Result.Failure;
            return invoker.Execute(new PlaceChipCommand(chip, spot));
        }

        // Remove a chip via command (supports Undo).
        public void ExecuteRemoveChip(Chip chip, BetSpot spot)
        {
            if (!IsBettingAllowed()) return;
            invoker.Execute(new RemoveChipCommand(chip, spot));
        }

        // Clear table via command (supports Undo).
        public void ExecuteClearTable()
        {
            if (!IsBettingAllowed()) return;
            invoker.Execute(new ClearTableCommand(tableLayout, chipTray));
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

        // Deduct chip value immediately when placed on table.
        private void HandleChipPlaced(Chip chip, BetSpot spot)
        {
            if (!activeBets.ContainsKey(spot))
            {
                activeBets[spot] = new List<Chip>();
            }

            if (!activeBets[spot].Contains(chip))
            {
                activeBets[spot].Add(chip);
            }

            playerBalance -= chip.Value;
           
            RouletteEventBus.RaiseBalanceChanged(playerBalance);
        }

        private void HandleChipRemoved(Chip chip, BetSpot spot)
        {
            if (activeBets.TryGetValue(spot, out var list))
            {
                list.Remove(chip);
            }

            playerBalance += chip.Value;
            
            RouletteEventBus.RaiseBalanceChanged(playerBalance);
        }

        private void HandleSpinFinished()
        {
            TransitionTo(stateResult);

            // Brief pause then payouts
            StartCoroutine(DelayedTransition(statePayouts, 1.5f));
        }
        public int GetTotalBetAmount()
        {
            int total = 0;
            foreach (var kvp in activeBets)
            {
                total += kvp.Key.GetTotalBet();
            }

            return total;
        }

        // Snapshot current bets for repeat feature before clearing table.
        private IEnumerator CleanupAndRestart()
        {
            // Snapshot bets for Repeat feature
            lastRoundBets.Clear();
            foreach (var kvp in activeBets)
            {
                foreach (var chip in kvp.Key.GetPlacedChips())
                {
                    lastRoundBets.Add((kvp.Key, chip.Value));
                }
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
            {
                spot.ClearAllChips();
            }

            activeBets.Clear();
        }
    }
}