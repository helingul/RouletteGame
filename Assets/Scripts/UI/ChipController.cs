using UnityEngine;
using UnityEngine.UI;

public class ChipController : MonoBehaviour
{
    [SerializeField] private Button addChipButton;
    [SerializeField] private ChipTray chipTray;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void Awake()
    {
        addChipButton.onClick.AddListener(OnAddChipClicked);

    }
    private void OnDestroy()
    {
        addChipButton.onClick.RemoveListener(OnAddChipClicked);
    }

    private void OnAddChipClicked()
    {
        chipTray.SpawnChip(10);
    }
}
