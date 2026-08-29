using UnityEngine;

public interface IAimProvider
{
    Vector2 AimDirection { get; }
    Vector2 AimWorldPosition { get; }
}
