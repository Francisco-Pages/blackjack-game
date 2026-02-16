using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private GameObject playingCardGroup;
    [SerializeField] private GameObject jokerCardGroup;
    [SerializeField] private GameObject consumableCardGroup;
    private HorizontalCardHolder playerCardHolderScript;
    private HorizontalCardHolder dealerCardHolderScript;
    private HorizontalCardHolder handCardHolderScript;
    private CardVisual dealerHoleCard;

    [SerializeField] private BetManager betManager;
    [SerializeField] private GamestateTextManager gsText;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text gameOverReason;
    private string gameOverText;

    private bool isPlayerTurn = false;
    [SerializeField] private int turnNumber = 0;

    public enum GameState
    {
        StartRound,
        Dealing,
        PlayerTurn,
        DealerTurn,
        ResolveRound,
        EndRound,
        GameOver
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
        handCardHolderScript = consumableCardGroup.GetComponent<HorizontalCardHolder>();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        switch (CurrentState)
        {
            case GameState.StartRound:
                gsText.UpdateGamestateText("");
                if (turnNumber == 0)
                    gsText.UpdateGamestateText("Make your bet and press the deck to start.");
                StartRound();
                break;
            case GameState.Dealing:
                if (turnNumber == 1)
                    gsText.UpdateGamestateText("Good...Good.");
                Dealing();
                break;
            case GameState.PlayerTurn:
                if (turnNumber == 1)
                    gsText.UpdateGamestateText("Now, play a card from your hand.");    
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
            case GameState.GameOver:
                GameOver();
                break;
        }
    }

    private void StartRound()
    {
        if (turnNumber == 0)
        {
            deckManager.deckData.Shuffle();
            StartCoroutine(DealInitialCardsToHand());
        }
        else
        {
            StartCoroutine(DealCardToHand());
        }
        turnNumber++;

        IEnumerator DealInitialCardsToHand()
        {
            yield return new WaitForSecondsRealtime(0.2f);
            deckManager.DealFaceCard(consumableCardGroup);
            yield return new WaitForSecondsRealtime(0.2f);
            deckManager.DealFaceCard(consumableCardGroup);
            yield return new WaitForSecondsRealtime(0.2f);
            deckManager.DealFaceCard(consumableCardGroup);
        }
        IEnumerator DealCardToHand()
        {
            deckManager.DealFaceCard(consumableCardGroup);
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
    private void Dealing()
    {
        // betManager.CommitBet();

        // deckManager.deckData.Shuffle();
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

        if (playerCardHolderScript.CheckBlackjack())
        {
            result = RoundResult.BlackJack;
            gsText.UpdateGamestateText("Blackjack!");
        } 
        else if (playerValue > 21)
        {
            result = RoundResult.PlayerBust;
            playerValue = 0;
            gsText.UpdateGamestateText("Player busts. Dealer wins.");
        }
        else if (dealerValue > 21)
        {
            result = RoundResult.DealerBust;
            dealerValue = 0;
            gsText.UpdateGamestateText("Dealer busts. Player wins.");
        }
        else if (playerValue > dealerValue)
        {
            result = RoundResult.PlayerWin;
            gsText.UpdateGamestateText("Player wins.");
        }
        else if (dealerValue > playerValue)
        {
            result = RoundResult.DealerWin;
            gsText.UpdateGamestateText("Dealer wins.");
        }
        else
        {
            result = RoundResult.Push;
            gsText.UpdateGamestateText("Push.");
        }

        int handsDiff = Math.Abs(playerValue - dealerValue);
        betManager.ResolveBet(result, handsDiff);
        // betManager.CalculateScore(resultScore);
        ChangeState(GameState.EndRound);
    }
    
    private void EndRound()
    {
        // delete all cards (or send them to a discard pile)
        StartCoroutine(RemoveCardsSequential());

        if (betManager.playerChipCount <= 0)
        {
            gameOverText = "You lost all your chips.";
            ChangeState(GameState.GameOver);
            return;
        }
        if (deckManager.deckData.Count < 5)
        {
            gameOverText = "The deck is out of cards.";
            ChangeState(GameState.GameOver);
            return;
        }
        if (betManager.dealerChipCount <= 0)
        {
            gameOverText = "You Win!";
            ChangeState(GameState.GameOver);
            return;
        }
        
    }

    private void GameOver()
    {
        gameOverScreen.SetActive(true);
        gameOverReason.text = gameOverText;
        StopAllCoroutines();
    }
    public void ReloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private IEnumerator RemoveCardsSequential()
    {
        foreach (Card card in playerCardHolderScript.cards)
        {
            card.cardVisual.SetFaceUp(false);
        }
        foreach (Card card in dealerCardHolderScript.cards)
        {
            card.cardVisual.SetFaceUp(false);
        }
        yield return new WaitForSecondsRealtime(0.2f);
        yield return (playerCardHolderScript.EmptyCardHolder());
        yield return new WaitForSecondsRealtime(0.2f);
        yield return (dealerCardHolderScript.EmptyCardHolder());
        yield return new WaitForSecondsRealtime(1f);
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
            gsText.UpdateGamestateText("Player busts!");
            EndPlayerTurn();
            ChangeState(GameState.ResolveRound);
        }
        // else if (handValue == 21)
        // {
        //     gsText.UpdateGamestateText("Player hits 21!");
        //     EndPlayerTurn();
        //     ChangeState(GameState.DealerTurn);
        // }
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

    // public void OnClickedBet()
    // {
    //     if (CurrentState != GameState.StartRound)
    //     return;

    //     betManager.IncreaseBet();
    // }

    public void OnClickedPlayCard()
    {
        if (CurrentState != GameState.PlayerTurn)
        {
            return;
        }
        handCardHolderScript.PlayCardFromHand(playingCardGroup);
        CheckPlayerHand();
        if (turnNumber == 1)
            gsText.UpdateGamestateText("Good...you didn't go over 21...or did you?");
    }
    public void OnClickedRestart()
    {
        ReloadGame();
    }

    private IEnumerator DealerPlay()
    {
        yield return new WaitForSeconds(0.5f); // Small pause after player ends

        int dealerValue = dealerCardHolderScript.GetHandValue();

        gsText.UpdateGamestateText("Dealer starting value: " + dealerValue);

        while (dealerValue < 17)
        {
            gsText.UpdateGamestateText("Dealer hits.");

            deckManager.DealFaceCard(jokerCardGroup);

            yield return new WaitForSeconds(0.6f); // Deal delay

            dealerValue = dealerCardHolderScript.GetHandValue();

            gsText.UpdateGamestateText("Dealer value now: " + dealerValue);
        }

        if (dealerValue > 21)
        {
            gsText.UpdateGamestateText("Dealer busts!");
        }
        else
        {
            gsText.UpdateGamestateText("Dealer stands at " + dealerValue);
        }

        yield return new WaitForSeconds(0.5f);

        ChangeState(GameState.ResolveRound);
    }
}
