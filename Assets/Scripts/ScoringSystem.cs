// using Unity.VisualScripting;
// using UnityEngine;

// public class ScoringSystem : MonoBehaviour
// {
//     [Header("Chips")] 
//     public int playerChips;
//     public int dealerChips;

//     [Header("UI")]
//     [SerializeField] private BankrollVisual chipBar;

//     private void Awake()
//     {
//         UpdateVisuals();
//         chipBar = chipBar.GetComponent<BankrollVisual>();
//     }
//     private void Start()
//     {
//         playerChips = 1000;
//         dealerChips = 1000;
//         UpdateVisuals();
//         chipBar.SetNetProgressFromChips(playerChips, dealerChips);
//     }

//     public void CalculateScore(RoundResult result)
//     {
//         if (result == RoundResult.PlayerWin)
//             playerChips += 100;
//         if (result == RoundResult.DealerWin)
//             dealerChips += 100;
//         UpdateVisuals();
//     }

//     private void UpdateVisuals()
//     {
//         chipBar.SetNetProgressFromChips(playerChips, dealerChips);
//     }
// }
// public enum RoundResult
// {
//     PlayerWin,
//     DealerWin,
//     Push
// }
