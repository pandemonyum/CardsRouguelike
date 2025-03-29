using UnityEngine;

using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public GameObject cardPrefab; // Prefab della carta UI
    public Transform handTransform; // Dove posizionare le carte
    
    public List<CardData> cardsInHand = new List<CardData>();
    public int maxHandSize = 10;

    [Header("Hover Effects")]
    public float cardSpreadDistance = 60f;  // Distanza di apertura a ventaglio
    public float hoverAnimationSpeed = 0.2f;  // Velocità dell'animazione
    private Card hoveredCard = null;

    // Metodo per gestire l'hover sulle carte
    public void HandleCardHover(Card hoveredCard, bool isEnter)
    {
        if (isEnter)
        {
            // Trova l'indice della carta con hover
            int hoveredIndex = -1;
            List<Card> cardsInHandComponents = new List<Card>();
            
            for (int i = 0; i < handTransform.childCount; i++)
            {
                Card card = handTransform.GetChild(i).GetComponent<Card>();
                cardsInHandComponents.Add(card);
                
                if (card == hoveredCard)
                {
                    hoveredIndex = i;
                }
            }
            
            if (hoveredIndex >= 0)
            {
                // Sposta le carte a sinistra della carta con hover verso sinistra
                for (int i = 0; i < hoveredIndex; i++)
                {
                    // Più la carta è vicina a quella con hover, più si sposta
                    float distanceFactor = 1f - (float)(hoveredIndex - i) / hoveredIndex;
                    if (distanceFactor < 0.3f) distanceFactor = 0.3f;  // Spostamento minimo
                    
                    cardsInHandComponents[i].ShiftCard(-cardSpreadDistance * distanceFactor, hoverAnimationSpeed);
                }
                
                // Sposta le carte a destra della carta con hover verso destra
                for (int i = hoveredIndex + 1; i < handTransform.childCount; i++)
                {
                    // Più la carta è vicina a quella con hover, più si sposta
                    float distanceFactor = 1f - (float)(i - hoveredIndex) / (handTransform.childCount - hoveredIndex);
                    if (distanceFactor < 0.3f) distanceFactor = 0.3f;  // Spostamento minimo
                    
                    cardsInHandComponents[i].ShiftCard(cardSpreadDistance * distanceFactor, hoverAnimationSpeed);
                }
            }
        }
        else
        {
            // Quando l'hover termina, riarrangia tutte le carte
            ArrangeCards();
        }
    }
    // Aggiunge una carta alla mano
    public void AddCard(CardData cardData)
    {
        if (cardsInHand.Count >= maxHandSize)
        {
            Debug.Log("Mano piena!");
            return;
        }
        
        cardsInHand.Add(cardData);
        
        // Crea l'oggetto carta visuale
        GameObject cardObject = Instantiate(cardPrefab, handTransform);
        Card cardComponent = cardObject.GetComponent<Card>();
        
        if (cardComponent != null)
        {
            cardComponent.Initialize(cardData);
        }
        
        // Riorganizza le carte nella mano
        ArrangeCards();
    }
    
    // Rimuove una carta dalla mano
    public void RemoveCard(Card card)
    {
        if (card.cardData != null && cardsInHand.Contains(card.cardData))
        {
            cardsInHand.Remove(card.cardData);
            Destroy(card.gameObject);
            
            // Riorganizza le carte nella mano
            ArrangeCards();
        }
    }
    
    // Dispone le carte in un arco
    void ArrangeCards()
    {
        int cardCount = handTransform.childCount;
        
        if (cardCount == 0) return;
        
        // Configurazione per l'allineamento orizzontale
        float cardWidth = 160f;  // Larghezza stimata di una carta (regola in base alle tue dimensioni)
        float spacing = 20f;     // Spazio tra le carte
        float totalWidth = (cardWidth + spacing) * cardCount - spacing;
        float startX = -totalWidth / 2 + cardWidth / 2;  // Per centrare le carte
        
        for (int i = 0; i < cardCount; i++)
        {
            Transform cardTransform = handTransform.GetChild(i);
            
            // Posiziona le carte in orizzontale con spazio uniforme
            float x = startX + i * (cardWidth + spacing);
            float y = 0f;  // Tutte le carte sulla stessa linea orizzontale
            
            // Imposta la posizione senza rotazione
            cardTransform.localPosition = new Vector3(x, y, 0);
            cardTransform.localRotation = Quaternion.identity;  // Senza rotazione
            
            // Resetta la posizione originale dopo il riordinamento
            Card cardComponent = cardTransform.GetComponent<Card>();
            if (cardComponent != null)
            {
                cardComponent.ResetOriginalPosition();
            }
        }
    }
    
    // Scarta tutta la mano
    public void DiscardHand()
    {
        for (int i = handTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(handTransform.GetChild(i).gameObject);
        }
        
        cardsInHand.Clear();
    }
    public void CardHovered(Card card)
    {
        hoveredCard = card;
        
        // Trova l'indice della carta con hover
        int hoveredIndex = -1;
        for (int i = 0; i < handTransform.childCount; i++)
        {
            if (handTransform.GetChild(i).GetComponent<Card>() == card)
            {
                hoveredIndex = i;
                break;
            }
        }
        
        if (hoveredIndex == -1) return;
        
        // Per ogni carta nella mano
        for (int i = 0; i < handTransform.childCount; i++)
        {
            // Salta la carta con hover
            if (i == hoveredIndex) continue;
            
            Card otherCard = handTransform.GetChild(i).GetComponent<Card>();
            
            // Sposta SOLO le carte a destra della carta con hover
            if (i > hoveredIndex) {
                // Calcola l'intensità dello spostamento (le carte più vicine si spostano di più)
                float intensity = 1f - Mathf.Clamp01((i - hoveredIndex) * 0.3f);
                float offset = cardSpreadDistance * intensity;
                
                // Sposta la carta con animazione
                Vector3 targetPos = otherCard.originalPosition + new Vector3(offset, 0, 0);
                otherCard.StopAllCoroutines();
                otherCard.StartCoroutine(otherCard.MoveCardTo(targetPos, 0.2f));
            }
            // Le carte a sinistra non si muovono
        }
    }

    // Chiamato quando il mouse esce da una carta
    public void CardUnhovered()
    {
        hoveredCard = null;
        
        // Riporta tutte le carte alla loro posizione originale
        for (int i = 0; i < handTransform.childCount; i++)
        {
            Card card = handTransform.GetChild(i).GetComponent<Card>();
            card.StopAllCoroutines();
            card.StartCoroutine(card.MoveCardTo(card.originalPosition, 0.2f));
        }
    }


    
}
