using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SelectWinningNumberController : MonoBehaviour
{
    [SerializeField] private GameObject screen;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button openButton;
    [SerializeField] private Button selectButton;

    [SerializeField] private TMP_InputField inputNumber;

    private int inputRouletteNumber;

    private void Start()
    {
        closeButton.onClick.AddListener(DisableScreen);
        openButton.onClick.AddListener(EnableScreen);
        selectButton.onClick.AddListener(OnSelectNumberClicked);

        inputNumber.onValueChanged.AddListener(OnInputNumberChanged);
    }
    private void OnDestroy()
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
            Debug.Log($"[MainScreenController] Input number is invalid. {value}");
            return false;
        }

        // Check for valid numbers
        if (RouletteGameManager.Instance.IsValidNumber(number) == false)
        {
            Debug.Log($"[MainScreenController] Input number is invalid. {value}");
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

    private void OnSettingsClicked()
    {

    }
    private void OnSelectNumberClicked()
    {

        if (RouletteGameManager.Instance == null)
        {
            Debug.Log($"[MainScreenController] RouletteGameManager is not valid.");
            return;
        }

        // Check for valid numbers
        if (RouletteGameManager.Instance.IsValidNumber(inputRouletteNumber) == false)
        {
            Debug.Log($"[MainScreenController] Input number is invalid. {inputRouletteNumber}");
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
