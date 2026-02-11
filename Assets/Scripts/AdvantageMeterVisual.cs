using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AdvantageMeterVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text runningCountText;
    [SerializeField] private DeckManager deckManager;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        runningCountText.text = deckManager.runningCount.ToString("N0");
    }
}
