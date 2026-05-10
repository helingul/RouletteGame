//////////////////////////////////////////////////////////////////////////
//  Represents a single clickable area on the roulette table.
//  Uses the strategy pattern for payout calculation (delegates
//  to IPayoutStrategy from PayoutStrategyFactory).
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

public class BetSpot : MonoBehaviour
{
    // Serialized config information
    [Header("Bet Info")]
    public BetType betType;
    [SerializeField] private int[] coveredNumbers;
    [SerializeField] private int payout;
    public string spotLabel;

    [Header("Snap Settings")]
    public float snapRadius = 0.3f;
    public Transform chipAnchorPoint;
    public bool allowMultipleChips = true;

    [Header("Visual")]
    public GameObject highlightObject;
    public Renderer spotRenderer;
    public Color normalColor = new Color(0f, 0.5f, 0f, 0.3f);
    public Color hoverColor = new Color(1f, 1f, 0f, 0.5f);

    [SerializeField] private float stackHeightStep = 0.025f;

    // Runtime states
    [SerializeField] private List<RouletteChip> placedChips = new List<RouletteChip>();
    private bool isHighlighted;

    // Strategy (set once on first access)
    private IPayoutStrategy payoutStrategy;
    private IPayoutStrategy PayoutStrategy
        => payoutStrategy ??= PayoutStrategyFactory.Get(betType);

    // Public properties
    public int[] CoveredNumbers => coveredNumbers;
    public int Payout => payout;
    public int ChipCount => placedChips.Count;
    public bool HasChips => placedChips.Count > 0;

    // Config setters (called by RouletteTableLayout)
    public void Configure(BetType type, int[] numbers, int payoutMultiplier, string label)
    {
        betType = type;
        coveredNumbers = numbers;
        payout = payoutMultiplier;
        spotLabel = label;
        payoutStrategy = null;   // reset so it re-resolves
    }

    // Unity lifecycle
    private void Awake()
    {
        if (chipAnchorPoint == null)
        {
            Debug.LogError("[BetSpot] chipAnchorPoint not set – using own transform.");
            chipAnchorPoint = transform;
        }
        SetupVisual();
    }

    // Public API

    public List<RouletteChip> GetPlacedChips() => placedChips;

    public bool CanAcceptChip(RouletteChip chip)
        => allowMultipleChips || placedChips.Count == 0;

    public Result PlaceChip(RouletteChip chip)
    {
        if (!CanAcceptChip(chip))
        {
            Debug.LogWarning($"[BetSpot] Cannot place chip on {spotLabel}.");
            return Result.Failure;
        }

        placedChips.Add(chip);

        float stackOffset = (placedChips.Count - 1) * stackHeightStep;
        Vector3 targetPos = chipAnchorPoint.position + Vector3.up * stackOffset;

        chip.SnapToPosition(targetPos, chipAnchorPoint.rotation);
        chip.SetCurrentSpot(this);

        UpdateVisual();

        // Notify via event bus
        RouletteEventBus.RaiseChipPlaced(chip, this);

        return Result.Success;
    }

    public void RemoveChip(RouletteChip chip)
    {
        if (!placedChips.Remove(chip)) return;

        UpdateVisual();
        RouletteEventBus.RaiseChipRemoved(chip, this);
    }

    public void ClearAllChips()
    {
        // Iterate copy to avoid mutating during loop
        var copy = new List<RouletteChip>(placedChips);
        foreach (var chip in copy)
        {
            if (chip != null) chip.ReturnToTray();
        }
        
        placedChips.Clear();
        
        UpdateVisual();
    }

    public int GetTotalBet()
    {
        int total = 0;
        
        foreach (var chip in placedChips)
        {
            if (chip != null)
            {
                total += chip.Value;
            }
        }
           
        return total;
    }


    // Delegates payout calculation to the assigned strategy.
    // Returns net result: positive = win profit, negative = loss.
    public int CalculatePayout(int winningNumber)
        => PayoutStrategy.Calculate(this, winningNumber);

    public void SetHighlight(bool active)
    {
        isHighlighted = active;
        if (highlightObject != null) highlightObject.SetActive(active);
        UpdateVisual();
    }

    // Private helpers
    private Result SetupVisual()
    {
        if (spotRenderer == null)
        {
            Debug.LogError($"[BetSpot] spotRenderer not set on {name}.");
            return Result.Failure;
        }
        
        spotRenderer.material.color = normalColor;
        
        return Result.Success;
    }

    private void UpdateVisual()
    {
        if (spotRenderer == null) return;
        spotRenderer.material.color = isHighlighted ? hoverColor : normalColor;
    }
}