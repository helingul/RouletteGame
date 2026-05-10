//////////////////////////////////////////////////////////////////////////
// Handles roullette ball spin logic.
//////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class RouletteBall : MonoBehaviour
{
    // Inspector refs

    [Header("References")]
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private Transform ballMesh;

    [Header("Visual")]
    [SerializeField] private float ballVisualRadius = 0.1f;

    [Header("Spin")]
    [SerializeField] private float startRadius = 5.5f;
    [SerializeField] private float endRadius = 4f;

    [Header("Wobble (last %25)")]
    [Range(0f, 1f)]
    [SerializeField] private float wobbleStartFraction = 0.75f;
    [SerializeField] private float wobbleAmplitude = 0.08f;
    [SerializeField] private float wobbleFrequency = 14f;

    [Header("Settle Bounce")]
    [SerializeField] private float settleDuration = 0.45f;
    [SerializeField] private float bounceHeight = 0.08f;

   
    // Runtime data
    private float currentAngle;
    private float startAngle;
    private float targetAngle;

    private float duration;
    private float timer;

    private bool isSpinning;
    private bool isSettling;
    private float settleTimer;
    private Vector3 settleStartLocalPos;
    private float settleStartRadius;

    private Vector3 lastPosition;

    public bool IsActive => isSpinning || isSettling;

    public void StartSpin(float targetWorldAngleRad, float spinDuration, int extraTurns = 7)
    {
        transform.SetParent(null, worldPositionStays: true);

        startAngle = currentAngle;
        duration = spinDuration;
        timer = 0f;
        isSpinning = true;
        isSettling = false;

        float currentMod = Mathf.Repeat(currentAngle, 2f * Mathf.PI);
        float targetMod = Mathf.Repeat(targetWorldAngleRad, 2f * Mathf.PI);

        float diff = targetMod - currentMod;
        if (diff > 0f) diff -= 2f * Mathf.PI;

        targetAngle = startAngle + diff - extraTurns * 2f * Mathf.PI;

        lastPosition = transform.position;
    }

    public void AttachToWheel(Transform wheelPivot)
    {
        transform.SetParent(wheelPivot, worldPositionStays: true);

        settleStartLocalPos = transform.localPosition;

        Vector3 flat = new Vector3(settleStartLocalPos.x, 0f, settleStartLocalPos.z);
        settleStartRadius = flat.magnitude;

        settleTimer = 0f;
        isSpinning = false;
        isSettling = true;
    }

    void Update()
    {
        if (isSpinning) UpdateSpin();
        else if (isSettling) UpdateSettle();
    }

    void UpdateSpin()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        float eased = EaseOutQuart(t);

        currentAngle = startAngle + (targetAngle - startAngle) * eased;

        float radius = Mathf.Lerp(startRadius, endRadius, eased);

        if (t >= wobbleStartFraction)
        {
            float wt = (t - wobbleStartFraction) / (1f - wobbleStartFraction);
            float decay = 1f - wt;
            radius += Mathf.Sin(wt * wobbleFrequency * Mathf.PI) * wobbleAmplitude * decay;
        }

        transform.position = OrbitPos(currentAngle, radius);
        RotateMesh();
    }

    void UpdateSettle()
    {
        settleTimer += Time.deltaTime;
        float t = Mathf.Clamp01(settleTimer / settleDuration);

        float radialOffset = bounceHeight * (1f - EaseOutBounce(t));
        float currentRadius = settleStartRadius + radialOffset;

        Vector3 flat = new Vector3(settleStartLocalPos.x, 0f, settleStartLocalPos.z);
        Vector3 radialDir = flat.normalized;

        Vector3 local = radialDir * currentRadius;
        local.y = settleStartLocalPos.y;
        transform.localPosition = local;

        if (t >= 1f)
        {
            isSettling = false;
            lastPosition = transform.position;
            Debug.Log("[RouletteBall] Placed onto slot.");
        }
    }

    Vector3 OrbitPos(float angleRad, float radius)
    {
        return orbitCenter.position +
               new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * radius;
    }

    void RotateMesh()
    {
        Vector3 delta = transform.position - lastPosition;
        float distance = delta.magnitude;

        if (distance > 0.00001f)
        {
            Vector3 axis = Vector3.Cross(Vector3.up, delta.normalized);
            float amount = (distance / (2f * Mathf.PI * ballVisualRadius)) * 360f * 0.85f;
            ballMesh.Rotate(axis, amount, Space.World);
        }

        lastPosition = transform.position;
    }

    static float EaseOutQuart(float t) => 1f - Mathf.Pow(1f - t, 4f);

    static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f, d1 = 2.75f;
        if (t < 1f / d1) return n1 * t * t;
        if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
        if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
        t -= 2.625f / d1; return n1 * t * t + 0.984375f;
    }
}