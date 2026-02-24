using UnityEngine;
using TMPro;

public class AdvantageMeterVisual : MonoBehaviour
{
    [SerializeField] private TMP_Text runningCountText;

    private void OnEnable()
    {
        DeckManager.OnRunningCountChanged += HandleRunningCountChanged;
    }

    private void OnDisable()
    {
        DeckManager.OnRunningCountChanged -= HandleRunningCountChanged;
    }

    private void HandleRunningCountChanged(int count)
    {
        runningCountText.text = count.ToString("N0");
    }
}
