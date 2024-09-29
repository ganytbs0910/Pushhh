using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using System;
using System.Collections.Generic;
using TMPro;
using Cysharp.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    public Button incrementButton;
    public TextMeshProUGUI currentWinningAmount;
    public TextMeshProUGUI userIdText;
    private int globalSlotCount = 0;
    private DatabaseReference globalSlotCountRef;

    public int count = 0;
    public int winningAmount = 0;
    private DatabaseReference dbReference;
    private string userId;

    [SerializeField] private string databaseUrl = "https://mon-c8c38-default-rtdb.firebaseio.com/";
    [SerializeField] private UIController uiController;

    private bool isInitialized = false;

    private async void Start()
    {
        try
        {
            await InitializeFirebaseAsync();
            await InitializeUserAsync();
            LoadCounterFromPlayerPrefs();
            UpdateCounterDisplay();
            incrementButton.onClick.AddListener(IncrementCounter);
            isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Start内でエラーが発生しました: {ex.Message}");
        }
    }

    private async UniTask InitializeFirebaseAsync()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            FirebaseDatabase database = FirebaseDatabase.GetInstance(app, databaseUrl);
            dbReference = database.RootReference;
            globalSlotCountRef = database.RootReference.Child("globalSlotCount");
        }
        else
        {
            Debug.LogError($"Firebaseの依存関係を解決できませんでした: {dependencyStatus}");
        }
        await UpdateGlobalSlotCount();
    }

    private async UniTask InitializeUserAsync()
    {
        userId = PlayerPrefs.GetString("UserId", "");
        if (string.IsNullOrEmpty(userId))
        {
            userId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("UserId", userId);
            PlayerPrefs.Save();
        }

        userIdText.text = $"ユーザーIDは「{userId}」です。 ";
    }

    private void LoadCounterFromPlayerPrefs()
    {
        count = PlayerPrefs.GetInt("LocalCounter", 0);
    }

    private void IncrementCounter()
    {
        count++;
        PlayerPrefs.SetInt("LocalCounter", count);
        PlayerPrefs.Save();
        UpdateCounterDisplay();
    }

    private void UpdateCounterDisplay()
    {
        if (currentWinningAmount != null)
        {
            winningAmount = (int)(globalSlotCount * 0.1f) + 1000;
            currentWinningAmount.text = $"現在の当選金額: {winningAmount}円";
        }
    }

    public async UniTask ResetCounter()
    {
        if (dbReference != null)
        {
            await ResetGlobalSlotCount();
        }

        count = 0;
        PlayerPrefs.SetInt("LocalCounter", count);
        PlayerPrefs.Save();
        winningAmount = 0;
        UpdateCounterDisplay();
    }

    private async UniTask ResetGlobalSlotCount()
    {
        await globalSlotCountRef.SetValueAsync(0);
        globalSlotCount = 0;
        UpdateCounterDisplay();
    }

    public async UniTask IncrementGlobalSlotCount()
    {
        if (!isInitialized) return;

        await globalSlotCountRef.RunTransaction(mutableData =>
        {
            int currentValue = mutableData.Value != null ? Convert.ToInt32(mutableData.Value) : 0;
            mutableData.Value = currentValue + 1;
            return TransactionResult.Success(mutableData);
        });
        await UpdateGlobalSlotCount();
        UpdateCounterDisplay();
    }

    private async UniTask UpdateGlobalSlotCount()
    {
        var snapshot = await globalSlotCountRef.GetValueAsync();
        globalSlotCount = snapshot.Exists ? Convert.ToInt32(snapshot.Value) : 0;
    }

    public async UniTask<int> CalculateWinningAmount()
    {
        if (!isInitialized) return 1000;

        await UpdateGlobalSlotCount();
        return (int)(globalSlotCount * 0.1f) + 1000;
    }

    // 新しく追加されたメソッド：当選金額をFirebaseに保存する
    public async UniTask SaveWinningAmount(int winAmount)
    {
        if (!isInitialized || string.IsNullOrEmpty(userId))
        {
            Debug.LogError("FirebaseInitializerが初期化されていないか、ユーザーIDが設定されていません");
            return;
        }

        try
        {
            // Dictionary<string, object> winningRecord = new Dictionary<string, object>
            // {
            //     { "winningAmount", winAmount },
            //     { "timestamp", DateTime.UtcNow.ToString("o") }
            // };

            // DatabaseReference newWinRef = dbReference.Child("users").Child(userId).Child("winnings").Push();
            // await newWinRef.SetValueAsync(winningRecord);

            // Debug.Log($"当選金額の保存に成功しました。金額: {winAmount}, レコードキー: {newWinRef.Key}");

            // 累計当選金額の更新
            await UpdateTotalWinningsAsync(winAmount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"当選金額の保存に失敗しました: {ex.Message}");
        }
    }

    public async UniTask UpdateTotalWinningsAsync(int newWinAmount)
    {
        try
        {
            var totalWinningsRef = dbReference.Child("users").Child(userId).Child("totalWinnings");
            await totalWinningsRef.RunTransaction(mutableData =>
            {
                int currentTotal = mutableData.Value != null ? Convert.ToInt32(mutableData.Value) : 0;
                mutableData.Value = currentTotal + newWinAmount;
                return TransactionResult.Success(mutableData);
            });
        }
        catch (Exception ex)
        {
            Debug.LogError($"累計当選金額の更新に失敗しました: {ex.Message}");
        }
    }
}