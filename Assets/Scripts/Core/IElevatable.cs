using UnityEngine;

public interface IElevatable
{
    int CurrentHeight { get; }
    bool IsOnStairs { get; set; }
    void SetElevation(int heightLevel, string sortingLayer);
}
