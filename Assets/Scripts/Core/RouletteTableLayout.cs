using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RouletteGame.Bets;
using RouletteGame.Common;
using RouletteGame.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RouletteGame.Core
{
    //////////////////////////////////////////////////////////////////////////
    // Generates and organizes all BetSpot objects on the roulette table before runtime 
    // by calling GenerateAllBetSpots context button.
    // Responsible for procedural layout creation and hierarchy setup.
    //////////////////////////////////////////////////////////////////////////
    public class RouletteTableLayout : MonoBehaviour
    {
        // Config (loaded from Resources)
        private RouletteTableData data;
        private PayoutList payoutList;

        // Getters
        private Vector3 NumberGridOrigin => data.NumberGridOrigin;
        private float GridCellWidth => data.GridCellWidth;
        private float GridCellDepth => data.GridCellDepth;
        private Vector3 OutsideOrigin => data.OutsideOrigin;
        private float DozenRowHeight => data.DozenRowHeight;
        private float OutsideTotalWidth => data.OutsideTotalWidth;
        private float ChipYOffset => data.ChipYOffset;
        private GameObject BetSpotPrefab => data.BetSpotPrefab;

        // Hierarchy parents
        private Transform straightParent, splitParent, streetParent;
        private Transform cornerParent, sixLineParent, outsideParent;

        // Spots list
        private List<BetSpot> allSpots = new List<BetSpot>();
        public IReadOnlyList<BetSpot> AllSpots => allSpots;

        // Number grid (European layout)
        private static readonly int[,] NumberGrid = new int[12, 3]
        {
        {  1,  2,  3 }, {  4,  5,  6 }, {  7,  8,  9 }, { 10, 11, 12 },
        { 13, 14, 15 }, { 16, 17, 18 }, { 19, 20, 21 }, { 22, 23, 24 },
        { 25, 26, 27 }, { 28, 29, 30 }, { 31, 32, 33 }, { 34, 35, 36 }
        };

        private static readonly HashSet<int> RedNumbers = new HashSet<int>
        { 1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36 };

        // Context menu button to create bets spots in inspector
        [ContextMenu("Generate All Bet Spots")]
        public Result GenerateAllBetSpots()
        {
            // Full generation pipeline: clears old spots, loads config data,
            // builds hierarchy, then generates all bet categories.

            ClearAllSpots();

            if (!LoadData())
            {
                Debug.Log($"[RouletteTableLayout] Failed to load data.");
                return Result.Failure;
            }
            
            BuildParentHierarchy();

            GenerateStraightBets();
            GenerateSplitBets();
            GenerateStreetBets();
            GenerateCornerBets();
            GenerateSixLineBets();
            GenerateColumnBets();
            GenerateDozenBets();
            GenerateOutsideBets();

            Debug.Log($"[RouletteTableLayout] Generated {allSpots.Count} bet spots.");
            return Result.Success;
        }

        private bool LoadData()
        {
            data = Resources.Load<RouletteTableData>("Data/RouletteTableData");
            if (data == null)
            {
                Debug.LogError("[RouletteTableLayout] RouletteTableData not found.");
                return false;
            }

            payoutList = Resources.Load<PayoutList>("Data/PayoutList");
            if (payoutList == null) 
            { 
                Debug.LogError("[RouletteTableLayout] PayoutList not found."); 
                return false; 
            }

            return true;
        }

        private void Awake()
        {
            allSpots = GetComponentsInChildren<BetSpot>().ToList();
        }
        private void BuildParentHierarchy()
        {
            straightParent = CreateChild("StraightSpots");
            splitParent = CreateChild("SplitSpots");
            streetParent = CreateChild("StreetSpots");
            cornerParent = CreateChild("CornerSpots");
            sixLineParent = CreateChild("SixLineSpots");
            outsideParent = CreateChild("OutsideSpots");
        }
        private Transform CreateChild(string childName)
        {
            var existing = transform.Find(childName);
            if (existing != null)
            {
                ClearChildren(existing);
                return existing;
            }

            GameObject go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        // Position helpers
        private Vector3 GridPos(float col, float row)
        {
            float x = NumberGridOrigin.x + GridCellWidth + col * GridCellWidth + GridCellWidth * 0.5f;
            float z = NumberGridOrigin.z + row * GridCellDepth + GridCellDepth * 0.5f;
            float y = NumberGridOrigin.y + ChipYOffset;
            return new Vector3(x, y, z);
        }
        private Vector3 ZeroPos()
        {
            return new Vector3(
                NumberGridOrigin.x + GridCellWidth * 0.5f,
                NumberGridOrigin.y + ChipYOffset,
                NumberGridOrigin.z + GridCellDepth * 1.5f);
        }

        private float ColLeftEdgeX(int col)
            => NumberGridOrigin.x + GridCellWidth + col * GridCellWidth;

        private Vector3 OutsidePos(int slotIndex, int slotCount, float zOffset)
        {
            float slotW = OutsideTotalWidth / slotCount;
            return new Vector3(
                OutsideOrigin.x + slotIndex * slotW + slotW * 0.5f,
                OutsideOrigin.y + ChipYOffset,
                OutsideOrigin.z + zOffset);
        }

        private int Multiplier(BetType t) => payoutList.GetMultiplier(t);

        // Bet Spot Generators
        private void GenerateStraightBets()
        {
            BetType bt = BetType.Straight;
            int m = Multiplier(bt);

            CreateSpot("Straight_0", bt, new[] { 0 }, m, ZeroPos(), straightParent, "0");

            for (int col = 0; col < 12; col++)
                for (int row = 0; row < 3; row++)
                {
                    int n = NumberGrid[col, row];
                    CreateSpot($"Straight_{n}", bt, new[] { n }, m, GridPos(col, row), straightParent, n.ToString());
                }
        }

        private void GenerateSplitBets()
        {
            BetType bt = BetType.Split;
            int m = Multiplier(bt);

            // Row-splits
            for (int col = 0; col < 12; col++)
                for (int row = 0; row < 2; row++)
                {
                    int a = NumberGrid[col, row], b = NumberGrid[col, row + 1];
                    CreateSpot($"Split_{a}_{b}", bt, new[] { a, b }, m, GridPos(col, row + 0.5f), splitParent, $"{a}|{b}");
                }

            // Column-splits
            for (int col = 0; col < 11; col++)
                for (int row = 0; row < 3; row++)
                {
                    int a = NumberGrid[col, row], b = NumberGrid[col + 1, row];
                    CreateSpot($"Split_{a}_{b}", bt, new[] { a, b }, m, GridPos(col + 0.5f, row), splitParent, $"{a}|{b}");
                }

            // Zero-splits
            for (int n = 1; n <= 3; n++)
            {
                Vector3 pos = Vector3.Lerp(ZeroPos(), GridPos(0, n - 1), 0.5f);
                CreateSpot($"Split_0_{n}", bt, new[] { 0, n }, m, pos, splitParent, $"0|{n}");
            }
        }

        private void GenerateStreetBets()
        {
            BetType bt = BetType.Street;
            int m = Multiplier(bt);

            for (int col = 0; col < 12; col++)
            {
                int[] nums = { NumberGrid[col, 0], NumberGrid[col, 1], NumberGrid[col, 2] };
                CreateSpot($"Street_{nums[0]}_{nums[1]}_{nums[2]}", bt, nums, m,
                    GridPos(col, -0.5f), streetParent, $"{nums[0]}-{nums[2]}");
            }
        }

        private void GenerateCornerBets()
        {
            BetType bt = BetType.Corner;
            int m = Multiplier(bt);

            for (int col = 0; col < 11; col++)
                for (int row = 0; row < 2; row++)
                {
                    int[] nums =
                    {
                NumberGrid[col,   row],     NumberGrid[col,   row + 1],
                NumberGrid[col+1, row],     NumberGrid[col+1, row + 1]
            };
                    CreateSpot($"Corner_{col}_{row}", bt, nums, m,
                        GridPos(col + 0.5f, row + 0.5f), cornerParent,
                        $"{nums[0]}|{nums[1]}|{nums[2]}|{nums[3]}");
                }
        }

        private void GenerateSixLineBets()
        {
            BetType bt = BetType.SixLine;
            int m = Multiplier(bt);

            for (int col = 1; col < 12; col++)
            {
                int[] nums =
                {
                NumberGrid[col-1,0], NumberGrid[col-1,1], NumberGrid[col-1,2],
                NumberGrid[col,  0], NumberGrid[col,  1], NumberGrid[col,  2]
            };
                CreateSpot($"SixLine_{col}", bt, nums, m,
                    new Vector3(ColLeftEdgeX(col), NumberGridOrigin.y + ChipYOffset, NumberGridOrigin.z),
                    sixLineParent, $"{nums[0]}-{nums[3]}");
            }
        }

        private void GenerateColumnBets()
        {
            BetType bt = BetType.Column;
            int m = Multiplier(bt);

            float colX = NumberGridOrigin.x + GridCellWidth * 13f + GridCellWidth * 0.5f;
            float y = NumberGridOrigin.y + ChipYOffset;

            for (int row = 0; row < 3; row++)
            {
                var nums = new List<int>();
                for (int col = 0; col < 12; col++) nums.Add(NumberGrid[col, row]);

                float z = NumberGridOrigin.z + row * GridCellDepth + GridCellDepth * 0.5f;
                CreateSpot($"Column_{row}", bt, nums.ToArray(), m,
                    new Vector3(colX, y, z), outsideParent, "2to1");
            }
        }

        private void GenerateDozenBets()
        {
            BetType bt = BetType.Dozen;
            int m = Multiplier(bt);

            CreateSpot("Dozen_1st", bt, Range(1, 12), m, OutsidePos(0, 3, DozenRowHeight * -0.5f), outsideParent, "1st 12");
            CreateSpot("Dozen_2nd", bt, Range(13, 24), m, OutsidePos(1, 3, DozenRowHeight * -0.5f), outsideParent, "2nd 12");
            CreateSpot("Dozen_3rd", bt, Range(25, 36), m, OutsidePos(2, 3, DozenRowHeight * -0.5f), outsideParent, "3rd 12");
        }

        private void GenerateOutsideBets()
        {
            float zOff = DozenRowHeight * -1.5f;

            (string label, BetType type, int[] covered)[] slots =
            {
            ("1-18",  BetType.HighLow,  Range(1, 18)),
            ("Even",  BetType.OddEven,  Even()),
            ("Red",   BetType.RedBlack, Reds()),
            ("Black", BetType.RedBlack, Blacks()),
            ("Odd",   BetType.OddEven,  Odd()),
            ("19-36", BetType.HighLow,  Range(19, 36)),
        };

            for (int i = 0; i < slots.Length; i++)
            {
                var (label, type, covered) = slots[i];
                int m = Multiplier(type);
                CreateSpot($"Outside_{label}", type, covered, m, OutsidePos(i, 6, zOff), outsideParent, label);
            }
        }

        // Instantiates a BetSpot prefab, configures it with betting rules,
        // and attaches it to the correct hierarchy parent.
        private BetSpot CreateSpot(string spotName, BetType type, int[] numbers, int payout,
                                    Vector3 worldPos, Transform parent, string label)
        {
            if (BetSpotPrefab == null) { Debug.LogError("[RouletteTableLayout] BetSpotPrefab is null."); return null; }

            GameObject go;
            go = (GameObject)PrefabUtility.InstantiatePrefab(BetSpotPrefab, parent);

            go.name = spotName;
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.identity;

            BetSpot spot = go.GetComponent<BetSpot>() ?? go.AddComponent<BetSpot>();
            spot.Configure(type, numbers, payout, label, go.transform, SnapRadius(type));

            EditorUtility.SetDirty(go);
            EditorUtility.SetDirty(spot);

            return spot;
        }

        // Clear helpers 
        private void ClearAllSpots()
        {
            allSpots.Clear();
            if (straightParent) ClearChildren(straightParent);
            if (splitParent) ClearChildren(splitParent);
            if (streetParent) ClearChildren(streetParent);
            if (cornerParent) ClearChildren(cornerParent);
            if (sixLineParent) ClearChildren(sixLineParent);
            if (outsideParent) ClearChildren(outsideParent);
        }

        private static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                DestroyImmediate(t.GetChild(i).gameObject);
        }

        public void ClearAllBets()
        {
            foreach (var spot in allSpots) spot.ClearAllChips();
        }

        // Number helpers
        private static float SnapRadius(BetType t) => t switch
        {
            BetType.Straight => 0.07f,
            BetType.Split => 0.05f,
            BetType.Street or BetType.SixLine => 0.08f,
            BetType.Corner => 0.05f,
            BetType.Dozen or BetType.Column => 0.16f,
            _ => 0.14f,
        };

        private static int[] Range(int a, int b)
        {
            var r = new List<int>();
            for (int i = a; i <= b; i++) r.Add(i);
            return r.ToArray();
        }

        private static int[] Even()
        {
            var r = new List<int>();
            for (int i = 2; i <= 36; i += 2) r.Add(i);
            return r.ToArray();
        }

        private static int[] Odd()
        {
            var r = new List<int>();
            for (int i = 1; i <= 35; i += 2) r.Add(i);
            return r.ToArray();
        }

        private static int[] Reds()
        {
            var r = new List<int>(RedNumbers);
            r.Sort();
            return r.ToArray();
        }

        private static int[] Blacks()
        {
            var r = new List<int>();
            for (int i = 1; i <= 36; i++)
                if (!RedNumbers.Contains(i)) r.Add(i);
            return r.ToArray();
        }
    }
}