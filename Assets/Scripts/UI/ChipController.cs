using UnityEngine;
using UnityEngine.UI;

public class ChipController : MonoBehaviour
{
    [SerializeField] private Button AddChipButton_5;
    [SerializeField] private Button AddChipButton_20;
    [SerializeField] private Button AddChipButton_50;
    [SerializeField] private Button AddChipButton_1000;
    [SerializeField] private Button AddChipButton_5000;

    [SerializeField] private Button UndoButton;
    [SerializeField] private Button ClearButton;

    private void Awake()
    {
        AddChipButton_5.onClick.AddListener(OnClick5);
        AddChipButton_20.onClick.AddListener(OnClick20);
        AddChipButton_50.onClick.AddListener(OnClick50);
        AddChipButton_1000.onClick.AddListener(OnClick1000);
        AddChipButton_5000.onClick.AddListener(OnClick5000);

        UndoButton.onClick.AddListener(UndoLastChip);
        ClearButton.onClick.AddListener(ClerarChips);

    }
    private void OnDestroy()
    {
        AddChipButton_5.onClick.RemoveListener(OnClick5);
        AddChipButton_20.onClick.RemoveListener(OnClick20);
        AddChipButton_50.onClick.RemoveListener(OnClick50);
        AddChipButton_1000.onClick.RemoveListener(OnClick1000);
        AddChipButton_5000.onClick.RemoveListener(OnClick5000);

        UndoButton.onClick.RemoveListener(UndoLastChip);
        ClearButton.onClick.RemoveListener(ClerarChips);
    }
    private void OnClick5() => OnAddChipClicked(5);
    private void OnClick20() => OnAddChipClicked(20);
    private void OnClick50() => OnAddChipClicked(50);
    private void OnClick1000() => OnAddChipClicked(1000);
    private void OnClick5000() => OnAddChipClicked(5000);

    private void OnAddChipClicked(int value)
    {
        RouletteEventBus.RaiseChipAdded(value);
    }

    private void UndoLastChip()
    {
        RouletteGameManager.Instance.UndoLastBet();
    }

    private void ClerarChips()
    {
        RouletteGameManager.Instance.ExecuteClearTable();
    }
}
