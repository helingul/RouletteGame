using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class WarningTextController : MonoBehaviour
{
    [SerializeField] private TMP_Text warningText;

    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float visibleDuration = 3f;

    private Coroutine fadeRoutine;
    private void Awake()
    {
        SetAlpha(0);
    }
    private void Start()
    {
        RouletteEventBus.OnBetExceedsBalance += HandleBetExceedsBalance;
        RouletteEventBus.OnChipTrayFull += HandleChipTrayFull;
    }
    private void OnDestroy()
    {
        RouletteEventBus.OnBetExceedsBalance -= HandleBetExceedsBalance;
        RouletteEventBus.OnChipTrayFull -= HandleChipTrayFull;
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
        warningText.text = message;

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
        Color color = warningText.color;
        color.a = alpha;
        warningText.color = color;
    }
}
