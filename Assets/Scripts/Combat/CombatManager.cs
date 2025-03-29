using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    public Hand playerHand;
    public TextMeshProUGUI energyText;
    public Button endTurnButton;
    public GameObject playerStatsUI;
    
    [Header("Player Stats")]
    public int maxEnergy = 3;
    public int currentEnergy;
    public int maxHealth = 75;
    public int currentHealth;
    public int currentBlock;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI blockText;
    public Slider healthBar;
    
    [Header("Deck Configuration")]
    public CardData[] playerDeck;
    private List<CardData> drawPile = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    
    [Header("Draw Pile UI")]
    public GameObject drawPileObject;
    public TextMeshProUGUI drawPileCountText;
    
    [Header("Discard Pile UI")]
    public GameObject discardPileObject;
    public TextMeshProUGUI discardPileCountText;
    
    [Header("Enemies")]
    public Enemy[] enemies;
    
    [Header("Turn Management")]
    public TextMeshProUGUI turnText;
    private int turnNumber = 1;
    private bool isPlayerTurn = true;
    
    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    
    void Start()
    {
        // Inizializza la UI
        UpdateAllUI();
        
        // Inizializza lo stato del giocatore
        currentHealth = maxHealth;
        currentBlock = 0;
        
        // Inizializza il mazzo
        InitializeDeck();
        
        // Avvia il combattimento
        StartNewCombat();
    }
    
    void InitializeDeck()
    {
        // Copia le carte dal playerDeck al drawPile
        drawPile.Clear();
        foreach (CardData card in playerDeck)
        {
            drawPile.Add(card);
        }
        
        // Mescola il drawPile
        ShuffleDeck();
    }
    
    void ShuffleDeck()
    {
        // Fisher-Yates shuffle algorithm
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardData temp = drawPile[i];
            drawPile[i] = drawPile[j];
            drawPile[j] = temp;
        }
    }
    
    void StartNewCombat()
    {
        turnNumber = 1;
        StartPlayerTurn();
    }
    
    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        
        // Aggiorna il testo del turno
        if (turnText != null)
        {
            turnText.text = "Turno " + turnNumber + ": Giocatore";
        }
        
        // Resetta l'energia
        currentEnergy = maxEnergy;
        
        // All'inizio di ogni turno, il blocco si resetta
        currentBlock = 0;
        
       
        
        // Pesca le carte (5 carte standard)
        for (int i = 0; i < 5; i++)
        {
            DrawCard();
        }
        
        // Abilita il pulsante di fine turno
        if (endTurnButton != null)
        {
            endTurnButton.interactable = true;
        }
         // Aggiorna la UI
        UpdateAllUI();
    }
    
    public void EndPlayerTurn()
    {
        isPlayerTurn = false;
        
        // Disabilita il pulsante di fine turno
        if (endTurnButton != null)
        {
            endTurnButton.interactable = false;
        }
        
        DiscardCarteRimaste();
        // Scarta tutte le carte dalla mano
        playerHand.DiscardHand();
        

        // Aggiorna i contatori del mazzo
        UpdateDeckCounters();
        
        // Avvia il turno del nemico
        StartCoroutine(EnemyTurnSequence());
    }

    private void DiscardCarteRimaste(){
        foreach (CardData card in playerHand.cardsInHand)
        {
            discardPile.Add(card);
        }
    }
    
    IEnumerator EnemyTurnSequence()
    {
        // Aggiorna il testo del turno
        if (turnText != null)
        {
            turnText.text = "Turno " + turnNumber + ": Nemico";
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Ogni nemico esegue la sua azione
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                yield return new WaitForSeconds(0.8f);
                enemy.PerformAction();
                
                // Controlla se il giocatore è ancora vivo dopo ogni azione
                if (currentHealth <= 0)
                {
                    EndCombat(false);
                    yield break;
                }
            }
        }
        
        yield return new WaitForSeconds(1f);
        
        // Incrementa il numero del turno
        turnNumber++;
        
        // Verifica se tutti i nemici sono sconfitti
        bool allEnemiesDefeated = true;
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject.activeSelf)
            {
                allEnemiesDefeated = false;
                break;
            }
        }
        
        if (allEnemiesDefeated)
        {
            EndCombat(true);
        }
        else
        {
            // Altrimenti, inizia il prossimo turno del giocatore
            StartPlayerTurn();
        }
    }
    
    public void DrawCard()
    {
        // Se il drawPile è vuoto, mescola il discardPile nel drawPile
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
            {
                // Non ci sono più carte da pescare
                Debug.Log("Non ci sono più carte da pescare!");
                return;
            }
            
            // Sposta le carte dalla pila degli scarti alla pila di pesca
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            
            // Mescola il mazzo di pesca
            ShuffleDeck();
        }
        
        // Pesca la prima carta dal drawPile
        CardData cardToDraw = drawPile[0];
        drawPile.RemoveAt(0);
        
        // Aggiungi la carta alla mano
        playerHand.AddCard(cardToDraw);
        
        // Aggiorna i contatori del mazzo
        UpdateDeckCounters();
    }
    
    public bool TryPlayCard(CardData card, int energyCost)
    {
        // Verifica se abbiamo abbastanza energia
        if (currentEnergy >= energyCost)
        {
            // Sottrai l'energia
            currentEnergy -= energyCost;
            
            // Aggiungi la carta al discardPile
            discardPile.Add(card);
            
            // Aggiorna la UI
            UpdateAllUI();
            
            return true;
        }
        
        return false;
    }
    
    // Metodo per aggiungere blocco al giocatore
    public void AddBlock(int amount)
    {
        currentBlock += amount;
        UpdateAllUI();
    }
    
    // Metodo per infliggere danno al giocatore
    public void TakeDamage(int amount)
    {
        // Prima assorbi il danno con il blocco
        if (currentBlock > 0)
        {
            if (currentBlock >= amount)
            {
                currentBlock -= amount;
                amount = 0;
            }
            else
            {
                amount -= currentBlock;
                currentBlock = 0;
            }
        }
        
        // Poi applica il danno rimanente alla salute
        if (amount > 0)
        {
            currentHealth -= amount;
            
            // Assicurati che la salute non scenda sotto zero
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
            
            // Controlla se il giocatore è stato sconfitto
            if (currentHealth <= 0)
            {
                EndCombat(false);
            }
        }
        
        // Aggiorna la UI
        UpdateAllUI();
    }
    
    // Termina il combattimento
    void EndCombat(bool playerWon)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                gameOverText.text = playerWon ? "Hai Vinto!" : "Hai Perso!";
            }
        }
        
        // Disabilita l'interazione con le carte e i pulsanti
        if (endTurnButton != null)
        {
            endTurnButton.interactable = false;
        }
        
        // Qui puoi aggiungere la logica per le ricompense o per tornare al menu
    }
    
    // Aggiorna tutti gli elementi della UI
    void UpdateAllUI()
    {
        UpdateEnergyDisplay();
        UpdateHealthDisplay();
        UpdateDeckCounters();
    }
    
    // Aggiorna il display dell'energia
    void UpdateEnergyDisplay()
    {
        if (energyText != null)
        {
            energyText.text = currentEnergy + "/" + maxEnergy;
        }
    }
    
    // Aggiorna il display della salute
    void UpdateHealthDisplay()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + "/" + maxHealth;
        }
        
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        if (blockText != null)
        {
            blockText.text = currentBlock > 0 ? currentBlock.ToString() : "";
        }
    }
    
    // Aggiorna i contatori del mazzo
    void UpdateDeckCounters()
    {
        if (drawPileCountText != null)
        {
            drawPileCountText.text = drawPile.Count.ToString();
        }
        
        if (discardPileCountText != null)
        {
            discardPileCountText.text = discardPile.Count.ToString();
        }
    }
    
    // Metodi pubblici per i pulsanti UI
    
    // Pulsante per terminare il turno
    public void OnEndTurnButtonClicked()
    {
        if (isPlayerTurn)
        {
            EndPlayerTurn();
        }
    }
    
    // Pulsante per visualizzare il mazzo di pesca
    public void OnDrawPileClicked()
    {
        // Qui puoi implementare la visualizzazione del mazzo
        Debug.Log("Carte nel mazzo di pesca: " + drawPile.Count);
    }
    
    // Pulsante per visualizzare la pila degli scarti
    public void OnDiscardPileClicked()
    {
        // Qui puoi implementare la visualizzazione degli scarti
        Debug.Log("Carte nella pila degli scarti: " + discardPile.Count);
    }

      
}