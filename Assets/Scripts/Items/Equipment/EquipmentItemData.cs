using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment Item", menuName = "Inventory/Equipment")]
public class EquipmentItemData : ItemData
{
    public int Width = 1;
    public int Height = 1;
    public EquipmentSlot Slot;
}