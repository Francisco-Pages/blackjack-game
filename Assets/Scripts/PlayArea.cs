using UnityEngine;
using TMPro;

public class PlayArea : MonoBehaviour
{
    [SerializeField] private HorizontalCardHolder dealerHand;
    [SerializeField] private HorizontalCardHolder playerHand;

    [SerializeField] private TMP_Text dealerHandTotalVisual;
    [SerializeField] private TMP_Text playerHandTotalVisual;
    
    public int dealerHandTotal;
    public int dealerAceCount;

    public int playerHandTotal;
    public int playerAceCount;

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
                Debug.Log(card.name);
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
                    Debug.Log(card.name);
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
}
