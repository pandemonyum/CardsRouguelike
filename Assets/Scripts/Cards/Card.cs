using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

// Aggiungiamo le interfacce necessarie per gestire interazioni e drag and drop
public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
                   IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public CardData cardData;
    
    [Header("Card UI Elements")]
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public Image cardFrame;
    
    [Header("Card Behavior")]
    private Vector3 startPosition;
    private Transform originalParent;
    private bool isDragging = false;
    
    [Header("Hover Effect")]
    public float hoverScaleFactor = 1.2f;
    public float hoverYOffset = 30f;
    public float hoverAnimationSpeed = 0.2f;
    private Vector3 originalScale;
    private bool isHovering = false;
    
    [Header("Playable Zone")]
    private PlayZone playZone;
    private CombatManager combatManager;
    
    // Colori per i diversi tipi di carte
    private Color attackColor = new Color(0.8f, 0.2f, 0.2f);
    private Color skillColor = new Color(0.2f, 0.5f, 0.8f);
    private Color powerColor = new Color(0.6f, 0.2f, 0.8f);

    public Vector3 originalPosition;

    [Header("Targeting")]
    public bool requiresTarget = false;

    [Header("Fan-Out Effect")]
    public float fanOutDistance = 40f;  // Distanza di spostamento laterale
    public bool isHoveredCard = false;  // Flag per la carta con hover

    void Start()
    {
        if (cardData != null)
        {
            UpdateCardVisuals();
            if (cardData.cardType == CardData.CardType.Attack)
            {
                requiresTarget = true;
            }
        }
        
        originalScale = transform.localScale;
        originalPosition = transform.position;

        // Trova i riferimenti necessari
        playZone = FindFirstObjectByType<PlayZone>();
        combatManager = FindFirstObjectByType<CombatManager>();
        
        // Se non trovi il PlayZone, mostra un warning
        if (playZone == null)
        {
            Debug.LogWarning("PlayZone non trovata. Assicurati di creare un oggetto con il componente PlayZone.");
        }
    }
    
    public void Initialize(CardData data)
    {
        cardData = data;
        UpdateCardVisuals();
    }
    // Metodo per resettare la posizione originale (utile quando la carta viene riordinata nella mano)
    public void ResetOriginalPosition()
    {
        originalPosition = transform.position;
    }
        
    void UpdateCardVisuals()
    {
        // Aggiorna tutti gli elementi visivi della carta
        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        costText.text = cardData.energyCost.ToString();
        
        if (cardData.artwork != null)
        {
            artworkImage.sprite = cardData.artwork;
        }
        
        // Imposta il colore in base al tipo di carta
        switch (cardData.cardType)
        {
            case CardData.CardType.Attack:
                cardFrame.color = attackColor;
                break;
            case CardData.CardType.Skill:
                cardFrame.color = skillColor;
                break;
            case CardData.CardType.Power:
                cardFrame.color = powerColor;
                break;
        }
    }
    
    // IMPLEMENTAZIONE EFFETTO HOVER
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isHovering = true;
            StopAllCoroutines();
            StartCoroutine(ScaleCard(originalScale * hoverScaleFactor, hoverAnimationSpeed));
            StartCoroutine(MoveCardTo(originalPosition + new Vector3(0, hoverYOffset, 0), hoverAnimationSpeed));
            
            // Avvisa la Hand che questa carta ha l'hover
            Hand hand = transform.parent?.GetComponent<Hand>();
            if (hand != null)
            {
                hand.CardHovered(this);
            }
        }
    }

   public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging)
        {
            isHovering = false;
            StopAllCoroutines();
            StartCoroutine(ScaleCard(originalScale, hoverAnimationSpeed));
            StartCoroutine(MoveCardTo(originalPosition, hoverAnimationSpeed));
            
            // Avvisa la Hand che l'hover è terminato
            Hand hand = transform.parent?.GetComponent<Hand>();
            if (hand != null)
            {
                hand.CardUnhovered();
            }
        }
    }
    
    IEnumerator ScaleCard(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float time = 0;
        
        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
    
    IEnumerator MoveCardUp(float targetYOffset, float duration)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + new Vector3(0, targetYOffset, 0);
        float time = 0;
        
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
    }
    public IEnumerator MoveCardTo(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float time = 0;
        
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
    }
    
    // IMPLEMENTAZIONE DRAG AND DROP
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Se non abbiamo abbastanza energia, non permettiamo di trascinare
        if (combatManager != null && cardData.energyCost > combatManager.currentEnergy)
        {
            eventData.pointerDrag = null;
            return;
        }
        
        isDragging = true;
        startPosition = transform.position;
        originalParent = transform.parent;
        
        // Sposta la carta in primo piano mentre viene trascinata
        transform.SetParent(transform.root);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        
        // Riporta la carta alla scala originale durante il drag
        StopAllCoroutines();
        transform.localScale = originalScale * 1.05f; // Leggermente più grande per visibilità
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = eventData.position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        
        // Verifica se la carta è stata rilasciata sulla zona di gioco
        if (playZone != null && playZone.IsCardOverPlayZone(this))
        {
            PlayCard();
        }
        else
        {
            // Ritorna alla posizione originale
            transform.SetParent(originalParent);
            transform.position = startPosition;
            
            // Ripristina l'effetto hover se il mouse è ancora sopra la carta
            if (isHovering)
            {
                StartCoroutine(ScaleCard(originalScale * hoverScaleFactor, hoverAnimationSpeed));
                StartCoroutine(MoveCardUp(hoverYOffset, hoverAnimationSpeed));
            }
            else
            {
                transform.localScale = originalScale;
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Se siamo in modalità targeting e questa carta è stata selezionata per targeting
        Debug.Log("Giocata carta: ");
        TargetingSystem targetingSystem = FindFirstObjectByType<TargetingSystem>();
        if (targetingSystem != null && targetingSystem.isTargeting && targetingSystem.selectedCard == this)
        {
            // Conferma il bersaglio con un click
            targetingSystem.ConfirmTarget();
        }
        // Altrimenti, potresti implementare altre funzionalità di click
    }
    
    // Metodo per giocare la carta
 
    public void PlayCard()
    {
        Debug.Log("Giocata carta: " + cardData.cardName);
        
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<CombatManager>();
        }
        
        // Se la carta richiede un bersaglio, attiva il sistema di targeting
        if (requiresTarget && combatManager != null && combatManager.currentEnergy >= cardData.energyCost)
    {
                // Attiva il sistema di targeting
                TargetingSystem targetingSystem = FindFirstObjectByType<TargetingSystem>();
                if (targetingSystem != null)
                {
                    targetingSystem.StartTargeting(this);
                    return; // Non completare il gioco della carta ora
                }
            
        }
        
        // Per carte che non richiedono bersaglio, o se il targeting fallisce
        // continua con la logica normale
        if (combatManager != null && combatManager.TryPlayCard(cardData, cardData.energyCost))
        {
            // Applica l'effetto della carta
            ApplyCardEffect();
            
            // Rimuovi la carta dalla mano
            Hand hand = originalParent.GetComponent<Hand>();
            if (hand != null)
            {
                hand.RemoveCard(this);
            }
            
            // Distruggi la carta
            Destroy(gameObject);
        }
        else
        {
            // Non abbastanza energia, torna nella mano
            transform.SetParent(originalParent);
            transform.position = startPosition;
            transform.localScale = originalScale;
        }
    }
    // Metodo per applicare l'effetto della carta
    void ApplyCardEffect()
    {
        switch (cardData.cardType)
        {
            case CardData.CardType.Attack:
                // Infligge danno al bersaglio
                Debug.Log("Infligge " + cardData.damage + " danno");
                
                // Trova un nemico e infliggi danno
                Enemy targetEnemy = FindFirstObjectByType<Enemy>();
                if (targetEnemy != null)
                {
                    targetEnemy.TakeDamage(cardData.damage);
                }
                break;
                
            case CardData.CardType.Skill:
                // Aggiunge blocco o altri effetti
                if (cardData.block > 0)
                {
                    Debug.Log("Aggiunge " + cardData.block + " blocco");
                    // TODO: Aggiungi blocco al giocatore
                }
                break;
                
            case CardData.CardType.Power:
                // Aggiunge un effetto permanente
                Debug.Log("Attiva potere: " + cardData.description);
                // TODO: Implementa effetti potere
                break;
        }
    }

    // Metodo per applicare l'effetto della carta
    public void ApplyCardEffect(Enemy target)
    {
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<CombatManager>();
        }
        
        // Verifica se abbiamo abbastanza energia
        if (combatManager != null && combatManager.TryPlayCard(cardData, cardData.energyCost))
        {
            Debug.Log("Giocata carta: " + cardData.cardName + " su bersaglio: " + target.enemyName);
            
            switch (cardData.cardType)
            {
                case CardData.CardType.Attack:
                    // Infligge danno al bersaglio specificato
                    Debug.Log("Infligge " + cardData.damage + " danno a " + target.enemyName);
                    target.TakeDamage(cardData.damage);
                    break;
                    
                // Per altri tipi di carte, implementa logica specifica
                case CardData.CardType.Skill:
                case CardData.CardType.Power:
                    // Usa la logica generica per carte non mirate
                    ApplyCardEffect();
                    break;
            }
            
            // Rimuovi la carta dalla mano
            Hand hand = originalParent.GetComponent<Hand>();
            if (hand != null)
            {
                hand.RemoveCard(this);
            }
            
            // Distruggi la carta
            Destroy(gameObject);
        }
    }

    public void ShiftCard(float xOffset, float duration)
    {
        StopCoroutine("ShiftCardCoroutine");
        StartCoroutine(ShiftCardCoroutine(xOffset, duration));
    }
    private IEnumerator ShiftCardCoroutine(float xOffset, float duration)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + new Vector3(xOffset, 0, 0);
        float time = 0;
        
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
    }

    
}