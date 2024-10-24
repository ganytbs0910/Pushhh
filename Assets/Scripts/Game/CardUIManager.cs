using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUIManager : MonoBehaviour
{
    [SerializeField] private Transform handContainer;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private CardManager playerCardManager;

    public void UpdateHandUI(int[] hand)
    {
        foreach (Transform child in handContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < hand.Length; i++)
        {
            CreateCardUI(hand[i], i);
        }
    }

    private void CreateCardUI(int cardValue, int index)
    {
        GameObject cardObj = Instantiate(cardPrefab, handContainer);
        cardObj.GetComponentInChildren<TMP_Text>().text = cardValue.ToString();
        Button cardButton = cardObj.GetComponent<Button>();
        cardButton.onClick.AddListener(() => PlayCard(index));
    }

    private void PlayCard(int index)
    {
        if (GameManager.Instance.gameState == GameState.InProgress && GameManager.Instance.isPlayerTurn)
        {
            playerCardManager.PlayCard(index);
            playerCardManager.OnTurnEnd();
            GameManager.Instance.EndTurn();
        }
        else
        {
            Debug.Log("It's not your turn or the game is not in progress.");
        }
    }
}