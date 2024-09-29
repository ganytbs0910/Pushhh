using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using TMPro;
using EnhancedScrollerDemos.SnappingDemo;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Linq;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button adsCredit, add5Credit, add25Credit, add55Credit, add120Credit, add250Credit;
    [SerializeField] private Image addCreditCompletePanel;
    [SerializeField] private Image shoppingPanel;
    [SerializeField] private Image lackCreditPanel;
    [SerializeField] private Image winningPanel;
    [SerializeField] private Transform newsContent;
    [SerializeField] private TMP_Text newsText;
    [SerializeField] private Image surprisedImage;
    [SerializeField] private TMP_Text remainPullText;
    [SerializeField] private TMP_Text prizeMoneyInHandText, totalWinningCountText, yourSpinCountText, maximumWinningAmountText;
    [SerializeField] private int maxNewsItems = 10; // 保存するニュース項目の最大数

    private SnappingDemo snappingDemo;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private List<NewsItem> newsItems = new List<NewsItem>();

    [System.Serializable]
    private class NewsItem
    {
        public string timestamp;
        public int winningAmount;
    }

    void Start()
    {
        snappingDemo = GetComponent<SnappingDemo>();
        SetupButtonListeners();
        LoadNewsItems();
        UpdateAllUIElements();
        DisplaySavedNews();
    }

    void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    private void SetupButtonListeners()
    {
        adsCredit.onClick.AddListener(() =>
        {
            Interstitial.instance.showInterstitialAd();
            AddPullCredit(1).Forget();
        });
        add5Credit.onClick.AddListener(() => AddPullCredit(5).Forget());
        add25Credit.onClick.AddListener(() => AddPullCredit(25).Forget());
        add55Credit.onClick.AddListener(() => AddPullCredit(55).Forget());
        add120Credit.onClick.AddListener(() => AddPullCredit(120).Forget());
        add250Credit.onClick.AddListener(() => AddPullCredit(250).Forget());
    }

    private void UpdateAllUIElements()
    {
        RemainCreditTextUpdate();
        PrizeMoneyInHandTextUpdate(PlayerPrefs.GetInt("PrizeMoneyInHand"));
        YourSpinCountTextUpdate();
        TotalWinningCountTextUpdate();
        MaximumWinningAmountTextUpdate();
    }

    public async UniTaskVoid AddPullCredit(int creditCount)
    {
        snappingDemo.AddPullNumber(creditCount);
        shoppingPanel.gameObject.SetActive(false);
        await UniTask.Delay(500, cancellationToken: _cts.Token);
        addCreditCompletePanel.gameObject.SetActive(true);
    }

    public void RemainCreditTextUpdate()
    {
        remainPullText.text = $"残り: {snappingDemo.remainPullNumber}回";
    }

    public void LackCreditPanelOpen()
    {
        lackCreditPanel.gameObject.SetActive(true);
    }

    public void PrizeMoneyInHandTextUpdate(int prizeMoney)
    {
        prizeMoneyInHandText.text = $"所持金額: {prizeMoney}円";
    }

    public void YourSpinCountTextUpdate()
    {
        yourSpinCountText.text = $"{PlayerPrefs.GetInt("LocalCounter")}回";
    }

    public void TotalWinningCountTextUpdate()
    {
        int totalWinningCount = PlayerPrefs.GetInt("TotalWinningCount", 0);
        totalWinningCountText.text = $"{totalWinningCount}回";
    }

    public void MaximumWinningAmountTextUpdate()
    {
        int maximumWinningAmount = PlayerPrefs.GetInt("MaximumWinningAmount", 0);
        maximumWinningAmountText.text = $"{maximumWinningAmount}円";
    }

    public void NewsTextUpdate(int winningAmount)
    {
        string timestamp = System.DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
        AddNewsItem(timestamp, winningAmount);

        // 既存のニュースアイテムを削除
        foreach (Transform child in newsContent)
        {
            Destroy(child.gameObject);
        }

        // 新しいニュースを含めて再表示
        DisplaySavedNews();

        surprisedImage.gameObject.SetActive(true);

        SaveNewsItems();
    }

    private void AddNewsItem(string timestamp, int winningAmount)
    {
        newsItems.Insert(0, new NewsItem { timestamp = timestamp, winningAmount = winningAmount });
        if (newsItems.Count > maxNewsItems)
        {
            newsItems.RemoveAt(newsItems.Count - 1);
        }
    }

    private void SaveNewsItems()
    {
        string json = JsonUtility.ToJson(new NewsItemList { items = newsItems });
        PlayerPrefs.SetString("SavedNewsItems", json);
        PlayerPrefs.Save();
    }

    private void LoadNewsItems()
    {
        if (PlayerPrefs.HasKey("SavedNewsItems"))
        {
            string json = PlayerPrefs.GetString("SavedNewsItems");
            NewsItemList loadedItems = JsonUtility.FromJson<NewsItemList>(json);
            newsItems = loadedItems.items;
        }
    }

    private void DisplaySavedNews()
    {
        foreach (var item in newsItems)
        {
            TMP_Text news = Instantiate(newsText, newsContent);
            news.text = $"【{item.timestamp}】\nゲストプレイさんが{item.winningAmount}円当選しました！";
        }
    }

    [System.Serializable]
    private class NewsItemList
    {
        public List<NewsItem> items;
    }
}