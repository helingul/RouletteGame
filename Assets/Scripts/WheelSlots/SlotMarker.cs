using UnityEngine;

namespace RouletteGame.WheelSlot
{
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
}