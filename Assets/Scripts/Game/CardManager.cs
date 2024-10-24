using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardManager : MonoBehaviour
{
    [SerializeField] private CardUIManager cardUIManager;
    [SerializeField] private bool isPlayerManager;

    private List<int> deck = new List<int>();
    private List<int> hand = new List<int>();
    private int turnsUntilVictory = -1;
    private bool invertWinCondition = false;
    private bool effectInversionActive = false;
    private int effectInversionTurns = 0;

    private Stack<int> lastPlayedCards = new Stack<int>();

    public void InitializeGame()
    {
        InitializeDeck();
        DrawInitialHand();
    }

    private void InitializeDeck()
    {
        deck = Enumerable.Range(0, 15).OrderBy(_ => Random.value).ToList();
    }

    private void DrawInitialHand()
    {
        for (int i = 0; i < 3; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (deck.Count > 0)
        {
            int card = deck[0];
            deck.RemoveAt(0);
            hand.Add(card);
            if (isPlayerManager)
            {
                cardUIManager.UpdateHandUI(hand.ToArray());
            }
        }
        else
        {
            Debug.LogWarning("No cards left in the deck!");
        }
    }

    public void PlayCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < hand.Count)
        {
            int card = hand[cardIndex];
            hand.RemoveAt(cardIndex);
            lastPlayedCards.Push(card);
            if (isPlayerManager)
            {
                cardUIManager.UpdateHandUI(hand.ToArray());
            }
            ApplyCardEffect(card);
        }
        else
        {
            Debug.LogWarning($"Invalid card index: {cardIndex}");
        }
    }

    public void PlayRandomCard()
    {
        if (hand.Count > 0)
        {
            int randomIndex = Random.Range(0, hand.Count);
            PlayCard(randomIndex);
        }
    }

    private void ApplyCardEffect(int card)
    {
        Debug.Log($"Playing card: {card}");
        switch (card)
        {
            case 0:
                Lose();
                break;
            case 1:
                Win();
                break;
            case 2:
                CopyLastCardEffect();
                break;
            case 3:
                EnableFutureCardKnowledge();
                break;
            case 4:
                DuplicateCard();
                break;
            case 5:
                // Link fate effect removed for simplicity
                break;
            case 6:
                // Copy hand effect removed for simplicity
                break;
            case 7:
                turnsUntilVictory = 3;
                break;
            case 8:
                // Give card effect removed for simplicity
                break;
            case 9:
                ActivateRandomCard();
                break;
            case 10:
                invertWinCondition = !invertWinCondition;
                break;
            case 11:
                ContinuousDrawUntilWinOrLoss();
                break;
            case 12:
                effectInversionActive = true;
                effectInversionTurns = 3;
                break;
            case 13:
                DrawBasedOnDiscard();
                break;
            case 14:
                CancelLastEffect();
                break;
            default:
                Debug.LogWarning($"Unknown card effect for card: {card}");
                break;
        }
    }

    private void Win()
    {
        if (effectInversionActive)
        {
            Lose();
            return;
        }
        if (invertWinCondition)
        {
            Lose();
            return;
        }
        GameManager.Instance.EndGame(isPlayerManager);
    }

    private void Lose()
    {
        if (effectInversionActive)
        {
            Win();
            return;
        }
        if (invertWinCondition)
        {
            Win();
            return;
        }
        GameManager.Instance.EndGame(!isPlayerManager);
    }

    private void CopyLastCardEffect()
    {
        if (lastPlayedCards.Count > 1)
        {
            int lastCard = lastPlayedCards.ElementAt(1); // Get the second last card
            ApplyCardEffect(lastCard);
        }
    }

    private void EnableFutureCardKnowledge()
    {
        // This effect is handled in the UI layer
        Debug.Log("Future card knowledge enabled");
    }

    private void DuplicateCard()
    {
        if (hand.Count > 0)
        {
            int cardToDuplicate = hand[Random.Range(0, hand.Count)];
            hand.Add(cardToDuplicate);
            if (isPlayerManager)
            {
                cardUIManager.UpdateHandUI(hand.ToArray());
            }
        }
    }

    private void ActivateRandomCard()
    {
        if (hand.Count > 0)
        {
            int randomIndex = Random.Range(0, hand.Count);
            PlayCard(randomIndex);
        }
    }

    private void ContinuousDrawUntilWinOrLoss()
    {
        while (deck.Count > 0 && GameManager.Instance.gameState == GameState.InProgress)
        {
            DrawCard();
            if (hand.Count > 0)
            {
                PlayCard(hand.Count - 1);
            }
            if (GameManager.Instance.gameState == GameState.Finished)
            {
                break;
            }
        }
    }

    private void DrawBasedOnDiscard()
    {
        int discardCount = Random.Range(0, hand.Count + 1);
        for (int i = 0; i < discardCount; i++)
        {
            if (hand.Count > 0)
            {
                int discardIndex = Random.Range(0, hand.Count);
                hand.RemoveAt(discardIndex);
            }
        }

        for (int i = 0; i < discardCount; i++)
        {
            DrawCard();
        }
    }

    private void CancelLastEffect()
    {
        if (lastPlayedCards.Count > 0)
        {
            lastPlayedCards.Pop();
            Debug.Log("Last card effect cancelled");
        }
    }

    public void OnTurnEnd()
    {
        if (turnsUntilVictory > 0)
        {
            turnsUntilVictory--;
            if (turnsUntilVictory == 0)
            {
                Win();
            }
        }

        if (effectInversionActive)
        {
            effectInversionTurns--;
            if (effectInversionTurns == 0)
            {
                effectInversionActive = false;
            }
        }

        DrawCard();
    }
}