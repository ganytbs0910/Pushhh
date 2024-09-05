using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using System;
using System.Threading;

public class SlotTutorial : MonoBehaviour
{
    [SerializeField] private UIController uiController;
    private const string FirstLaunchKey = "IsFirstLaunch";
    private const string VersionKey = "GameVersion";
    private const string CurrentVersion = "1.0"; // アップデート時に変更

    private ReactiveProperty<bool> isFirstLaunch = new ReactiveProperty<bool>(true);

    private void Awake()
    {
        CheckFirstLaunch().Forget();
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
        else
        {
            Destroy(gameObject);
        }
    }

    private async UniTask FirstLaunchProcess()
    {
        Debug.Log("初回起動時の処理を実行中...");

        // UIControllerの初期化を待つ
        await UniTask.WaitForEndOfFrame();
        // その他の初回起動時の処理


        Debug.Log("初回起動時の処理が完了しました。");
    }

    private void OnDestroy()
    {
        isFirstLaunch.Dispose();
    }
}