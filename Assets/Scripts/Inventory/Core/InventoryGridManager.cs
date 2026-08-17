using UnityEngine;

public class InventoryGridManager : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 6;

    //Getters for Grid Width and Height
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;

    private EquipmentItemData[,] gridMatrix;

    private void Awake()
    {
        // Allocates the 10x6 grid in memory (all cells start as null)
        gridMatrix = new EquipmentItemData[gridWidth, gridHeight];
    }

    /// <summary>
    /// 1. Bounds and Overlap Checking
    /// </summary>
    public bool CanPlaceItem(EquipmentItemData item, int targetX, int targetY)
    {
        // Check boundary limits
        if (targetX < 0 || targetY < 0) return false;
        if (targetX + item.Width > gridWidth) return false;
        if (targetY + item.Height > gridHeight) return false;

        // Check for overlapping items in target cells
        for (int x = targetX; x < targetX + item.Width; x++)
        {
            for (int y = targetY; y < targetY + item.Height; y++)
            {
                if (gridMatrix[x, y] != null)
                {
                    return false; // Cell is already occupied
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 2. Placement & Insertion
    /// </summary>
    public bool PlaceItem(EquipmentItemData item, int targetX, int targetY)
    {
        if (!CanPlaceItem(item, targetX, targetY)) return false;

        // Populate all cells in the item's W x H footprint with its reference
        for (int x = targetX; x < targetX + item.Width; x++)
        {
            for (int y = targetY; y < targetY + item.Height; y++)
            {
                gridMatrix[x, y] = item;
            }
        }

        return true;
    }

    /// <summary>
    /// 3. Removal Logic
    /// </summary>
    public void RemoveItem(EquipmentItemData item)
    {
        // Scan the matrix and clear all matching reference cells back to null
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (gridMatrix[x, y] == item)
                {
                    gridMatrix[x, y] = null;
                }
            }
        }
    }
}