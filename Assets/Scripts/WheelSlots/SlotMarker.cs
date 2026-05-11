using UnityEngine;

public class SlotMarker : MonoBehaviour
{
    public int Number;

    [HideInInspector]
    public float LocalAngle;

    public void Initialize(int number, float localAngle)
    {
        Number = number;
        LocalAngle = localAngle;
    }
}

