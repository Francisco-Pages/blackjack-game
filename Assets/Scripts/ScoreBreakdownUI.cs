using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreBreakdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Text breakdownText;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button continueButton;

    [SerializeField] private float delayBetweenLines = 0.2f;

    private void Start()
    {
        continueButton.onClick.AddListener(Dismiss);
    }

    public static bool IsDismissed { get; private set; } = true;

    private Coroutine _reveal;

    private void OnEnable()
    {
        ScoringSystem.OnBreakdownReady += Show;
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        ScoringSystem.OnBreakdownReady -= Show;
        GameManager.OnGameStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.StartRound)
        {
            if (_reveal != null) StopCoroutine(_reveal);
            panel.SetActive(false);
        }
    }

    private void Show(ScoreBreakdown b)
    {
        IsDismissed = false;
        if (_reveal != null) StopCoroutine(_reveal);
        breakdownText.text = "";
        panel.SetActive(true);
        _reveal = StartCoroutine(RevealLines(BuildLines(b)));
    }

    public void Dismiss()
    {
        if (_reveal != null) StopCoroutine(_reveal);
        panel.SetActive(false);
        IsDismissed = true;
    }

    private IEnumerator RevealLines(List<string> lines)
    {
        string accumulated = "";
        foreach (string line in lines)
        {
            accumulated += (accumulated == "" ? "" : "\n") + line;
            breakdownText.text = accumulated;
            yield return new WaitForSecondsRealtime(delayBetweenLines);
        }
    }

    private List<string> BuildLines(ScoreBreakdown b)
    {
        var lines = new List<string>();

        if (b.isBust)
        {
            lines.Add("BUST");
            lines.Add($"Hand     -{b.handChips}");
            lines.Add($"Overage  {b.delta}");
            lines.Add("──────────");
            int bustBase = b.handChips + (-b.delta);
            lines.Add($"Base     -{bustBase}");
            if (b.session != 1f)
                lines.Add($"× Mult   {b.session:0.0}×");
            if (b.streak != 1f)
                lines.Add($"× Streak {b.streak:0.0}×");
            lines.Add("──────────");
            lines.Add($"= {b.total} chips");
            return lines;
        }

        lines.Add($"Hand      {b.handChips}");
        if (b.suitBonus != 0)
            lines.Add($"Bonus    +{b.suitBonus}");
        lines.Add(b.delta >= 0 ? $"Δ        +{b.delta}" : $"Δ         {b.delta}");
        lines.Add("──────────");

        int base_ = b.handChips + b.suitBonus + b.delta;
        lines.Add($"Base      {base_}");
        lines.Add($"× {TierLabel(b.tier),-8}{b.tier:0.00}×");
        if (b.session != 1f)
            lines.Add($"× Mult    {b.session:0.0}×");
        if (b.streak != 1f)
            lines.Add($"× Streak  {b.streak:0.0}×");

        lines.Add("──────────");
        lines.Add($"= {b.total} chips");
        return lines;
    }

    private string TierLabel(float tier)
    {
        if (tier >= 2f)    return "Blackjack";
        if (tier >= 1.75f) return "3-Card 21";
        if (tier >= 1.5f)  return "5-Card";
        if (tier <= -1f)   return "Loss";
        if (tier <= 0.25f) return "Push";
        return "Win";
    }
}
