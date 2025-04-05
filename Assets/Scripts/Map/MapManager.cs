using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 4;
    [SerializeField] private int mapHeight = 6;
    [SerializeField] private float horizontalSpacing = 150f;
    [SerializeField] private float verticalSpacing = 120f;
    [SerializeField] private int startingNodes = 2;
    [SerializeField] private int pathsPerNode = 2;
    [SerializeField] private float nodeChanceVariation = 0.2f;
    
    [Header("Node Types")]
    [SerializeField, Range(0f, 1f)] private float normalNodeChance = 0.7f;
    [SerializeField, Range(0f, 1f)] private float eliteNodeChance = 0.1f;
    [SerializeField, Range(0f, 1f)] private float restNodeChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float shopNodeChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float mysteryNodeChance = 0.1f;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private RectTransform mapContainer;
    [SerializeField] private Button proceedButton;
    [SerializeField] private Image cursorImage;
    
    [Header("Game Scene")]
    [SerializeField] private string gameBattleScene = "SampleScene";
    
    // Map state
    private List<MapNode> allNodes = new List<MapNode>();
    private List<MapNode> selectableNodes = new List<MapNode>();
    private MapNode currentNode;
    private MapNode selectedNode;
    
    private void Start()
    {
        if (proceedButton != null)
        {
            proceedButton.onClick.AddListener(ProceedToNextScene);
            proceedButton.gameObject.SetActive(false);
        }
        
        GenerateMap();
    }
    
    private void Update()
    {
        // Move cursor to the selected node
        if (cursorImage != null && selectedNode != null)
        {
            cursorImage.transform.position = Vector3.Lerp(
                cursorImage.transform.position, 
                selectedNode.transform.position, 
                Time.deltaTime * 10f);
        }
    }
    
    private void GenerateMap()
    {
        if (nodePrefab == null || mapContainer == null)
        {
            Debug.LogError("Missing required references for map generation!");
            return;
        }
        
        // Calculate centering offset
        RectTransform containerRect = mapContainer as RectTransform;
    
        // Calcola offset per centrare orizzontalmente
        float centerXOffset = (mapWidth - 1) * horizontalSpacing * 0.5f;
        
        // Calcola l'altezza disponibile per la mappa
        float availableHeight = containerRect.rect.height - 40f; // 20px di margine sopra e sotto
        
        // Adatta la spaziatura verticale se necessario
        float calculatedVerticalSpacing = availableHeight / (mapHeight - 1);
        if (calculatedVerticalSpacing < 100f) // Spaziatura minima
        {
            calculatedVerticalSpacing = 100f;
        }
        else if (calculatedVerticalSpacing > verticalSpacing)
        {
            calculatedVerticalSpacing = verticalSpacing; // Usa la spaziatura predefinita se c'è abbastanza spazio
        }
        
        // Create a 2D array to store nodes
        MapNode[,] nodeGrid = new MapNode[mapWidth, mapHeight];
        
        // Generate nodes in a grid layout
        for (int y = 0; y < mapHeight; y++)
        {
            // Calculate how many nodes to create in this row
            int nodesInRow = (y == 0 || y == mapHeight - 1) ? 1 : Random.Range(2, mapWidth + 1);
            
            // Calculate spacing for this row to distribute nodes evenly
            float rowSpacing = (nodesInRow > 1) ? (mapWidth - 1) * horizontalSpacing / (nodesInRow - 1) : 0;
            
            // Create the nodes for this row
            List<int> rowPositions = new List<int>();
            
            // First and last rows special cases
            if (y == 0)
            {
                // Bottom row (one centered node)
                rowPositions.Add(mapWidth / 2);
            }
            else if (y == mapHeight - 1)
            {
                // Top row (one centered node - boss)
                rowPositions.Add(mapWidth / 2);
            }
            else
            {
                // Generate node positions for this row, distributing them randomly
                for (int i = 0; i < nodesInRow; i++)
                {
                    int randomPos;
                    do
                    {
                        randomPos = Random.Range(0, mapWidth);
                    } while (rowPositions.Contains(randomPos));
                    
                    rowPositions.Add(randomPos);
                }
            }
            
            // Create the actual nodes
            foreach (int x in rowPositions)
            {
                // Create a node instance from the prefab
                GameObject nodeObj = Instantiate(nodePrefab, mapContainer);
                RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
                
                // Position the node with variation
                float xPos = x * horizontalSpacing - centerXOffset;
                float yPos = y * calculatedVerticalSpacing ;
                rectTransform.anchoredPosition = new Vector2(xPos, yPos);
                
                // Add a small random offset for a more natural look
                if (y != 0 && y != mapHeight - 1)
                {
                    float xOffset = Random.Range(-horizontalSpacing * 0.15f, horizontalSpacing * 0.15f);
                    float yOffset = Random.Range(-calculatedVerticalSpacing  * 0.1f, calculatedVerticalSpacing  * 0.1f);
                    rectTransform.anchoredPosition += new Vector2(xOffset, yOffset);
                }
                
                // Setup node properties
                MapNode node = nodeObj.GetComponent<MapNode>();
                if (node != null)
                {
                    // Set node ID and type
                    int nodeId = allNodes.Count;
                    MapNode.NodeType nodeType;
                    
                    // Determine node type
                    if (y == 0)
                    {
                        // Starting node
                        nodeType = MapNode.NodeType.Normal;
                        node.isSelectable = true;
                        selectableNodes.Add(node);
                    }
                    else if (y == mapHeight - 1)
                    {
                        // Boss node
                        nodeType = MapNode.NodeType.Boss;
                    }
                    else
                    {
                        // Regular node with weighted random type
                        nodeType = GetRandomNodeType();
                    }
                    
                    // Setup the node
                    node.SetupNode(nodeType, nodeId);
                    node.isRevealed = true;
                    
                    // Add to grid and list
                    nodeGrid[x, y] = node;
                    allNodes.Add(node);
                    
                    // Add click event
                    Button button = nodeObj.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => { node.OnClick(); });
                    }
                }
                else
                {
                    Debug.LogError("Node prefab is missing the MapNode component!");
                }
            }
        }
        
        // Connect nodes with paths
        for (int y = 0; y < mapHeight - 1; y++)
        {
            // Find all nodes in the current row
            List<MapNode> currentRowNodes = new List<MapNode>();
            for (int x = 0; x < mapWidth; x++)
            {
                if (nodeGrid[x, y] != null)
                {
                    currentRowNodes.Add(nodeGrid[x, y]);
                }
            }
            
            // Find all nodes in the next row
            List<MapNode> nextRowNodes = new List<MapNode>();
            for (int x = 0; x < mapWidth; x++)
            {
                if (nodeGrid[x, y + 1] != null)
                {
                    nextRowNodes.Add(nodeGrid[x, y + 1]);
                }
            }
            
            // Connect current row nodes to next row nodes
            foreach (MapNode currentRowNode in currentRowNodes)
            {
                // Determine how many paths from this node
                int pathCount = (y == 0) ? 
                    Mathf.Min(pathsPerNode, nextRowNodes.Count) : 
                    Random.Range(1, Mathf.Min(pathsPerNode + 1, nextRowNodes.Count + 1));
                
                // Create a copy of the list to remove from
                List<MapNode> availableNodes = new List<MapNode>(nextRowNodes);
                
                for (int i = 0; i < pathCount; i++)
                {
                    if (availableNodes.Count > 0)
                    {
                        // Select a random node from the next row
                        int randomIndex = Random.Range(0, availableNodes.Count);
                        MapNode nextRowNode = availableNodes[randomIndex];
                        
                        // Connect the nodes
                        currentRowNode.Connect(nextRowNode);
                        
                        // Remove the node from available nodes to prevent duplicate connections
                        availableNodes.RemoveAt(randomIndex);
                    }
                }
            }
        }
        
        // Make sure every node in the next row has at least one incoming connection
        for (int y = 1; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                MapNode node = nodeGrid[x, y];
                if (node != null)
                {
                    bool hasIncomingConnection = false;
                    
                    // Check for incoming connections
                    foreach (MapNode potentialParent in allNodes)
                    {
                        if (potentialParent.connectedNodes.Contains(node))
                        {
                            hasIncomingConnection = true;
                            break;
                        }
                    }
                    
                    // If no incoming connection, create one from a random node in the previous row
                    if (!hasIncomingConnection)
                    {
                        List<MapNode> previousRowNodes = new List<MapNode>();
                        for (int prevX = 0; prevX < mapWidth; prevX++)
                        {
                            if (nodeGrid[prevX, y - 1] != null)
                            {
                                previousRowNodes.Add(nodeGrid[prevX, y - 1]);
                            }
                        }
                        
                        if (previousRowNodes.Count > 0)
                        {
                            MapNode randomPreviousNode = previousRowNodes[Random.Range(0, previousRowNodes.Count)];
                            randomPreviousNode.Connect(node);
                        }
                    }
                }
            }
        }
        
        // Set initial cursor position
        if (cursorImage != null && selectableNodes.Count > 0)
        {
            cursorImage.transform.position = selectableNodes[0].transform.position;
            cursorImage.gameObject.SetActive(true);
        }
    }
    
    private MapNode.NodeType GetRandomNodeType()
    {
        // Add variation to the chances
        float normal = normalNodeChance * Random.Range(1f - nodeChanceVariation, 1f + nodeChanceVariation);
        float elite = eliteNodeChance * Random.Range(1f - nodeChanceVariation, 1f + nodeChanceVariation);
        float rest = restNodeChance * Random.Range(1f - nodeChanceVariation, 1f + nodeChanceVariation);
        float shop = shopNodeChance * Random.Range(1f - nodeChanceVariation, 1f + nodeChanceVariation);
        float mystery = mysteryNodeChance * Random.Range(1f - nodeChanceVariation, 1f + nodeChanceVariation);
        
        // Normalize the chances
        float total = normal + elite + rest + shop + mystery;
        normal /= total;
        elite /= total;
        rest /= total;
        shop /= total;
        mystery /= total;
        
        // Get a random value
        float randomValue = Random.value;
        
        // Determine the node type
        if (randomValue < normal)
            return MapNode.NodeType.Normal;
        else if (randomValue < normal + elite)
            return MapNode.NodeType.Elite;
        else if (randomValue < normal + elite + rest)
            return MapNode.NodeType.Rest;
        else if (randomValue < normal + elite + rest + shop)
            return MapNode.NodeType.Shop;
        else
            return MapNode.NodeType.Mystery;
    }
    
    public void NodeSelected(MapNode node)
    {
        if (selectableNodes.Contains(node))
        {
            // Update current and selected nodes
            currentNode = node;
            selectedNode = node;
            
            // Update UI
            UpdateSelectableNodes();
            
            // Show proceed button
            if (proceedButton != null)
            {
                proceedButton.gameObject.SetActive(true);
            }
        }
    }
    
    private void UpdateSelectableNodes()
    {
        // Clear current selectable nodes
        foreach (MapNode node in selectableNodes)
        {
            node.SetSelectable(false);
        }
        selectableNodes.Clear();
        
        // Mark current node as complete
        if (currentNode != null)
        {
            currentNode.SetComplete(true);
            
            // Add connected nodes to selectable nodes
            foreach (MapNode connectedNode in currentNode.connectedNodes)
            {
                if (!connectedNode.isComplete)
                {
                    connectedNode.SetSelectable(true);
                    selectableNodes.Add(connectedNode);
                }
            }
        }
    }
    
    public void ProceedToNextScene()
    {
        // Save the selected node type so the game knows what type of encounter to load
        if (selectedNode != null)
        {
            PlayerPrefs.SetInt("SelectedNodeType", (int)selectedNode.nodeType);
            PlayerPrefs.Save();
            
            // Load the appropriate scene
            SceneManager.LoadScene(gameBattleScene);
        }
    }
}