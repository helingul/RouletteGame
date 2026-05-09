using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class RouletteTableLayout
{
    // Scriptable object references for retrieving config information
    private RouletteTableData rouletteTableData;
    private PayoutList payoutList;

    // Propertires for RouletteTableData config
    private Vector3 NumberGridOrigin => rouletteTableData.NumberGridOrigin;
    private float gridCellWidth => rouletteTableData.GridCellWidth;
    private float gridCellDepth => rouletteTableData.GridCellDepth;
    private Vector3 outsideOrigin => rouletteTableData.OutsideOrigin;
    private float dozenRowHeight => rouletteTableData.DozenRowHeight;
    private float outsideTotalWidth => rouletteTableData.OutsideTotalWidth;
    private float chipYOffset => rouletteTableData.ChipYOffset;
    private GameObject betSpotPrefab => rouletteTableData.BetSpotPrefab;

    // Parent objects for inspector organization
    private Transform straightSpotsParent;
    private Transform splitSpotsParent;
    private Transform streetSpotsParent;
    private Transform cornerSpotsParent;
    private Transform sixLineSpotsParent;
    private Transform outsideSpotsParent;

    // TODO: Make this private 
    [HideInInspector] public List<BetSpot> allSpots = new List<BetSpot>();

    // Number grid layout.
    private static readonly int[,] numberGrid = new int[12, 3]
    {
        {  1,  2,  3 }, {  4,  5,  6 }, {  7,  8,  9 }, { 10, 11, 12 },
        { 13, 14, 15 }, { 16, 17, 18 }, { 19, 20, 21 }, { 22, 23, 24 },
        { 25, 26, 27 }, { 28, 29, 30 }, { 31, 32, 33 }, { 34, 35, 36 }
    };

    // Red numbers
    private static readonly HashSet<int> redNumbers = new HashSet<int>
    { 1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36 };


    private int GetPayoutMultiplier(BetType betType) => payoutList.GetMultiplier(betType);
    private Result Initialize()
    {
        rouletteTableData = Resources.Load<RouletteTableData>("Data/RouletteTableData");
        if(rouletteTableData == null)
        {
            // TODO: Debug
            return Result.Failure;
        }
        
        payoutList = Resources.Load<PayoutList>("Data/PayoutList");
        if (payoutList == null)
        {
            // TODO: Debug
            return Result.Failure;
        }

        GameObject go = new GameObject("RouletteTableLayout");
        straightSpotsParent = CreateParent("StraightSpots", go.transform);
        splitSpotsParent = CreateParent("SplitSpots", go.transform);
        streetSpotsParent = CreateParent("Streetpots", go.transform);
        cornerSpotsParent = CreateParent("CornerSpots", go.transform);
        sixLineSpotsParent = CreateParent("SixLineSpots", go.transform);
        outsideSpotsParent = CreateParent("OutsideSpots", go.transform);

        return Result.Success;
    }

    private Transform CreateParent(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName);
        go.transform.parent = parent;

        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        return go.transform;
    }

    // Generate all bet spots
    public Result GenerateAllBetSpots()
    {
        ClearAllSpots();

        if (Initialize() == Result.Failure)
        {
            // TODO : Log
            return Result.Failure;
        }

        // Bets inside the grid
        GenerateStraightBets();
        GenerateSplitBets();
        GenerateStreetBets();
        GenerateCornerBets();
        GenerateSixLineBets();

        // Bets outside the grid
        GenerateColumnBets();
        GenerateDozenBets();
        GenerateOutsideBets();

        return Result.Success;
    }


    // Helpers for calculating grid positions
    private Vector3 GridPos(float col, float row)
    {
        float x = NumberGridOrigin.x
                  + gridCellWidth
                  + col * gridCellWidth
                  + gridCellWidth * 0.5f;
        float z = NumberGridOrigin.z + row * gridCellDepth + gridCellDepth * 0.5f;
        float y = NumberGridOrigin.y + chipYOffset;
        return new Vector3(x, y, z);
    }
    private Vector3 ZeroPos()
    {
        float x = NumberGridOrigin.x + gridCellWidth * 0.5f;
        float z = NumberGridOrigin.z + gridCellDepth * 1.5f;
        float y = NumberGridOrigin.y + chipYOffset;
        return new Vector3(x, y, z);
    }


    // Returns left edge of the columns
    private float ColLeftEdgeX(int col)
        => NumberGridOrigin.x + gridCellWidth + col * gridCellWidth;

    // Returns bet positions outside of the grid
    private Vector3 OutsidePos(int slotIndex, int slotCount, float zOffset)
    {
        float slotW = outsideTotalWidth / slotCount;
        float x = outsideOrigin.x + slotIndex * slotW + slotW * 0.5f;
        float z = outsideOrigin.z + zOffset;
        float y = outsideOrigin.y + chipYOffset;
        return new Vector3(x, y, z);
    }

    // Generates bets for each  number to the center of the grid slot.
    private void GenerateStraightBets()
    {
        BetType betType = BetType.Straight;
        int payoutMultiplier = GetPayoutMultiplier(betType);

        // Create spot for straight bet on "0".
        CreateSpot("Straight_0",
                    betType, 
                    new[] { 0 },
                    payoutMultiplier,
                    ZeroPos(), 
                    straightSpotsParent, 
                    "0");

        for (int col = 0; col < 12; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                int num = numberGrid[col, row];

                CreateSpot($"Straight_{num}",
                             betType,
                             new[] { num },
                             payoutMultiplier,
                             GridPos(col, row),
                             straightSpotsParent,
                             num.ToString());
            }
        }  
    }

    // Generates bets for each adjacent number.
    private void GenerateSplitBets()
    {
        BetType betType = BetType.Split;
        int payoutMultiplier = GetPayoutMultiplier(betType);

        // Z-split (same col, adjacent rows)
        for (int col = 0; col < 12; col++)
        {
            for (int row = 0; row < 2; row++)
            {
                int a = numberGrid[col, row];
                int b = numberGrid[col, row + 1];


                CreateSpot($"Split_{a}_{b}",
                             betType, 
                             new[] { a, b },
                             payoutMultiplier,
                             GridPos(col, row + 0.5f), 
                             splitSpotsParent, 
                             $"{a}|{b}");
            }
        }
           

        // X-split (same row, adjacent columns)
        for (int col = 0; col < 11; col++)
        {
            for (int row = 0; row < 3; row++)
            {
                int a = numberGrid[col, row]; 
                int b = numberGrid[col + 1, row];

                CreateSpot($"Split_{a}_{b}",
                             betType, 
                             new[] { a, b },
                             payoutMultiplier,
                             GridPos(col + 0.5f, row), 
                             splitSpotsParent, 
                             $"{a}|{b}");
            }
        }

        // Split with 0 
        for (int n = 1; n <= 3; n++)
        {
            Vector3 pos = Vector3.Lerp(ZeroPos(), GridPos(0, n - 1), 0.5f);

            CreateSpot(
                $"Split_0_{n}", 
                betType, 
                new[] { 0, n },
                payoutMultiplier,
                pos, 
                splitSpotsParent, 
                $"0|{n}");
        }
    }

    // Generates bets for each column including 3 numbers.
    private void GenerateStreetBets()
    {
        BetType betType = BetType.Street;
        int payoutMultiplier = GetPayoutMultiplier(betType);

        for (int col = 0; col < 12; col++)
        {
            int[] nums = { numberGrid[col, 0], numberGrid[col, 1], numberGrid[col, 2] };

            Vector3 pos = GridPos(col, -0.5f);
            
            CreateSpot(
                $"Street_{nums[0]}_{nums[1]}_{nums[2]}",
                betType,
                nums,
                payoutMultiplier,
                pos, 
                streetSpotsParent,
                $"{nums[0]}-{nums[1]}-{nums[2]}");
        }
    }

    // Generates corner bets that includes 4 numbers.
    private void GenerateCornerBets()
    {
        BetType betType = BetType.Corner;
        int payoutMultiplier = GetPayoutMultiplier(betType);

        for (int col = 0; col < 11; col++)
        {
            for (int row = 0; row < 2; row++)
            {
                int[] nums = {
                    numberGrid[col,row], numberGrid[col,row+1],
                    numberGrid[col+1,row], numberGrid[col+1,row+1]
                };

                CreateSpot($"Corner_{nums[0]}_{nums[1]}_{nums[2]}_{nums[3]}",
                           betType, 
                           nums,
                           payoutMultiplier, 
                           GridPos(col + 0.5f, row + 0.5f),
                           cornerSpotsParent, 
                           $"{nums[0]}|{nums[1]}|{nums[2]}|{nums[3]}");
            }
        }
    }

    // Generates six linde bets including 6 numbers.
    private void GenerateSixLineBets()
    {
        BetType betType = BetType.SixLine;
        int payoutMultiplier = GetPayoutMultiplier(betType);

        for (int col = 1; col < 12; col++)
        {
            int[] nums = { numberGrid[col-1,0], numberGrid[col-1,1], numberGrid[col-1,2],
                            numberGrid[col,0],   numberGrid[col,1],   numberGrid[col,2]};

            float x = ColLeftEdgeX(col);
            float z = NumberGridOrigin.z;
            float y = NumberGridOrigin.y + chipYOffset;

            CreateSpot(
                $"SixLine_{col}",
                betType,
                nums,
                payoutMultiplier,
                new Vector3(x, y, z),
                sixLineSpotsParent,
                $"{nums[0]}-{nums[3]}");
        }
    }

    // Generates column bets
    private void GenerateColumnBets()
    {
        BetType betType = BetType.Column;
        int payoutMultiplier = GetPayoutMultiplier(betType);

        float colX = NumberGridOrigin.x + gridCellWidth * 13f + gridCellWidth * 0.5f;
        float y = NumberGridOrigin.y + chipYOffset;

        for (int row = 0; row < 3; row++)
        {
            var nums = new List<int>();
            
            for (int col = 0; col < 12; col++)
            {
                nums.Add(numberGrid[col, row]);
            }

            float z = NumberGridOrigin.z + row * gridCellDepth + gridCellDepth * 0.5f;
            
            CreateSpot(
                $"Column_{row}",
                betType, 
                nums.ToArray(),
                payoutMultiplier,
                new Vector3(colX, y, z), 
                outsideSpotsParent,
                row == 0 ? "2to1" : row == 1 ? "2to1" : "2to1");
        }
    }

    // Generates dozen bets
    private void GenerateDozenBets()
    {
        BetType betType = BetType.Column;
        int payoutMultiplier = GetPayoutMultiplier(betType);

        float zCenter = outsideOrigin.z + chipYOffset + dozenRowHeight * 0.5f;

        CreateSpot(
            "Dozen_1st",
            betType, 
            GenerateRange(1, 12),
            payoutMultiplier,
            OutsidePos(0, 3, dozenRowHeight * -0.5f), 
            outsideSpotsParent, 
            "1st 12");

        CreateSpot("Dozen_2nd",
            betType, 
            GenerateRange(13, 24),
            payoutMultiplier,
            OutsidePos(1, 3, dozenRowHeight * -0.5f), 
            outsideSpotsParent, 
            "2nd 12");

        CreateSpot("Dozen_3rd",
            betType, 
            GenerateRange(25, 36),
            payoutMultiplier,
            OutsidePos(2, 3, dozenRowHeight * -0.5f), 
            outsideSpotsParent, 
            "3rd 12");
    }

    // Generates outside bets
    private void GenerateOutsideBets()
    {
        float zOff = dozenRowHeight * -1.5f;

        string[] labels = { "1-18", "Even", "Red", "Black", "Odd", "19-36" };

        BetType[] types = { BetType.HighLow, BetType.OddEven, BetType.RedBlack, 
                            BetType.RedBlack, BetType.OddEven, BetType.HighLow };
        
        int[][] covered = {
            GenerateRange(1,18), GenerateEven(),  GetRedNumbers(),
            GetBlackNumbers(),   GenerateOdd(),   GenerateRange(19,36)
        };

        for (int i = 0; i < 6; i++)
        {
            BetType betType = types[i];
            int payoutMultiplier = GetPayoutMultiplier(betType);

            CreateSpot($"Outside_{labels[i]}",
                        betType, 
                        covered[i],
                        payoutMultiplier,
                        OutsidePos(i, 6, zOff), 
                        outsideSpotsParent, 
                        labels[i]);
        }
    }

    // Creates bet spot objects


    private BetSpot CreateSpot(string spotName, BetType type, int[] numbers, int payout,
                     Vector3 worldPos, Transform parent, string label)
    {
        if (betSpotPrefab == null)
        {
            Debug.LogError("Bet spot prefab is empty");
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(betSpotPrefab, parent);

        go.transform.position = worldPos;
        go.transform.rotation = Quaternion.identity;

        go.name = spotName;

        BetSpot spot = go.GetComponent<BetSpot>();

        if (spot == null)
            spot = go.AddComponent<BetSpot>();

        spot.betType = type;
        spot.coveredNumbers = numbers;
        spot.payout = payout;
        spot.spotLabel = label;
        spot.chipAnchorPoint = go.transform;
        spot.snapRadius = GetSnapRadius(type);

        allSpots.Add(spot);

        // Editor'de değişikliklerin serialize olması için
        EditorUtility.SetDirty(go);
        EditorUtility.SetDirty(spot);

        return spot;
    }

    private float GetSnapRadius(BetType type)
    {
        switch (type)
        {
            case BetType.Straight: return 0.07f;
            case BetType.Split: return 0.05f;
            case BetType.Street: case BetType.SixLine: return 0.08f;
            case BetType.Corner: return 0.05f;
            case BetType.Dozen: case BetType.Column: return 0.16f;
            default: return 0.14f;
        }
    }

    // Clear all spots
    private void ClearAllSpots()
    {
        if(allSpots == null) return;
        
        allSpots.Clear();
        
        ClearChildren(straightSpotsParent);
        ClearChildren(splitSpotsParent);
        ClearChildren(streetSpotsParent);
        ClearChildren(cornerSpotsParent);
        ClearChildren(sixLineSpotsParent);
        ClearChildren(outsideSpotsParent);
    }

    private void ClearChildren(Transform t)
    {
        if (t == null) return;
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(t.GetChild(i).gameObject);
        }
    }

    // Helpers
    private int[] GenerateRange(int a, int b) 
    { 
        var r = new List<int>();

        for (int i = a; i <= b; i++) r.Add(i);

        return r.ToArray(); 
    }
    private int[] GenerateEven()
    { 
        var r = new List<int>();
        
        for (int i = 2; i <= 36; i += 2) r.Add(i);
     
        return r.ToArray();
    }
    private int[] GenerateOdd() 
    { 
        var r = new List<int>(); 
        
        for (int i = 1; i <= 35; i += 2) r.Add(i);
        
        return r.ToArray(); 
    }
    private int[] GetRedNumbers()
    { 
        var r = new List<int>(redNumbers); 
        r.Sort(); 

        return r.ToArray();
    }
    private int[] GetBlackNumbers()
    { 
        var r = new List<int>();
        for (int i = 1; i <= 36; i++)
        {
            if (!redNumbers.Contains(i)) r.Add(i);
        }
        
        return r.ToArray();
    }
}
