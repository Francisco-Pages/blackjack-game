using UnityEngine;

public class BetManager : MonoBehaviour
{
    [Header("Chips")]
    public int playerChipCount = 100;
    public int dealerChipCount = 50;

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

    private void OnEnable()
    {
        ScoringSystem.OnChipsCalculated += ApplyChipDelta;
    }
    private void OnDisable()
    {
        ScoringSystem.OnChipsCalculated -= ApplyChipDelta;
    }

    private void ApplyChipDelta(int delta)
    {
        playerChipCount += delta;
        dealerChipCount -= delta;
        currentBet = 0;
        UpdateVisuals();
    }
    private void UpdateVisuals()
    {
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