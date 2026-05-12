using RouletteGame.Bets;
using RouletteGame.Common;
using RouletteGame.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace RouletteGame.Chip
{
    //////////////////////////////////////////////////////////////////////////
    // Represents a draggable roulette chip that can be placed on bet spots,
    // snapped back to the tray, and reused through object pooling.
    // Handles drag interaction, highlighting, animations, and table placement.
    //////////////////////////////////////////////////////////////////////////
    public class Chip : MonoBehaviour
    {
        // Inspector refs
        [Header("Drag Settings")]
        [SerializeField] private float dragHeight = 0.7f;
        [SerializeField] private float snapAnimDuration = 0.15f;
        [SerializeField] private float returnAnimDuration = 0.25f;
        [SerializeField] private LayerMask tableLayer;
        [SerializeField] private LayerMask betSpotLayer;
        [SerializeField] private LayerMask raycastMask;

        //////////////////////////////////////////////////////////////////////////
        // Runtime state
        private bool isDragging;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private BetSpot currentSpot;
        private BetSpot hoveredSpot;
        private Camera mainCamera;
        private Coroutine snapCoroutine;

        private Vector3 trayPosition;
        private Quaternion trayRotation;
        private BetSpot[] allSpots;

        private ChipTray chipTray;
        private ChipPool chipPool;   // injected – used for pooled return

        // Public properties
        public int Value { get; private set; }
        public bool IsDragging => isDragging;
        public bool IsPlaced => currentSpot != null;

        //////////////////////////////////////////////////////////////////////////
        public BetSpot GetCurrentSpot() => currentSpot;

        public void SetCurrentSpot(BetSpot spot) => currentSpot = spot;

        // Initialization (called by ChipFactory / ChipTray)
        public void InitializeChip(int chipValue, ChipTray tray, ChipPool pool = null)
        {
            Value = chipValue;
            chipTray = tray;
            chipPool = pool;
        }

        public void SetTrayPosition(Vector3 pos, Quaternion rot)
        {
            trayPosition = pos;
            trayRotation = rot;
            transform.position = pos;
            transform.rotation = rot;
        }

        private void Awake()
        {
            // Cache all bet spots for drag hover / placement checks.
            // TODO: Replace scene-wide lookup with injected references.
            allSpots = UnityEngine.Object.FindObjectsByType<BetSpot>(
                               FindObjectsInactive.Include, FindObjectsSortMode.None);
            mainCamera = Camera.main;
            trayPosition = transform.position;
            trayRotation = transform.rotation;
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && IsChipPressed()) StartDragging();
            
            if (Mouse.current.leftButton.isPressed && isDragging)
            {
                UpdateDragPosition();
                FindAndHighlightNearestSpot();
            }
            
            if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging) StopDragging();
        }

        // Drag logic
        private bool IsChipPressed()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return false;

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastMask))
            {
                Chip chip = hit.collider.GetComponentInParent<Chip>();
                return chip == this;
            }

            return false;
        }

        private void StartDragging()
        {
            isDragging = true;

            // Temporarily detach chip from its current spot while dragging.
            if (currentSpot != null) currentSpot.RemoveChip(this);

            originalPosition = transform.position;
            originalRotation = transform.rotation;

            if (snapCoroutine != null) StopCoroutine(snapCoroutine);

            transform.position += Vector3.up * dragHeight;
        }

        private void UpdateDragPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, betSpotLayer))
            {
                transform.position = hit.point + Vector3.up * dragHeight;
                return;
            }
            if (Physics.Raycast(ray, out hit, 100f, tableLayer))
            {
                transform.position = hit.point + Vector3.up * dragHeight;
                return;
            }

            // Fallback drag plane when no collider is hit.
            Plane plane = new Plane(Vector3.up, Vector3.up * dragHeight);
            if (plane.Raycast(ray, out float dist)) transform.position = ray.GetPoint(dist);
        }

        private void StopDragging()
        {
            isDragging = false;

            if (hoveredSpot != null) { hoveredSpot.SetHighlight(false); hoveredSpot = null; }

            BetSpot nearest = FindNearestValidSpot();
            
            // Try placing on a valid spot; otherwise return to tray.
            if (nearest != null && nearest.CanAcceptChip(this))
            {
                if (RouletteGameManager.Instance.ExecutePlaceChip(this, nearest) == Result.Failure)
                {
                    ReturnToTray();
                }
            }
            else
            {
                ReturnToTray();
            }
        }

        private BetSpot FindNearestValidSpot()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, betSpotLayer))
                if (hit.collider.TryGetComponent(out BetSpot spot) && spot.CanAcceptChip(this))
                    return spot;
            return null;
        }

        private void FindAndHighlightNearestSpot()
        {
            BetSpot nearest = FindNearestValidSpot();
            if (nearest == hoveredSpot) return;

            if (hoveredSpot != null) hoveredSpot.SetHighlight(false);
            hoveredSpot = nearest;
            if (hoveredSpot != null) hoveredSpot.SetHighlight(true);
        }

        public void SnapToPosition(Vector3 targetPos, Quaternion targetRot)
        {
            if (snapCoroutine != null) StopCoroutine(snapCoroutine);
            snapCoroutine = StartCoroutine(SmoothMove(targetPos, targetRot, snapAnimDuration));
        }

        // Returns chip to the tray.
        // If a ChipPool is assigned, the chip will be returned to the pool
        // after the animation completes instead of staying alive.
        public void ReturnToTray()
        {
            bool wasPlaced = currentSpot != null;

            if (wasPlaced)
            {
                currentSpot.RemoveChip(this);
                currentSpot = null;
            }

            if (snapCoroutine != null) StopCoroutine(snapCoroutine);

            if (wasPlaced)
            {
                // Refresh tray target position in case chip position in tray has changed.
                trayPosition = chipTray != null
                    ? chipTray.GetChipPosition(this)
                    : trayPosition;
            }

            snapCoroutine = StartCoroutine(ReturnAndPool(trayPosition, trayRotation, returnAnimDuration));
        }

        private IEnumerator ReturnAndPool(Vector3 targetPos, Quaternion targetRot, float duration)
        {
            yield return SmoothMove(targetPos, targetRot, duration);

            // If managed by a pool, return there; otherwise just stay put (tray manages it)
            if (chipPool != null)
                chipPool.Return(this);
        }

        private IEnumerator SmoothMove(Vector3 targetPos, Quaternion targetRot, float duration)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Cubic ease-out for smoother chip movement.
                float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;
        }
    }
}