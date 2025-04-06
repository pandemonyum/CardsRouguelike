using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData
{
    public string map_id;
    public int current_level;
    public int rows;
    public int columns;
    public List<NodeData> nodes;
}

[Serializable]
public class NodeData
{
    public string id;
    public string type;
    public NodePosition position;
    public List<string> connections;
    public string state;
}

[Serializable]
public class NodePosition
{
    public int row;
    public int column;
}