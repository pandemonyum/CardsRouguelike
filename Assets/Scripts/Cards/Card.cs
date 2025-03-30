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
    public Transform originalParent;
    private bool isDragging = false;
    
    [Header("Hover Effect")]
    public float hoverScaleFactor = 1.2f;
    public float hoverYOffset = 30f;
    public float hoverAnimationSpeed = 0.2f;
    public Vector3 originalScale;
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

    [Header("Targeting")]
    public float targetingThreshold = 50f; // La distanza verso l'alto per attivare il targeting
    private Vector2 dragStartPosition;
    private bool isTargeting = false;

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
        dragStartPosition = eventData.position;
        
        // Sposta la carta in primo piano mentre viene trascinata
        transform.SetParent(transform.root);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        
        // Riporta la carta alla scala originale durante il drag
        StopAllCoroutines();
        transform.localScale = originalScale * 1.05f; // Leggermente più grande per visibilità
    }
    
   public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // Se la carta richiede un bersaglio e il trascinamento è verso l'alto
        if (requiresTarget && !isTargeting)
        {
            // Calcola la distanza verticale del trascinamento
            float verticalDrag = dragStartPosition.y - eventData.position.y;
            
            // Se il trascinamento verso l'alto supera la soglia, attiva il targeting
            if (verticalDrag < -targetingThreshold)
            {
                isTargeting = true;
                
                // Attiva il sistema di targeting
                TargetingSystem targetingSystem = FindFirstObjectByType<TargetingSystem>();
                if (targetingSystem != null)
                {
                    // Posiziona la carta nella parte inferiore dello schermo
                    transform.position = new Vector3(Screen.width / 2, 100, 0);
                    targetingSystem.StartTargeting(this, eventData);
                    return;
                }
            }
        }
        
        // Se non stiamo ancora in modalità targeting, segui normalmente il mouse
        if (!isTargeting)
        {
            transform.position = eventData.position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        
        // Se eravamo in modalità targeting, verifica se il mouse è sopra un nemico
        if (isTargeting)
        {
            isTargeting = false;
            
            TargetingSystem targetingSystem = FindFirstObjectByType<TargetingSystem>();
            if (targetingSystem != null && targetingSystem.isTargeting)
            {
               // Verifica se il mouse è sopra un nemico
                bool isOverEnemy = false;
                int enemyIndex = -1;

                foreach (Enemy enemy in targetingSystem.availableTargets)
                {
                    // Ottieni la posizione dello schermo del nemico
                    Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
                    
                    // Calcola la distanza dal mouse
                    float hitDistance = Vector2.Distance(new Vector2(eventData.position.x, eventData.position.y), 
                                                        new Vector2(enemyScreenPos.x, enemyScreenPos.y));
                    
                    // Debug per vedere la distanza
                    Debug.Log($"Distanza da {enemy.enemyName}: {hitDistance}");
                    
                    // Usa un raggio di rilevamento più grande (150-200 pixel è un buon valore)
                    if (hitDistance < Properies.TarghetHitDistance)
                    {
                        isOverEnemy = true;
                        enemyIndex = targetingSystem.availableTargets.IndexOf(enemy);
                        Debug.Log($"Mouse sopra nemico: {enemy.enemyName}");
                        break;
                    }
                }
                
                if (isOverEnemy)
                {
                    // Se il mouse è sopra un nemico, imposta l'indice e conferma
                    targetingSystem.SetTargetIndex(enemyIndex);
                    targetingSystem.ConfirmTarget();
                }
                else
                {
                    // Se non è sopra un nemico, annulla il targeting
                    targetingSystem.CancelTargeting();
                }
                return;
            }
        }
        
        // Altrimenti usa la logica regolare di rilascio
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
        
        // Per carte che non richiedono bersaglio
        if (!requiresTarget)
        {
            // Verifica se abbiamo abbastanza energia
            if (combatManager != null && combatManager.TryPlayCard(cardData, cardData.energyCost))
            {
                // Applica l'effetto della carta senza bersaglio
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
        else
        {
            // Per carte che richiedono bersaglio, non facciamo nulla qui
            // perché il targeting è gestito da OnDrag e dal TargetingSystem
            // Riportiamo la carta nella mano
            transform.SetParent(originalParent);
            transform.position = startPosition;
            transform.localScale = originalScale;
            
            // Opzionale: mostra un messaggio all'utente
            Debug.Log("Questa carta richiede un bersaglio. Trascina verso l'alto per mirare.");
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