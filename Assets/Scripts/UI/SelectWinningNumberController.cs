using RouletteGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteGame.UI
{
    ///////////////////////////////////////////////////////////////////////////
    // Controls the UI screen for manually selecting the roulette winning number.
    // Handles input validation, screen visibility, and submits the selected
    // number to the game manager.
    // Ensures only valid roulette numbers can be applied as the forced winning result.
    ///////////////////////////////////////////////////////////////////////////
    public class SelectWinningNumberController : MonoBehaviour
    {
        [SerializeField] private GameObject screen;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button openButton;
        [SerializeField] private Button selectButton;

        [SerializeField] private TMP_InputField inputNumber;
       
        ///////////////////////////////////////////////////////////////////////////
        private int inputRouletteNumber;

        ///////////////////////////////////////////////////////////////////////////
        private void OnEnable()
        {
            DisableScreen();

            closeButton.onClick.AddListener(DisableScreen);
            openButton.onClick.AddListener(EnableScreen);
            selectButton.onClick.AddListener(OnSelectNumberClicked);

            inputNumber.onValueChanged.AddListener(OnInputNumberChanged);
        }
        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(DisableScreen);
            openButton.onClick.RemoveListener(EnableScreen);
            selectButton.onClick.RemoveListener(OnSelectNumberClicked);

            inputNumber.onValueChanged.RemoveListener(OnInputNumberChanged);
        }

        private bool ValidateInputNumber(string value, out int number)
        {
            number = -1;

            if (string.IsNullOrWhiteSpace(value)) return false;

            if (!int.TryParse(value, out number))
            {
                Debug.Log($"[SelectWinningNumberController] Input number is invalid. {value}");
                return false;
            }

            // Check for valid numbers
            if (RouletteGameManager.Instance.IsValidNumber(number) == false)
            {
                Debug.Log($"[SelectWinningNumberController] Input number is invalid. {value}");
                return false;
            }

            return true;
        }
        private void OnInputNumberChanged(string value)
        {
            if (ValidateInputNumber(value, out inputRouletteNumber) == false)
            {
                inputNumber.text = "Invalid";
            }
        }
        private void OnSelectNumberClicked()
        {

            if (RouletteGameManager.Instance == null)
            {
                Debug.Log($"[SelectWinningNumberController] RouletteGameManager is not valid.");
                return;
            }

            // Check for valid numbers
            if (RouletteGameManager.Instance.IsValidNumber(inputRouletteNumber) == false)
            {
                Debug.Log($"[SelectWinningNumberController] Input number is invalid. {inputRouletteNumber}");
                return;
            }

            RouletteGameManager.Instance.SetWinningNumber(inputRouletteNumber);
            DisableScreen();
        }

        private void DisableScreen()
        {
            screen.SetActive(false);
        }
        private void EnableScreen()
        {
            screen.SetActive(true);
        }

    }
}