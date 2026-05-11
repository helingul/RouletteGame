using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MainScreenController : MonoBehaviour
{
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text winningAmountText;
    [SerializeField] private TMP_Text currentBetsText;
    [SerializeField] private TMP_Text totalSpinsText;

    [SerializeField] private Button spinButton;
    [SerializeField] private Button selectWinningNumberButton;

    private void Start()
    {
        RouletteEventBus.OnBalanceChanged += HandleBalanceChanged;
        RouletteEventBus.OnChipPlaced += HandleChipPlaced;
        RouletteEventBus.OnChipRemoved += HandleChipRemoved;
        RouletteEventBus.OnSpinStarted += HandleSpinStarted;
        RouletteEventBus.OnRoundResult += HandleRountResult;

        spinButton.onClick.AddListener(HandleSpinButtonClicked);

        Initialize();
    }
    private void OnDestroy()
    {
        RouletteEventBus.OnBalanceChanged -= HandleBalanceChanged;
        RouletteEventBus.OnChipPlaced -= HandleChipPlaced;
        RouletteEventBus.OnChipRemoved -= HandleChipRemoved;
        RouletteEventBus.OnSpinStarted -= HandleSpinStarted;
        RouletteEventBus.OnRoundResult -= HandleRountResult;

        spinButton.onClick.RemoveListener(HandleSpinButtonClicked);
    }

    private void Initialize()
    {
        balanceText.text = RouletteGameManager.Instance.Balance.ToString();
        totalSpinsText.text = RouletteGameManager.Instance.History.Count.ToString();
        winningAmountText.text = RouletteGameManager.Instance.TotalWin.ToString();
    }

    private void HandleBalanceChanged(int balance)
    {
        balanceText.text = balance.ToString();
    }

    private void HandleChipPlaced(Chip chip, BetSpot betSpot)
    {
        SetCurrentBets();
    }

    private void HandleChipRemoved(Chip chip, BetSpot betSpot)
    {
        SetCurrentBets();
    }

    private void SetCurrentBets()
    {
        currentBetsText.text = RouletteGameManager.Instance.GetTotalBetAmount().ToString();
    }

    private void HandleSpinStarted()
    {
        EnableSpinButtons(false);
    }

    private void HandleRountResult(int net)
    {
        totalSpinsText.text = RouletteGameManager.Instance.History.Count.ToString();
        winningAmountText.text = RouletteGameManager.Instance.TotalWin.ToString();

        EnableSpinButtons(true);
    }

    private void EnableSpinButtons(bool enabled)
    {
        spinButton.enabled = enabled;
        selectWinningNumberButton.enabled = enabled;
    }

    private void HandleSpinButtonClicked()
    {
        RouletteGameManager.Instance.StartSpin();
    }
}
