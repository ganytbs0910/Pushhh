using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;
using UnityEngine;

public class ExampleController : MonoBehaviour, IEnhancedScrollerDelegate
{
    private List<ExampleCellData> _data;

    [SerializeField]
    private EnhancedScroller _scroller;
    [SerializeField]
    private ExampleCellView _cellViewPrefab;

    private void Start()
    {
        // データを作成
        _data = new List<ExampleCellData>();
        for (int i = 0; i < 30; i++)
        {
            _data.Add(new ExampleCellData { message = "cell " + i });
        }

        _scroller.cellViewVisibilityChanged += view =>
        {
            if (view.active)
            {
                // セルが表示状態になった時の処理
                var cellView = (ExampleCellView)view;
                cellView.SetData(_data[view.dataIndex]);
            }
        };

        // セルがインスタンス化されたときの処理
        _scroller.cellViewInstantiated += (scroller, view) =>
        {
            var cellView = (ExampleCellView)view;
            cellView.onClick = x => Debug.Log("Clicked: " + x.dataIndex);
        };

        // Scrollerにデリゲート登録
        _scroller.Delegate = this;
        // ReloadDataをするとビューが更新される
        _scroller.ReloadData();
    }

    // セルの数を返す
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _data.Count;
    }

    // セルのサイズ（縦幅or横幅）を返す
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 50;
    }

    // セルのViewを返す
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        // Scroller.GetCellView()を呼ぶと新規生成orリサイクルを自動的に行ったViewを返してくれる
        return scroller.GetCellView(_cellViewPrefab);
    }
}