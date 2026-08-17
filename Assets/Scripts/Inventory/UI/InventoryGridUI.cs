using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryGridUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryGridManager gridManager;
    [SerializeField] private RectTransform gridContainerRect;

    [Header("Grid Layout Settings")]
    [SerializeField] private float cellSize = 64f;
    [SerializeField] private float spacing = 2f;

    private void Update()
    {
        // Check if mouse is connected and left button was clicked
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (GetGridPosition(mousePosition, out int gridX, out int gridY))
            {
                Debug.Log($"Clicked inside Grid! Matrix Position: X = {gridX}, Y = {gridY}");
            }
            else
            {
                Debug.Log("Clicked outside the grid bounds.");
            }
        }
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