//////////////////////////////////////////////////////////////////////////
//  ScriptableObject – stores all chip definitions
//  Definition list is mapped by its value for fast access
//  Used by ChipFactory
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;
using static ChipFactory;

[CreateAssetMenu(fileName = "ChipDefinitionList", menuName = "Roulette/ChipDefinitionList")]
public class ChipDefinitionList : ScriptableObject
{
   
    [SerializeField] private List<ChipFactory.ChipDefinition> chipDefinitions = new List<ChipFactory.ChipDefinition>();

    private Dictionary<int, ChipFactory.ChipDefinition> chipDefinitionMap;

    public IReadOnlyList<ChipFactory.ChipDefinition> ChipDefinitions => chipDefinitions;

    private void OnEnable()
    {
        chipDefinitionMap = new Dictionary<int, ChipFactory.ChipDefinition>();

        foreach (var chipDefinition in chipDefinitions)
            chipDefinitionMap.Add(chipDefinition.value, chipDefinition);
    }

    public ChipFactory.ChipDefinition TryGetChipDefinition(int value)
    {
        ChipFactory.ChipDefinition chipDefinition = null;
       return chipDefinitionMap.TryGetValue(value, out chipDefinition) ? chipDefinition : null;
    }
}