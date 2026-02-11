using UnityEngine;
using System.Collections;



public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameObject playingCardGroup;
    [SerializeField] private GameObject jokerCardGroup;
    private HorizontalCardHolder playerCardHolderScript;
    private HorizontalCardHolder dealerCardHolderScript;
    private CardVisual dealerHoleCard;

    [SerializeField] private BetManager betManager;

    private bool isPlayerTurn = false;

    public enum GameState
    {
        StartRound,
        Dealing,
        PlayerTurn,
        DealerTurn,
        ResolveRound,
        EndRound
    }

    public static GameManager Instance;
    public GameState CurrentState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ChangeState(GameState.StartRound);
        playerCardHolderScript = playingCardGroup.GetComponent<HorizontalCardHolder>();
        dealerCardHolderScript = jokerCardGroup.GetComponent<HorizontalCardHolder>();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (CurrentState)
        {
            case GameState.StartRound:
                StartRound();
                break;
            case GameState.Dealing:
                Dealing();
                break;
            case GameState.PlayerTurn:
                PlayerTurn();
                break;
            case GameState.DealerTurn:
                DealerTurn();
                break;
            case GameState.ResolveRound:
                ResolveRound();
                break;   
            case GameState.EndRound:
                EndRound();
                break;
        }
    }

    private void StartRound()
    {
        
    }
    private void Dealing()
    {
        betManager.CommitBet();

        deckManager.deckData.Shuffle();
        StartCoroutine(DealInitialHand());


        IEnumerator DealInitialHand()
        {
            deckManager.DealFaceCard(playingCardGroup);
            yield return new WaitForSecondsRealtime(0.2f);
            dealerHoleCard = deckManager.DealFaceCard(jokerCardGroup, false, false);
            yield return new WaitForSecondsRealtime(0.2f);
            deckManager.DealFaceCard(playingCardGroup);
            yield return new WaitForSecondsRealtime(0.2f);
            deckManager.DealFaceCard(jokerCardGroup);
            ChangeState(GameState.PlayerTurn);
        }

    }
    private void PlayerTurn()
    {
        Debug.Log("Player's turn.");
        isPlayerTurn = true;
        CheckPlayerHand();
    }
    private void DealerTurn()
    {
        Debug.Log("Dealer's turn.");
        isPlayerTurn = false;
        if (dealerHoleCard != null)
        {
            dealerHoleCard.SetFaceUp(true);
            deckManager.runningCount += deckManager.GetCardCountValue(dealerHoleCard.parentCard.cardData);
            deckManager.faceDownCards.Remove(dealerHoleCard);
            
        }
        StartCoroutine(DealerPlay());
    }
    private void ResolveRound()
    {
        int playerValue = playerCardHolderScript.GetHandValue();
        int dealerValue = dealerCardHolderScript.GetHandValue();
        Debug.Log($"Player: {playerValue} | Dealer: {dealerValue}");

        RoundResult result;

        if (playerValue > 21)
        {
            result = RoundResult.DealerWin;
            Debug.Log("Dealer wins.");
        }
        else if (dealerValue > 21)
        {
            result = RoundResult.PlayerWin;
            Debug.Log("Player wins.");
        }
        else if (playerValue > dealerValue)
        {
            result = RoundResult.PlayerWin;
            Debug.Log("Player wins.");
        }
        else if (dealerValue > playerValue)
        {
            result = RoundResult.DealerWin;
            Debug.Log("Dealer wins.");
        }
        else
        {
            result = RoundResult.Push;
            Debug.Log("Push.");
        }

        betManager.ResolveBet(result);
        ChangeState(GameState.EndRound);
    }
    private void EndRound()
    {
        // delete all cards (or send them to a discard pile)
        playerCardHolderScript.EmptyCardHolder();
        dealerCardHolderScript.EmptyCardHolder();
        ChangeState(GameState.StartRound);
    }

    public void OnClickedDealInitialHand()
    {
        if (CurrentState == GameState.StartRound)
        {
            ChangeState(GameState.Dealing);
        }
    }

    public void OnClickedHit()
    {
        if (!isPlayerTurn) return;
        deckManager.DealFaceCard(playingCardGroup);
        CheckPlayerHand();
    }
    private void CheckPlayerHand()
    {
        int handValue = playerCardHolderScript.GetHandValue();

        if (handValue > 21)
        {
            Debug.Log("Player busts!");
            EndPlayerTurn();
            ChangeState(GameState.ResolveRound);
        }
        else if (handValue == 21)
        {
            Debug.Log("Player hits 21!");
            EndPlayerTurn();
            ChangeState(GameState.DealerTurn);
        }
    }
    private void EndPlayerTurn()
    {
        isPlayerTurn = false;
    }

    public void OnClickedStand()
    {
        if (!isPlayerTurn) return;
        isPlayerTurn = false;
        ChangeState(GameState.DealerTurn);
    }

    public void OnClickedBet()
    {
        if (CurrentState != GameState.StartRound)
        return;

        betManager.IncreaseBet();
    }

    private IEnumerator DealerPlay()
    {
        yield return new WaitForSeconds(0.5f); // Small pause after player ends

        int dealerValue = dealerCardHolderScript.GetHandValue();

        Debug.Log("Dealer starting value: " + dealerValue);

        while (dealerValue < 17)
        {
            Debug.Log("Dealer hits.");

            deckManager.DealFaceCard(jokerCardGroup);

            yield return new WaitForSeconds(0.6f); // Deal delay

            dealerValue = dealerCardHolderScript.GetHandValue();

            Debug.Log("Dealer value now: " + dealerValue);
        }

        if (dealerValue > 21)
        {
            Debug.Log("Dealer busts!");
        }
        else
        {
            Debug.Log("Dealer stands at " + dealerValue);
        }

        yield return new WaitForSeconds(0.5f);

        ChangeState(GameState.ResolveRound);
    }
}
