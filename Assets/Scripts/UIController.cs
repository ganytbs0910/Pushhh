using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button pullButton;
    [SerializeField] private TMP_Text currentWinningAmount;
    [SerializeField] private int winningAmount;

    void Start()
    {
        //currentWinningAmount.text = winningAmount.ToString();
        // pullButton.onClick.AddListener(() =>
        // {
        //     VibrationController.VibrateHeavy();
        // });
    }

    public void AddProbability()
    {

    }
}