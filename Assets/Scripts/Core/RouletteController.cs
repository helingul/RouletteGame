using RouletteGame.Ball;
using RouletteGame.WheelSlot;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace RouletteGame.Core
{
    //////////////////////////////////////////////////////////////////////////
    // Controls the roulette wheel rotation and ball spin/settle sequence.
    // Coordinates physical simulation timing and emits a spin-finished event
    // when the ball has fully settled into a slot.
    //////////////////////////////////////////////////////////////////////////

    public class RouletteController : MonoBehaviour
    {
        // Inspector refs

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

        //////////////////////////////////////////////////////////////////////////
        
        private SlotMarker[] pockets;
        private float wheelAngleDeg;
        private bool isReadyToSpin = true;

        //////////////////////////////////////////////////////////////////////////
        private void Start()
        {
            pockets = pocketsParent.GetComponentsInChildren<SlotMarker>();
        }

        private void Update()
        {
            // Continuously rotates the wheel at a constant speed.
            wheelAngleDeg += wheelSpeedDeg * Time.deltaTime;
            wheelPivot.rotation = Quaternion.Euler(0f, wheelAngleDeg, 0f);
        }

        // Spins ball to the pocket matching winningNumber.
        public void Spin(int winningNumber)
        {
            // Start a new spin only if wheel is idle.
            if (!isReadyToSpin)
            {
                Debug.LogWarning("[RouletteController] Spin requested while already spinning.");
                return;
            }

            // Find the target pocket corresponding to the winning number.
            SlotMarker target = pockets.FirstOrDefault(p => p.Number == winningNumber);
            if (target == null)
            {
                Debug.LogError($"[RouletteController] No pocket found for number {winningNumber}!");
                return;
            }

            isReadyToSpin = false;
            StartCoroutine(SpinRoutine(target));
        }

        private IEnumerator SpinRoutine(SlotMarker target)
        {
            // Compute final wheel angle and convert target pocket into ball spin angle.
            float wheelAngleAtEnd = wheelAngleDeg + wheelSpeedDeg * spinDuration;
            float pocketWorldAngleDeg = -wheelAngleAtEnd + target.LocalAngle;
            float targetRad = pocketWorldAngleDeg * Mathf.Deg2Rad;

            // Start ball spin animation with deterministic landing target.
            ball.StartSpin(targetRad, spinDuration, extraTurns);

            // Wait for ball spin phase to complete before attaching to wheel.
            yield return new WaitForSeconds(spinDuration);

            // Switch ball to wheel space for final settling behavior.
            ball.AttachToWheel(wheelPivot);

            // Wait until all ball motion (wobble + bounce) finishes.
            yield return new WaitUntil(() => !ball.IsActive);

            isReadyToSpin = true;

            Debug.Log($"[RouletteController] Ball settled – winner: {target.Number}");

            // Notify via event bus instead of calling GameManager directly
            RouletteEventBus.RaiseSpinFinished();
        }
    }
}