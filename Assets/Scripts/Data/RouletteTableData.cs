using UnityEngine;

namespace RouletteGame.Data
{
    //////////////////////////////////////////////////////////////////////////
    // ScriptableObject that defines all layout configuration data for the roulette table.
    // Includes grid positioning, outside bet areas, and shared visual/physics offsets.
    // Used by RouletteTableLayout for procedural table generation
    //////////////////////////////////////////////////////////////////////////

    [CreateAssetMenu(fileName = "RouletteTableData", menuName = "Roulette/RouletteTableData")]
    public class RouletteTableData : ScriptableObject
    {
        //////////////////////////////////////////////////////////////////////////
        [Header("Number Bet Space")]

        [Tooltip("Left corner of the grid")]
        [SerializeField] private Vector3 numberGridOrigin = Vector3.zero;

        [Tooltip("X length of column")]
        [SerializeField] private float gridCellWidth = 1.55f;

        [Tooltip("Z depth of a row")]
        [SerializeField] private float gridCellDepth = 2.7f;

        ///////////////////////////////////////////////////////////////////////////
        [Header("Outside Bet Space")]

        [Tooltip("Left corner of the outside bets")]
        [SerializeField] private Vector3 outsideOrigin = new Vector3(0, 0, -0.5f);

        [Tooltip("Z height of dozens column")]
        [SerializeField] private float dozenRowHeight = 1.32f;

        [Tooltip("Total width of outside bets")]
        [SerializeField] private float outsideTotalWidth = 18.5f;

        //////////////////////////////////////////////////////////////////////////
        [Header("Common settings")]
        [Tooltip("Chip Y offset")]
        [SerializeField] private float chipYOffset = 0.01f;

        [Header("Prefab")]
        [SerializeField] private GameObject betSpotPrefab;

        //////////////////////////////////////////////////////////////////////////
        // Properties
        public Vector3 NumberGridOrigin => numberGridOrigin;
        public float GridCellWidth => gridCellWidth;
        public float GridCellDepth => gridCellDepth;
        public Vector3 OutsideOrigin => outsideOrigin;
        public float DozenRowHeight => dozenRowHeight;
        public float OutsideTotalWidth => outsideTotalWidth;
        public float ChipYOffset => chipYOffset;
        public GameObject BetSpotPrefab => betSpotPrefab;
        
    }
}