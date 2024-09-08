using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializeUI : MonoBehaviour
{
    [SerializeField] private GameObject playerStatusPanel, upperToggle, middlePanel, underPanel;
    [SerializeField] private GameObject infoPanel, MyPagePanel, winnningsPanel, settingPanel, addCreditCompletePanel, LackCreditPanel, shoppingPanel;
    [SerializeField] private GameObject howToPlayPanel, recrivePrizeMoneyPanel, authorThoughts, levelPanel, moneyInProssessionPanel, privacyPanel;

    void Start()
    {
        playerStatusPanel.SetActive(true);
        upperToggle.SetActive(true);
        middlePanel.SetActive(true);
        underPanel.SetActive(true);
        infoPanel.SetActive(false);
        MyPagePanel.SetActive(false);
        winnningsPanel.SetActive(false);
        settingPanel.SetActive(false);
        addCreditCompletePanel.SetActive(false);
        LackCreditPanel.SetActive(false);
        shoppingPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
        recrivePrizeMoneyPanel.SetActive(false);
        authorThoughts.SetActive(false);
        levelPanel.SetActive(true);
        moneyInProssessionPanel.SetActive(false);
        privacyPanel.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
