using UnityEngine;

public interface ITankControl
{
    Vector2 Move { get; }
    Vector3 AimDir { get; }
    bool isAim { get; }
    bool isFire { get; }
}