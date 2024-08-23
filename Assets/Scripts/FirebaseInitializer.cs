using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using System;
using TMPro;
using Cysharp.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    public Button incrementButton;
    public TextMeshProUGUI counterText;

    private int count = 0;
    private DatabaseReference dbReference;

    [SerializeField]
    private string databaseUrl = "https://mon-c8c38-default-rtdb.firebaseio.com/";

    private async void Start()
    {
        try
        {
            Debug.Log("Starting Firebase initialization");
            await InitializeFirebaseAsync();
            Debug.Log("Firebase initialization completed");
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
        Debug.Log("Starting Firebase initialization");
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        Debug.Log($"Dependency status: {dependencyStatus}");
        if (dependencyStatus == DependencyStatus.Available)
        {
            try
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log($"Firebase app initialized: {app.Name}");

                FirebaseDatabase database = FirebaseDatabase.GetInstance(app, databaseUrl);
                Debug.Log($"Firebase database instance created: {database.App.Name}");
                dbReference = database.RootReference;
                Debug.Log($"Database reference set: {dbReference.Key}");

                // データベースの接続状態を確認するリスナーを追加
                database.RootReference.Child(".info/connected").ValueChanged += (object sender, ValueChangedEventArgs e) =>
                {
                    bool connected = (bool)e.Snapshot.Value;
                    Debug.Log($"Firebase connection state changed: {(connected ? "connected" : "disconnected")}");
                };

                Debug.Log($"Database URL: {database.App.Options.DatabaseUrl}");

                // データベースへの単純な書き込みテスト
                await database.RootReference.Child("test").SetValueAsync("Connection test");
                Debug.Log("Test write to database successful");
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
            Debug.Log("Starting LoadCounterAsync");
            Debug.Log($"Database reference: {dbReference.Key}");
            var snapshot = await dbReference.Child("counter").GetValueAsync();
            Debug.Log($"Snapshot retrieved: {snapshot != null}");
            if (snapshot != null && snapshot.Exists)
            {
                count = Convert.ToInt32(snapshot.Value);
                Debug.Log($"Counter value loaded: {count}");
            }
            else
            {
                Debug.Log("Counter does not exist in database");
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
        counterText.text = $"Count: {count}";
    }

    private async UniTask SaveCounterAsync()
    {
        try
        {
            await dbReference.Child("counter").SetValueAsync(count);
            Debug.Log($"Firebase save successful. New count: {count}");
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
}