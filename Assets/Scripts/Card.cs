using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

public class Card : MonoBehaviour
{
    // カードの表面と裏面のスプライト
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;

    // カードのタイトルとサブタイトルのテキスト
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subTitleText;

    // カードの内容を含むRectTransform
    [SerializeField] private RectTransform cardContent;

    // カードのイメージコンポーネント
    private Image cardImage;

    private void Awake()
    {
        // イメージコンポーネントの取得
        cardImage = GetComponent<Image>();
    }



    // カードの初期化と回転を行う関数
    public async void Start()
    {
        // 初期状態の設定
        cardImage.sprite = frontSprite;

        // 1秒待機
        await UniTask.Delay(TimeSpan.FromSeconds(1));

        // カードを180度回転
        await RotateCard(180f, 1f);
    }

    // カードを回転させる関数
    private async UniTask RotateCard(float targetAngle, float duration)
    {
        float startAngle = transform.localEulerAngles.y;
        float elapsedTime = 0f;
        bool halfwayPassed = false;

        while (elapsedTime < duration)
        {
            // 現在の回転角度を計算
            float t = elapsedTime / duration;
            float currentAngle = Mathf.LerpAngle(startAngle, startAngle + targetAngle, t);

            // カードの回転を更新
            transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
            if (cardContent != null)
            {
                cardContent.localRotation = Quaternion.Euler(0, -currentAngle, 0);
            }

            // 90度以上回転したらカードの面を切り替え
            if (!halfwayPassed && Mathf.DeltaAngle(startAngle, currentAngle) >= 90f)
            {
                titleText.gameObject.SetActive(true);
                subTitleText.gameObject.SetActive(true);
                cardImage.sprite = backSprite;
                halfwayPassed = true;
            }

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        // 最終的な回転角度を設定
        transform.localRotation = Quaternion.Euler(0, startAngle + targetAngle, 0);
        if (cardContent != null)
        {
            cardContent.localRotation = Quaternion.Euler(0, -(startAngle + targetAngle), 0);
        }
    }

    public void SetCardDetail(Sprite cardSprite, string title, string subTitle)
    {
        titleText.text = title;
        subTitleText.text = subTitle;
        frontSprite = cardSprite;
    }
}