using UnityEngine;

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
    public Color occupiedColor = new Color(0, 1, 0, 0.4f);
}
