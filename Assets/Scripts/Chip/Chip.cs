using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RouletteChip : MonoBehaviour
{
    public event Action<RouletteChip> OnReturnedToTray;
    public event Action<RouletteChip> OnPlacedOnBet;

    [Header("Chip Settings")]
    [SerializeField] private int value = 10;
    [SerializeField] private ChipColor chipColor;

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

    private bool isDragging = false;
    private bool isSnapping = false;
    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private BetSpot currentSpot = null;
    private BetSpot hoveredSpot = null;
    private Camera mainCamera;
    private Coroutine snapCoroutine;

    private Vector3 trayPosition;
    private Quaternion trayRotation;
    private BetSpot[] allSpots;

    private ChipTray chipTray;
    public ChipColor ChipColor => chipColor;
    public int Value => value;
    public BetSpot GetCurrentSpot() => currentSpot;
    public bool IsDragging => isDragging;
    public bool IsPlaced => currentSpot != null;

    public void InititalizeChip(int value, ChipColor chipColor, ChipTray chipTray)
    {
        this.chipColor = chipColor;
        this.value = value;
        this.chipTray = chipTray;
    }
    public void SetCurrentSpot(BetSpot spot)
    {
        currentSpot = spot;
    }
    void Awake()
    {
        allSpots = UnityEngine.Object.FindObjectsByType<BetSpot>(FindObjectsInactive.Include,
                                                     FindObjectsSortMode.None);
        mainCamera = Camera.main;
        trayPosition = transform.position;
        trayRotation = transform.rotation;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private bool IsChipPressed()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return false;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastMask))
        {
            RouletteChip chip = hit.collider.GetComponentInParent<RouletteChip>();

            if (chip != this)
                return false;
        }
        else
        {
            return false;
        }

        return true;
    }
    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(IsChipPressed())
            {
                StartDragging();
            }
        }

        if (Mouse.current.leftButton.isPressed)
        {
            if (!isDragging) return;
            UpdateDragPosition();
            FindAndHighlightNearestSpot();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (!isDragging) return;
            StopDragging();
        }
    }
   
    private void StartDragging()
    {
        isDragging = true;

        // Remove from previous spot
        if (currentSpot != null)
        {
            currentSpot.RemoveChip(this);
        }

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // Stop current coroutine
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);

        // Visual effects
        if (dragGlowEffect != null) dragGlowEffect.SetActive(true);
        if (dragTrail != null) dragTrail.enabled = true;

        // Move chip position up
        transform.position += Vector3.up * dragHeight;
    }

    private void UpdateDragPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        // Check betSpotLayer
        if (Physics.Raycast(ray, out hit, 100f, betSpotLayer))
        {
            Vector3 pos = hit.point;
            pos.y += dragHeight;
            transform.position = pos;
            return;
        }

        // Check tableLayer
        if (Physics.Raycast(ray, out hit, 100f, tableLayer))
        {
            Vector3 pos = hit.point;
            pos.y += dragHeight;
            transform.position = pos;
            return;
        }

        // If both failes move on a plane
        Plane dragPlane = new Plane(Vector3.up, Vector3.up * dragHeight);
        float distance;
        if (dragPlane.Raycast(ray, out distance))
        {
            transform.position = ray.GetPoint(distance);
        }
    }

    private void StopDragging()
    {
        isDragging = false;

        // Remove visual effects
        if (dragGlowEffect != null) dragGlowEffect.SetActive(false);
        if (dragTrail != null) dragTrail.enabled = false;

        // Remove previous highlights
        if (hoveredSpot != null)
        {
            hoveredSpot.SetHighlight(false);
            hoveredSpot = null;
        }

        // Find nearest spot
        BetSpot nearestSpot = FindNearestValidSpot();

        if (nearestSpot != null && nearestSpot.CanAcceptChip(this))
        {
            if(nearestSpot.PlaceChip(this) == Result.Success)
            {
                OnPlacedOnBet?.Invoke(this);
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
        {
            if (hit.collider.TryGetComponent(out BetSpot spot))
            {
                if (spot.CanAcceptChip(this))
                    return spot;
            }
        }

        return null;
    }

    private void FindAndHighlightNearestSpot()
    {
        BetSpot nearest = FindNearestValidSpot();

        if (nearest != hoveredSpot)
        {
            // Remove old highlight
            if (hoveredSpot != null)
                hoveredSpot.SetHighlight(false);

            // Add new highlight
            hoveredSpot = nearest;
            if (hoveredSpot != null)
                hoveredSpot.SetHighlight(true);
        }
    }

    public void SnapToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
        snapCoroutine = StartCoroutine(SmoothMove(targetPos, targetRot, snapAnimDuration));
    }
    public void ReturnToTray()
    {
        bool wasPlacedToBet = currentSpot != null;
       
        if (wasPlacedToBet)
        {
            currentSpot.RemoveChip(this);
            currentSpot = null;
        }

        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);

        if (wasPlacedToBet)
        {
            trayPosition = chipTray.GetChipPosition(this);
            OnReturnedToTray?.Invoke(this);
        }

        snapCoroutine = StartCoroutine(SmoothMove(trayPosition, trayRotation, returnAnimDuration));
    }

    IEnumerator SmoothMove(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        isSnapping = true;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease out cubic
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
        isSnapping = false;
    }
    public void SetTrayPosition(Vector3 pos, Quaternion rot)
    {
        trayPosition = pos;
        trayRotation = rot;
        transform.position = pos;
        transform.rotation = rot;
    }
}