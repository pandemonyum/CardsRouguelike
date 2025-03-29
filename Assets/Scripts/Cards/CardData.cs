using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public string description;
    public int energyCost;
    public Sprite artwork;
    public CardType cardType;
    public CardRarity rarity;
    
    // Attributi extra per le carte di attacco
    public int damage;
    
    // Attributi extra per le carte di difesa/abilità
    public int block;
    
    // Attributi extra per carte con effetti speciali
    public int magicNumber; // Numero variabile usato per effetti speciali
    
    // Enumerazioni per i tipi di carte
    public enum CardType
    {
        Attack,
        Skill,
        Power,
        Status,
        Curse
    }
    
    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Special,
        Curse
    }
}
