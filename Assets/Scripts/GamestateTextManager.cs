using UnityEngine;
using TMPro;
using System.Collections;

public class GamestateTextManager : MonoBehaviour
{
    [SerializeField] private TMP_Text gamestate;

    public void UpdateGamestateText(string newGamestate)
    {
        gamestate.text = newGamestate;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.StartRound:
                UpdateGamestateText("Press the deck to start.");
                break;

            case GameManager.GameState.PlayerTurn:
                UpdateGamestateText("Your turn. Hit, or stand. \nPlay or discard.");
                break;
        }
    }
}
