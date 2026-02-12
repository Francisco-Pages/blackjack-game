using UnityEngine;
using TMPro;

public class GamestateTextManager : MonoBehaviour
{
    [SerializeField] private TMP_Text gamestate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateGamestateText(string newGamestate)
    {
        gamestate.text = newGamestate;
    }
}
