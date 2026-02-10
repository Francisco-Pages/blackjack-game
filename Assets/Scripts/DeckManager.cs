using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;
using UnityEngine.UI;
public class DeckManager : MonoBehaviour
{

    [Header("Deck Cards")]
    public List<CardData> deckData;
    [SerializeField] private GameObject cardVisualPrefab;

    private VisualCardsHandler visualHandler;
    [HideInInspector] public CardVisual cardVisual;

    private Canvas canvas;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Sprite cardBack;


    public CardVisual DealFaceCard(GameObject cardGroup, bool faceUp=true, bool animate=true)
    {
        if (deckData.Count == 0)
        {
            return null;
        }

        // create a slot prefab that holds the card's position, 
        // and get the card object inside the card prefab
        GameObject newCard = Instantiate(slotPrefab, cardGroup.transform);
        Card newCardScript = newCard.GetComponentInChildren<Card>();
        
        // set the card data at the top of the deck to this new card
        newCardScript.cardData = deckData[0];
        deckData.RemoveAt(0);

        // get the card area to anchor the cards to 
        // and add the card to that card area cards list
        HorizontalCardHolder cardGroupScript = cardGroup.GetComponent<HorizontalCardHolder>();
        cardGroupScript.cards.Add(newCardScript);
        cardGroupScript.UpdateCardsList();
        // give it an individual name, in this case the number of the card from the deck
        int cardIndex = deckData.Count + 1;
        newCardScript.name = cardIndex.ToString();
        
        // get the visualHandler and canvas references
        visualHandler = FindFirstObjectByType<VisualCardsHandler>();
        canvas = GetComponentInParent<Canvas>();
        
        // set the visuals to the card
        cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        cardVisual.name = cardIndex.ToString();
        newCardScript.cardVisual = cardVisual;
        cardVisual.Initialize(newCardScript);

        if (!faceUp)
        {
            cardVisual.SetFaceUp(false, false);
        }

        newCardScript.PointerEnterEvent.AddListener(cardGroupScript.CardPointerEnter);
        newCardScript.PointerExitEvent.AddListener(cardGroupScript.CardPointerExit);
        newCardScript.BeginDragEvent.AddListener(cardGroupScript.BeginDrag);
        newCardScript.EndDragEvent.AddListener(cardGroupScript.EndDrag);        
        
        return cardVisual;
    }

}
