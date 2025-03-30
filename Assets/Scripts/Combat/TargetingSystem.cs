// Salva come Assets/Scripts/Combat/TargetingSystem.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TargetingSystem : MonoBehaviour
{
    public GameObject targetArrow;
    public float arrowOffset = 100f; // Distanza sopra il nemico

    public List<Enemy> availableTargets = new List<Enemy>();
    private int currentTargetIndex = 0;
    public Card selectedCard;
    public bool isTargeting = false;

    // Singleton pattern per accesso facile
    public static TargetingSystem Instance;

    private LineRenderer targetingLine;
    private Color normalLineColor = Color.white;
    private Color validTargetColor = Color.red;
    private PointerEventData currentPointerData;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Inizializza il LineRenderer per la freccia di targeting
        targetingLine = gameObject.AddComponent<LineRenderer>();
        targetingLine.startWidth = 5f;
        targetingLine.sortingOrder = 100;
        targetingLine.endWidth = 5f;
        targetingLine.positionCount = 2;
        targetingLine.material = new Material(Shader.Find("Sprites/Default"));
        targetingLine.startColor = normalLineColor;
        targetingLine.endColor = normalLineColor;
        targetingLine.enabled = false;
    }

    void Start()
    {
        // Disattiva la freccia all'inizio
        if (targetArrow != null)
            targetArrow.SetActive(false);

        // Trova tutti i nemici attivi
        RefreshTargets();
    }

    // Chiamato quando si gioca una carta che richiede un bersaglio
    public void StartTargeting(Card card, PointerEventData eventData)
    {
        RefreshTargets();

        // Se non ci sono bersagli, esci
        if (availableTargets.Count == 0)
            return;

        selectedCard = card;
        isTargeting = true;
        currentPointerData = eventData;

        // Attiva la linea di targeting
        if (targetingLine != null)
        {
            targetingLine.enabled = true;
        }

        // Nascondi la freccia tradizionale
        if (targetArrow != null)
        {
            targetArrow.SetActive(false);
        }
    }
    public void SetTargetIndex(int index)
    {
        if (index >= 0 && index < availableTargets.Count)
        {
            currentTargetIndex = index;
        }
    }

    // Aggiorna la posizione della freccia sopra il nemico corrente
    void UpdateArrowPosition()
    {
        if (availableTargets.Count == 0 || currentTargetIndex >= availableTargets.Count)
            return;

        Enemy target = availableTargets[currentTargetIndex];
        Vector3 position = target.transform.position;
        targetArrow.transform.position = new Vector3(position.x, position.y + arrowOffset, position.z);
    }

    // Cambia il bersaglio selezionato
    public void NextTarget()
    {
        currentTargetIndex = (currentTargetIndex + 1) % availableTargets.Count;
        UpdateArrowPosition();
    }

    public void PreviousTarget()
    {
        currentTargetIndex = (currentTargetIndex - 1 + availableTargets.Count) % availableTargets.Count;
        UpdateArrowPosition();
    }

    // Conferma il bersaglio e applica l'effetto della carta
    public void ConfirmTarget()
    {
        if (!isTargeting || availableTargets.Count == 0 || currentTargetIndex < 0 || currentTargetIndex >= availableTargets.Count)
        {
            CancelTargeting();
            return;
        }

        // Prendi il bersaglio corrente
        Enemy target = availableTargets[currentTargetIndex];

        // Applica l'effetto della carta al bersaglio
        if (selectedCard != null)
        {
            selectedCard.ApplyCardEffect(target);
        }

        // Resetta il sistema
        CancelTargeting();
    }

    // Annulla la selezione del bersaglio
    public void CancelTargeting()
    {
        isTargeting = false;

        // Disattiva la linea di targeting
        if (targetingLine != null)
        {
            targetingLine.enabled = false;
        }

        // Se la carta era in fase di targeting, riportala alla posizione originale
        if (selectedCard != null)
        {
            selectedCard.transform.SetParent(selectedCard.originalParent);
            selectedCard.transform.position = selectedCard.originalPosition;
            selectedCard.transform.localScale = selectedCard.originalScale;
        }

        selectedCard = null;
        currentPointerData = null;
    }

    // Aggiorna la lista dei nemici disponibili
    void RefreshTargets()
    {
        availableTargets.Clear();
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            if (enemy.gameObject.activeSelf && enemy.currentHealth > 0)
            {
                availableTargets.Add(enemy);
            }
        }
    }

    // Input da tastiera e click
    void EnableTargetingInput()
    {
        // Questo verrà gestito nell'Update
    }

    void DisableTargetingInput()
    {
        // Resetta stati di input se necessario
    }

    void Update()
{
    if (!isTargeting)
        return;
    
    // Aggiorna la linea di targeting se siamo in modalità targeting
    if (targetingLine != null && targetingLine.enabled && selectedCard != null)
    {
        // Imposta il punto di partenza della linea alla posizione della carta
        Vector3 startPos = selectedCard.transform.position;
        // Il punto finale è la posizione corrente del mouse
        Vector3 endPos = Input.mousePosition;
        
        // Imposta le posizioni della linea
        targetingLine.SetPosition(0, startPos);
        targetingLine.SetPosition(1, endPos);
        
        // Verifica se il mouse è sopra un nemico
        bool isOverEnemy = false;
        int hoveredEnemyIndex = -1;

        for (int i = 0; i < availableTargets.Count; i++)
        {
            Enemy enemy = availableTargets[i];
            
            // Ottieni la posizione dello schermo del nemico
            Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
            
            // Calcola la distanza dal mouse
            float hitDistance = Vector2.Distance(new Vector2(Input.mousePosition.x, Input.mousePosition.y), 
                                                new Vector2(enemyScreenPos.x, enemyScreenPos.y));
            
            // Usa un raggio di rilevamento adeguato
            if (hitDistance < Properies.TarghetHitDistance)
            {
                isOverEnemy = true;
                hoveredEnemyIndex = i;
                currentTargetIndex = hoveredEnemyIndex;
                break;
            }
        }
        
        // Se non siamo sopra un nemico, non impostiamo nessun indice di nemico target
        if (!isOverEnemy)
        {
            currentTargetIndex = -1;
        }
        
        // Cambia il colore della linea in base al fatto che il mouse sia sopra un nemico
        Color lineColor = isOverEnemy ? validTargetColor : normalLineColor;
        targetingLine.startColor = lineColor;
        targetingLine.endColor = lineColor;
    }
    
    // Conferma con il rilascio del pulsante sinistro del mouse SOLO se il mouse è sopra un nemico
    if (Input.GetMouseButtonUp(0))
    {
        // Verifica se il mouse è sopra un nemico prima di confermare
        bool isOverEnemy = false;
        int enemyIndex = -1;

        for (int i = 0; i < availableTargets.Count; i++)
        {
            Enemy enemy = availableTargets[i];
            Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(enemy.transform.position);
            float hitDistance = Vector2.Distance(new Vector2(Input.mousePosition.x, Input.mousePosition.y), 
                                            new Vector2(enemyScreenPos.x, enemyScreenPos.y));
            
            if (hitDistance < Properies.TarghetHitDistance)
            {
                isOverEnemy = true;
                enemyIndex = i;
                break;
            }
        }
        
        if (isOverEnemy)
        {
            // Se il mouse è sopra un nemico, imposta l'indice del target e conferma
            currentTargetIndex = enemyIndex;
            ConfirmTarget();
        }
        else
        {
            // Se il mouse NON è sopra un nemico, annulla il targeting e riporta la carta nella mano
            CancelTargeting();
        }
    }
    
    // Annulla con ESC o click destro
    if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
    {
        CancelTargeting();
    }
}

    // Ritorna il nemico attualmente selezionato
    public Enemy GetCurrentTarget()
    {
        if (availableTargets.Count == 0 || currentTargetIndex >= availableTargets.Count)
            return null;

        return availableTargets[currentTargetIndex];
    }
}