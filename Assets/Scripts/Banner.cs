using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class Banner : MonoBehaviour
{
    private BannerView bannerView;

    public void Start()
    {

        // Google AdMob Initial
        MobileAds.Initialize(initStatus => { });

        this.RequestBanner();
    }

    private void RequestBanner()
    {
#if UNITY_ANDROID
    string adUnitId = "ca-app-pub-3940256099942544/6300978111"; // テスト用広告ユニットID
#elif UNITY_IPHONE
    string adUnitId = "ca-app-pub-3940256099942544/2934735716"; // テスト用広告ユニットID
#else
        string adUnitId = "unexpected_platform";
#endif
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);

    }
}