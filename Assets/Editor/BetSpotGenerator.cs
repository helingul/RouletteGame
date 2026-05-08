#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class BetSpotGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Bet Spots")]
    public static void GenerateBetSpots()
    {
        RouletteTableLayout rouletteTableLayout = new RouletteTableLayout();
        rouletteTableLayout.GenerateAllBetSpots();
    }
}
#endif