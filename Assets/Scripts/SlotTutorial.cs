using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UniRx;
using System;
using System.Threading;
using DG.Tweening;

public class SlotTutorial : MonoBehaviour
{
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private GameObject slotTutorialPanel;
    [SerializeField] private Button tutorialPanelCloseButton, closeButton;
    [SerializeField] private Image unmaskedPanel, tutorialPanel;
    [SerializeField] private RectTransform arrowImage;
    [SerializeField] private UIController uiController;
    private const string FirstLaunchKey = "IsFirstLaunch";
    private const string VersionKey = "GameVersion";
    private const string CurrentVersion = "1.0"; // アップデート時に変更

    private ReactiveProperty<bool> isFirstLaunch = new ReactiveProperty<bool>(true);

    private void Awake()
    {
        CheckFirstLaunch().Forget();
    }
    void Start()
    {
        tutorialPanelCloseButton.onClick.AddListener(() =>
        {
            tutorialPanel.gameObject.SetActive(false);
            unmaskedPanel.gameObject.SetActive(true);
            //arrowImageを左下に-3移動させるアニメーションを繰り返す
            // 初期位置を保存
            Vector2 initialPosition = arrowImage.anchoredPosition;
            // 移動アニメーションを作成
            Sequence sequence = DOTween.Sequence();
            // 左下に移動
            sequence.Append(arrowImage.DOAnchorPos(initialPosition + new Vector2(-moveDistance, -moveDistance), moveDuration)
                .SetEase(Ease.InOutQuad));
            // 元の位置に戻る
            sequence.Append(arrowImage.DOAnchorPos(initialPosition, 1)
                .SetEase(Ease.InOutQuad));
            // ループを設定
            sequence.SetLoops(-1, LoopType.Restart);
        });
        closeButton.onClick.AddListener(() =>
        {
            tutorialPanel.gameObject.SetActive(false);
            unmaskedPanel.gameObject.SetActive(true);
            //arrowImageを左下に-3移動させるアニメーションを繰り返す
            // 初期位置を保存
            Vector2 initialPosition = arrowImage.anchoredPosition;
            // 移動アニメーションを作成
            Sequence sequence = DOTween.Sequence();
            // 左下に移動
            sequence.Append(arrowImage.DOAnchorPos(initialPosition + new Vector2(-moveDistance, -moveDistance), moveDuration)
                .SetEase(Ease.InOutQuad));
            // 元の位置に戻る
            sequence.Append(arrowImage.DOAnchorPos(initialPosition, 1)
                .SetEase(Ease.InOutQuad));
            // ループを設定
            sequence.SetLoops(-1, LoopType.Restart);
        });
    }

    private async UniTaskVoid CheckFirstLaunch()
    {
        string savedVersion = PlayerPrefs.GetString(VersionKey, "");
        isFirstLaunch.Value = string.IsNullOrEmpty(savedVersion) || savedVersion != CurrentVersion;
        if (isFirstLaunch.Value)
        {
            await FirstLaunchProcess();
            PlayerPrefs.SetString(VersionKey, CurrentVersion);
            PlayerPrefs.Save();
        }
        else Destroy(gameObject);
    }

    private async UniTask FirstLaunchProcess()
    {
        Debug.Log("初回起動時の処理を実行中...");
        // UIControllerの初期化を待つ
        await UniTask.WaitForEndOfFrame();
        slotTutorialPanel.gameObject.SetActive(true);
        // その他の初回起動時の処理
        tutorialPanel.gameObject.SetActive(true);
        Debug.Log("初回起動時の処理が完了しました。");
    }

    private void OnDestroy()
    {
        isFirstLaunch.Dispose();
    }
}