using System.Collections.Generic;
using UnityEngine;

// TODO: Add an interface for exposing logic
public class BetSpot : MonoBehaviour
{
    [Header("Bet Info")]
    public BetType betType;
    public int[] coveredNumbers;
    public int payout;
    public string spotLabel;

    [Header("Snap Settings")]
    public float snapRadius = 0.3f;
    public Transform chipAnchorPoint;
    public bool allowMultipleChips = true;

    [Header("Visual")]
    public GameObject highlightObject;
    public Renderer spotRenderer;
    public Color normalColor = new Color(0, 0.5f, 0, 0.3f);
    public Color hoverColor = new Color(1, 1, 0, 0.5f);


    [SerializeField]private List<RouletteChip> placedChips = new List<RouletteChip>();
    private bool isHighlighted = false;

    [SerializeField] private float stackHeightStep = 0.025f;

    public List<RouletteChip> GetPlacedChips() => placedChips;
    public int ChipCount => placedChips.Count;
    public bool HasChips => placedChips.Count > 0;


    private void Awake()
    {
        // If chipAnchorPoint is not assigned use object tranform.
        if (chipAnchorPoint == null)
        {
            Debug.LogError($"[BetSpot] Chip anchor point is not set.");
            chipAnchorPoint = transform;
        }
           

        SetupVisual();
    }

    private Result SetupVisual()
    {
        if (spotRenderer == null)
        {
            Debug.LogError($"[BetSpot] Failed to setup visual. Spot renderer is not set.");
            return Result.Failure;
        }

        spotRenderer.material.color = normalColor;
        return Result.Success;
    }

    // PUBLIC API
    public bool CanAcceptChip(RouletteChip chip)
    {
        return allowMultipleChips || placedChips.Count == 0;
    }

    public Result PlaceChip(RouletteChip chip)
    {
        if (!CanAcceptChip(chip))
        {
            Debug.LogError($"[BetSpot] Failed to place chip. " +
                $"AllowMultipleChips: {allowMultipleChips} placedChipCount: {placedChips.Count}.");

            return Result.Failure;
        }

        placedChips.Add(chip);

        // Calculate chip offset
        float stackOffset = (placedChips.Count - 1) * stackHeightStep;
        Vector3 targetPos = chipAnchorPoint.position + Vector3.up * stackOffset;

        chip.SnapToPosition(targetPos, chipAnchorPoint.rotation);
        chip.SetCurrentSpot(this);

        UpdateVisual();

        RouletteGameManager.Instance?.OnChipPlaced(chip, this);

        return Result.Success;
    }

    public void RemoveChip(RouletteChip chip)
    {
        placedChips.Remove(chip);
        UpdateVisual();
        RouletteGameManager.Instance?.OnChipRemoved(chip, this);
    }
    public void ClearAllChips()
    {
        foreach (var chip in placedChips)
        {
            if (chip != null)
                chip.ReturnToTray();
        }
        placedChips.Clear();
        UpdateVisual();
    }

    public int GetTotalBet()
    {
        int total = 0;
        foreach (var chip in placedChips)
            if (chip != null) total += chip.Value;
        return total;
    }

    public int CalculatePayout(int winningNumber)
    {
        bool isWinner = false;
        foreach (int num in coveredNumbers)
        {
            if (num == winningNumber)
            {
                isWinner = true;
                break;
            }
        }

        if (!isWinner) return -GetTotalBet(); // Loss
        return GetTotalBet() * payout;        // Win/Profit
    }

    public void SetHighlight(bool active)
    {
        isHighlighted = active;
        UpdateVisual();

        if (highlightObject != null)
            highlightObject.SetActive(active);
    }

    private void UpdateVisual()
    {
        if (spotRenderer == null) return;

        if (isHighlighted)
            spotRenderer.material.color = hoverColor;
        else
            spotRenderer.material.color = normalColor;
    }
}
