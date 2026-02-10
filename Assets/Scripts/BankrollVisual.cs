// using System;
// using UnityEngine;
// using System.Collections;
// using UnityEngine.UI;
// public class BankrollVisual : MonoBehaviour
// {
//     private RectTransform rect;
//     private Image image;
//     [SerializeField] private float maxWidth = 600f;
//     [SerializeField] private float animationSpeed = 5f;
//     public float currentPercentage;
//     private Coroutine currentAnimation;

//     [SerializeField] private Color green = Color.green;
//     [SerializeField] private Color yellow = Color.yellow;
//     [SerializeField] private Color red = Color.red;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         rect = GetComponent<RectTransform>();
//         image = GetComponent<Image>();
//     }

//     public void SetNetProgress(float percent)
//     {
//         percent = Mathf.Clamp01(percent);
//         currentPercentage = percent;

//         float targetWidth = maxWidth * percent;
//         Color targetColor = GetColorFromPercent(percent);

//         if (currentAnimation != null)
//             StopCoroutine(currentAnimation);

//         currentAnimation =
//             StartCoroutine(AnimateWidthAndColor(targetWidth, targetColor));
//     }

//     public void SetNetProgressFromChips(int playerChips, int dealerChips)
//     {
//         int totalChips = playerChips + dealerChips;

//         // Prevent divide by zero
//         if (totalChips <= 0)
//         {
//             SetNetProgress(0f);
//             return;
//         }

//         float percent = (float)playerChips / totalChips;

//         SetNetProgress(percent);
//     }

//     private IEnumerator AnimateWidthAndColor(float targetWidth, Color targetColor)
//     {
//         float startWidth = rect.sizeDelta.x;
//         Color startColor = image.color;

//         float time = 0f;

//         while (time < 1f)
//         {
//             time += Time.deltaTime * animationSpeed;

//             // Smooth step for nicer easing
//             float t = Mathf.SmoothStep(0f, 1f, time);

//             // Width animation
//             float newWidth = Mathf.Lerp(startWidth, targetWidth, t);
//             rect.sizeDelta =
//                 new Vector2(newWidth, rect.sizeDelta.y);

//             // Color animation
//             Color newColor =
//                 Color.Lerp(startColor, targetColor, t);
//             image.color = newColor;

//             yield return null;
//         }

//         // Snap final values
//         rect.sizeDelta =
//             new Vector2(targetWidth, rect.sizeDelta.y);

//         image.color = targetColor;
//     }

//     private Color GetColorFromPercent(float percent)
//     {
//         if (percent < 1f / 3f)
//             return red;

//         if (percent < 2f / 3f)
//             return yellow;

//         return green;
//     }
// }

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro; // ← Add if using TextMeshPro

public class BankrollVisual : MonoBehaviour
{
    private RectTransform rect;
    private Image image;

    [Header("Bar Settings")]
    [SerializeField] private float maxWidth = 600f;
    [SerializeField] private float animationSpeed = 5f;
    public float currentPercentage;
    private Coroutine currentAnimation;

    [Header("Chip Text")]
    [SerializeField] private TMP_Text chipCountText; // Assign in Inspector
    private int displayedChips;
    private int targetChips;

    [Header("Colors")]
    [SerializeField] private Color green = Color.green;
    [SerializeField] private Color yellow = Color.yellow;
    [SerializeField] private Color red = Color.red;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    // Called by BetManager
    public void SetNetProgressFromChips(int playerChips, int dealerChips)
    {
        int totalChips = playerChips + dealerChips;

        if (totalChips <= 0)
        {
            SetNetProgress(0f, playerChips);
            return;
        }

        float percent = (float)playerChips / totalChips;

        SetNetProgress(percent, playerChips);
    }

    public void SetNetProgress(float percent, int playerChips)
    {
        percent = Mathf.Clamp01(percent);
        currentPercentage = percent;
        targetChips = playerChips;

        float targetWidth = maxWidth * percent;
        Color targetColor = GetColorFromPercent(percent);

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation =
            StartCoroutine(AnimateWidthAndColor(targetWidth, targetColor));
    }

    private IEnumerator AnimateWidthAndColor(float targetWidth, Color targetColor)
    {
        float startWidth = rect.sizeDelta.x;
        Color startColor = image.color;

        int startChips = displayedChips;
        int endChips = targetChips;

        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * animationSpeed;

            float t = Mathf.SmoothStep(0f, 1f, time);

            // Width animation
            float newWidth = Mathf.Lerp(startWidth, targetWidth, t);
            rect.sizeDelta =
                new Vector2(newWidth, rect.sizeDelta.y);

            // Color animation
            Color newColor =
                Color.Lerp(startColor, targetColor, t);
            image.color = newColor;

            // Chip count animation
            float chipValue = Mathf.Lerp(startChips, endChips, t);
            displayedChips = Mathf.RoundToInt(chipValue);

            UpdateChipText(displayedChips, new Vector2(targetWidth, rect.sizeDelta.y));

            yield return null;
        }

        // Snap final values
        rect.sizeDelta =
            new Vector2(targetWidth, rect.sizeDelta.y);

        image.color = targetColor;

        displayedChips = endChips;
        UpdateChipText(displayedChips, new Vector2(targetWidth, rect.sizeDelta.y));
    }

    private void UpdateChipText(int amount, Vector2 targetSize)
    {
        if (chipCountText == null) return;

        // Comma formatting
        chipCountText.text = amount.ToString("N0");
        chipCountText.rectTransform.sizeDelta = targetSize;
    }

    private Color GetColorFromPercent(float percent)
    {
        if (percent < 1f / 3f)
            return red;

        if (percent < 2f / 3f)
            return yellow;

        return green;
    }
}