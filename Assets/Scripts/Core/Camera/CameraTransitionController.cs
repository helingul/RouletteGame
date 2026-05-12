using UnityEngine;

namespace RouletteGame.Core.Camera
{
    //////////////////////////////////////////////////////////////////////////
    // Handles smooth camera transitions between targets using
    // configurable easing curves and timed interpolation.
    //////////////////////////////////////////////////////////////////////////
    public class CameraTransitionController : MonoBehaviour
    {
        // Inspector refs
        [Header("Movement")]
        [SerializeField] private float moveDuration = 1.5f;

        [Header("Easing")]
        [SerializeField]
        private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        //////////////////////////////////////////////////////////////////////////
        private Vector3 startPosition;
        private Quaternion startRotation;

        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private float timer;
        private bool isMoving;

        //////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            // Interpolate camera position and rotation while transition is active.
            if (!isMoving) return;

            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / moveDuration);

            // Easing
            float easedT = moveCurve.Evaluate(t);

            // Smoothly interpolate position and rotation using easing curve.
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                easedT
            );

            transform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                easedT
            );

            // Finish transition
            // Snap exactly to target.
            if (t >= 1f)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;

                isMoving = false;
            }
        }

        // Start a new camera transition from current state to target transform.
        public void FocusToTarget(Transform target)
        {
            if (target == null) return;

            startPosition = transform.position;
            startRotation = transform.rotation;

            targetPosition = target.position;
            targetRotation = target.rotation;

            timer = 0f;
            isMoving = true;
        }
    }
}