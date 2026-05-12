using RouletteGame.Core;
using UnityEngine;

namespace RouletteGame.Core.Audio
{
    //////////////////////////////////////////////////////////////////////////
    // Listens to roulette gameplay events and triggers corresponding
    // sound effects through the AudioManager.
    // Keeps audio playback decoupled from gameplay systems.
    //////////////////////////////////////////////////////////////////////////
    public class AudioEventListener : MonoBehaviour
    {
        // Inspector refs

        [SerializeField] private AudioManager audioManager;
        
        //////////////////////////////////////////////////////////////////////////
        private void Start()
        {
            // Subscribe to gameplay events that should trigger audio feedback.
            RouletteEventBus.OnSpinStarted += HandleSpinStarted;
            RouletteEventBus.OnRoundResult += HandleRoundResult;
        }

        private void OnDestroy()
        {
            RouletteEventBus.OnSpinStarted -= HandleSpinStarted;
            RouletteEventBus.OnRoundResult -= HandleRoundResult;
        }

        private void HandleSpinStarted()
        {
            audioManager?.PlaySFX(AudioManager.SfxType.RouletteBallSpin);
        }

        private void HandleRoundResult(int netEarning)
        {
            if (netEarning == 0) return;

            // Select win or lose sound based on round outcome.
            AudioManager.SfxType type = netEarning > 0 ?
                AudioManager.SfxType.WinSound : AudioManager.SfxType.LoseSound;

            audioManager?.PlaySFX(type);
        }
    }
}