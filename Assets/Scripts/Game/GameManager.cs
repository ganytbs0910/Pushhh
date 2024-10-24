using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private CardManager playerCardManager;
    [SerializeField] private CardManager botCardManager;
    [SerializeField] private CardUIManager cardUIManager;

    public GameState gameState { get; private set; } = GameState.WaitingToStart;
    public bool isPlayerTurn { get; private set; } = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        startGameButton.onClick.AddListener(StartGame);
        UpdateStatusText();
    }

    public void StartGame()
    {
        gameState = GameState.InProgress;
        playerCardManager.InitializeGame();
        botCardManager.InitializeGame();
        isPlayerTurn = Random.value > 0.5f;
        UpdateStatusText();
        if (!isPlayerTurn)
        {
            BotTurn();
        }
    }

    public void EndTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        UpdateStatusText();
        if (!isPlayerTurn)
        {
            BotTurn();
        }
    }

    private void BotTurn()
    {
        botCardManager.PlayRandomCard();
        EndTurn();
    }

    public void EndGame(bool playerWins)
    {
        gameState = GameState.Finished;
        statusText.text = playerWins ? "あなたの勝ち！" : "ボットの勝ち！";
    }

    private void UpdateStatusText()
    {
        statusText.text = gameState switch
        {
            GameState.WaitingToStart => "スタートを押してゲームを開始",
            GameState.InProgress => isPlayerTurn ? "あなたのターン" : "ボットのターン",
            GameState.Finished => "ゲーム終了",
            _ => "不明な状態"
        };
    }
}

public enum GameState
{
    WaitingToStart,
    InProgress,
    Finished
}