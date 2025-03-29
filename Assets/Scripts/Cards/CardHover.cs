using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Aggiungi questo script ai tuoi prefab di carta
public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Impostazioni Hover")]
    [SerializeField] private float hoverLiftAmount = 20f;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverSpeed = 8f;

    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private CardHand handManager;
    private bool isHovered = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.localPosition;
        originalScale = rectTransform.localScale;
    }

    private void Start()
    {
        // Cerca il CardHand più vicino nella gerarchia
        handManager = GetComponentInParent<CardHand>();
        if (handManager == null)
        {
            Debug.LogWarning("CardHand non trovato. Assicurati che la carta sia figlio di un oggetto con CardHand.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        StartCoroutine(AnimateHover(true));
        
        if (handManager != null)
        {
            handManager.SpreadCardsApart(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        StartCoroutine(AnimateHover(false));
        
        if (handManager != null)
        {
            handManager.ResetCardPositions();
        }
    }

    private IEnumerator AnimateHover(bool hovering)
    {
        Vector3 targetPosition = hovering 
            ? originalPosition + new Vector3(0, hoverLiftAmount, 0) 
            : originalPosition;
        
        Vector3 targetScale = hovering 
            ? originalScale * hoverScale 
            : originalScale;
        
        float elapsed = 0;
        Vector3 startPosition = rectTransform.localPosition;
        Vector3 startScale = rectTransform.localScale;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * hoverSpeed;
            rectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, Mathf.SmoothStep(0, 1, elapsed));
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, Mathf.SmoothStep(0, 1, elapsed));
            yield return null;
        }
    }

    // Metodo chiamato dal CardHand per spostare la carta orizzontalmente
    public void OffsetHorizontally(float offsetX, float duration)
    {
        StartCoroutine(AnimateHorizontalMove(offsetX, duration));
    }

    private IEnumerator AnimateHorizontalMove(float offsetX, float duration)
    {
        Vector3 targetPosition = new Vector3(
            originalPosition.x + offsetX,
            rectTransform.localPosition.y,
            rectTransform.localPosition.z
        );
        
        float elapsed = 0;
        Vector3 startPosition = rectTransform.localPosition;
        
        // Se la carta è hoverable, mantieni l'altezza maggiore
        if (isHovered)
        {
            targetPosition.y = startPosition.y;
        }
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                Mathf.SmoothStep(0, 1, elapsed / duration)
            );
            yield return null;
        }
    }

    // Resetta alla posizione originale (mantenendo eventuali modifiche Y per l'hover)
    public void ResetHorizontalPosition(float duration)
    {
        StartCoroutine(AnimateHorizontalReset(duration));
    }

    private IEnumerator AnimateHorizontalReset(float duration)
    {
        Vector3 targetPosition = new Vector3(
            originalPosition.x,
            isHovered ? rectTransform.localPosition.y : originalPosition.y,
            rectTransform.localPosition.z
        );
        
        float elapsed = 0;
        Vector3 startPosition = rectTransform.localPosition;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                Mathf.SmoothStep(0, 1, elapsed / duration)
            );
            yield return null;
        }
    }
}