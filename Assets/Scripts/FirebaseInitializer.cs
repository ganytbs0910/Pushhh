using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using System.Collections.Generic;
using System;
using TMPro;
using Cysharp.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    public Button incrementButton;
    public TextMeshProUGUI allCountText;
    public TextMeshProUGUI currentWinningAmount;
    public TextMeshProUGUI userIdText;

    public int count = 0;
    public int winningAmount = 0;
    private DatabaseReference dbReference;
    private string userId; // 新しく追加：ユーザーID

    [SerializeField]
    private string databaseUrl = "https://mon-c8c38-default-rtdb.firebaseio.com/";
    UIController uiController;

    private async void Start()
    {
        try
        {
            await InitializeFirebaseAsync();
            await InitializeUserAsync();
            await LoadCounterAsync();
            UpdateCounterDisplay();
            incrementButton.onClick.AddListener(IncrementCounter);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in Start: {ex.Message}\nStackTrace: {ex.StackTrace}");
        }
    }

    private async UniTask InitializeUserAsync()
    {
        userId = PlayerPrefs.GetString("UserId", "");
        if (string.IsNullOrEmpty(userId))
        {
            userId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("UserId", userId);
            PlayerPrefs.Save();

            // Firebaseにユーザー情報を保存
            await dbReference.Child("users").Child(userId).SetValueAsync(new Dictionary<string, object>
            {
                { "createdAt", DateTime.UtcNow.ToString("o") },
                { "lastLogin", DateTime.UtcNow.ToString("o") }
            });
        }
        else
        {
            // 既存ユーザーの場合、最終ログイン時間を更新
            await dbReference.Child("users").Child(userId).Child("lastLogin").SetValueAsync(DateTime.UtcNow.ToString("o"));
        }

        userIdText.text = $"User ID: {userId}";
        Debug.Log($"User initialized with ID: {userId}");
    }

    private async UniTask InitializeFirebaseAsync()
    {
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            try
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                FirebaseDatabase database = FirebaseDatabase.GetInstance(app, databaseUrl);
                dbReference = database.RootReference;
                database.RootReference.Child(".info/connected").ValueChanged += (object sender, ValueChangedEventArgs e) =>
                {
                    bool connected = (bool)e.Snapshot.Value;
                };
                await database.RootReference.Child("test").SetValueAsync("Connection test");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Firebase initialization error: {ex.GetType().Name} - {ex.Message}");
                Debug.LogError($"StackTrace: {ex.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }

    private async UniTask LoadCounterAsync()
    {
        try
        {
            var snapshot = await dbReference.Child("counter").GetValueAsync();
            if (snapshot != null && snapshot.Exists)
            {
                count = Convert.ToInt32(snapshot.Value);
            }
            else
            {
                Debug.Log("Counter does not exist in database");
                count = 0;
            }
        }
        catch (DatabaseException dbEx)
        {
            Debug.LogError($"Database exception: {dbEx.Message}");
            Debug.LogError($"InnerException: {dbEx.InnerException?.Message}");
            Debug.LogError($"StackTrace: {dbEx.StackTrace}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase read failed: {ex.GetType().Name} - {ex.Message}");
            Debug.LogError($"InnerException: {ex.InnerException?.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }
    }

    private async void IncrementCounter()
    {
        count++;
        UpdateCounterDisplay();
        await SaveCounterAsync();
    }

    private void UpdateCounterDisplay()
    {
        allCountText.text = $"{count}回";
        currentWinningAmount.text = $"現在の当選金額: {(int)(count * 1.5f) + 1000}円";

    }

    private async UniTask SaveCounterAsync()
    {
        try
        {
            await dbReference.Child("counter").SetValueAsync(count);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase save failed: {ex.GetType().Name} - {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.LogError($"Inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                Debug.LogError($"Inner StackTrace: {ex.InnerException.StackTrace}");
            }
        }
    }

    public async void ResetCounter()
    {
        try
        {
            count = 0;
            UpdateCounterDisplay();
            await SaveCounterAsync();
            Debug.Log("Counter reset successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to reset counter: {ex.GetType().Name} - {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Debug.LogError($"Inner exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                Debug.LogError($"Inner StackTrace: {ex.InnerException.StackTrace}");
            }
        }
    }
    public async UniTask SaveWinningRecordAsync(int amount)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not initialized");
            return;
        }

        try
        {
            // 当選記録を作成
            Dictionary<string, object> winningRecord = new Dictionary<string, object>
            {
                { "amount", amount },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            // ユーザーの当選履歴に新しいレコードを追加
            DatabaseReference newWinRef = dbReference.Child("users").Child(userId).Child("winnings").Push();
            await newWinRef.SetValueAsync(winningRecord);
            string newWinKey = newWinRef.Key;

            // 全体の当選履歴にも追加（ユーザーIDを含める）
            Dictionary<string, object> globalWinningRecord = new Dictionary<string, object>(winningRecord)
            {
                { "userId", userId }
            };
            await dbReference.Child("globalWinnings").Push().SetValueAsync(globalWinningRecord);

            Debug.Log($"Winning record saved successfully. Amount: {amount}, Record key: {newWinKey}");

            // オプション：ユーザーの累計当選金額を更新
            await UpdateTotalWinningsAsync(amount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save winning record: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }
    }

    // 累計当選金額を更新する補助関数
    private async UniTask UpdateTotalWinningsAsync(int newWinAmount)
    {
        try
        {
            var totalWinningsSnapshot = await dbReference.Child("users").Child(userId).Child("totalWinnings").GetValueAsync();
            int currentTotal = totalWinningsSnapshot.Exists ? Convert.ToInt32(totalWinningsSnapshot.Value) : 0;
            int newTotal = currentTotal + newWinAmount;
            await dbReference.Child("users").Child(userId).Child("totalWinnings").SetValueAsync(newTotal);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to update total winnings: {ex.Message}");
        }
    }

    // この関数を呼び出して当選を記録
    public async void RecordWinning()
    {
        winningAmount = (int)(count * 1.5f + 1000);
        PlayerPrefs.SetInt("PrizeMoneyInHand", PlayerPrefs.GetInt("PrizeMoneyInHand") + winningAmount);
        uiController.PrizeMoneyInHandTextUpdate(winningAmount);
        await SaveWinningRecordAsync(winningAmount);
        // ここで UI の更新などの追加の処理を行うことができます
    }

}