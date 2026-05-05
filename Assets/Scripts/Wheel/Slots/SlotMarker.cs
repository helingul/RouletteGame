using UnityEngine;

public class SlotMarker : MonoBehaviour
{
    public int Number { get; private set; }

    [HideInInspector]
    public float LocalAngle { get; private set; }

    public void Initialize(int number, float localAngle)
    {
        Number = number;
        LocalAngle = localAngle;
    }
}

