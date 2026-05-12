using RouletteGame.Chip;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteGame.Data
{
    //////////////////////////////////////////////////////////////////////////
    // ScriptableObject that stores all chip visual/behavior definitions.
    // Provides fast lookup (value -> ChipDefinition) for ChipFactory usage
    //////////////////////////////////////////////////////////////////////////

    [CreateAssetMenu(fileName = "ChipDefinitionList", menuName = "Roulette/ChipDefinitionList")]
    public class ChipDefinitionList : ScriptableObject
    {
        [SerializeField] 
        private List<ChipFactory.ChipDefinition> chipDefinitions;

        //////////////////////////////////////////////////////////////////////////
        private Dictionary<int, ChipFactory.ChipDefinition> chipDefinitionMap;
        public IReadOnlyList<ChipFactory.ChipDefinition> ChipDefinitions => chipDefinitions;

        //////////////////////////////////////////////////////////////////////////
        // Builds dictionary cache for fast lookup of chip definitions by value.
        private void OnEnable()
        {
            chipDefinitionMap = new Dictionary<int, ChipFactory.ChipDefinition>();

            foreach (var chipDefinition in chipDefinitions)
                chipDefinitionMap.Add(chipDefinition.value, chipDefinition);
        }

        // Returns chip definition for given value.
        // Returns null if value is not found in the dictionary.
        public ChipFactory.ChipDefinition GetChipDefinition(int value)
        {
            ChipFactory.ChipDefinition chipDefinition = null;

            return chipDefinitionMap.TryGetValue(value, out chipDefinition) ? chipDefinition : null;
        }
    }
}