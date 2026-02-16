// using System;
// using UnityEngine;
// using DG.Tweening;
// using System.Collections;
// using UnityEngine.EventSystems;
// using Unity.Collections;
// using UnityEngine.UI;
// using Unity.VisualScripting;
// using UnityEngine.InputSystem;

// public class CardVisual : MonoBehaviour
// {
//     private bool initalize = false;

//     [Header("Card")]
//     public Card parentCard;
//     private Transform cardTransform;
//     private Vector3 rotationDelta;
//     private int savedIndex;
//     Vector3 movementDelta;
//     private Canvas canvas;

//     [Header("References")]
//     public Transform visualShadow;
//     private float shadowOffset = 20;
//     private Vector2 shadowDistance;
//     private Canvas shadowCanvas;
//     [SerializeField] private Transform shakeParent;
//     [SerializeField] private Transform tiltParent;

//     [SerializeField] private Transform spriteChild;
//     private Image cardImage;
//     private RectTransform cardRect;
//     private Sprite cardFrontSprite;
//     [SerializeField] private Sprite cardBackSprite;
//     public bool faceUp = true;


//     [Header("Follow Parameters")]
//     [SerializeField] private float followSpeed = 30;

//     [Header("Rotation Parameters")]
//     [SerializeField] private float rotationAmount = 20;
//     [SerializeField] private float rotationSpeed = 20;
//     [SerializeField] private float autoTiltAmount = 30;
//     [SerializeField] private float manualTiltAmount = 20;
//     [SerializeField] private float tiltSpeed = 20;

//     [Header("Scale Parameters")]
//     [SerializeField] private bool scaleAnimations = true;
//     [SerializeField] private float scaleOnHover = 1.15f;
//     [SerializeField] private float scaleOnSelect = 1.25f;
//     [SerializeField] private float scaleTransition = .15f;
//     [SerializeField] private Ease scaleEase = Ease.OutBack;

//     [Header("Select Parameters")]
//     [SerializeField] private float selectPunchAmount = 20;

//     [Header("Hover Parameters")]
//     [SerializeField] private float hoverPunchAngle = 5;
//     [SerializeField] private float hoverTransition = .15f;

//     [Header("Swap Parameters")]
//     [SerializeField] private bool swapAnimations = true;
//     [SerializeField] private float swapRotationAngle = 30;
//     [SerializeField] private float swapTransition = .15f;
//     [SerializeField] private int swapVibrato = 5;

//     [Header("Curve")]
//     [SerializeField] private CurveParameters curve;

//     private float curveYOffset;
//     private float curveRotationOffset;
//     private Coroutine pressCoroutine;

//     private void Start()
//     {
//         shadowDistance = visualShadow.localPosition;
//     }

//     public void Initialize(Card target, int index = 0)
//     {
//         //Declarations
//         parentCard = target;
//         cardTransform = target.transform;
//         canvas = GetComponent<Canvas>();
//         shadowCanvas = visualShadow.GetComponent<Canvas>();
//         cardImage = spriteChild.GetComponent<Image>();
//         cardImage.sprite = parentCard.cardData.frontSprite;
//         cardFrontSprite = parentCard.cardData.frontSprite;

//         //Event Listening
//         parentCard.PointerEnterEvent.AddListener(PointerEnter);
//         parentCard.PointerExitEvent.AddListener(PointerExit);
//         parentCard.BeginDragEvent.AddListener(BeginDrag);
//         parentCard.EndDragEvent.AddListener(EndDrag);
//         parentCard.PointerDownEvent.AddListener(PointerDown);
//         parentCard.PointerUpEvent.AddListener(PointerUp);
//         parentCard.SelectEvent.AddListener(Select);

//         //Initialization
//         initalize = true;

//         // gameObject.SetActive(true);
        
//     }

    // public void UpdateIndex(int length)
    // {
    //     transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    // }

//     void Update()
//     {
        
//         if (!initalize || parentCard == null) return;

//         HandPositioning();
//         SmoothFollow();
//         FollowRotation();
//         CardTilt();

