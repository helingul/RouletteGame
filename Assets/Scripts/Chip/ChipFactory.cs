//////////////////////////////////////////////////////////////////////////
//  ChipFactory and ChipPool
//  FACTORY PATTERN  – ChipFactory encapsulates chip creation.
//  OBJECT POOL PATTERN – ChipPool recycles chips instead of
//                        Instantiate/Destroy each round.
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////
//  ChipFactory
//////////////////////////////////////////////////////////////////////////
// Single responsibility: instantiate a Chip from the
// correct prefab and initialise it. Callers never touch prefabs.
public class ChipFactory
{
    [System.Serializable]
    public class ChipDefinition
    {
        public int value;
        public GameObject prefab;
        public Vector3 relativeLocation;  // tray-local spawn point
    }

    private readonly ChipDefinitionList chipDefinitionList;
    private readonly Transform container;
    private readonly ChipTray tray;

    public ChipFactory(ChipDefinitionList chipDefinitionList, Transform container, ChipTray tray)
    {
        this.chipDefinitionList = chipDefinitionList;
        this.container = container;
        this.tray = tray;
    }


    // Creates a new chip of the given value. Returns null on failure.
    public Chip Create(int value)
    {
        ChipDefinition def = chipDefinitionList.TryGetChipDefinition(value);

        if (def == null || def.prefab == null)
        {
            Debug.LogError($"[ChipFactory] No definition for chip value {value}.");
            return null;
        }

        GameObject go = Object.Instantiate(def.prefab, container);
        Chip chip = go.GetComponent<Chip>();

        if (chip == null)
        {
            Debug.LogError("[ChipFactory] Prefab missing RouletteChip component.");
            Object.Destroy(go);
            return null;
        }

        chip.InititalizeChip(value, tray);
        return chip;
    }
}


//////////////////////////////////////////////////////////////////////////
//  ChipPool  (OBJECT POOL PATTERN)
//////////////////////////////////////////////////////////////////////////
// Recycles Chip objects.
// Get()  ? pulls from pool or creates via factory.
// Return() ? deactivates and puts back in pool.
public class ChipPool
{
    private readonly ChipFactory factory;
    private readonly Dictionary<int, Queue<Chip>> pools
        = new Dictionary<int, Queue<Chip>>();

    public ChipPool(ChipFactory factory)
    {
        this.factory = factory;
    }

    // Public API

    // Gets a chip of the given value (from pool or freshly created).
    public Chip Get(int value)
    {
        EnsureBucket(value);

        Chip chip;
        if (pools[value].Count > 0)
        {
            chip = pools[value].Dequeue();
            chip.gameObject.SetActive(true);
            Debug.Log($"[ChipPool] Reused chip {value}. Pool size: {pools[value].Count}");
        }
        else
        {
            chip = factory.Create(value);
            Debug.Log($"[ChipPool] Created new chip {value}.");
        }

        return chip;
    }

    // Returns a chip to the pool (deactivates it).
    public void Return(Chip chip)
    {
        if (chip == null) return;

        chip.gameObject.SetActive(false);
        EnsureBucket(chip.Value);
        pools[chip.Value].Enqueue(chip);
        Debug.Log($"[ChipPool] Returned chip {chip.Value}. Pool size: {pools[chip.Value].Count}");
    }

    // Warms the pool with a pre-spawned count per value.
    public void Prewarm(int value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Chip chip = factory.Create(value);
            if (chip != null) Return(chip);
        }
        Debug.Log($"[ChipPool] Prewarmed {count} chips of value {value}.");
    }

    // Destroys all pooled chips (call on scene unload).
    public void ClearAll()
    {
        foreach (var bucket in pools.Values)
        {
            foreach (var chip in bucket)
            {
                if (chip != null)
                    Object.Destroy(chip.gameObject);
            }
            bucket.Clear();
        }
        pools.Clear();
    }

    // Private helpers
    private void EnsureBucket(int value)
    {
        if (!pools.ContainsKey(value))
            pools[value] = new Queue<Chip>();
    }
}