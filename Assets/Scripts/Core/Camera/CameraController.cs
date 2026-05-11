using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CameraTransitionController cameraTransitionController;
    [SerializeField] private Transform WheelFocusPoint;
    [SerializeField] private Transform TableFocusPoint;

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
        cameraTransitionController?.FocusToTarget(WheelFocusPoint);
    }

    private void HandleSpinFinished()
    {
        cameraTransitionController?.FocusToTarget(TableFocusPoint);
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