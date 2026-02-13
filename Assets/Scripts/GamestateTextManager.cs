using UnityEngine;
using TMPro;

public class GamestateTextManager : MonoBehaviour
{
    [SerializeField] private TMP_Text gamestate;

    public void UpdateGamestateText(string newGamestate)
    {
        gamestate.text = newGamestate;
    }
}
