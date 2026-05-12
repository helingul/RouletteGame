using RouletteGame.Bets;
using RouletteGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteGame.UI
{
    using Chip = Chip.Chip;

    ///////////////////////////////////////////////////////////////////////////
    // Controls the main UI screen of the Roulette game.
    // Responsible for updating balance, bets, winnings, and spin statistics.
    // Also manages spin interactions and listens to core game events to
    // keep UI state synchronized.
    ///////////////////////////////////////////////////////////////////////////
    public class MainScreenController : MonoBehaviour
    {
        ///////////////////////////////////////////////////////////////////////////
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private TMP_Text winningAmountText;
        [SerializeField] private TMP_Text currentBetsText;
        [SerializeField] private TMP_Text totalSpinsText;

        [SerializeField] private Button spinButton;
        [SerializeField] private Button selectWinningNumberButton;

        ///////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            RouletteEventBus.OnBalanceChanged += HandleBalanceChanged;
            RouletteEventBus.OnChipPlaced += HandleChipPlaced;
            RouletteEventBus.OnChipRemoved += HandleChipRemoved;
            RouletteEventBus.OnSpinStarted += HandleSpinStarted;
            RouletteEventBus.OnRoundResult += HandleRoundResult;

            spinButton.onClick.AddListener(HandleSpinButtonClicked);

            Initialize();
        }
        private void OnDisable()
        {
            RouletteEventBus.OnBalanceChanged -= HandleBalanceChanged;
            RouletteEventBus.OnChipPlaced -= HandleChipPlaced;
            RouletteEventBus.OnChipRemoved -= HandleChipRemoved;
            RouletteEventBus.OnSpinStarted -= HandleSpinStarted;
            RouletteEventBus.OnRoundResult -= HandleRoundResult;

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

        private void HandleRoundResult(int net)
        {
            totalSpinsText.text = RouletteGameManager.Instance.History.Count.ToString();
            winningAmountText.text = RouletteGameManager.Instance.TotalWin.ToString();

            EnableSpinButtons(true);
        }

        private void EnableSpinButtons(bool state)
        {
            spinButton.interactable = state;
            selectWinningNumberButton.interactable = state;
        }

        private void HandleSpinButtonClicked()
        {
            RouletteGameManager.Instance.StartSpin();
        }
    }
}