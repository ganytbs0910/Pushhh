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
    public TextMeshProUGUI allCountText;
    public TextMeshProUGUI currentWinningAmount;

    private int count = 0;
    private DatabaseReference dbReference;

    [SerializeField]
    private string databaseUrl = "https://mon-c8c38-default-rtdb.firebaseio.com/";

    private async void Start()
    {
        try
        {
            await InitializeFirebaseAsync();
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
        currentWinningAmount.text = $"現在の当選金額: {(int)(count * 1.5f)}円";
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
}