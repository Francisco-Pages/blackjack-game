using UnityEngine;

public class BetManager : MonoBehaviour
{
    [Header("Chips")]
    public int playerChipCount = 1000;
    public int dealerChipCount = 1000;

    [Header("Betting")]
    public int betMultiplier = 20;
    public int currentBet;
    public int minBet;
    public int maxBet;

    [Header("UI")]
    [SerializeField] private BankrollVisual chipBar;


    private void Awake()
    {
        // chipBar = chipBar.GetComponent<BankrollVisual>();
    }
    private void Start()
    {
        // RecalculateBets();
        UpdateVisuals();
        chipBar.SetNetProgressFromChips(playerChipCount, dealerChipCount);
    }

    /// <summary>
    /// Call this when Bet button is pressed
    /// </summary>
    public void IncreaseBet()
    {
        RecalculateBets();

        int increment = minBet;

        currentBet += increment;
        currentBet = Mathf.Clamp(currentBet, minBet, maxBet);

        Debug.Log($"Current Bet: {currentBet}");
        // UpdateVisuals();
        chipBar.SetNetProgressFromChips(playerChipCount-currentBet, dealerChipCount);
    }

    /// <summary>
    /// Lock bet and subtract chips at start of round
    /// </summary>
    public void CommitBet()
    {
        currentBet = Mathf.Clamp(currentBet, minBet, maxBet);
        playerChipCount -= currentBet;

        UpdateVisuals();
    }

    /// <summary>
    /// Resolve outcome after round ends
    /// </summary>
    public void ResolveBet(RoundResult result, int handsDiff)
    {
        switch (result)
        {
            case RoundResult.BlackJack:
                playerChipCount += handsDiff * betMultiplier * 2;
                dealerChipCount -= handsDiff * betMultiplier * 2;
                break;
            case RoundResult.PlayerWin:
                playerChipCount += handsDiff * betMultiplier;
                dealerChipCount -= handsDiff * betMultiplier;
                break;
            case RoundResult.DealerWin:
                dealerChipCount += handsDiff * betMultiplier;
                playerChipCount -= handsDiff * betMultiplier;
                break;
            case RoundResult.PlayerBust:
                playerChipCount -= handsDiff * betMultiplier;
                dealerChipCount += handsDiff * betMultiplier;
                break;
            case RoundResult.DealerBust:
                playerChipCount += handsDiff * betMultiplier;
                dealerChipCount -= handsDiff * betMultiplier;
                break;
            case RoundResult.Push:
                break;
        }

        currentBet = 0;
        UpdateVisuals();
    }

    public void CalculateScore(int result)
    {
        playerChipCount += result * betMultiplier;
        dealerChipCount -= result * betMultiplier;
        UpdateVisuals();
    }



    private void RecalculateBets()
    {
        minBet = Mathf.Max(1, Mathf.FloorToInt(playerChipCount * 0.1f));
        maxBet = playerChipCount;

        if (currentBet < minBet)
            currentBet = minBet;
    }

    private void UpdateVisuals()
    {
        chipBar.SetNetProgressFromChips(playerChipCount, dealerChipCount);
    }
}

public enum RoundResult
{
    BlackJack,
    PlayerWin,
    DealerWin,
    Push,
    PlayerBust,
    DealerBust
}