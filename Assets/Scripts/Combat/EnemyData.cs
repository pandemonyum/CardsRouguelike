using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public Sprite enemySprite;
    
    [Header("Stats")]
    public int maxHealth = 45;
    public int baseDamage = 8;
    public int baseBlock = 5;
    
    [Header("Behavior")]
    [Range(0, 100)]
    public int attackChance = 60;
    [Range(0, 100)]
    public int defendChance = 30;
    [Range(0, 100)]
    public int buffChance = 10;
    [Range(0, 100)]
    public int debuffChance = 0;
    
    [Header("Sprites")]
    public Sprite attackIntentionIcon;
    public Sprite defendIntentionIcon;
    public Sprite buffIntentionIcon;
    public Sprite debuffIntentionIcon;
}