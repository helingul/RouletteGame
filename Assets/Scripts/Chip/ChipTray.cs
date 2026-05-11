//////////////////////////////////////////////////////////////////////////
//  Physical tray MonoBehaviour.
//  - Owns ChipFactory (Factory Pattern) and ChipPool (Pool Pattern)
//  - Listens to EventBus (Observer) to track placed/returned chips
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

public class ChipTray : MonoBehaviour
{
    // Chip definitions
    [SerializeField] private ChipDefinitionList chipDefinitionList;

    [Header("Settings")]
    [SerializeField] private int maxChipsOnTable = 20;
    [SerializeField] private Transform chipContainer;
    [SerializeField] private float stackHeightOffset = 1f;

    [Header("Pool")]
    [SerializeField] private int prewarmCountPerType = 3;

    // Internal subsystems
    private ChipFactory factory;
    private ChipPool pool;

    // Active chips per value (for stack-height calculation)
    private Dictionary<int, List<Chip>> activeChips = new();

    // Properties (used by ChipPool / factory)
    public ChipFactory Factory => factory;
    public ChipPool Pool => pool;

    // Lifecycle
    private void Awake()
    {
        factory = new ChipFactory(chipDefinitionList, chipContainer, this);
        pool = new ChipPool(factory);

        // Prewarm pool for each defined chip type
        foreach (var def in chipDefinitionList.ChipDefinitions)
            pool.Prewarm(def.value, prewarmCountPerType);

        // Subscribe to event bus
        RouletteEventBus.OnChipPlaced += HandleChipPlaced;
        RouletteEventBus.OnChipRemoved += HandleChipRemoved;
        RouletteEventBus.OnChipAdded += HandleChipAdded;
    }

    private void OnDestroy()
    {
        RouletteEventBus.OnChipPlaced -= HandleChipPlaced;
        RouletteEventBus.OnChipRemoved -= HandleChipRemoved;
        RouletteEventBus.OnChipAdded -= HandleChipAdded;

        pool?.ClearAll();
    }

    // Public API

    // Spawns (or retrieves from pool) a chip of the given value.
    public Chip SpawnChip(int value)
    {
        int activeCount = CountActive();
        if (activeCount >= maxChipsOnTable)
        {
            RouletteEventBus.RaiseChipTrayFull();
            Debug.LogWarning("[ChipTray] Max chip count reached.");
            return null;
        }

        Chip chip = pool.Get(value);
        if (chip == null) return null;

        // Position chip in tray
        Vector3 pos = GetChipPosition(chip);
        Quaternion rot = Quaternion.identity;
        chip.SetTrayPosition(pos, rot);
        chip.gameObject.SetActive(true);

        TrackActive(chip);

        return chip;
    }

    // Returns chip with given value from tray.
    public Chip GetChipFromTray(int value)
    {
        if (activeChips.TryGetValue(value, out List<Chip> chips))
        {
            if (chips.Count > 0)
            {
                return chips[0];
            }
        }

        return null;
    }

    // Calculates world-space tray position for a chip with stacking.
    public Vector3 GetChipPosition(Chip chip)
    {
        ChipFactory.ChipDefinition def = chipDefinitionList.TryGetChipDefinition(chip.Value);
        if (def == null) return transform.position;

        int stackIndex = 0;
        if (activeChips.TryGetValue(def.value, out var list))
            stackIndex = list.Count;

        Vector3 local = def.relativeLocation;
        local.y += stackIndex * stackHeightOffset;
        return transform.TransformPoint(local);
    }

    public void ClearAll()
    {
        foreach (var bucket in activeChips.Values)
        {
            foreach (var chip in bucket)
            {
                if (chip == null) continue;
                pool.Return(chip);
            }
            bucket.Clear();
        }
        activeChips.Clear();
    }

    // Private helpers
    private void TrackActive(Chip chip)
    {
        if (!activeChips.TryGetValue(chip.Value, out var list))
        {
            list = new List<Chip>();
            activeChips[chip.Value] = list;
        }
        if (!list.Contains(chip)) list.Add(chip);
    }

    private void UntrackActive(Chip chip)
    {
        if (activeChips.TryGetValue(chip.Value, out var list))
            list.Remove(chip);
    }

    private int CountActive()
    {
        int n = 0;
        foreach (var b in activeChips.Values) n += b.Count;
        return n;
    }

    // EventBus handlers
    private void HandleChipPlaced(Chip chip, BetSpot _)
        => UntrackActive(chip);     // chip left the tray

    private void HandleChipRemoved(Chip chip, BetSpot _)
        => TrackActive(chip);       // chip returned to tray area

    private void HandleChipAdded(int value)
      => SpawnChip(value);          // chip added to the tray
}