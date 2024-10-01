using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject matchmakingUI;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text roomCountText;

    void Start()
    {
        statusText.text = "";
        roomCountText.text = "";
    }

    void Update()
    {

    }

    public void ShowGameUI()
    {
        matchmakingUI.SetActive(false);
    }

    public void ShowMatchmakingUI()
    {
        matchmakingUI.SetActive(true);
    }

    public void UpdateStatus(string status)
    {
        statusText.text = status;
    }

    public void UpdateRoomCount(string roomCount)
    {
        roomCountText.text = roomCount;
    }
}
