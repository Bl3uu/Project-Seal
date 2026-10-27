using System.Collections.Generic;
using UnityEngine;

public class AStarPathFinder : MonoBehaviour
{
    public static AStarPathFinder Instance { get; private set;  }

    [Header("Grid Settings")]
    [SerializeField] private Vector2 gridWorldSize = new Vector2(30f, 30f);
    [SerializeField] private float nodeRadius = 0.25f;
    [SerializeField] private LayerMask obstacleMask;

    private Node[,] grid;
    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeGridParameters();
        GenerateGrid();
    }

    private void InitializeGridParameters()
    {
        nodeDiameter = nodeRadius * 2f;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
    }

    public void GenerateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        Vector2 gridCenter = transform.position;
        Vector2 gridHalfSize = gridWorldSize / 2f;
        Vector2 worldBottomLeft = gridCenter - Vector2.right * gridHalfSize.x - Vector2.up * gridHalfSize.y;
    
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                float xOffset = (x * nodeDiameter) + nodeRadius;
                float yOffset = (y * nodeDiameter) + nodeRadius;
                Vector2 worldPoint = worldBottomLeft + new Vector2(xOffset, yOffset);

                bool isWalkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, obstacleMask);
                grid[x, y] = new Node(isWalkable, worldPoint, x, y);
            }
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                grid[x, y].neighbors = GetNeighboringNodes(grid[x, y]);
            }
        }
    }

    public void SetNodeWalkable(Vector2 worldPosition, bool isWalkable)
    {
        Node targetNode = NodeFromWorldPoint(worldPosition);
        if (targetNode != null)
        {
            targetNode.isWalkable = isWalkable;
        }
    }

    public List<Vector2> FindPath(Vector2 startPosition, Vector2 targetPosition)
    {
        Node startNode = NodeFromWorldPoint(startPosition);
        Node targetNode = NodeFromWorldPoint(targetPosition);

        if (startNode == null || targetNode == null || !targetNode.isWalkable)
        {
            return null;
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        ResetGridPathfindingData();

        startNode.gScore = 0f;
        startNode.hScore = Vector2.Distance(startNode.worldPosition, targetNode.worldPosition);
    
        while (openSet.Count > 0)
        {
            Node currentNode = FindNodeWithLowestFScore(openSet);

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in currentNode.neighbors)
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }

                float distanceToNeighbor = Vector2.Distance(currentNode.worldPosition, neighbor.worldPosition);
                float tentativeGScore = currentNode.gScore + distanceToNeighbor;

                if (tentativeGScore < neighbor.gScore)
                {
                    neighbor.parentNode = currentNode;
                    neighbor.gScore = tentativeGScore;
                    neighbor.hScore = Vector2.Distance(neighbor.worldPosition, targetNode.worldPosition);
                
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private Node FindNodeWithLowestFScore(List<Node> openSet)
    {
        Node bestNode = openSet[0];

        for (int i = 1; i < openSet.Count; i++)
        {
            Node candidateNode = openSet[i];

            bool hasLowerFScore = candidateNode.FScore < bestNode.FScore;
            bool isFScoreTied = Mathf.Approximately(candidateNode.FScore, bestNode.FScore);
            bool hasLowerHScore = candidateNode.hScore < bestNode.hScore;

            if (hasLowerFScore || (isFScoreTied && hasLowerHScore))
            {
                bestNode = candidateNode;
            }
        }

        return bestNode;
    }

    private void ResetGridPathfindingData()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                grid[x, y].gScore = float.MaxValue;
                grid[x, y].parentNode = null;
            }
        }
    }

    private List<Vector2> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2> path = new List<Vector2>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.worldPosition);
            currentNode = currentNode.parentNode;
        }

        path.Reverse();
        return path;
    }

    private List<Node> GetNeighboringNodes(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0)
                {
                    continue;
                }

                int neighborGridX = node.gridX + xOffset;
                int neighborGridY = node.gridY + yOffset;

                bool isWithinGridBounds = neighborGridX >= 0 && neighborGridX < gridSizeX &&
                                          neighborGridY >= 0 && neighborGridY < gridSizeY;

                if (isWithinGridBounds)
                {
                    neighbors.Add(grid[neighborGridX, neighborGridY]);
                }
            }
        }

        return neighbors;
    }

    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        Vector2 gridCenter = transform.position;
        Vector2 gridHalfSize = gridWorldSize / 2f;

        float normalizedX = (worldPosition.x - gridCenter.x + gridHalfSize.x) / gridWorldSize.x;
        float normalizedY = (worldPosition.y - gridCenter.y + gridHalfSize.y) / gridWorldSize.y;

        float clampedX = Mathf.Clamp01(normalizedX);
        float clampedY = Mathf.Clamp01(normalizedY);

        int xIndex = Mathf.RoundToInt((gridSizeX - 1) * clampedX);
        int yIndex = Mathf.RoundToInt((gridSizeY - 1) * clampedY);

        return grid[xIndex, yIndex];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 0));

        if (grid != null)
        {
            foreach (Node node in grid)
            {
                if (node.isWalkable)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.15f);
                }
                else
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                }

                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeRadius * 1.8f));
            }
        }
    }
}
