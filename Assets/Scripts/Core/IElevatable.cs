using UnityEngine;

public interface IElevatable
{
    bool IsOnStairs { get; set; }
    void SetElevation(string physicsLayer, string sortingLayer);
}
