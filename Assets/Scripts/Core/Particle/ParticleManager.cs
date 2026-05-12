using RouletteGame.Bets;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteGame.Core.Particle
{
    using Chip = Chip.Chip;
    using Camera = UnityEngine.Camera;

    //////////////////////////////////////////////////////////////////////////
    // Central particle effect controller that listens to gameplay events
    // and spawns visual feedback effects such as win and chip placement VFX.
    //////////////////////////////////////////////////////////////////////////
    public class ParticleManager : MonoBehaviour
    {
        // Inspector refs
        [SerializeField] private List<GameObject> winParticlePrefabs;
        [SerializeField] private GameObject chipPlacementParticlePrefab;

        //////////////////////////////////////////////////////////////////////////
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
            RouletteEventBus.OnRoundResult += HandleRoundResult;
            RouletteEventBus.OnChipPlaced += HandleChipPlaced;
        }

        private void UnsubscribeFromEventBus()
        {
            RouletteEventBus.OnRoundResult -= HandleRoundResult;
            RouletteEventBus.OnChipPlaced -= HandleChipPlaced;
        }

        // Spawn win particles at screen center when player wins.
        private void HandleRoundResult(int net)
        {
            // Convert screen center to world position for VFX spawn.
            Vector3 position = Camera.main.ViewportToWorldPoint(
                        new Vector3(0.5f, 0.5f, 5f));
           
            if (net > 0)
            {
                // Spawn all configured win effects.
                foreach (var prefab in winParticlePrefabs)
                {
                    if (prefab == null)
                    {
                        Debug.LogError("[ParticleManager] Particle in win particles is not valid.");
                        continue;
                    }

                    Instantiate(prefab, position, Quaternion.identity);
                }
            }
        }

        // Play chip placement feedback at chip position.
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
}