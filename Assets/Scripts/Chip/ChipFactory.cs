using RouletteGame.Data;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteGame.Chip
{
    //////////////////////////////////////////////////////////////////////////
    // Provides chip creation and recycling systems for the roulette game.
    // Uses the Factory Pattern for chip instantiation and the Object Pool
    // Pattern to reuse chip instances efficiently.
    //////////////////////////////////////////////////////////////////////////
    
    // Instantiate a Chip from the correct prefab and initialise it.
    // Callers never touch prefabs.
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
            ChipDefinition def = chipDefinitionList.GetChipDefinition(value);

            if (def == null || def.prefab == null)
            {
                Debug.LogError($"[ChipFactory] No definition for chip value {value}.");
                return null;
            }

            // Instantiate under the shared chip container for hierarchy organization.
            GameObject go = Object.Instantiate(def.prefab, container);
            
            Chip chip = go.GetComponent<Chip>();

            if (chip == null)
            {
                Debug.LogError("[ChipFactory] Prefab missing chip component.");
                Object.Destroy(go);
                return null;
            }

            chip.InitializeChip(value, tray);
            return chip;
        }
    }


    //////////////////////////////////////////////////////////////////////////
    // ChipPool uses ObjectPool to create chips
    // Get pulls from pool or creates via factory.
    // Return deactivates and puts back in pool.
    //////////////////////////////////////////////////////////////////////////

    public class ChipPool
    {
        private readonly ChipFactory factory;
        private readonly Dictionary<int, Queue<Chip>> pools
            = new Dictionary<int, Queue<Chip>>();

        public ChipPool(ChipFactory factory)
        {
            this.factory = factory;
        }

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

        // Pre-create pooled chips to avoid runtime instantiation spikes.
        public void Prewarm(int value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Chip chip = factory.Create(value);
                
                if (chip != null) Return(chip);
            }

            Debug.Log($"[ChipPool] Prewarmed {count} chips of value {value}.");
        }

        // Destroys all pooled chips (called on scene unload).
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

        // Lazily create a queue for this chip denomination.
        private void EnsureBucket(int value)
        {
            if (!pools.ContainsKey(value))
                pools[value] = new Queue<Chip>();
        }
    }
}