using System;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreBreakdown
{
    public bool isBust;
    public int handChips;   // player total
    public int suitBonus;   // flat suit synergy chips
    public int delta;       // Δ margin
    public float tier;
    public float session;
    public float streak;
    public int total;       // final chip delta
}

public class ScoringSystem : MonoBehaviour
{
    public static ScoringSystem Instance;

    [Header("References")]
    [SerializeField] private PlayArea playArea;
    [SerializeField] private HorizontalCardHolder playerHand;

    [Header("Session State")]
    public float sessionMult = 1f;
    public int winStreak = 0;

    public static event Action<int> OnChipsCalculated;
    public static event Action<ScoreBreakdown> OnBreakdownReady;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        GameManager.OnRoundResolved += HandleRoundResolved;
    }

    private void OnDisable()
    {
        GameManager.OnRoundResolved -= HandleRoundResolved;
    }

    private void HandleRoundResolved(RoundResult result, int _)
    {
        List<Card> cards = playerHand.cards;
        int playerTotal = playArea.GetPlayerHandValue();
        int dealerTotal = playArea.GetDealerHandValue();

        ScoreBreakdown breakdown;
        int chipDelta;

        if (result == RoundResult.PlayerBust)
        {
            int overage   = playerTotal - 21;
            float streak  = GetStreakBonus(winStreak);
            chipDelta     = -Mathf.RoundToInt((playerTotal + overage) * sessionMult * streak);
            breakdown = new ScoreBreakdown
            {
                isBust    = true,
                handChips = playerTotal,
                suitBonus = 0,
                delta     = -overage,
                tier      = 0f,
                session   = sessionMult,
                streak    = streak,
                total     = chipDelta
            };
        }
        else
        {
            int suitBonus = CalculateSuitBonus(cards);
            int handChips = playerTotal + suitBonus;

            int delta;
            if (result == RoundResult.DealerBust)
                delta = dealerTotal - 21;
            else if (result == RoundResult.Push)
                delta = 0;
            else
                delta = playerTotal - dealerTotal;

            float tier   = GetTierMultiplier(result, cards, playerTotal);
            float streak = GetStreakBonus(winStreak);

            chipDelta = Mathf.RoundToInt((handChips + delta) * tier * sessionMult * streak);
            breakdown = new ScoreBreakdown
            {
                isBust    = false,
                handChips = playerTotal,
                suitBonus = suitBonus,
                delta     = delta,
                tier      = tier,
                session   = sessionMult,
                streak    = streak,
                total     = chipDelta
            };
        }

        bool isWin = result == RoundResult.PlayerWin
                  || result == RoundResult.DealerBust
                  || result == RoundResult.BlackJack;
        winStreak = isWin ? winStreak + 1 : 0;

        OnBreakdownReady?.Invoke(breakdown);
        OnChipsCalculated?.Invoke(chipDelta);
    }

    // ── Tier Multiplier ────────────────────────────────────────────────────────

    private float GetTierMultiplier(RoundResult result, List<Card> cards, int playerTotal)
    {
        if (result == RoundResult.Push)
            return 0.25f;

        if (result == RoundResult.DealerWin)
            return -1f;

        // Win cases — checked from highest to lowest bonus
        if (result == RoundResult.BlackJack)
            return 2f;

        // Natural blackjack: player wins with exactly 2 cards totaling 21
        if ((result == RoundResult.PlayerWin || result == RoundResult.DealerBust)
            && cards.Count == 2 && playerTotal == 21)
            return 2f;

        // 3+ cards totaling exactly 21
        if (cards.Count >= 3 && playerTotal == 21)
            return 1.75f;

        // 5-card hand (any winning total that isn't a 21)
        if (cards.Count >= 5)
            return 1.5f;

        return 1f;
    }

    // ── Suit Synergy Bonus ─────────────────────────────────────────────────────

    private int CalculateSuitBonus(List<Card> cards)
    {
        if (cards.Count < 2) return 0;

        int bonus = 0;

        // Flush: all cards share the same suit → +5
        bool isFlush = true;
        Suit firstSuit = cards[0].cardData.suit;
        foreach (Card card in cards)
        {
            if (card.cardData.suit != firstSuit) { isFlush = false; break; }
        }
        if (isFlush) bonus += 5;

        // Zebra: cards strictly alternate red/black → +3
        if (IsZebra(cards)) bonus += 3;

        // All face cards (J, Q, K only) → +4
        bool allFaceCards = true;
        foreach (Card card in cards)
        {
            Rank r = card.cardData.rank;
            if (r != Rank.Jack && r != Rank.Queen && r != Rank.King)
            {
                allFaceCards = false;
                break;
            }
        }
        if (allFaceCards) bonus += 4;

        // Paired starting hand: first two cards share the same rank → +2
        if (cards[0].cardData.rank == cards[1].cardData.rank) bonus += 2;

        return bonus;
    }

    private bool IsZebra(List<Card> cards)
    {
        bool firstIsRed = IsRed(cards[0].cardData.suit);
        for (int i = 0; i < cards.Count; i++)
        {
            bool shouldBeRed = i % 2 == 0 == firstIsRed;
            if (IsRed(cards[i].cardData.suit) != shouldBeRed) return false;
        }
        return true;
    }

    private bool IsRed(Suit suit) => suit == Suit.Hearts || suit == Suit.Diamonds;

    // ── Streak Bonus ───────────────────────────────────────────────────────────

    private float GetStreakBonus(int streak)
    {
        if (streak >= 7) return 3f;
        if (streak >= 5) return 2f;
        if (streak >= 3) return 1.5f;
        if (streak >= 2) return 1.25f;
        return 1f;
    }
}