//         // if (spriteChild.rotation.y > 90)
//         // {
//         //     cardImage.sprite = cardBackSprite;
//         //     // spriteChild.Rotate(0f, 90f, 0f);
//         //     // Debug.Log(spriteChild.rotation);
//         // }
//         // if (spriteChild.rotation.y < 90)
//         // {
//         //     cardImage.sprite = parentCard.cardData.frontSprite;
//         // }

//         // if the Y rotation is > 90, cardImage.sprite = backSprite
//         // if the Y rotation is < 90, cardImage.sprite = FrontSprite
//         // if Y rotation
//     }

//     private void HandPositioning()
//     {
//         curveYOffset = (curve.positioning.Evaluate(parentCard.NormalizedPosition()) * curve.positioningInfluence) * parentCard.SiblingAmount();
//         curveYOffset = parentCard.SiblingAmount() < 5 ? 0 : curveYOffset;
//         curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition());
//     }

//     private void SmoothFollow()
//     {
//         Vector3 verticalOffset = (Vector3.up * (parentCard.isDragging ? 0 : curveYOffset));
//         transform.position = Vector3.Lerp(transform.position, cardTransform.position + verticalOffset, followSpeed * Time.deltaTime);
//     }

//     private void FollowRotation()
//     {
//         Vector3 movement = (transform.position - cardTransform.position);
//         movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
//         Vector3 movementRotation = (parentCard.isDragging ? movementDelta : movement) * rotationAmount;
//         rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
//         transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
//     }

//     private void CardTilt()
//     {
//         savedIndex = parentCard.isDragging ? savedIndex : parentCard.ParentIndex();
//         float sine = Mathf.Sin(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);
//         float cosine = Mathf.Cos(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);

//         Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
//         float tiltX = parentCard.isHovering ? ((offset.y * -1) * manualTiltAmount) : 0;
//         float tiltY = parentCard.isHovering ? ((offset.x) * manualTiltAmount) : 0;
//         float tiltZ = parentCard.isDragging ? tiltParent.eulerAngles.z : (curveRotationOffset * (curve.rotationInfluence * parentCard.SiblingAmount()));

//         float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
//         float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
//         float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

//         tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
//     }

//     private void Select(Card card, bool state)
//     {
//         DOTween.Kill(2, true);
//         float dir = state ? 1 : 0;
//         shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
//         shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle/2), hoverTransition, 20, 1).SetId(2);

//         if(scaleAnimations)
//             transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

//     }

//     public void Swap(float dir = 1)
//     {
//         if (!swapAnimations)
//             return;

//         DOTween.Kill(2, true);
//         shakeParent.DOPunchRotation((Vector3.forward * swapRotationAngle) * dir, swapTransition, swapVibrato, 1).SetId(3);
//     }

//     private void BeginDrag(Card card)
//     {
//         if(scaleAnimations)
//             transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);

//         canvas.overrideSorting = true;
//     }

//     private void EndDrag(Card card)
//     {
//         canvas.overrideSorting = false;
//         transform.DOScale(1, scaleTransition).SetEase(scaleEase);
//     }

//     private void PointerEnter(Card card)
//     {
//         if(scaleAnimations)
//             transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

//         DOTween.Kill(2, true);
//         shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
//     }

//     private void PointerExit(Card card)
//     {
//         if (!parentCard.wasDragged)
//             transform.DOScale(1, scaleTransition).SetEase(scaleEase);
//     }

//     private void PointerUp(Card card, bool longPress)
//     {
//         if(scaleAnimations)
//             transform.DOScale(longPress ? scaleOnHover : scaleOnSelect, scaleTransition).SetEase(scaleEase);
//         canvas.overrideSorting = false;

//         visualShadow.localPosition = shadowDistance;
//         shadowCanvas.overrideSorting = true;
//     }

//     private void PointerDown(Card card)
//     {
//         if(scaleAnimations)
//             transform.DOScale(scaleOnSelect, scaleTransition).SetEase(scaleEase);
            
//         visualShadow.localPosition += (-Vector3.up * shadowOffset);
//         shadowCanvas.overrideSorting = false;
//     }

// }


