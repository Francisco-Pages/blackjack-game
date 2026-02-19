using UnityEngine;
using TMPro;

public class PlayArea : MonoBehaviour
{
    [SerializeField] private DeckManager deck;
    [SerializeField] private HorizontalCardHolder dealerHand;
    [SerializeField] private HorizontalCardHolder playerHand;

    [SerializeField] private TMP_Text dealerHandTotalVisual;
    [SerializeField] private TMP_Text playerHandTotalVisual;
    
    public int dealerHandTotal;
    public int dealerAceCount;

    public int playerHandTotal;
    public int playerAceCount;

    [SerializeField] private TMP_Text loseProbabilityText;
    public float loseProbability;

    private void Update()
    {
        loseProbability = GetBustProbability();
        loseProbabilityText.text = $"Chance of losing: {loseProbability*100}%";
    }
    private void OnEnable()
    {
        HorizontalCardHolder.OnCardsListUpdated += UpdateHandTotal;
    }
    private void OnDisable()
    {
        HorizontalCardHolder.OnCardsListUpdated -= UpdateHandTotal;
    }
    private void UpdateHandTotal(GameObject cardHolderObj)
    {
        if (cardHolderObj.GetComponent<HorizontalCardHolder>() == playerHand)
        {
            Debug.Log("Player hand updated.");
            playerHandTotal = 0;
            playerAceCount = 0;
            foreach (Card card in playerHand.cards)
            {
                int cardValue = card.GetCardValue();
                playerHandTotal += cardValue;
                if (cardValue == 11)
                    playerAceCount++;
            }            
        }
        if (cardHolderObj.GetComponent<HorizontalCardHolder>() == dealerHand)
        {
            Debug.Log("Dealer hand updated.");
            dealerHandTotal = 0;
            dealerAceCount = 0;
            foreach (Card card in dealerHand.cards)
            {
                if (card.cardVisual != null && card.cardVisual.faceUp)
                {
                    int cardValue = card.GetCardValue();
                    dealerHandTotal += cardValue;
                    if (cardValue == 11)
                        dealerAceCount++;
                }
            }
        }
    }


    public bool CheckBlackjack()
    {
        if (dealerHand.cards.Count == 2 && GetDealerHandValue() == 21)
        {
            return true;
        }
        return false;
    }
    public int GetPlayerHandValue()
    {
        int currentAceCount = playerAceCount;
        int currentTotalValue = playerHandTotal;
        while (currentTotalValue > 21 && currentAceCount > 0)
        {
            currentTotalValue -= 10;
            currentAceCount--;
        }
        playerHandTotalVisual.text = currentTotalValue.ToString();

        return currentTotalValue;
    }

    public int GetDealerHandValue()
    {
        int currentAceCount = dealerAceCount;
        int currentTotalValue = dealerHandTotal;
        while (currentTotalValue > 21 && currentAceCount > 0)
        {
            currentTotalValue -= 10;
            currentAceCount--;
        }
        dealerHandTotalVisual.text = currentTotalValue.ToString();

        return currentTotalValue;
    }

    public float GetBustProbability()
    {
        int minBustCard = 21 - playerHandTotal;
        int bustCardsCount = 0;
        foreach (CardData card in deck.deckData)
        {
            int cardRank = (int)card.rank;
            if (cardRank > 11)
            {
                cardRank = 10;
            }

            if (cardRank > minBustCard)
            {
                bustCardsCount++;
            }
        }
        float bustProbability = bustCardsCount / (float)deck.deckData.Count;
        return bustProbability;
    }

    public void ResetHandCounts()
    {
        dealerHandTotalVisual.text = 0.ToString();
        dealerHandTotal = 0;
        dealerAceCount = 0;

        playerHandTotalVisual.text = 0.ToString();
        playerHandTotal = 0;
        playerAceCount = 0;
    }
}
