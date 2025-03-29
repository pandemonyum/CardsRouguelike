using UnityEngine;

public class PlayZone : MonoBehaviour
{
    [Header("Visual Feedback")]
    public bool showGizmo = true;
    public Color gizmoColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);
    
    // La zona di gioco è definita dal RectTransform di questo oggetto
    private RectTransform rectTransform;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogError("PlayZone richiede un RectTransform. Aggiungilo al GameObject.");
        }
    }
    
    // Verifica se una carta è sopra la zona di gioco
    public bool IsCardOverPlayZone(Card card)
    {
        Vector3 cardPosition = card.transform.position;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        // Verifica se la posizione della carta è all'interno dei confini della zona di gioco
        return cardPosition.x >= corners[0].x && cardPosition.x <= corners[2].x &&
               cardPosition.y >= corners[0].y && cardPosition.y <= corners[2].y;
    }
    
    // Visualizza un'area nel Scene view
    void OnDrawGizmos()
    {
        if (showGizmo && rectTransform != null)
        {
            Gizmos.color = gizmoColor;
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            // Disegna un quadrilatero
            Gizmos.DrawLine(corners[0], corners[1]);
            Gizmos.DrawLine(corners[1], corners[2]);
            Gizmos.DrawLine(corners[2], corners[3]);
            Gizmos.DrawLine(corners[3], corners[0]);
            
            // Riempimento
            Vector3 center = (corners[0] + corners[2]) * 0.5f;
            Vector3 size = corners[2] - corners[0];
            Gizmos.DrawCube(center, size);
        }
    }
}