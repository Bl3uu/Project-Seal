using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2 worldPosition;
    public bool isWalkable;
    public int gridX;
    public int gridY;

    public float gScore;
    public float hScore;
    public Node parentNode;
    public List<Node> neighbors = new List<Node>();

    public float FScore => gScore + hScore;

    public Node(bool isWalkable, Vector2 worldPosition, int gridX, int gridY)
    {
        this.isWalkable = isWalkable;
        this.worldPosition = worldPosition;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
