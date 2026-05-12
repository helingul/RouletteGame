using UnityEngine;

namespace RouletteGame.Core.Camera
{
    //////////////////////////////////////////////////////////////////////////
    // Controls camera focus transitions during roulette gameplay.
    // Listens to game events and switches between table and wheel views.
    //////////////////////////////////////////////////////////////////////////
    public class CameraController : MonoBehaviour
    {
        // Inspector refs
        [SerializeField] private CameraTransitionController cameraTransitionController;
        [SerializeField] private Transform wheelFocusPoint;
        [SerializeField] private Transform tableFocusPoint;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            SubscribeToEventBus();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEventBus();
        }

        private void HandleSpinStarted()
        {
            cameraTransitionController?.FocusToTarget(wheelFocusPoint);
        }

        private void HandleSpinFinished()
        {
            cameraTransitionController?.FocusToTarget(tableFocusPoint);
        }

        private void SubscribeToEventBus()
        {
            RouletteEventBus.OnSpinStarted += HandleSpinStarted;
            RouletteEventBus.OnSpinFinished += HandleSpinFinished;
        }

        private void UnsubscribeFromEventBus()
        {
            RouletteEventBus.OnSpinStarted -= HandleSpinStarted;
            RouletteEventBus.OnSpinFinished -= HandleSpinFinished;
        }
    }
}