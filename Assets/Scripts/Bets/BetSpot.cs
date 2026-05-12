using RouletteGame.Common;
using RouletteGame.Core;
using System.Collections.Generic;
using UnityEngine;


namespace RouletteGame.Bets
{
    using Chip = Chip.Chip;

    //////////////////////////////////////////////////////////////////////////
    // Represents a single betting area on the roulette table.
    // Handles chip placement, stacking, highlighting, and payout
    // calculation through a strategy-based payout system..
    //////////////////////////////////////////////////////////////////////////
    public class BetSpot : MonoBehaviour
    {
        // Inspector refs
        [Header("Bet Info")]
        [SerializeField] private BetType betType;
        [SerializeField] private int[] coveredNumbers;
        [SerializeField] private int payout;
        [SerializeField] private string spotLabel;

        [Header("Snap Settings")]
        [SerializeField] private float snapRadius = 0.3f;
        [SerializeField] private Transform chipAnchorPoint;
        [SerializeField] private bool allowMultipleChips = true;

        [Header("Visual")]
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private Renderer spotRenderer;
        [SerializeField] private Color normalColor = new Color(0f, 0.5f, 0f, 0.3f);
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0f, 0.5f);

        [Header("Stack Settings")]
        [SerializeField] private float stackHeightStep = 0.17f;

        //////////////////////////////////////////////////////////////////////////
        // Runtime states
        private List<Chip> placedChips = new List<Chip>();
        private bool isHighlighted;

        // Lazily resolve payout strategy based on current bet type.
        private IPayoutStrategy payoutStrategy;
        private IPayoutStrategy PayoutStrategy
            => payoutStrategy ??= PayoutStrategyFactory.Get(betType);

        //////////////////////////////////////////////////////////////////////////
        // Public properties
        public string SpotLabel => spotLabel;
        public int[] CoveredNumbers => coveredNumbers;
        public int Payout => payout;

        //////////////////////////////////////////////////////////////////////////
        // Config setters (called by RouletteTableLayout)
        public void Configure(
            BetType type, 
            int[] numbers, 
            int payoutMultiplier, 
            string label, 
            Transform chipAnchorPoint, 
            float snapRadius)
        {
            betType = type;
            coveredNumbers = numbers;
            payout = payoutMultiplier;
            spotLabel = label;
            payoutStrategy = null;   // reset so it re-resolves
            this.chipAnchorPoint = chipAnchorPoint;
            this.snapRadius = snapRadius;
        }

        private void Awake()
        {
            if (chipAnchorPoint == null)
            {
                Debug.LogError("[BetSpot] ChipAnchorPoint not set – using own transform.");
                chipAnchorPoint = transform;
            }

            SetupVisual();
        }

        public int ChipCount => placedChips.Count;
        public bool HasChips => placedChips.Count > 0;
        public List<Chip> GetPlacedChips() => placedChips;
        public bool CanAcceptChip(Chip chip) => allowMultipleChips || placedChips.Count == 0;
        
        public Result PlaceChip(Chip chip)
        {
            if (RouletteGameManager.Instance.Balance < chip.Value)
            {
                RouletteEventBus.RaiseBetExceedsBalance();
                return Result.Failure;
            }

            if (CanAcceptChip(chip) == false)
            {
                Debug.LogWarning($"[BetSpot] Cannot place chip on {spotLabel}.");
                return Result.Failure;
            }

            placedChips.Add(chip);

            // Stack chips vertically so overlapping bets remain visible.
            float stackOffset = (placedChips.Count - 1) * stackHeightStep;
            Vector3 targetPos = chipAnchorPoint.position + Vector3.up * stackOffset;

            chip.SnapToPosition(targetPos, chipAnchorPoint.rotation);
            chip.SetCurrentSpot(this);

            UpdateVisual();

            // Notify via event bus
            RouletteEventBus.RaiseChipPlaced(chip, this);

            return Result.Success;
        }

        public Result RemoveChip(Chip chip)
        {
            if (placedChips.Remove(chip) == false)
            {
                Debug.LogWarning($"[BetSpot] Failed to remove chip on {spotLabel}" +
                    $" placedChips does not contain the given chip.");
                return Result.Failure;
            }

            UpdateVisual();
            RouletteEventBus.RaiseChipRemoved(chip, this);

            return Result.Success;
        }


        public void ClearAllChips()
        {
            // Iterate over a copy because ReturnToTray() modifies placedChips.
            var copy = new List<Chip>(placedChips);
           
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
        // Returns net profit result: positive = win profit, negative = loss.
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
}