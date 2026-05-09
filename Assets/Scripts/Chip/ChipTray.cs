using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ChipTray : MonoBehaviour
{
    [System.Serializable]
    public class ChipDefinition
    {
        public int value;
        public ChipColor color;
        public GameObject prefab;
        public Vector3 relativeLocation;
    }

    public List<ChipDefinition> chipDefinitions = new List<ChipDefinition>();

    [Header("Settings")]
    public int maxChipsOnTable = 20;
    public Transform chipContainer;
    public float stackHeightOffset = 1f;

    private Dictionary<ChipColor, List<RouletteChip>> activeChips = new();

    public RouletteChip SpawnChip(int value)
    {
        if (activeChips.Count >= maxChipsOnTable)
        {
            Debug.LogWarning("[ChipTray] Failed to spawn chip. Maximum chip count is reached.");
            return null;
        }

        if (RouletteGameManager.Instance != null &&
            RouletteGameManager.Instance.Balance < value)
        {
            Debug.LogWarning("[ChipTray] Failed to spawn chip. Insufficient balance.");
            return null;
        }

        ChipDefinition def = chipDefinitions.Find(d => d.value == value);
        if (def == null || def.prefab == null)
        {
            Debug.LogError($"[ChipTray] Failed to spawn chip. Chip with value {value} cannot be found.");
            return null;
        }

        GameObject chipGO = Instantiate(def.prefab, chipContainer);
        RouletteChip chip = chipGO.GetComponent<RouletteChip>();
    
        if (chip == null)
        {
            Debug.LogError("[ChipTray] Prefab'da RouletteChip component'i yok!");
            Destroy(chipGO);
            return null;
        }

        Vector3 position = GetChipPosition(chip);
        chip.transform.position = position;

        chip.InititalizeChip(value, def.color, this);
        chip.SetTrayPosition(chipGO.transform.position, chipGO.transform.rotation);

        AddChip(chip);

        chip.OnReturnedToTray += HandleChipReturnedToTray;
        chip.OnPlacedOnBet += HandleChipPlacedOnBet;

        return chip;
    }

    public Vector3 GetChipPosition(RouletteChip chip)
    {
        ChipDefinition def = chipDefinitions.Find(d => d.value == chip.Value);
        if (def == null || def.prefab == null)
        {
            Debug.LogError($"[ChipTray] Failed to spawn chip. Chip with value {chip.Value} cannot be found.");
            return Vector3.zero;
        }

        // Calculate chip count of same color
        int stackIndex = 0;
        if (activeChips.TryGetValue(def.color, out var sameColorList))
        {
            stackIndex = sameColorList.Count;
        }

        // Find stack position
        Vector3 spawnPos = def.relativeLocation;
        spawnPos.y += stackIndex * stackHeightOffset;

        return transform.TransformPoint(spawnPos);
    }

    // Adds chip to the active chips
    private void AddChip(RouletteChip chip)
    {
        if (!activeChips.TryGetValue(chip.ChipColor, out var list))
        {
            list = new List<RouletteChip>();
            activeChips.Add(chip.ChipColor, list);
        }

        list.Add(chip);
    }

    private Result RemoveChip(RouletteChip chip)
    {
        if (activeChips.TryGetValue(chip.ChipColor, out var list))
        {
            list.Remove(chip);

            return Result.Success;
        }

        return Result.Failure;
    }
    public void ClearAll()
    {
        foreach (var chipPair in activeChips)
        {
            foreach (var chip in chipPair.Value)
            {
                if (chip != null)
                {
                    chip.OnReturnedToTray -= HandleChipReturnedToTray;
                    chip.OnPlacedOnBet -= HandleChipPlacedOnBet;

                    Destroy(chip.gameObject);
                }
            }
        }

        activeChips.Clear();
    }

    private void HandleChipReturnedToTray(RouletteChip chip)
    {
        AddChip(chip);
    }

    private void HandleChipPlacedOnBet(RouletteChip chip)
    {
        RemoveChip(chip);
    }
}
