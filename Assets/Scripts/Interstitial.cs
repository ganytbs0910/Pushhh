using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using TMPro;

public class Interstitial : MonoBehaviour
{
    private InterstitialAd interstitial;

    void Start()
    {
        loadInterstitialAd();
    }

    public void loadInterstitialAd()
    {
#if UNITY_ANDROID
    string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
    string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
        string adUnitId = "unexpected_platform";
#endif
        InterstitialAd.Load(adUnitId, new AdRequest(),
        (InterstitialAd ad, LoadAdError loadAdError) =>
        {
            if (loadAdError != null)
            {
                interstitial.Destroy();
                return;
            }
            else if (ad == null)
            {
                return;
            }
            ad.OnAdFullScreenContentClosed += () =>
            {
                HandleOnAdClosed();
            };
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                HandleOnAdClosed();
            };
            interstitial = ad;
        });
    }

    private void HandleOnAdClosed()
    {
        this.interstitial.Destroy();
        this.loadInterstitialAd();
    }

    public void showInterstitialAd()
    {
        if (interstitial != null)
        {
            if (interstitial.CanShowAd())
            {
                interstitial.Show();
            }
        }
    }
}