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

    void Start()
    {
        pockets = wheelPivot.GetComponentsInChildren<SlotMarker>();
        Spin(0);
    }

    void Update()
    {
        wheelAngleDeg += wheelSpeedDeg * Time.deltaTime;
        wheelPivot.rotation = Quaternion.Euler(0f, wheelAngleDeg, 0f);
    }

    public void Spin(int winningNumber)
    {
        if (!isReadyToSpin) return;

        SlotMarker target = pockets.FirstOrDefault(p => p.Number == winningNumber);
        if (target == null)
        {
            Debug.LogError($"[RouletteController] {winningNumber} failed to find winning number!");
            return;
        }

        isReadyToSpin = false;
        StartCoroutine(SpinRoutine(target));
    }

    IEnumerator SpinRoutine(SlotMarker target)
    {
        float wheelAngleAtEnd = wheelAngleDeg + wheelSpeedDeg * spinDuration;

        float pocketWorldAngleDeg = -wheelAngleAtEnd + target.LocalAngle;

        float targetRad = pocketWorldAngleDeg * Mathf.Deg2Rad;

        ball.StartSpin(targetRad, spinDuration, extraTurns);

        yield return new WaitForSeconds(spinDuration);

        ball.AttachToWheel(wheelPivot);

        yield return new WaitUntil(() => !ball.IsActive);

        isReadyToSpin = true;
        Debug.Log($"[RouletteController] Winner: {target.Number}");
    }
}