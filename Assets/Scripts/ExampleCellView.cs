using EnhancedUI.EnhancedScroller;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ExampleCellData
{
    public string message;
}
public class ExampleCellView : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text _text;
    [SerializeField]
    private Button _button;

    public Action<ExampleCellView> onClick;

    public void SetData(ExampleCellData data)
    {
        _text.text = data.message;
    }

    private void Awake()
    {
        _button.onClick.AddListener(() => onClick?.Invoke(this));
    }
}