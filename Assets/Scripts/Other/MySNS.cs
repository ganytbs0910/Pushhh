using UnityEngine;
using UnityEngine.UI; // UI要素を使用するために必要

public class MySNS : MonoBehaviour
{
    [SerializeField] private string twitterURL = "https://x.com/Gan_tonanoru";
    [SerializeField] private string youtubeURL = "https://www.youtube.com/channel/UC6z6qwz1Mq3E5h35gj7cKjw";
    [SerializeField] private string privacyPolicyURL = "https://gan-tonanoru.studio.site/";
    [SerializeField] private string termsOfServiceURL = "https://gan-termsofservice.studio.site/";

    // ボタンの参照
    [SerializeField] private Button twitterButton;
    [SerializeField] private Button youtubeButton;
    [SerializeField] private Button privacyPolicyButton;
    [SerializeField] private Button termsOfServiceButton;

    void Start()
    {
        // ボタンにリスナーを追加
        if (twitterButton != null) twitterButton.onClick.AddListener(OpenTwitter);
        if (youtubeButton != null) youtubeButton.onClick.AddListener(OpenYouTube);
        if (privacyPolicyButton != null) privacyPolicyButton.onClick.AddListener(OpenPrivacyPolicy);
        if (termsOfServiceButton != null) termsOfServiceButton.onClick.AddListener(OpenTermsOfService);
    }

    void OnDestroy()
    {
        // リスナーの解除（メモリリーク防止）
        if (twitterButton != null) twitterButton.onClick.RemoveListener(OpenTwitter);
        if (youtubeButton != null) youtubeButton.onClick.RemoveListener(OpenYouTube);
        if (privacyPolicyButton != null) privacyPolicyButton.onClick.RemoveListener(OpenPrivacyPolicy);
        if (termsOfServiceButton != null) termsOfServiceButton.onClick.RemoveListener(OpenTermsOfService);
    }

    public void OpenTwitter()
    {
        OpenURL(twitterURL, "Twitter");
    }

    public void OpenYouTube()
    {
        OpenURL(youtubeURL, "YouTube");
    }

    public void OpenPrivacyPolicy()
    {
        OpenURL(privacyPolicyURL, "Privacy Policy");
    }

    public void OpenTermsOfService()
    {
        OpenURL(termsOfServiceURL, "Terms of Service");
    }

    private void OpenURL(string url, string platformName)
    {
        try
        {
            Application.OpenURL(url);
            Debug.Log($"Opening {platformName} URL: {url}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error opening {platformName} URL: {e.Message}");
        }
    }
}