//////////////////////////////////////////////////////////////////////////
//  RouletteController.cs
//  Controls the physical wheel and ball animation.
//  Fires RouletteEventBus.RaiseSpinFinished() when done so
//  RouletteGameManager can transition states without coupling.
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Linq;
using UnityEngine;

public class RouletteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform wheelPivot;
    [SerializeField] private RouletteBall ball;

    [Header("Pockets")]
    [SerializeField] private GameObject pocketsParent;

    [Header("Wheel")]
    [SerializeField] private float wheelSpeedDeg = 120f;

    [Header("Ball")]
    [SerializeField] private float spinDuration = 8f;
    [SerializeField] private int extraTurns = 7;

    private SlotMarker[] pockets;
    private float wheelAngleDeg;
    private bool isReadyToSpin = true;

    // Lifecycle
    private void Start()
    {
        pockets = pocketsParent.GetComponentsInChildren<SlotMarker>();
    }

    private void Update()
    {
        wheelAngleDeg += wheelSpeedDeg * Time.deltaTime;
        wheelPivot.rotation = Quaternion.Euler(0f, wheelAngleDeg, 0f);
    }

    // Public API

    // Spins ball to the pocket matching winningNumber.
    // Fires RouletteEventBus.RaiseSpinFinished() when settled.
    public void Spin(int winningNumber)
    {
        if (!isReadyToSpin)
        {
            Debug.LogWarning("[RouletteController] Spin requested while already spinning.");
            return;
        }

        SlotMarker target = pockets.FirstOrDefault(p => p.Number == winningNumber);
        if (target == null)
        {
            Debug.LogError($"[RouletteController] No pocket found for number {winningNumber}!");
            return;
        }

        isReadyToSpin = false;
        StartCoroutine(SpinRoutine(target));
    }

    // Internal
    private IEnumerator SpinRoutine(SlotMarker target)
    {
        float wheelAngleAtEnd = wheelAngleDeg + wheelSpeedDeg * spinDuration;
        float pocketWorldAngleDeg = -wheelAngleAtEnd + target.LocalAngle;
        float targetRad = pocketWorldAngleDeg * Mathf.Deg2Rad;

        ball.StartSpin(targetRad, spinDuration, extraTurns);

        yield return new WaitForSeconds(spinDuration);

        ball.AttachToWheel(wheelPivot);

        yield return new WaitUntil(() => !ball.IsActive);

        isReadyToSpin = true;

        Debug.Log($"[RouletteController] Ball settled – winner: {target.Number}");

        // Notify via event bus instead of calling GameManager directly
        RouletteEventBus.RaiseSpinFinished();
    }
}