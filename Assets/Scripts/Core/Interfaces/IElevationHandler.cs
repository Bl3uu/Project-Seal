using UnityEngine;

public interface IElevationHandler
{
    bool IsOnStairs { get; set; }
    void SetElevation(string physicsLayer, string sortingLayer);
}
