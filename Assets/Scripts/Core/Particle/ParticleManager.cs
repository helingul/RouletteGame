
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> winParticlePrefabs;
    [SerializeField] private GameObject chipPlacementParticlePrefab;
    private void Awake()
    {
        SubscribeToEventBus();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEventBus();
    }
    private void SubscribeToEventBus()
    {
        RouletteEventBus.OnRoundResult += HandleRountResult;
        RouletteEventBus.OnChipPlaced += HandleChipPlaced;
    }

    private void UnsubscribeFromEventBus()
    {
        RouletteEventBus.OnRoundResult -= HandleRountResult;
        RouletteEventBus.OnChipPlaced -= HandleChipPlaced;
    }

    private void HandleRountResult(int net)
    {
        Vector3 position = Camera.main.ViewportToWorldPoint(
                    new Vector3(0.5f, 0.5f, 5f));
        if (net > 0)
        {
            foreach (var prefab in winParticlePrefabs)
            {
                if(prefab == null)
                {
                    Debug.LogError("[ParticleManager] Particle in win particles is not valid.");
                    continue;
                }

                Instantiate(prefab, position, Quaternion.identity);
            }
        }
    }

    private void HandleChipPlaced(Chip chip, BetSpot betSpot)
    {
        if (chipPlacementParticlePrefab == null)
        {
            Debug.LogError("[ParticleManager] Chip placement particle is not valid.");
            return;
        }

        Instantiate(chipPlacementParticlePrefab, chip.transform.position, Quaternion.identity);
    }
}