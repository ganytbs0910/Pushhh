using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using TMPro;
using EnhancedScrollerDemos.SnappingDemo;
using Cysharp.Threading.Tasks;
using System.Threading;

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
    private SnappingDemo snappingDemo;

    private CancellationTokenSource _cts = new CancellationTokenSource();

    void Start()
    {
        snappingDemo = GetComponent<SnappingDemo>();
        SetupButtonListeners();
        UpdateAllUIElements();
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
        if (!PlayerPrefs.HasKey("TotalWinningCount")) PlayerPrefs.SetInt("TotalWinningCount", 0);
        totalWinningCountText.text = $"{PlayerPrefs.GetInt("TotalWinningCount")}回";
        maximumWinningAmountText.text = $"{PlayerPrefs.GetInt("HighestPrizeMoney")}円";
    }

    public void NewsTextUpdate(int winningAmount)
    {
        TMP_Text news = Instantiate(newsText, newsContent);
        news.text = $"【{System.DateTime.Now.ToString("HH:mm")}】 ゲストプレイさんが{winningAmount}円当選しました！";
        surprisedImage.gameObject.SetActive(true);
    }
}