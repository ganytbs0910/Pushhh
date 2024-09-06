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
    private string userId;

    [SerializeField]
    private string databaseUrl = "https://mon-c8c38-default-rtdb.firebaseio.com/";
    [SerializeField] private UIController uiController;

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
                { "lastLogin", DateTime.UtcNow.ToString("o") },
                { "balance", 0 } // 初期残高を0に設定
            });
        }
        else
        {
            // 既存ユーザーの場合、最終ログイン時間を更新
            await dbReference.Child("users").Child(userId).Child("lastLogin").SetValueAsync(DateTime.UtcNow.ToString("o"));
        }

        // ユーザーの残高を同期
        await SyncUserBalance();

        userIdText.text = $"ユーザーIDは「{userId}」です。 ";
    }

    private async UniTask SyncUserBalance()
    {
        try
        {
            var balanceSnapshot = await dbReference.Child("users").Child(userId).Child("balance").GetValueAsync();
            if (balanceSnapshot.Exists)
            {
                int firebaseBalance = Convert.ToInt32(balanceSnapshot.Value);
                int localBalance = PlayerPrefs.GetInt("PrizeMoneyInHand", 0);

                // ローカルの残高とFirebaseの残高が異なる場合、Firebaseの値を使用
                if (firebaseBalance != localBalance)
                {
                    PlayerPrefs.SetInt("PrizeMoneyInHand", firebaseBalance);
                    PlayerPrefs.Save();
                    Debug.Log($"User balance synced from Firebase: {firebaseBalance}");
                }
            }
            else
            {
                // Firebaseに残高が存在しない場合、ローカルの値を使用して初期化
                int localBalance = PlayerPrefs.GetInt("PrizeMoneyInHand", 0);
                await dbReference.Child("users").Child(userId).Child("balance").SetValueAsync(localBalance);
                Debug.Log($"Initial user balance set in Firebase: {localBalance}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to sync user balance: {ex.Message}");
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
        if (allCountText != null)
        {
            allCountText.text = $"{count}回";
        }
        if (currentWinningAmount != null)
        {
            currentWinningAmount.text = $"現在の当選金額: {(int)(count * 0.1f) + 1000}円";
        }
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

    public async UniTask ResetCounter()
    {
        try
        {
            Debug.Log("ResetCounter started");

            if (uiController == null)
            {
                Debug.LogError("UIController is not set. Please assign it in the Inspector.");
                return;
            }

            // 現在の所持金額を取得
            int currentBalance = PlayerPrefs.GetInt("PrizeMoneyInHand", 0);

            // 当選金額を加算
            int newBalance = currentBalance + winningAmount;

            // ローカルの所持金額を更新
            PlayerPrefs.SetInt("PrizeMoneyInHand", newBalance);
            PlayerPrefs.Save();

            // UIを更新
            uiController.PrizeMoneyInHandTextUpdate(newBalance);
            Debug.Log($"1. PrizeMoneyInHandTextUpdate called with new balance: {newBalance}");

            // Firebaseの残高を更新
            if (dbReference != null)
            {
                await dbReference.Child("users").Child(userId).Child("balance").SetValueAsync(newBalance);
                Debug.Log($"2. Firebase balance updated to: {newBalance}");
            }
            else
            {
                Debug.LogError("dbReference is null. Firebase might not be initialized properly.");
            }

            // カウンターと当選金額をリセット
            count = 0;
            winningAmount = 0;
            UpdateCounterDisplay();
            Debug.Log("3. Counter and winning amount reset, display updated");

            // Firebaseのカウンターをリセット
            if (dbReference != null)
            {
                await SaveCounterAsync();
                Debug.Log("4. Firebase counter reset");
            }

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

    public async void RecordWinning()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is not initialized");
            return;
        }

        try
        {
            int currentBalance = PlayerPrefs.GetInt("PrizeMoneyInHand", 0);
            int newBalance = currentBalance + winningAmount;
            PlayerPrefs.SetInt("PrizeMoneyInHand", newBalance);
            PlayerPrefs.Save();

            uiController.PrizeMoneyInHandTextUpdate(newBalance);

            // 当選記録を作成
            Dictionary<string, object> winningRecord = new Dictionary<string, object>
            {
                { "userId", userId },
                { "winningAmount", winningAmount },
                { "newBalance", newBalance },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            // ユーザーの当選履歴に新しいレコードを追加
            DatabaseReference newWinRef = dbReference.Child("users").Child(userId).Child("winnings").Push();
            await newWinRef.SetValueAsync(winningRecord);

            // 全体の当選履歴にも追加
            await dbReference.Child("globalWinnings").Push().SetValueAsync(winningRecord);

            // ユーザーの現在の残高を更新
            await dbReference.Child("users").Child(userId).Child("balance").SetValueAsync(newBalance);

            Debug.Log($"Winning record saved successfully. Amount: {winningAmount}, New Balance: {newBalance}, Record key: {newWinRef.Key}");

            // ユーザーの累計当選金額を更新
            await UpdateTotalWinningsAsync(winningAmount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to record winning: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }
    }

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
}