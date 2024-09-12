using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using TMPro;
using EnhancedScrollerDemos.SnappingDemo;
using Cysharp.Threading.Tasks; // UniTaskの名前空間を追加
using System.Threading; // CancellationTokenを使用するために追加

public class UIController : MonoBehaviour
{

    [SerializeField] private Button adsCredit, add5Credit, add25Credit, add55Credit, add120Credit, add250Credit;
    [SerializeField] private Image addCreditCompletePanel;
    [SerializeField] private Image shoppingPanel;
    [SerializeField] private Image lackCreditPanel;
    [SerializeField] private TMP_Text remainPullText;
    [SerializeField]
    private TMP_Text prizeMoneyInHandText, totalWinningCountText, yourSpinCountText, maximumWinningAmountText;
    SnappingDemo snappingDemo;

    private CancellationTokenSource _cts = new CancellationTokenSource();

    void Start()
    {
        snappingDemo = GetComponent<SnappingDemo>();
        remainPullText.text = "残り: " + snappingDemo.remainPullNumber + "回";
        adsCredit.onClick.AddListener(() =>
        {
            Interstitial.instance.showInterstitialAd();
            AddPullCredit(1).Forget();
        });
        add5Credit.onClick.AddListener(() =>
        {
            AddPullCredit(5).Forget();
        });
        add25Credit.onClick.AddListener(() =>
        {
            AddPullCredit(25).Forget();
        });
        add55Credit.onClick.AddListener(() =>
        {
            AddPullCredit(55).Forget();
        });
        add120Credit.onClick.AddListener(() =>
        {
            AddPullCredit(120).Forget();
        });
        add250Credit.onClick.AddListener(() =>
        {
            AddPullCredit(250).Forget();
        });
        PrizeMoneyInHandTextUpdate(PlayerPrefs.GetInt("PrizeMoneyInHandText"));
        YourSpinCountTextUpdate();
        TotalWinningCountTextUpdate();
        UpdateHighestPrizeMoney();
    }

    void OnDestroy()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    ///クレジットを追加する
    public async UniTaskVoid AddPullCredit(int creditCount)
    {
        snappingDemo.remainPullNumber += creditCount;
        remainPullText.text = "残り: " + snappingDemo.remainPullNumber + "回";
        PlayerPrefs.SetInt("remainPullNumber", snappingDemo.remainPullNumber);
        shoppingPanel.gameObject.SetActive(false);
        await UniTask.Delay(500, cancellationToken: _cts.Token);
        addCreditCompletePanel.gameObject.SetActive(true);
    }

    public void RemainCreditTextUpdate()
    {
        remainPullText.text = "残り: " + snappingDemo.remainPullNumber + "回";
    }

    public void LackCreditPanelOpen()
    {
        lackCreditPanel.gameObject.SetActive(true);
    }

    // 所持金額を表示する
    public void PrizeMoneyInHandTextUpdate(int prizeMoney)
    {
        prizeMoneyInHandText.text = "所持金額: " + prizeMoney + "円";
    }

    // 自分のスピン回数を表示する
    public void YourSpinCountTextUpdate()
    {
        yourSpinCountText.text = PlayerPrefs.GetInt("LocalCounter") + "回";
    }

    // 総当選回数を表示する
    public void TotalWinningCountTextUpdate()
    {
        if (!PlayerPrefs.HasKey("TotalWinningCount")) PlayerPrefs.SetInt("TotalWinningCount", 0);
        totalWinningCountText.text = PlayerPrefs.GetInt("TotalWinningCount") + "回";
    }

    //過去最高当選金額の更新
    public void UpdateHighestPrizeMoney()
    {
        maximumWinningAmountText.text = PlayerPrefs.GetInt("HighestPrizeMoney") + "円";
    }
}