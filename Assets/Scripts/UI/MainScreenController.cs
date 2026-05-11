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


    [SerializeField] private TMP_Text messageText;

    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float visibleDuration = 3f;

    private Coroutine fadeRoutine;
    private void Awake()
    {
        SetAlpha(0);
    }


    private void Start()
    {
        RouletteEventBus.OnBalanceChanged += HandleBalanceChanged;
        RouletteEventBus.OnChipPlaced += HandleChipPlaced;
        RouletteEventBus.OnChipRemoved += HandleChipRemoved;
        RouletteEventBus.OnBetExceedsBalance += HandleBetExceedsBalance;
        RouletteEventBus.OnChipTrayFull += HandleChipTrayFull;
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
        RouletteEventBus.OnBetExceedsBalance -= HandleBetExceedsBalance;
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
    private void HandleBetExceedsBalance()
    {
        ShowWarning("Bet exceeds balance");
    }
    private void HandleChipTrayFull()
    {
        ShowWarning("Chip tray capacity is reached");
    }

    private void ShowWarning(string warning)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(ShowWarningCoroutine(warning));
    }
    private IEnumerator ShowWarningCoroutine(string message)
    {
        messageText.text = message;

        yield return Fade(0, 1);

        yield return new WaitForSeconds(visibleDuration);

        yield return Fade(1, 0);
    }

    private IEnumerator Fade(float from, float to)
    {
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t = time / fadeDuration;

            SetAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        Color color = messageText.color;
        color.a = alpha;
        messageText.color = color;
    }
}
