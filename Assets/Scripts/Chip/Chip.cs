//////////////////////////////////////////////////////////////////////////
//  Physical chip GameObject the player drags around.
//  - Uses ChipPool via ReturnToTray() (Object Pool Pattern)
//  - Fires events through RouletteEventBus (Observer Pattern)
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RouletteChip : MonoBehaviour
{
    // Events (still raised for components that listen directly)
    public event Action<RouletteChip> OnReturnedToTray;
    public event Action<RouletteChip> OnPlacedOnBet;

    // Config properties
    [Header("Chip Settings")]
    [SerializeField] private int value = 10;

    [Header("Drag Settings")]
    [SerializeField] private float dragHeight = 0.7f;
    [SerializeField] private float snapAnimDuration = 0.15f;
    [SerializeField] private float returnAnimDuration = 0.25f;
    [SerializeField] private LayerMask tableLayer;
    [SerializeField] private LayerMask betSpotLayer;
    [SerializeField] private LayerMask raycastMask;

    [Header("Visuals")]
    [SerializeField] private GameObject dragGlowEffect;
    [SerializeField] private TrailRenderer dragTrail;

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
    public int Value => value;
    public bool IsDragging => isDragging;
    public bool IsPlaced => currentSpot != null;
    public BetSpot GetCurrentSpot() => currentSpot;

    // Initialisation (called by ChipFactory / ChipTray)
    public void InititalizeChip(int chipValue, ChipTray tray, ChipPool pool = null)
    {
        value = chipValue;
        chipTray = tray;
        chipPool = pool;
    }

    public void SetCurrentSpot(BetSpot spot) => currentSpot = spot;

    public void SetTrayPosition(Vector3 pos, Quaternion rot)
    {
        trayPosition = pos;
        trayRotation = rot;
        transform.position = pos;
        transform.rotation = rot;
    }

    // Unity lifecycle
    private void Awake()
    {
        // TODO: Fix this 
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
            RouletteChip chip = hit.collider.GetComponentInParent<RouletteChip>();
            return chip == this;
        }
        return false;
    }

    private void StartDragging()
    {
        isDragging = true;

        if (currentSpot != null)
            currentSpot.RemoveChip(this);

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (snapCoroutine != null) StopCoroutine(snapCoroutine);

        if (dragGlowEffect != null) dragGlowEffect.SetActive(true);
        if (dragTrail != null) dragTrail.enabled = true;

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

        Plane plane = new Plane(Vector3.up, Vector3.up * dragHeight);
        if (plane.Raycast(ray, out float dist))
            transform.position = ray.GetPoint(dist);
    }

    private void StopDragging()
    {
        isDragging = false;

        if (dragGlowEffect != null) dragGlowEffect.SetActive(false);
        if (dragTrail != null) dragTrail.enabled = false;

        if (hoveredSpot != null) { hoveredSpot.SetHighlight(false); hoveredSpot = null; }

        BetSpot nearest = FindNearestValidSpot();

        if (nearest != null && nearest.CanAcceptChip(this))
        {
            if (nearest.PlaceChip(this) == Result.Success)
                OnPlacedOnBet?.Invoke(this);
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
            trayPosition = chipTray != null
                ? chipTray.GetChipPosition(this)
                : trayPosition;
            OnReturnedToTray?.Invoke(this);
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
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }
}