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

    [SerializeField] private PlayArea playArea;

    [SerializeField] private BetManager betManager;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text gameOverReason;
    private string gameOverText;

    private bool isPlayerTurn = false;
    [SerializeField] private int turnNumber = 0;

    private static readonly WaitForSecondsRealtime Wait02 = new(0.2f);
    private static readonly WaitForSecondsRealtime Wait1  = new(1f);
    private static readonly WaitForSecondsRealtime Wait3  = new(3f);
    private static readonly WaitForSeconds         Wait05 = new(0.5f);
    private static readonly WaitForSeconds         Wait06 = new(0.6f);

    public static event Action<GameState> OnGameStateChanged;
    public static event Action<RoundResult, int> OnRoundResolved;
    public static event Action<string> OnGameMessage;
    public static event Action OnPlayerTurnStarted;
    public static event Action OnPlayerTurnEnded;
    public static event Action OnGameOver;
    
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
        playerCardHolderScript = playingCardGroup.GetComponent<HorizontalCardHolder>();
        dealerCardHolderScript = jokerCardGroup.GetComponent<HorizontalCardHolder>();
        handCardHolderScript = consumableCardGroup.GetComponent<HorizontalCardHolder>();
        ChangeState(GameState.StartRound);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;

        // Broadcast to listeners
        OnGameStateChanged?.Invoke(newState);

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
            case GameState.GameOver:
                GameOver();
                break;
        }
    }

    private void StartRound()
    {
        playArea.ResetHandCounts();
        if (turnNumber == 0)
        {
            deckManager.deckData.Shuffle();
            StartCoroutine(DealInitialCardsToHand());
        }
        
        if (handCardHolderScript.cards.Count < 3 && turnNumber != 0)
        {
            StartCoroutine(DealCardToHand());
        }
        turnNumber++;

        IEnumerator DealInitialCardsToHand()
        {
            yield return Wait02;
            deckManager.DealFaceCard(consumableCardGroup);
            yield return Wait02;
            deckManager.DealFaceCard(consumableCardGroup);
            yield return Wait02;
            deckManager.DealFaceCard(consumableCardGroup);
        }
        IEnumerator DealCardToHand()
        {
            deckManager.DealFaceCard(consumableCardGroup);
            yield return Wait02;
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
            yield return Wait02;
            deckManager.DealFaceCard(jokerCardGroup);
            yield return Wait02;
            deckManager.DealFaceCard(playingCardGroup);
            yield return Wait02;
            deckManager.DealFaceCard(jokerCardGroup, false, false);
            ChangeState(GameState.PlayerTurn);
        }

    }
    private void PlayerTurn()
    {
        Debug.Log("Player's turn.");
        isPlayerTurn = true;
        OnPlayerTurnStarted?.Invoke();
        CheckPlayerHand();
    }
    private void EndPlayerTurn()
    {
        Debug.Log("Player turn ended.");
        isPlayerTurn = false;
        OnPlayerTurnEnded?.Invoke();
    }

    private void DealerTurn()
    {
        Debug.Log("Dealer's turn.");
        isPlayerTurn = false;

        playArea.RevealFaceDownCards();

        StartCoroutine(DealerPlay());
    }
    private void ResolveRound()
    {
        int playerValue = playArea.GetPlayerHandValue();
        int dealerValue = playArea.GetDealerHandValue();
        Debug.Log($"Player: {playerValue} | Dealer: {dealerValue}");

        RoundResult result;

        if (playArea.CheckBlackjack())
        {
            result = RoundResult.BlackJack;
            OnGameMessage?.Invoke("Blackjack!");
        }
        else if (playerValue > 21)
        {
            result = RoundResult.PlayerBust;
            playerValue = 0;
            OnGameMessage?.Invoke("Player busts. Dealer wins.");
        }
        else if (dealerValue > 21)
        {
            result = RoundResult.DealerBust;
            dealerValue = 0;
            OnGameMessage?.Invoke("Dealer busts. Player wins.");
        }
        else if (playerValue > dealerValue)
        {
            result = RoundResult.PlayerWin;
            OnGameMessage?.Invoke("Player wins.");
        }
        else if (dealerValue > playerValue)
        {
            result = RoundResult.DealerWin;
            OnGameMessage?.Invoke("Dealer wins.");
        }
        else
        {
            result = RoundResult.Push;
            OnGameMessage?.Invoke("Push.");
        }

        int handsDiff = Math.Abs(playerValue - dealerValue);
        OnRoundResolved?.Invoke(result, handsDiff);
        ChangeState(GameState.EndRound);
    }
    
    private void EndRound()
    {
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

        OnGameOver?.Invoke();

        StopAllCoroutines();
    }
    public void ReloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private IEnumerator RemoveCardsSequential()
    {
        // yield return Wait3;
        yield return new WaitUntil(() => ScoreBreakdownUI.IsDismissed);
        playArea.ResetHandCounts();
        foreach (Card card in playerCardHolderScript.cards)
        {
            card.cardVisual.SetFaceUp(false);
        }
        foreach (Card card in dealerCardHolderScript.cards)
        {
            card.cardVisual.SetFaceUp(false);
        }
        yield return Wait02;
        yield return playerCardHolderScript.EmptyCardHolder();
        yield return Wait02;
        yield return dealerCardHolderScript.EmptyCardHolder();
        yield return Wait1;
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
        int handValue = playArea.GetPlayerHandValue();

        if (handValue > 21)
        {
            OnGameMessage?.Invoke("Player busts!");
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
    // private void EndPlayerTurn()
    // {
    //     isPlayerTurn = false;
    // }

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
        handCardHolderScript.PlayCard(playingCardGroup);
        CheckPlayerHand();
        if (turnNumber == 1)
            OnGameMessage?.Invoke("Good...you didn't go over 21...or did you?");
    }

    public void OnClickedDiscard()
    {
        if (CurrentState != GameState.PlayerTurn)
        {
            return;
        }
        StartCoroutine(DiscardSequence());

    }
    public void OnClickedRestart()
    {
        ReloadGame();
    }

    private IEnumerator DiscardSequence()
    {
        deckManager.deckData.Shuffle();

        bool arrived = false;
        handCardHolderScript.DiscardFirstCard(onArrived: () => arrived = true);
        yield return new WaitUntil(() => arrived);
        yield return Wait02;
        deckManager.DealFaceCard(consumableCardGroup);
    }

    private IEnumerator DealerPlay()
    {
        yield return Wait05; // Small pause after player ends

        int dealerValue = playArea.GetDealerHandValue();

        OnGameMessage?.Invoke("Dealer starting value: " + dealerValue);

        while (dealerValue < 17)
        {
            OnGameMessage?.Invoke("Dealer hits.");

            deckManager.DealFaceCard(jokerCardGroup);

            yield return Wait06; // Deal delay

            dealerValue = playArea.GetDealerHandValue();

            OnGameMessage?.Invoke("Dealer value now: " + dealerValue);
        }

        if (dealerValue > 21)
        {
            OnGameMessage?.Invoke("Dealer busts!");
        }
        else
        {
            OnGameMessage?.Invoke("Dealer stands at " + dealerValue);
        }

        yield return Wait05;

        ChangeState(GameState.ResolveRound);
    }
}
