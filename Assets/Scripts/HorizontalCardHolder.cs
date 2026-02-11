using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class HorizontalCardHolder : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    public RectTransform rect;

    [Header("Cards in Hand")]
    public List<Card> cards;
    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;
    [SerializeField] private GameObject cardVisualPrefab;
    private VisualCardsHandler visualHandler;
    [HideInInspector] public CardVisual cardVisual;    
    private Canvas canvas;


    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;
    public int totalValue = 0;
    public int aceCount = 0;
    // public int softTotal = 0;

    // public void Initialize()
    // {
    //     // check if there are fewer cards in the deck than cardsToSpawn
    //     List<CardData> currentDeck = deck.GetComponent<Deck>().deckData;

    //     if (currentDeck.Count < cardsToSpawn)
    //     {
    //         cardsToSpawn = currentDeck.Count;
    //     }

    //     for (int i = 0; i < cardsToSpawn; i++)
    //     {
    //         Instantiate(slotPrefab, transform);
    //     }

    //     rect = GetComponent<RectTransform>();
    //     cards = GetComponentsInChildren<Card>().ToList();
    //     int cardCount = 0; 
    //     foreach (Card card in cards)
    //     {
    //         // Assign a CardData from the deck 
    //         card.cardData = currentDeck[0];
    //         // Remove that card from the deck
    //         currentDeck.RemoveAt(0);

    //         // Get CardVisual
    //         CardVisual visual = card.GetComponentInChildren<CardVisual>();
    //         card.cardVisual = visual;
    //         card.PointerEnterEvent.AddListener(CardPointerEnter);
    //         card.PointerExitEvent.AddListener(CardPointerExit);
    //         card.BeginDragEvent.AddListener(BeginDrag);
    //         card.EndDragEvent.AddListener(EndDrag);
    //         card.name = cardCount.ToString();
    //         cardCount++;
    //     }

    //     StartCoroutine(Frame());

    //     IEnumerator Frame()
    //     {
    //         yield return new WaitForSecondsRealtime(.1f);
    //         for (int i = 0; i < cards.Count; i++)
    //         {
    //             if (cards[i].cardVisual != null)
    //                 cards[i].cardVisual.UpdateIndex(transform.childCount);
    //         }
    //     }
    // }


    public void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    public int GetHandValue()
    {
        int currentAceCount = aceCount;
        int currentTotalValue = totalValue;
        while (currentTotalValue > 21 && currentAceCount > 0)
        {
            currentTotalValue -= 10;
            currentAceCount--;
        }
        
        return currentTotalValue;
    }

    public void BeginDrag(Card card)
    {
        selectedCard = card;
    }


    public void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0,selectedCard.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    public void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    public void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    public void UpdateCardsList()
    {
        totalValue = 0;
        aceCount = 0;
        cards = GetComponentsInChildren<Card>().ToList();
        foreach (Card card in cards)
        {
            if ((int)card.cardData.rank == 1)
            {
                totalValue += 11;
                aceCount ++;
            }
            else if ((int)card.cardData.rank >= 11)
            {
                totalValue += 10;
            }
            else
            {
                totalValue += (int)card.cardData.rank;
            }
        }
    }

    void Update()
    {
        // Delete the card from holder if slot object is deleted
        for (int i=0; i<cards.Count; i++)
        {
            if (cards[i] == null)
            {
                cards.RemoveAt(i);
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        for (int i = 0; i < cards.Count; i++)
        {
            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        
        UpdateCardsList();
    }

    public IEnumerator EmptyCardHolder()
    {
        yield return StartCoroutine(RemoveCards());
    }

    IEnumerator RemoveCards()
    {
        for (int i=cards.Count-1; i>=0; i--)
        {
            cards[i].cardVisual.DOKill();
            Destroy(cards[i].transform.parent.gameObject);
            cards.RemoveAt(i);
            yield return new WaitForSecondsRealtime(.2f);
        }
    }


    public CardVisual PlayCardFromHand(GameObject cardGroup)
    {
        if (cards.Count == 0)
        {
            return null;
        }

        Card playedCard = cards[0];


        // create a slot prefab that holds the card's position, 
        // and get the card object inside the card prefab
        GameObject newCard = Instantiate(slotPrefab, cardGroup.transform);
        Card newCardScript = newCard.GetComponentInChildren<Card>();
        
        newCardScript.cardData = cards[0].cardData;
        newCardScript.name = cards[0].name;

        Destroy(cards[0].transform.parent.gameObject);
        cards.Remove(cards[0]);

        // get the card area to anchor the cards to 
        // and add the card to that card area cards list
        HorizontalCardHolder cardGroupScript = cardGroup.GetComponent<HorizontalCardHolder>();
        cardGroupScript.cards.Add(newCardScript);
        cardGroupScript.UpdateCardsList();
        // give it an individual name, in this case the number of the card from the deck
        
        // get the visualHandler and canvas references
        visualHandler = FindFirstObjectByType<VisualCardsHandler>();
        canvas = GetComponentInParent<Canvas>();
        
        // set the visuals to the card
        cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        cardVisual.name = newCardScript.name;
        newCardScript.cardVisual = cardVisual;
        cardVisual.Initialize(newCardScript);

        cardGroupScript.UpdateCardsList();
        
        // if (!faceUp)
        // {
        //     cardVisual.SetFaceUp(false, false);
        //     faceDownCards.Add(cardVisual);
        // }
        // else
        // {
        //     runningCount += GetCardCountValue(newCardScript.cardData);
        // }

        newCardScript.PointerEnterEvent.AddListener(cardGroupScript.CardPointerEnter);
        newCardScript.PointerExitEvent.AddListener(cardGroupScript.CardPointerExit);
        newCardScript.BeginDragEvent.AddListener(cardGroupScript.BeginDrag);
        newCardScript.EndDragEvent.AddListener(cardGroupScript.EndDrag);        
        
        
        return cardVisual;
    }
}
