using System.Collections.Generic;
using UnityEngine;

// Aggiungi questo script al parent delle tue carte
public class CardHand : MonoBehaviour
{
    [Header("Impostazioni Spostamento")]
    [SerializeField] private float spreadDistance = 35f;
    [SerializeField] private float animationDuration = 0.2f;

    private List<CardHover> cardsInHand = new List<CardHover>();
    private CardHover currentHoveredCard;

    private void Start()
    {
        RefreshCardsList();
    }

    // Raccogli tutte le carte con componenti CardHover
    public void RefreshCardsList()
    {
        cardsInHand.Clear();
        CardHover[] cardHovers = GetComponentsInChildren<CardHover>();
        
        foreach (var card in cardHovers)
        {
            cardsInHand.Add(card);
        }
    }

    // Metodo chiamato da CardHover quando viene attivato l'hover
    public void SpreadCardsApart(CardHover hoveredCard)
    {
        currentHoveredCard = hoveredCard;
        int hoveredIndex = cardsInHand.IndexOf(hoveredCard);
        
        if (hoveredIndex < 0)
        {
            Debug.LogWarning("Carta con hover non trovata nella lista!");
            return;
        }
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (i == hoveredIndex)
                continue; // Salta la carta con hover
            
            float offsetX = 0;
            
            if (i < hoveredIndex)
            {
                // Carte a sinistra si spostano ulteriormente a sinistra
                offsetX = -spreadDistance;
            }
            else
            {
                // Carte a destra si spostano ulteriormente a destra
                offsetX = spreadDistance;
            }
            
            // Applica lo spostamento
            cardsInHand[i].OffsetHorizontally(offsetX, animationDuration);
        }
    }

    // Riporta tutte le carte alla posizione originale
    public void ResetCardPositions()
    {
        currentHoveredCard = null;
        
        foreach (var card in cardsInHand)
        {
            card.ResetHorizontalPosition(animationDuration);
        }
    }
}