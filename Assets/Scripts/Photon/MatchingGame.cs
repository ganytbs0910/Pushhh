using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Linq;
using System.Collections.Generic;

// ... (previous classes remain unchanged)

public class MatchingGame : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject machMakingUI;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Button matchingButton;
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private byte maxPlayersPerRoom = 2;


    private bool isConnecting = false;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        matchingButton.OnClickAsObservable()
            .Subscribe(_ => StartMatching())
            .AddTo(this);
    }

    public void StartMatching()
    {
        if (isConnecting) return;
        machMakingUI.SetActive(true);
        gameUI.SetActive(false);
        isConnecting = true;
        uiManager.UpdateStatus("サーバーに接続中...");

        if (PhotonNetwork.IsConnected)
        {
            uiManager.UpdateStatus("既に接続済みです。ランダムな部屋に参加しています...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            uiManager.UpdateStatus("ネットワークに接続中...");
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("準備完了！サーバーに接続しました。");
        if (isConnecting)
        {
            uiManager.UpdateStatus("ランダムな部屋に参加しています...");
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        uiManager.UpdateStatus($"切断されました: {cause}。再試行してください。");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        uiManager.UpdateStatus("ランダムな部屋への参加に失敗しました。新しい部屋を作成しています...");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        isConnecting = false;
        uiManager.UpdateStatus($"部屋に参加しました。対戦相手を待っています...");
        uiManager.UpdateRoomCount($"現在のプレイヤー数: {PhotonNetwork.CurrentRoom.PlayerCount}");

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            StartGame();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        uiManager.UpdateStatus($"プレイヤーが参加しました。{PhotonNetwork.CurrentRoom.PlayerCount} / {maxPlayersPerRoom} プレイヤー。");

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersPerRoom)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            uiManager.UpdateStatus("ゲームを開始しています...");
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnLeftRoom()
    {
        uiManager.UpdateStatus("部屋を退出しました。");
        uiManager.ShowMatchmakingUI();
    }
}