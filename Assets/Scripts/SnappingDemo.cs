using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SnappingDemo : MonoBehaviour
{
    [SerializeField] private Button spinButton;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text winningStatusText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text remainPullText;

    private LevelSystem levelSystem;
    public int remainPullNumber;
    private const float JACKPOT_PROBABILITY = 0.001f;
    private const float BASE_WIN_PROBABILITY = 0.1f;
    private const int MAX_RESULT = 7; // 最大の結果値（7）を定数として定義
    [SerializeField] private FirebaseInitializer firebaseInitializer;

    void Awake()
    {
        remainPullNumber = PlayerPrefs.GetInt("remainPullNumber", 10);
        spinButton.onClick.AddListener(SpinButton_OnClick);
        levelSystem = new LevelSystem();
        LoadLevelData();
        UpdateLevelUI();
    }

    void Start()
    {
        winningStatusText.text = "";
        UpdateRemainPullText();
    }

    public void SpinButton_OnClick()
    {
        if (remainPullNumber <= 0)
        {
            resultText.text = "霊力が足りません。瞑想して回復してください。";
            return;
        }

        remainPullNumber--;
        PlayerPrefs.SetInt("remainPullNumber", remainPullNumber);
        UpdateRemainPullText();

        int result = DetermineResult();
        DisplayResult(result);
        firebaseInitializer.IncrementGlobalSlotCount();
        levelSystem.AddExp(10);
        UpdateLevelUI();
        SaveLevelData();
    }

    private int DetermineResult()
    {
        float levelBonus = levelSystem.GetWinProbabilityBonus();

        if (Random.value < JACKPOT_PROBABILITY)
        {
            return MAX_RESULT; // 悟り (7)
        }
        else if (Random.value < (BASE_WIN_PROBABILITY + levelBonus))
        {
            return Random.Range(1, MAX_RESULT + 1);
        }
        else
        {
            return Random.Range(1, MAX_RESULT + 1);
        }
    }

    private void DisplayResult(int result)
    {
        resultText.text = $"運勢: {result}";

        switch (result)
        {
            case 1:
                winningStatusText.text = "小吉：心の変化が訪れそうです。内なる声に耳を傾けましょう。";
                levelSystem.AddExp(50);
                break;
            case 3:
                winningStatusText.text = "中吉：良い縁が巡ってきそうです。周りの人々との絆を大切にしましょう。";
                // 課金クーポン獲得ロジックをここに実装
                break;
            case 5:
                winningStatusText.text = "大吉：大きな幸運が訪れそうです。チャンスを逃さず、前進しましょう。";
                // 次回の当選確率上昇ロジックをここに実装
                break;
            case 7:
                int winningAmount = CalculateWinningAmount();
                winningStatusText.text = $"極吉：悟りの境地に近づいています！{winningAmount}の祝福を受けました。";
                UpdateWinningStats(winningAmount);
                firebaseInitializer.ResetCounter();
                break;
            default:
                winningStatusText.text = $"{result}：平穏な日々が続きそうです。日常の小さな幸せを大切にしましょう。";
                break;
        }

        UpdateLevelUI();
    }

    private int CalculateWinningAmount()
    {
        return (int)(firebaseInitializer.count * 0.1f) + 1000;
    }

    private void UpdateWinningStats(int winningAmount)
    {
        int currentTotalWinnings = PlayerPrefs.GetInt("TotalWinningCount", 0) + 1;
        PlayerPrefs.SetInt("TotalWinningCount", currentTotalWinnings);

        int currentMaxWinning = PlayerPrefs.GetInt("MaximumWinningAmount", 0);
        if (winningAmount > currentMaxWinning)
        {
            PlayerPrefs.SetInt("MaximumWinningAmount", winningAmount);
        }
        PlayerPrefs.Save();
        firebaseInitializer.ResetCounter();
    }

    private void UpdateRemainPullText()
    {
        remainPullText.text = $"残り占い回数: {remainPullNumber}";
    }

    private void LoadLevelData()
    {
        levelSystem.CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        levelSystem.CurrentExp = PlayerPrefs.GetInt("CurrentExp", 0);
    }

    private void SaveLevelData()
    {
        PlayerPrefs.SetInt("CurrentLevel", levelSystem.CurrentLevel);
        PlayerPrefs.SetInt("CurrentExp", levelSystem.CurrentExp);
        PlayerPrefs.Save();
    }

    private void UpdateLevelUI()
    {
        levelText.text = $"霊力レベル: {levelSystem.CurrentLevel}";
        expText.text = $"カルマポイント: {levelSystem.CurrentExp} / {levelSystem.ExpToNextLevel}";
    }
}

public class LevelSystem
{
    public int CurrentLevel { get; set; }
    public int CurrentExp { get; set; }
    public int ExpToNextLevel { get; private set; }

    private const int BaseExpToNextLevel = 200;
    private const float ExpGrowthRate = 1.8f;
    private const float ExpCurveStrength = 1.2f;

    public LevelSystem()
    {
        CurrentLevel = 1;
        CurrentExp = 0;
        CalculateExpToNextLevel();
    }

    public void AddExp(int exp)
    {
        CurrentExp += exp;
        while (CurrentExp >= ExpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        CurrentLevel++;
        CurrentExp -= ExpToNextLevel;
        CalculateExpToNextLevel();
    }

    private void CalculateExpToNextLevel()
    {
        ExpToNextLevel = (int)(BaseExpToNextLevel * Mathf.Pow(ExpGrowthRate, CurrentLevel - 1) * Mathf.Pow(CurrentLevel, ExpCurveStrength));
    }

    public float GetWinProbabilityBonus()
    {
        return Mathf.Min((CurrentLevel - 1) * 0.005f, 0.1f);
    }
}