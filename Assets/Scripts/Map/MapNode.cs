using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapNode : MonoBehaviour
{
    [Header("Node Settings")]
    public int nodeId;
    public NodeType nodeType;
    public bool isRevealed = true;
    public bool isSelectable;
    public bool isComplete;

    [Header("Visual Elements")]
    public Image nodeImage;
    public Image highlightImage;
    public Image completionImage;
    public TextMeshProUGUI nodeLabel;

    [Header("Node Icons")]
    public Sprite normalNodeSprite;
    public Sprite eliteNodeSprite;
    public Sprite shopNodeSprite;
    public Sprite restNodeSprite;
    public Sprite bossNodeSprite;
    public Sprite mysteryNodeSprite;

    [Header("Node States")]
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.yellow;
    public Color completeColor = Color.gray;
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("Connections")]
    public List<MapNode> connectedNodes = new List<MapNode>();
    public List<LineRenderer> connectionLines = new List<LineRenderer>();

    private bool isHighlighted;

    [SerializeField] private GameObject lineImagePrefab; // Crea un prefab semplice con un'immagine bianca
    private List<RectTransform> connectionImages = new List<RectTransform>();

    public enum NodeType
    {
        Normal,
        Elite,
        Shop,
        Rest,
        Boss,
        Mystery
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void SetupNode(NodeType type, int id)
    {
        nodeType = type;
        nodeId = id;

        // Setup the node sprite based on its type
        Sprite sprite = normalNodeSprite;
        string labelText = "";

        switch (type)
        {
            case NodeType.Normal:
                sprite = normalNodeSprite;
                labelText = "Battle";
                break;
            case NodeType.Elite:
                sprite = eliteNodeSprite;
                labelText = "Elite";
                break;
            case NodeType.Shop:
                sprite = shopNodeSprite;
                labelText = "Shop";
                break;
            case NodeType.Rest:
                sprite = restNodeSprite;
                labelText = "Rest";
                break;
            case NodeType.Boss:
                sprite = bossNodeSprite;
                labelText = "Boss";
                break;
            case NodeType.Mystery:
                sprite = mysteryNodeSprite;
                labelText = "?";
                break;
        }

        if (nodeImage != null)
            nodeImage.sprite = sprite;

        if (nodeLabel != null)
        {
            nodeLabel.text = labelText;
            nodeLabel.transform.rotation = Quaternion.identity;
            nodeLabel.transform.localScale = Vector3.one;
        }


        UpdateVisuals();
    }

    public void Connect(MapNode otherNode)
    {
        if (!connectedNodes.Contains(otherNode))
        {
            connectedNodes.Add(otherNode);
            CreateConnectionLine(otherNode);
        }

        if (!otherNode.connectedNodes.Contains(this))
        {
            otherNode.connectedNodes.Add(this);
        }
    }

    private void CreateConnectionLine(MapNode target)
    {
        // Usa il nuovo sistema di linee UI
        UILineConnector line = UILineConnector.CreateLine(
                transform,
                transform as RectTransform,
                target.transform as RectTransform,
                new Color(1f, 1f, 1f, 0.8f), // Colore bianco più opaco
                3f  // Larghezza della linea
            );

        // Crea una texture tratteggiata
        Texture2D dashTex = new Texture2D(16, 1);
        for (int x = 0; x < 16; x++)
        {
            Color pixelColor = x < 8 ? Color.white : new Color(1, 1, 1, 0);
            dashTex.SetPixel(x, 0, pixelColor);
        }
        dashTex.Apply();
        dashTex.wrapMode = TextureWrapMode.Repeat;

        // Applica la texture
        Image image = line.GetComponent<Image>();
        if (image != null)
        {
            Sprite dashSprite = Sprite.Create(dashTex,
                                            new Rect(0, 0, 16, 1),
                                            new Vector2(0.5f, 0.5f));
            image.sprite = dashSprite;
            // Imposta il materiale per supportare la ripetizione della texture
            Material mat = new Material(Shader.Find("UI/Default"));
            mat.mainTextureScale = new Vector2(5, 1); // Aumenta la scala per più tratteggi
            image.material = mat;
        }

        // Salva nella lista delle connessioni
        connectionObjects.Add(line.gameObject);

    }
    private List<GameObject> connectionObjects = new List<GameObject>();


    // Aggiungi questo metodo:
    private void UpdateLinePosition(LineRenderer line, MapNode target)
    {
        if (line != null && target != null)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, target.transform.position);

            // Debug per verificare che le posizioni siano corrette
            Debug.Log($"Linea da {transform.position} a {target.transform.position}");
        }
    }

    private void LateUpdate()
    {
        // Update connection line positions (needed for UI elements)
        /* for (int i = 0; i < connectedNodes.Count && i < connectionLines.Count; i++)
         {
             if (connectionLines[i] != null)
             {
                 connectionLines[i].SetPosition(0, transform.position);
                 connectionLines[i].SetPosition(1, connectedNodes[i].transform.position);
             }
         }*/

        // Aggiorna tutte le connessioni
        for (int i = 0; i < connectedNodes.Count && i < connectionObjects.Count; i++)
    {
        GameObject lineObj = connectionObjects[i];
        MapNode targetNode = connectedNodes[i];
        
        if (lineObj != null && targetNode != null)
        {
            UILineConnector line = lineObj.GetComponent<UILineConnector>();
            if (line != null)
            {
                // Usa UpdateLine invece di SetPoints
                line.UpdateLine(4f);
            }
        }
    }
    }

    public void SetHighlighted(bool highlighted)
    {
        isHighlighted = highlighted;
        UpdateVisuals();
    }

    public void SetSelectable(bool selectable)
    {
        isSelectable = selectable;
        UpdateVisuals();
    }

    public void SetComplete(bool complete)
    {
        isComplete = complete;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (nodeImage != null)
        {
            if (!isRevealed)
            {
                nodeImage.sprite = mysteryNodeSprite;
                nodeImage.color = normalColor;
            }
            else
            {
                // Set appropriate sprite
                switch (nodeType)
                {
                    case NodeType.Normal:
                        nodeImage.sprite = normalNodeSprite;
                        break;
                    case NodeType.Elite:
                        nodeImage.sprite = eliteNodeSprite;
                        break;
                    case NodeType.Shop:
                        nodeImage.sprite = shopNodeSprite;
                        break;
                    case NodeType.Rest:
                        nodeImage.sprite = restNodeSprite;
                        break;
                    case NodeType.Boss:
                        nodeImage.sprite = bossNodeSprite;
                        break;
                    case NodeType.Mystery:
                        nodeImage.sprite = mysteryNodeSprite;
                        break;
                }

                // Set color based on state
                if (isComplete)
                    nodeImage.color = completeColor;
                else if (!isSelectable)
                    nodeImage.color = disabledColor;
                else if (isHighlighted)
                    nodeImage.color = highlightedColor;
                else
                    nodeImage.color = normalColor;
            }
        }

        // Update highlight image
        if (highlightImage != null)
        {
            highlightImage.enabled = isHighlighted && isSelectable;
        }

        // Update completion image
        if (completionImage != null)
        {
            completionImage.enabled = isComplete;
        }
    }

    public void OnClick()
    {
        if (isSelectable)
        {
            MapManager mapManager = FindObjectOfType<MapManager>();
            if (mapManager != null)
            {
                mapManager.NodeSelected(this);
            }
        }
    }

    public void OnHover(bool isHovering)
    {
        if (isSelectable)
        {
            SetHighlighted(isHovering);
        }
    }
}
