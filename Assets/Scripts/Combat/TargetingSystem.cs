// Salva come Assets/Scripts/Combat/TargetingSystem.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TargetingSystem : MonoBehaviour
{
    public GameObject targetArrow;
    public float arrowOffset = 100f; // Distanza sopra il nemico
    
    private List<Enemy> availableTargets = new List<Enemy>();
    private int currentTargetIndex = 0;
    public Card selectedCard;
    public bool isTargeting = false;
    
    // Singleton pattern per accesso facile
    public static TargetingSystem Instance;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
    public void StartTargeting(Card card)
    {
        RefreshTargets();
        
        // Se non ci sono bersagli, esci
        if (availableTargets.Count == 0)
            return;
            
        selectedCard = card;
        isTargeting = true;
        
        // Attiva la freccia
        if (targetArrow != null)
        {
            targetArrow.SetActive(true);
            currentTargetIndex = 0;
            UpdateArrowPosition();
        }
        
        // Attiva input da tastiera per cambiare bersaglio
        EnableTargetingInput();
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
        if (!isTargeting || availableTargets.Count == 0)
            return;
            
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
        selectedCard = null;
        
        // Disattiva la freccia
        if (targetArrow != null)
            targetArrow.SetActive(false);
            
        // Disattiva input da tastiera
        DisableTargetingInput();
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
            
        // Cambia bersaglio con tasti freccia
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            NextTarget();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PreviousTarget();
        }
        
        // Conferma con Enter o click sinistro
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            ConfirmTarget();
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