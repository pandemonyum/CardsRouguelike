using UnityEngine;
using UnityEngine.UI;

public class UILineConnector : MonoBehaviour
{
    public static UILineConnector CreateLine(Transform parent, RectTransform startNode, RectTransform endNode, Color color, float width = 2f)
    {
        GameObject lineObj = new GameObject("UILine");
        lineObj.transform.SetParent(parent);
        lineObj.transform.SetAsFirstSibling(); // Posiziona dietro agli altri elementi
        
        UILineConnector line = lineObj.AddComponent<UILineConnector>();
        Image image = lineObj.AddComponent<Image>();
        
        // Crea texture bianca 1x1
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        
        // Crea uno sprite dalla texture
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        image.color = color;
        
        // Configura RectTransform e disabilita raycast per la linea
        RectTransform rect = lineObj.GetComponent<RectTransform>();
        line.rectTransform = rect;
        image.raycastTarget = false;
        
        // Salva i nodi di riferimento
        line.startNodeRect = startNode;
        line.endNodeRect = endNode;
        
        // Aggiorna la linea
        line.UpdateLine(width);
        
        return line;
    }
    
    public RectTransform rectTransform;
    public RectTransform startNodeRect;
    public RectTransform endNodeRect;
    public float lineWidth = 2f;
    
    public void UpdateLine(float width = 2f)
    {
        if (startNodeRect == null || endNodeRect == null)
            return;
            
        lineWidth = width;
        
        // Ottieni le posizioni dei centri dei nodi in coordinate mondiali
        Vector3[] corners = new Vector3[4];
        startNodeRect.GetWorldCorners(corners);
        Vector3 startCenter = (corners[0] + corners[2]) / 2f;
        
        endNodeRect.GetWorldCorners(corners);
        Vector3 endCenter = (corners[0] + corners[2]) / 2f;
        
        // Calcola lunghezza e angolo
        Vector2 dir = endCenter - startCenter;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // Configura il rettangolo
        rectTransform.sizeDelta = new Vector2(distance, width);
        rectTransform.pivot = new Vector2(0, 0.5f);
        
        // Posiziona all'inizio e ruota verso la fine
        rectTransform.position = startCenter;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
    
    private void LateUpdate()
    {
        // Aggiorna la linea ad ogni frame per mantenere la connessione
        UpdateLine(lineWidth);
    }
}