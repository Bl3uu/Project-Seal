using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Equipment/Weapon")]
public class WeaponItemData : EquipmentItemData
{
    public int Damage = 10;
    public float AttackRate = 1.0f;
}