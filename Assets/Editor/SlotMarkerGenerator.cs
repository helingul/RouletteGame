#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SlotMarkerGenerator : EditorWindow
{
    [MenuItem("Tools/Create Roulette Slots")]
    public static void CreateSlots()
    {
        // TODO: wheelOrder and pocketRadius should be retrieved from config
        int[] wheelOrder = {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13,
            36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14,
            31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };

        float pocketRadius = 4f;
        int count = wheelOrder.Length;

        GameObject slotsParent = new GameObject("Pockets");
        Undo.RegisterCreatedObjectUndo(slotsParent, "Create Roulette Pockets");

        for (int i = 0; i < count; i++)
        {
            int number = wheelOrder[i];

            float angleDeg = -i * (360f / count);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Cos(angleRad) * pocketRadius,
                0f,
                Mathf.Sin(angleRad) * pocketRadius
            );

            GameObject slot = new GameObject($"Pocket_{number:D2}");
            slot.transform.SetParent(slotsParent.transform);
            slot.transform.localPosition = pos;

            SlotMarker marker = slot.AddComponent<SlotMarker>();
            marker.Initialize(number, angleDeg);
        }

        Selection.activeGameObject = slotsParent;
        Debug.Log($"[SlotMarkerGenerator] {count} slots created.");
    }
}
#endif