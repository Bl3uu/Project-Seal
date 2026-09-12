using UnityEngine;

public interface IMoveable
{
    float MoveSpeed { get; set; }
    float StairYBias { get; set; }
    Vector2 MoveDirection { get; }
}
