using UnityEngine;

[CreateAssetMenu(fileName = "New Resource Item", menuName = "Inventory/Resource")]
public class ResourceItemData : ItemData
{
    public int MaxStackQuantity = 99;
    public string CraftingCategory;
}