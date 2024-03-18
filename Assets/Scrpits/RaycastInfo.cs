using System;
using UnityEngine;

[Serializable]
public struct RaycastOrigins
{
    public Vector2 TopLeft;
    public Vector2 TopRight;
    public Vector2 BottomLeft;
    public Vector2 BottomRight;
}

[Serializable]
public struct CollisionInfo
{
    public bool Above;
    public bool Below;
    public bool Left;
    public bool Right;
    public bool BelowOneway;

    public void Reset()
    {
        Above = false;
        Below = false;
        Left = false;
        Right = false;
        BelowOneway = false;
    }
}