using System;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.Collections;
using UnityEngine.UI;
// using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class CardVisual : MonoBehaviour
{
    private bool initalize = false;

    [Header("Card")]
    public Card parentCard;
    public Transform cardTransform;
    private Vector3 rotationDelta;
    private int savedIndex;
    Vector3 movementDelta;
    private Canvas canvas;

    [Header("References")]
    public Transform visualShadow;
    private float shadowOffset = 20;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;

    [SerializeField] private Transform spriteChild;
    private Image cardImage;
    private RectTransform cardRect;
    private Sprite cardFrontSprite;
    [SerializeField] private Sprite cardBackSprite;

    [Header("Face State")]
    public bool faceUp = true;
    private bool isFaceUpVisual;

    [Header("Flip")]
    [SerializeField] private float flipDuration = 0.25f;
    private Tween flipTween;

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20;

    [Header("Hover Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;

    [Header("Swap Parameters")]
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 30;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 5;

    [Header("Curve")]
    [SerializeField] private CurveParameters curve;

    private float curveYOffset;
    private float curveRotationOffset;
    private Coroutine pressCoroutine;

    private void Start()
    {
        shadowDistance = visualShadow.localPosition;
    }

    public void Initialize(Card target, int index = 0)
    {
        parentCard = target;
        cardTransform = target.transform;
        canvas = GetComponent<Canvas>();
        shadowCanvas = visualShadow.GetComponent<Canvas>();

        cardImage = spriteChild.GetComponent<Image>();
        cardFrontSprite = parentCard.cardData.frontSprite;

        // Set initial face state
        isFaceUpVisual = faceUp;
        cardImage.sprite = faceUp ? cardFrontSprite : cardBackSprite;
        spriteChild.localRotation = Quaternion.Euler(0, faceUp ? 0 : 180, 0);

        // Events
        parentCard.PointerEnterEvent.AddListener(PointerEnter);
        parentCard.PointerExitEvent.AddListener(PointerExit);
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
        parentCard.PointerDownEvent.AddListener(PointerDown);
        parentCard.PointerUpEvent.AddListener(PointerUp);
        parentCard.SelectEvent.AddListener(Select);

        initalize = true;
    }

    /* =========================
       FLIP LOGIC
    ========================= */

    public void SetFaceUp(bool state, bool animate = true)
    {
        if (isFaceUpVisual == state) return;

        faceUp = state;
        isFaceUpVisual = state;

        if (flipTween != null)
            flipTween.Kill();

        if (!animate)
        {
            spriteChild.localRotation = Quaternion.Euler(0, state ? 0 : 180, 0);
            cardImage.sprite = state ? cardFrontSprite : cardBackSprite;
            return;
        }

        Sequence seq = DOTween.Sequence();

        // First half
        seq.Append(
            spriteChild.DOLocalRotate(
                new Vector3(0, 90, 0),
                flipDuration / 2
            ).SetEase(Ease.InQuad)
        );

        // Swap sprite
        seq.AppendCallback(() =>
        {
            cardImage.sprite = state ? cardFrontSprite : cardBackSprite;
        });

        // Second half
        seq.Append(
            spriteChild.DOLocalRotate(
                new Vector3(0, state ? 0 : 180, 0),
                flipDuration / 2
            ).SetEase(Ease.OutQuad)
        );

        flipTween = seq;
    }

    /* =========================
       UPDATE
    ========================= */

    void Update()
    {
        if (!initalize || parentCard == null) return;

        HandPositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();
    }

    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }

    /* =========================
       POSITIONING / MOTION
    ========================= */

    private void HandPositioning()
    {
        curveYOffset =
            (curve.positioning.Evaluate(parentCard.NormalizedPosition())
            * curve.positioningInfluence)
            * parentCard.SiblingAmount();

        curveYOffset = parentCard.SiblingAmount() < 5 ? 0 : curveYOffset;
        curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition());
    }

    private void SmoothFollow()
    {
        Vector3 verticalOffset =
            (Vector3.up * (parentCard.isDragging ? 0 : curveYOffset));

        transform.position = Vector3.Lerp(
            transform.position,
            cardTransform.position + verticalOffset,
            followSpeed * Time.deltaTime
        );
    }

    private void FollowRotation()
    {
        Vector3 movement = (transform.position - cardTransform.position);

        movementDelta = Vector3.Lerp(
            movementDelta,
            movement,
            25 * Time.deltaTime
        );

        Vector3 movementRotation =
            (parentCard.isDragging ? movementDelta : movement)
            * rotationAmount;

        rotationDelta = Vector3.Lerp(
            rotationDelta,
            movementRotation,
            rotationSpeed * Time.deltaTime
        );

        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            Mathf.Clamp(rotationDelta.x, -60, 60)
        );
    }

    private void CardTilt()
    {
        savedIndex = parentCard.isDragging
            ? savedIndex
            : parentCard.ParentIndex();

        float sine =
            Mathf.Sin(Time.time + savedIndex)
            * (parentCard.isHovering ? .2f : 1);

        float cosine =
            Mathf.Cos(Time.time + savedIndex)
            * (parentCard.isHovering ? .2f : 1);

        Vector3 offset =
            transform.position
            - Camera.main.ScreenToWorldPoint(
                Pointer.current.position.ReadValue()
            );

        float tiltX =
            parentCard.isHovering
            ? ((offset.y * -1) * manualTiltAmount)
            : 0;

        float tiltY =
            parentCard.isHovering
            ? ((offset.x) * manualTiltAmount)
            : 0;

        float tiltZ =
            parentCard.isDragging
            ? tiltParent.eulerAngles.z
            : (curveRotationOffset
            * (curve.rotationInfluence
            * parentCard.SiblingAmount()));

        float lerpX =
            Mathf.LerpAngle(
                tiltParent.eulerAngles.x,
                tiltX + (sine * autoTiltAmount),
                tiltSpeed * Time.deltaTime
            );

        float lerpY =
            Mathf.LerpAngle(
                tiltParent.eulerAngles.y,
                tiltY + (cosine * autoTiltAmount),
                tiltSpeed * Time.deltaTime
            );

        float lerpZ =
            Mathf.LerpAngle(
                tiltParent.eulerAngles.z,
                tiltZ,
                tiltSpeed / 2 * Time.deltaTime
            );

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    /* =========================
       INTERACTION
    ========================= */

    public void Select(Card card, bool state)
    {
        DOTween.Kill(2, true);

        float dir = state ? 1 : 0;

        shakeParent.DOPunchPosition(
            shakeParent.up * selectPunchAmount * dir,
            scaleTransition,
            10,
            1
        );

        shakeParent.DOPunchRotation(
            Vector3.forward * (hoverPunchAngle / 2),
            hoverTransition,
            20,
            1
        ).SetId(2);

        if (scaleAnimations)
            Debug.Log("Doing punch animation");
            transform.DOScale(scaleOnHover, scaleTransition)
                .SetEase(scaleEase);
    }

    public void Swap(float dir = 1)
    {
        if (!swapAnimations) return;

        DOTween.Kill(2, true);

        shakeParent.DOPunchRotation(
            (Vector3.forward * swapRotationAngle) * dir,
            swapTransition,
            swapVibrato,
            1
        ).SetId(3);
    }

    public void BeginDrag(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition)
                .SetEase(scaleEase);

        canvas.overrideSorting = true;
    }

    public void EndDrag(Card card)
    {
        canvas.overrideSorting = false;

        transform.DOScale(1, scaleTransition)
            .SetEase(scaleEase);
    }

    public void PointerEnter(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnHover, scaleTransition)
                .SetEase(scaleEase);

        DOTween.Kill(2, true);

        shakeParent.DOPunchRotation(
            Vector3.forward * hoverPunchAngle,
            hoverTransition,
            20,
            1
        ).SetId(2);
    }

    public void PointerExit(Card card)
    {
        if (!parentCard.wasDragged)
            transform.DOScale(1, scaleTransition)
                .SetEase(scaleEase);
    }

    public void PointerUp(Card card, bool longPress)
    {
        if (scaleAnimations)
            transform.DOScale(
                longPress ? scaleOnHover : scaleOnSelect,
                scaleTransition
            ).SetEase(scaleEase);

        canvas.overrideSorting = false;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.overrideSorting = true;
    }

    public void PointerDown(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect, scaleTransition)
                .SetEase(scaleEase);

        visualShadow.localPosition += (-Vector3.up * shadowOffset);
        shadowCanvas.overrideSorting = false;
    }
}