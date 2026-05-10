using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MainScreenController : MonoBehaviour
{
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button startWithRandomNumberButton;

    [SerializeField] private TMP_InputField inputNumber;

    private int inputRouletteNumber;

    private void Start()
    {
        settingsButton.onClick.AddListener(OnSettingsClicked);
        startButton.onClick.AddListener(OnStartClicked);
        startWithRandomNumberButton.onClick.AddListener(OnStartWithRandomNumberClicked);

        inputNumber.onValueChanged.AddListener(OnInputNumberChanged);
    }
    private void OnDestroy()
    {
        settingsButton.onClick.RemoveListener(OnSettingsClicked);
        startButton.onClick.RemoveListener(OnStartClicked);
        startWithRandomNumberButton.onClick.RemoveListener(OnStartWithRandomNumberClicked);

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
    private void OnStartClicked()
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

        //RouletteGameManager.Instance.StartBettingPhase(inputRouletteNumber);

        DisableScreen();
    }
    private void OnStartWithRandomNumberClicked()
    {
        if (RouletteGameManager.Instance == null)
        {
            Debug.Log($"[MainScreenController] RouletteGameManager is not valid.");
            return;
        }

        //int randomNumber = RouletteGameManager.Instance.GenerateRandomNumber();

        //RouletteGameManager.Instance.StartBettingPhase(randomNumber);

        DisableScreen();
    }

    private void DisableScreen()
    {
        gameObject.SetActive(false);
    }

}
