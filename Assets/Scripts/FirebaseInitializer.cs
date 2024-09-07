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
            LoadCounterFromPlayerPrefs();
            UpdateCounterDisplay();
            incrementButton.onClick.AddListener(IncrementCounter);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Start内でエラーが発生しました: {ex.Message}\nスタックトレース: {ex.StackTrace}");
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
            }
            catch (Exception ex)
            {
                Debug.LogError($"Firebase初期化エラー: {ex.GetType().Name} - {ex.Message}");
                Debug.LogError($"スタックトレース: {ex.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"Firebaseの依存関係を解決できませんでした: {dependencyStatus}");
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

            await dbReference.Child("users").Child(userId).SetValueAsync(new Dictionary<string, object>
            {
                { "createdAt", DateTime.UtcNow.ToString("o") },
                { "lastLogin", DateTime.UtcNow.ToString("o") },
                { "balance", 0 }
            });
        }
        else
        {
            await dbReference.Child("users").Child(userId).Child("lastLogin").SetValueAsync(DateTime.UtcNow.ToString("o"));
        }

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

                if (firebaseBalance != localBalance)
                {
                    PlayerPrefs.SetInt("PrizeMoneyInHand", firebaseBalance);
                    PlayerPrefs.Save();
                    Debug.Log($"ユーザーの残高をFirebaseから同期しました: {firebaseBalance}");
                }
            }
            else
            {
                int localBalance = PlayerPrefs.GetInt("PrizeMoneyInHand", 0);
                await dbReference.Child("users").Child(userId).Child("balance").SetValueAsync(localBalance);
                Debug.Log($"初期ユーザー残高をFirebaseに設定しました: {localBalance}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ユーザー残高の同期に失敗しました: {ex.Message}");
        }
    }

    private void LoadCounterFromPlayerPrefs()
    {
        count = PlayerPrefs.GetInt("LocalCounter", 0);
    }

    private void IncrementCounter()
    {
        count++;
        SaveCounterToPlayerPrefs();
        UpdateCounterDisplay();
    }

    private void SaveCounterToPlayerPrefs()
    {
        PlayerPrefs.SetInt("LocalCounter", count);
        PlayerPrefs.Save();
    }

    private void UpdateCounterDisplay()
    {
        if (currentWinningAmount != null)
        {
            winningAmount = (int)(count * 0.1f) + 1000;
            currentWinningAmount.text = $"現在の当選金額: {winningAmount}円";
        }
    }

    public async UniTask ResetCounter()
    {
        try
        {
            Debug.Log("カウンターリセットを開始しました");

            if (uiController == null)
            {
                Debug.LogError("UIControllerが設定されていません。Inspectorで割り当ててください。");
                return;
            }

            int currentBalance = PlayerPrefs.GetInt("PrizeMoneyInHand", 0);
            int newBalance = currentBalance + winningAmount;

            PlayerPrefs.SetInt("PrizeMoneyInHand", newBalance);
            PlayerPrefs.Save();
            uiController.PrizeMoneyInHandTextUpdate(newBalance);

            if (dbReference != null)
            {
                await dbReference.Child("users").Child(userId).Child("balance").SetValueAsync(newBalance);
            }
            else
            {
                Debug.LogError("dbReferenceがnullです。Firebaseが正しく初期化されていない可能性があります。");
            }

            count = 0;
            SaveCounterToPlayerPrefs();
            winningAmount = 0;
            UpdateCounterDisplay();
        }
        catch (Exception ex)
        {
            Debug.LogError($"カウンターのリセットに失敗しました: {ex.GetType().Name} - {ex.Message}");
            Debug.LogError($"スタックトレース: {ex.StackTrace}");
        }
    }

    public async void RecordWinning()
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("ユーザーIDが初期化されていません");
            return;
        }

        try
        {
            int currentBalance = PlayerPrefs.GetInt("PrizeMoneyInHand", 0);
            int newBalance = currentBalance + winningAmount;
            PlayerPrefs.SetInt("PrizeMoneyInHand", newBalance);
            PlayerPrefs.Save();

            uiController.PrizeMoneyInHandTextUpdate(newBalance);

            Dictionary<string, object> winningRecord = new Dictionary<string, object>
            {
                { "userId", userId },
                { "winningAmount", winningAmount },
                { "newBalance", newBalance },
                { "timestamp", DateTime.UtcNow.ToString("o") }
            };

            DatabaseReference newWinRef = dbReference.Child("users").Child(userId).Child("winnings").Push();
            await newWinRef.SetValueAsync(winningRecord);

            await dbReference.Child("globalWinnings").Push().SetValueAsync(winningRecord);

            await dbReference.Child("users").Child(userId).Child("balance").SetValueAsync(newBalance);

            Debug.Log($"当選記録の保存に成功しました。金額: {winningAmount}, 新しい残高: {newBalance}, レコードキー: {newWinRef.Key}");

            await UpdateTotalWinningsAsync(winningAmount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"当選の記録に失敗しました: {ex.Message}");
            Debug.LogError($"スタックトレース: {ex.StackTrace}");
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
            Debug.LogError($"累計当選金額の更新に失敗しました: {ex.Message}");
        }
    }
}