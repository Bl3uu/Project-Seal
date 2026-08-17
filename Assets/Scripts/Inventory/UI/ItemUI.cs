using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private RectTransform rectTransform;

    public EquipmentItemData Data { get; private set; }

    public void Setup(EquipmentItemData data, float cellSize, float spacing)
    {
        Data = data;

        if (data.Icon != null)
        {
            itemIcon.sprite = data.Icon;
        }

        // Set pivot to Top-Left (0, 1) to match grid coordinate math
        rectTransform.pivot = new Vector2(0f, 1f);

        // Dynamically scale UI size based on item grid width and height
        float widthPixels = (data.Width * cellSize) + ((data.Width - 1) * spacing);
        float heightPixels = (data.Height * cellSize) + ((data.Height - 1) * spacing);

        rectTransform.sizeDelta = new Vector2(widthPixels, heightPixels);
    }
}