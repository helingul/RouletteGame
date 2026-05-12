using RouletteGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteGame.UI
{
    ///////////////////////////////////////////////////////////////////////////
    // Handles the UI logic for chip selection and table control.
    // Responsible for raising chip add events, and managing undo/clear actions.
    // Also enables or disables betting UI based on game state events
    // (spin start and round result).
    ///////////////////////////////////////////////////////////////////////////
    public class ChipController : MonoBehaviour
    {
        ///////////////////////////////////////////////////////////////////////////
        [System.Serializable]
        private class ChipButton
        {
            public Button button;
            public int value;
        }

        ///////////////////////////////////////////////////////////////////////////
        
        [Header("Chip Buttons")]
        [SerializeField] private ChipButton[] chipButtons;

        [Header("Actions")]
        [SerializeField] private Button undoButton;
        [SerializeField] private Button clearButton;

        ///////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            foreach (var chip in chipButtons)
            {
                chip.button.onClick.AddListener(() => OnChipClicked(chip.value));
            }

            undoButton.onClick.AddListener(UndoLastChip);
            clearButton.onClick.AddListener(ClearChips);

            RouletteEventBus.OnSpinStarted += HandleSpinStarted;
            RouletteEventBus.OnRoundResult += HandleRoundResult;

        }
        private void OnDisable()
        {
            foreach (var chip in chipButtons)
            {
                chip.button.onClick.RemoveAllListeners();
            }

            undoButton.onClick.RemoveListener(UndoLastChip);
            clearButton.onClick.RemoveListener(ClearChips);

            RouletteEventBus.OnSpinStarted -= HandleSpinStarted;
            RouletteEventBus.OnRoundResult -= HandleRoundResult;
        }
        private void OnChipClicked(int value)
        {
            RouletteEventBus.RaiseChipAdded(value);
        }
        private void UndoLastChip()
        {
            RouletteGameManager.Instance.UndoLastBet();
        }

        private void ClearChips()
        {
            RouletteGameManager.Instance.ExecuteClearTable();
        }

        private void HandleSpinStarted()
        {
            SetInteractable(false);
        }

        private void HandleRoundResult(int net)
        {
            SetInteractable(true);
        }

        private void SetInteractable(bool state)
        {
            foreach (var chip in chipButtons)
            {
                chip.button.interactable = state;
            }

            undoButton.interactable = state;
            clearButton.interactable = state;
        }
    }
}