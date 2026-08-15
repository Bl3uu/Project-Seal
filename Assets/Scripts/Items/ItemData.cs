using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string ID;
    public string Name;
    public Sprite Icon;
    [TextArea] public string Description;
}