using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public string enemyName = "Nemico";
    public int maxHealth = 45;
    public int currentHealth;
    public int baseDamage = 8;
    public int baseBlock = 5;
    
    [Header("UI Elements")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI intentionText;
    public Image intentionIcon;
    
    [Header("Visual Elements")]
    public Image enemyImage;
    public Animator animator;  // Opzionale, per animazioni
    
    [Header("Intention Settings")]
    public Sprite attackIntentionIcon;
    public Sprite defendIntentionIcon;
    public Sprite buffIntentionIcon;
    public Sprite debuffIntentionIcon;

    [Header("Block Display")]
    public GameObject blockDisplay;  // A GameObject that contains your block icon and text
    public TextMeshProUGUI blockText;  // Text to show the block amount
    
    // Riferimento al combat manager
    private CombatManager combatManager;
    
    // Stato corrente e intenzione
    private enum EnemyIntention { Attack, Defend, Buff, Debuff }
    private EnemyIntention currentIntention;
    
    // Pattern di comportamento
    [Header("Behavior Pattern")]
    [Range(0, 100)]
    public int attackChance = 60;
    [Range(0, 100)]
    public int defendChance = 30;
    [Range(0, 100)]
    public int buffChance = 10;
    [Range(0, 100)]
    public int debuffChance = 0;
    
    // Modificatori di stato
    private int damageModifier = 0;
    private int blockModifier = 0;
    private int buffCounter = 0;
    
    void Start()
    {
        // Inizializza la salute
        currentHealth = maxHealth;
        
        // Trova il combat manager
        combatManager = FindObjectOfType<CombatManager>();
        
        // Aggiorna la UI
        UpdateHealthUI();
        
        // Imposta la prima intenzione
        SetRandomIntention();
    }
    
    // Aggiorna la UI della salute
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = currentHealth + "/" + maxHealth;
        }
    }
    
    // Imposta un'intenzione basata sulle probabilità configurate
    public void SetRandomIntention()
    {
        // Calcola il totale delle probabilità
        int totalChance = attackChance + defendChance + buffChance + debuffChance;
        
        // Normalizza a 100 se necessario
        if (totalChance == 0)
        {
            attackChance = 100;
            totalChance = 100;
        }
        
        // Genera un numero casuale
        int roll = Random.Range(0, totalChance);
        
        // Determina l'intenzione in base al roll
        if (roll < attackChance)
        {
            currentIntention = EnemyIntention.Attack;
            UpdateIntentionUI("Attacco", attackIntentionIcon, baseDamage + damageModifier);
        }
        else if (roll < attackChance + defendChance)
        {
            currentIntention = EnemyIntention.Defend;
            UpdateIntentionUI("Difesa", defendIntentionIcon, baseBlock + blockModifier);
        }
        else if (roll < attackChance + defendChance + buffChance)
        {
            currentIntention = EnemyIntention.Buff;
            UpdateIntentionUI("Potenziamento", buffIntentionIcon);
        }
        else
        {
            currentIntention = EnemyIntention.Debuff;
            UpdateIntentionUI("Indebolimento", debuffIntentionIcon);
        }
    }
    
    // Aggiorna l'icona e il testo dell'intenzione
    void UpdateIntentionUI(string intentionName, Sprite icon, int value = 0)
    {
        if (intentionText != null)
        {
            if (value > 0)
            {
                intentionText.text = intentionName + " " + value;
            }
            else
            {
                intentionText.text = intentionName;
            }
        }
        
        if (intentionIcon != null && icon != null)
        {
            intentionIcon.sprite = icon;
        }
    }
    
    // Esegue l'azione basata sull'intenzione corrente
    public void PerformAction()
    {
        if (combatManager == null)
        {
            combatManager = FindFirstObjectByType<CombatManager>();
            if (combatManager == null)
            {
                Debug.LogError("Combat Manager non trovato. Impossibile eseguire l'azione.");
                return;
            }
        }
        if (currentBlock > 0)
        {
            currentBlock = 0;
            UpdateBlockUI();
        }
        
        // Esegui l'azione in base all'intenzione corrente
        switch (currentIntention)
        {
            case EnemyIntention.Attack:
                PerformAttack();
                break;
                
            case EnemyIntention.Defend:
                PerformDefend();
                break;
                
            case EnemyIntention.Buff:
                PerformBuff();
                break;
                
            case EnemyIntention.Debuff:
                PerformDebuff();
                break;
        }
        
        // Riduci il contatore dei buff
        if (buffCounter > 0)
        {
            buffCounter--;
            if (buffCounter == 0)
            {
                // Resetta i modificatori quando i buff scadono
                damageModifier = 0;
                blockModifier = 0;
            }
        }

        
        // Imposta una nuova intenzione per il prossimo turno
        SetRandomIntention();
    }

   

    
    // Esegue un attacco contro il giocatore
    void PerformAttack()
    {
        int damage = baseDamage + damageModifier;
        
        // Animazione di attacco (opzionale)
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        Debug.Log(enemyName + " attacca per " + damage + " danni!");
        
        // Infliggi danno al giocatore
        if (combatManager != null)
        {
            combatManager.TakeDamage(damage);
        }
    }
    
    // Guadagna blocco
    void PerformDefend()
    {
        int block = baseBlock + blockModifier;
        
        // Animazione di difesa (opzionale)
        if (animator != null)
        {
            animator.SetTrigger("Defend");
        }
        
        Debug.Log(enemyName + " si difende per " + block + " blocco!");
        
        // Aggiungi blocco all'enemy (da implementare)
        AddBlock(block);
    }
    
    // Potenzia le proprie statistiche
    void PerformBuff()
    {
        // Animazione di buff (opzionale)
        if (animator != null)
        {
            animator.SetTrigger("Buff");
        }
        
        // Aumenta il danno base del 50% per 2 turni
        damageModifier += Mathf.CeilToInt(baseDamage * 0.5f);
        buffCounter = 2;
        
        Debug.Log(enemyName + " si potenzia! Danno aumentato a " + (baseDamage + damageModifier) + " per 2 turni!");
        
        // Aggiorna immediatamente l'intenzione se è di attacco
        if (currentIntention == EnemyIntention.Attack)
        {
            UpdateIntentionUI("Attacco", attackIntentionIcon, baseDamage + damageModifier);
        }
    }
    
    // Applica un debuff al giocatore
    void PerformDebuff()
    {
        // Animazione di debuff (opzionale)
        if (animator != null)
        {
            animator.SetTrigger("Debuff");
        }
        
        Debug.Log(enemyName + " applica un debuff al giocatore!");
        
        // Implementa l'effetto debuff sul giocatore (funzionalità da sviluppare)
    }
    
    // Meccanica di blocco
    private int currentBlock = 0;
    
    // Aggiunge blocco
    public void AddBlock(int amount)
    {
        currentBlock += amount;
        // Qui aggiorna un'eventuale UI del blocco nemico
        UpdateBlockUI();
    }

    private void UpdateBlockUI()
{
    if (blockDisplay != null)
    {
        // Only show the block display when there is block
        blockDisplay.SetActive(currentBlock > 0);
    }
    
    if (blockText != null)
    {
        if(currentBlock==0)
            blockText.text = "";
        else
            blockText.text = currentBlock.ToString();
    }
}
    
    // Prende danno, tenendo conto del blocco
    public void TakeDamage(int amount)
    {
        // Prima sottrai il blocco
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
             UpdateBlockUI();
        }
        
        // Poi applica il danno rimanente
        if (amount > 0)
        {
            currentHealth -= amount;
            
            // Attiva l'animazione di danno (opzionale)
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
            
            // Controlla se il nemico è stato sconfitto
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        // Aggiorna la UI
        UpdateHealthUI();
    }
    
    // Gestisce la morte del nemico
    void Die()
    {
        Debug.Log(enemyName + " è stato sconfitto!");
        
        // Animazione di morte (opzionale)
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        currentHealth=0;
        // Disattiva questo nemico
        //gameObject.SetActive(false);
        
        // Qui puoi aggiungere logica per ricompense, ecc.
    }
    
    // Crea un'anteprima visuale nel pannello Scene
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Disegna un'etichetta con il nome
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + new Vector3(0, 1, 0), enemyName);
        #endif
    }
}