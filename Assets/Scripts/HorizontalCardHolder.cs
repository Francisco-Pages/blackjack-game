using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using TMPro;

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

    [SerializeField] private GameObject discardPile;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;
    // public int totalValue = 0;
    // public int aceCount = 0;

    public static event Action<GameObject> OnCardsListUpdated;

    public void Start()
    {
        rect = GetComponent<RectTransform>();
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
        cards = GetComponentsInChildren<Card>().ToList();
        OnCardsListUpdated?.Invoke(gameObject);
        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
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

        
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }
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
            // Destroy(cards[i].transform.parent.gameObject);
            // cards.RemoveAt(i);
            DiscardCard(cards[i]);
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
        
        newCardScript.cardData = playedCard.cardData;
        newCardScript.name = playedCard.name;

        playedCard.cardVisual.parentCard = newCardScript;
        playedCard.cardVisual.cardTransform = newCardScript.transform;

        newCardScript.cardVisual = playedCard.cardVisual;
        playedCard.cardVisual = null;

        // playedCard.cardVisual.parentCard.PointerEnterEvent.AddListener(newCardScript.cardVisual.PointerEnter);
        // // parentCard.PointerExitEvent.AddListener(PointerExit);
        // // parentCard.BeginDragEvent.AddListener(BeginDrag);
        // // parentCard.EndDragEvent.AddListener(EndDrag);
        // // parentCard.PointerDownEvent.AddListener(PointerDown);
        // // parentCard.PointerUpEvent.AddListener(PointerUp);
        // // parentCard.SelectEvent.AddListener(Select);

        Destroy(playedCard.transform.parent.gameObject);
        cards.Remove(playedCard);

        // get the card area to anchor the cards to 
        // and add the card to that card area cards list
        HorizontalCardHolder cardGroupScript = cardGroup.GetComponent<HorizontalCardHolder>();
        cardGroupScript.cards.Add(newCardScript);


        this.UpdateCardsList();
        // give it an individual name, in this case the number of the card from the deck
        
        // get the visualHandler and canvas references
        // visualHandler = FindFirstObjectByType<VisualCardsHandler>();
        // canvas = GetComponentInParent<Canvas>();
        
        // // set the visuals to the card
        // cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        // cardVisual.name = newCardScript.name;
        // newCardScript.cardVisual = cardVisual;
        // cardVisual.Initialize(newCardScript);

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

        // card object animations
        newCardScript.PointerEnterEvent.AddListener(cardGroupScript.CardPointerEnter);
        newCardScript.PointerExitEvent.AddListener(cardGroupScript.CardPointerExit);
        // newCardScript.BeginDragEvent.AddListener(cardGroupScript.BeginDrag);
        // newCardScript.EndDragEvent.AddListener(cardGroupScript.EndDrag);  

        // card visual object animations
        newCardScript.PointerEnterEvent.AddListener(newCardScript.cardVisual.PointerEnter);
        newCardScript.PointerExitEvent.AddListener(newCardScript.cardVisual.PointerExit);
        // newCardScript.BeginDragEvent.AddListener(newCardScript.cardVisual.BeginDrag);
        // newCardScript.EndDragEvent.AddListener(newCardScript.cardVisual.EndDrag);
        // newCardScript.PointerDownEvent.AddListener(newCardScript.cardVisual.PointerDown);
        // newCardScript.PointerUpEvent.AddListener(newCardScript.cardVisual.PointerUp);
        // newCardScript.SelectEvent.AddListener(newCardScript.cardVisual.Select);
        return cardVisual;
    }

        public void DiscardCard(Card discardedCard)
    {
        if (cards.Count == 0)
        {
            return;
        }

        // create a slot prefab that holds the card's position, 
        // and get the card object inside the card prefab
        GameObject newCard = Instantiate(slotPrefab, discardPile.transform);
        Card newCardScript = newCard.GetComponentInChildren<Card>();
        
        newCardScript.cardData = discardedCard.cardData;
        newCardScript.name = discardedCard.name;

        discardedCard.cardVisual.parentCard = newCardScript;
        discardedCard.cardVisual.cardTransform = newCardScript.transform;

        newCardScript.cardVisual = discardedCard.cardVisual;
        discardedCard.cardVisual = null;

        newCardScript.cardVisual.SetFaceUp(false);
        // playedCard.cardVisual.parentCard.PointerEnterEvent.AddListener(newCardScript.cardVisual.PointerEnter);
        // // parentCard.PointerExitEvent.AddListener(PointerExit);
        // // parentCard.BeginDragEvent.AddListener(BeginDrag);
        // // parentCard.EndDragEvent.AddListener(EndDrag);
        // // parentCard.PointerDownEvent.AddListener(PointerDown);
        // // parentCard.PointerUpEvent.AddListener(PointerUp);
        // // parentCard.SelectEvent.AddListener(Select);

        Destroy(discardedCard.transform.parent.gameObject);
        cards.Remove(discardedCard);

        // get the card area to anchor the cards to 
        // and add the card to that card area cards list
        HorizontalCardHolder cardGroupScript = discardPile.GetComponent<HorizontalCardHolder>();
        cardGroupScript.cards.Add(newCardScript);


        this.UpdateCardsList();
        // give it an individual name, in this case the number of the card from the deck
        
        // get the visualHandler and canvas references
        // visualHandler = FindFirstObjectByType<VisualCardsHandler>();
        // canvas = GetComponentInParent<Canvas>();
        
        // // set the visuals to the card
        // cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CardVisual>();
        // cardVisual.name = newCardScript.name;
        // newCardScript.cardVisual = cardVisual;
        // cardVisual.Initialize(newCardScript);

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

        // card object animations
        newCardScript.PointerEnterEvent.AddListener(cardGroupScript.CardPointerEnter);
        newCardScript.PointerExitEvent.AddListener(cardGroupScript.CardPointerExit);
        // newCardScript.BeginDragEvent.AddListener(cardGroupScript.BeginDrag);
        // newCardScript.EndDragEvent.AddListener(cardGroupScript.EndDrag);  

        // card visual object animations
        newCardScript.PointerEnterEvent.AddListener(newCardScript.cardVisual.PointerEnter);
        newCardScript.PointerExitEvent.AddListener(newCardScript.cardVisual.PointerExit);
        // newCardScript.BeginDragEvent.AddListener(newCardScript.cardVisual.BeginDrag);
        // newCardScript.EndDragEvent.AddListener(newCardScript.cardVisual.EndDrag);
        // newCardScript.PointerDownEvent.AddListener(newCardScript.cardVisual.PointerDown);
        // newCardScript.PointerUpEvent.AddListener(newCardScript.cardVisual.PointerUp);
        // newCardScript.SelectEvent.AddListener(newCardScript.cardVisual.Select);
    }

    public void DiscardFirstCard()
    {
        DiscardCard(cards[0]);
    }
}
