using UnityEngine;

public class BetManager : MonoBehaviour
{
    [Header("Chips")]
    public int playerChipCount = 1000;
    public int dealerChipCount = 1000;

    [Header("Betting")]
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
        RecalculateBets();
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
    public void ResolveBet(RoundResult result)
    {
        switch (result)
        {
            case RoundResult.PlayerWin:
                playerChipCount += currentBet * 2;
                dealerChipCount -= currentBet;
                break;

            case RoundResult.Push:
                playerChipCount += currentBet;
                break;

            case RoundResult.DealerWin:
                dealerChipCount += currentBet;
                break;
        }

        currentBet = 0;
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
    PlayerWin,
    DealerWin,
    Push
}