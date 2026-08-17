using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryGridUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryGridManager gridManager;
    [SerializeField] private RectTransform gridContainerRect;
    [SerializeField] private RectTransform itemContainer;

    [Header("Test Spawning")]
    [SerializeField] private ItemUI itemPrefab;
    [SerializeField] private EquipmentItemData testItemData;

    [Header("Grid Layout Settings")]
    [SerializeField] private float cellSize = 64f;
    [SerializeField] private float spacing = 2f;

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (GetGridPosition(mousePosition, out int gridX, out int gridY))
            {
                // Check backend logic to see if item fits at (gridX, gridY)
                if (gridManager.CanPlaceItem(testItemData, gridX, gridY))
                {
                    gridManager.PlaceItem(testItemData, gridX, gridY);
                    SpawnItemUI(testItemData, gridX, gridY);
                }
                else
                {
                    Debug.Log($"Cannot place {testItemData.Name} at X = {gridX}, Y = {gridY}. Space occupied or out of bounds.");
                }
            }
        }
    }

    private void SpawnItemUI(EquipmentItemData data, int gridX, int gridY)
    {
        ItemUI spawnedItem = Instantiate(itemPrefab, itemContainer);
        spawnedItem.Setup(data, cellSize, spacing);

        RectTransform itemRect = spawnedItem.GetComponent<RectTransform>();
        itemRect.anchoredPosition = GetPositionFromGrid(gridX, gridY);
    }

    public Vector2 GetPositionFromGrid(int gridX, int gridY)
    {
        float posX = gridX * (cellSize + spacing);
        float posY = -(gridY * (cellSize + spacing));
        return new Vector2(posX, posY);
    }

    /// <summary>
    /// Translates raw mouse screen position into array grid indices.
    /// </summary>
    public bool GetGridPosition(Vector2 mouseScreenPosition, out int gridX, out int gridY)
    {
        gridX = -1;
        gridY = -1;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridContainerRect, mouseScreenPosition, null, out Vector2 localPoint))
        {
            return false;
        }

        float totalCellSizeX = cellSize + spacing;
        float totalCellSizeY = cellSize + spacing;

        gridX = Mathf.FloorToInt(localPoint.x / totalCellSizeX);
        gridY = Mathf.FloorToInt(-localPoint.y / totalCellSizeY);

        // Validates calculated position against backend matrix dimensions
        return (gridX >= 0 && gridX < gridManager.GridWidth &&
                gridY >= 0 && gridY < gridManager.GridHeight);
    }
}