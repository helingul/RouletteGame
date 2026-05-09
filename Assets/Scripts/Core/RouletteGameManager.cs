using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class RouletteGameManager : MonoBehaviour
{
    public enum GameState
    {
        WaitingToStart,
        PlacingBets,
        SpinningWheel,
        ShowingResult,
        Payouts
    }

    public GameState currentState = GameState.WaitingToStart;

    // TODO: Made Singleton for testings
    public static RouletteGameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    [Header("Settings")]
    public int playerBalance = 1000;
    public int minBet = 1;
    public int maxBet = 500;

    [Header("Game References")]
    public RouletteTableLayout tableLayout;
    public ChipTray chipTray;

    [Header("Events")]
    public UnityEvent<int> OnBalanceChanged;
    public UnityEvent<int> OnWinningNumber;
    public UnityEvent<int> OnRoundResult;
    public UnityEvent OnBettingStarted;
    public UnityEvent OnBettingEnded;

    private Dictionary<BetSpot, List<RouletteChip>> activeBets
        = new Dictionary<BetSpot, List<RouletteChip>>();

    private int lastWinningNumber = -1;
    private List<int> history = new List<int>();
    public int LastWinningNumber => lastWinningNumber;
    public List<int> History => history;
    public int Balance => playerBalance;

    public void StartBettingPhase()
    {
        if (currentState == GameState.SpinningWheel) return;

        currentState = GameState.PlacingBets;
        OnBettingStarted?.Invoke();
    }
    int DetermineWinningNumber()
    {
        return Random.Range(0, 37);
    }
    void ProcessPayouts(int winningNumber)
    {
        currentState = GameState.Payouts;

        int totalWin = 0;
        int totalBet = 0;

        foreach (var kvp in activeBets)
        {
            BetSpot spot = kvp.Key;
            int betAmount = spot.GetTotalBet();
            totalBet += betAmount;

            int result = spot.CalculatePayout(winningNumber);

            if (result > 0)
            {
                totalWin += betAmount + result;
                Debug.Log($"{spot.spotLabel}: +{result} won");
            }
            else
            {
                Debug.Log($"{spot.spotLabel}: -{betAmount} lost");
            }
        }

        int netResult = totalWin - totalBet;
        playerBalance += netResult;

        OnRoundResult?.Invoke(netResult);
        OnBalanceChanged?.Invoke(playerBalance);

        StartCoroutine(CleanupAndRestart());
    }

    IEnumerator CleanupAndRestart()
    {
        yield return new WaitForSeconds(2f);

        ClearAllBets();

        currentState = GameState.PlacingBets;
        OnBettingStarted?.Invoke();
    }

    public void OnChipPlaced(RouletteChip chip, BetSpot spot)
    {
        if (!activeBets.ContainsKey(spot))
            activeBets[spot] = new List<RouletteChip>();

        if (!activeBets[spot].Contains(chip))
            activeBets[spot].Add(chip);

        playerBalance -= chip.Value;
        OnBalanceChanged?.Invoke(playerBalance);
    }

    public void OnChipRemoved(RouletteChip chip, BetSpot spot)
    {
        if (activeBets.ContainsKey(spot))
            activeBets[spot].Remove(chip);

        playerBalance += chip.Value;
        OnBalanceChanged?.Invoke(playerBalance);
    }

    public bool IsBettingAllowed()
    {
        // TODO: Returned true for testing
        return true; //currentState == GameState.PlacingBets; 
    }

    public int GetTotalBetAmount()
    {
        int total = 0;
        foreach (var kvp in activeBets)
            total += kvp.Key.GetTotalBet();
        return total;
    }

    public void ClearAllBets()
    {
        if (tableLayout == null) return;
        foreach (var spot in tableLayout.allSpots)
            spot.ClearAllChips();
        activeBets.Clear();
    }
    public void RepeatLastBets()
    {
    }
}
