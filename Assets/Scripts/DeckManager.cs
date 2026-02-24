using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.PlayerLoop;
using Unity.VisualScripting;
using UnityEngine.UI;
using TMPro;
public class DeckManager : MonoBehaviour
{

    public static event Action<CardVisual> OnCardDealtFaceDown;
    public static event Action<int> OnRunningCountChanged;

    [Header("Deck Cards")]
    public List<CardData> deckData;
    [SerializeField] private GameObject cardVisualPrefab;

    [SerializeField] private TMP_Text cardsInDeckText;
    private VisualCardsHandler visualHandler;

    private Canvas canvas;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Sprite cardBack;

    public GameObject deckImagePrefab;
    private float deckImageOffset;
    public GameObject deckPosition;
    private Vector3 deckTopPosition;

    private int _runningCount;
    public int runningCount => _runningCount;

    public void CountCard(CardData cardData)
    {
        _runningCount += GetCardCountValue(cardData);
        OnRunningCountChanged?.Invoke(_runningCount);
    }

    public AudioSource audioSource;
    public AudioClip clip;

    // private void CalculateRunningCount()
    // {
    //     foreach (CardData card in deckData)
    //     {
    //         runningCount += GetCardCountValue(card);
    //     }
    // }

    private void OnEnable()
    {
        HorizontalCardHolder.OnCardDiscarded += HandleCardDiscarded;
    }

    private void OnDisable()
    {
        HorizontalCardHolder.OnCardDiscarded -= HandleCardDiscarded;
    }

    private void HandleCardDiscarded(CardData cardData)
    {
        deckData.Add(cardData);
        StartCoroutine(AddTopCard());
    }

    private void Start()
    {
        foreach (CardData card in deckData)
        {
            CountCard(card);
        }
        StartCoroutine(FillDeck());
    }

    private IEnumerator FillDeck()
    {
        for (int i = 0; i < deckData.Count; i++)
        {
            GameObject img = Instantiate(deckImagePrefab, deckPosition.transform);

            deckTopPosition = new Vector3(i * 0.3f,i * 0.3f,0f);
            img.transform.localPosition = deckTopPosition;
            yield return new WaitForSecondsRealtime(0.01f);
        }
        yield return new WaitForSecondsRealtime(0.7f);
    }
    public IEnumerator AddTopCard()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        for (int i = 0; i < deckData.Count; i++)
        {
            deckTopPosition = new Vector3(i * 0.3f,i * 0.3f,0f);
        }
        GameObject img = Instantiate(deckImagePrefab, deckPosition.transform);
        img.transform.localPosition = deckTopPosition;
    }

    private void Update()
    {
        cardsInDeckText.text = deckData.Count.ToString();
    }

    public int GetCardCountValue(CardData cardData)
    {
        if ((int)cardData.rank >= 2 && (int)cardData.rank <= 6)
        {
            return 1;
        }
        if ((int)cardData.rank == 1 || (int)cardData.rank >= 10)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    public void DealFaceCard(GameObject cardGroup, bool faceUp=true, bool animate=true)
    {
        if (deckData.Count == 0)
        {
            return;
        }
        // get the visualHandler and canvas references
        visualHandler = FindFirstObjectByType<VisualCardsHandler>();
        canvas = GetComponentInParent<Canvas>();

        // create a slot prefab that holds the card's position, 
        // and get the card object inside the card prefab
        GameObject newCard = Instantiate(slotPrefab, cardGroup.transform);
        Card newCardScript = newCard.GetComponentInChildren<Card>();
        
        // set the card data at the top of the deck to this new card
        newCardScript.cardData = deckData[0];
        deckData.RemoveAt(0);

        // set the visuals to the card
        CardVisual cardVisual = Instantiate(cardVisualPrefab, transform).GetComponent<CardVisual>();
        cardVisual.transform.localPosition = deckTopPosition;
        newCardScript.cardVisual = cardVisual;
        cardVisual.Initialize(newCardScript);
        
        if (!faceUp)
        {
            cardVisual.SetFaceUp(false, false);
            OnCardDealtFaceDown?.Invoke(cardVisual);
        }
        else
        {
            CountCard(newCardScript.cardData);
        }
        if (transform.childCount > 0)
        {
            Transform lastChild = deckPosition.transform.GetChild(deckPosition.transform.childCount - 1);
            Destroy(lastChild.gameObject);
        }
        // get the card area to anchor the cards to 
        // and add the card to that card area cards list
        HorizontalCardHolder cardGroupScript = cardGroup.GetComponent<HorizontalCardHolder>();
        cardGroupScript.cards.Add(newCardScript);
        cardGroupScript.UpdateCardsList();
        // give it an individual name, in this case the number of the card from the deck
        int cardIndex = deckData.Count + 1;
        newCardScript.name = cardIndex.ToString();
        cardVisual.name = cardIndex.ToString();
        
        audioSource.PlayOneShot(clip);
        

        newCardScript.PointerEnterEvent.AddListener(cardGroupScript.CardPointerEnter);
        newCardScript.PointerExitEvent.AddListener(cardGroupScript.CardPointerExit);
        newCardScript.BeginDragEvent.AddListener(cardGroupScript.BeginDrag);
        newCardScript.EndDragEvent.AddListener(cardGroupScript.EndDrag);        
        
    }

    public void AddCard(CardData cardData)
    {
        if (cardData != null)
            deckData.Add(cardData);
    }

}
