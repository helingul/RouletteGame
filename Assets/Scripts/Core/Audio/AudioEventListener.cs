using UnityEngine;

// NOTE: This class would be extented if the game had more sfx to play.
// AudioEventListener should bind to events to play the sfx, not control any
// sound or sfx.
public class AudioEventListener : MonoBehaviour
{
   [SerializeField] private AudioManager audioManager;

    private void Start()
    {
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

        AudioManager.SfxType type = netEarning > 0 ?
            AudioManager.SfxType.WinSound : AudioManager.SfxType.LoseSound;
        
        audioManager?.PlaySFX(type);
    }
}