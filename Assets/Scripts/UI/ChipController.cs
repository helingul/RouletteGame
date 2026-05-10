using UnityEngine;
using UnityEngine.UI;

public class ChipController : MonoBehaviour
{
    [SerializeField] private Button button5;
    [SerializeField] private Button button20;
    [SerializeField] private Button button50;
    [SerializeField] private Button button1000;
    [SerializeField] private Button button5000;
    [SerializeField] private ChipTray chipTray;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void Awake()
    {
        button5.onClick.AddListener(OnClick5);
        button20.onClick.AddListener(OnClick20);
        button50.onClick.AddListener(OnClick50);
        button1000.onClick.AddListener(OnClick1000);
        button5000.onClick.AddListener(OnClick5000);

    }
    private void OnDestroy()
    {
        button5.onClick.RemoveListener(OnClick5);
        button20.onClick.RemoveListener(OnClick20);
        button50.onClick.RemoveListener(OnClick50);
        button1000.onClick.RemoveListener(OnClick1000);
        button5000.onClick.RemoveListener(OnClick5000);
    }
    private void OnClick5() => OnAddChipClicked(5);
    private void OnClick20() => OnAddChipClicked(20);
    private void OnClick50() => OnAddChipClicked(50);
    private void OnClick1000() => OnAddChipClicked(1000);
    private void OnClick5000() => OnAddChipClicked(5000);

    private void OnAddChipClicked(int value)
    {
        chipTray.SpawnChip(value);
    }
}
