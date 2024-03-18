using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BasicController : MonoBehaviour
{
    public enum CollisionDetectMode
    {
        /// <summary>
        /// 不移动时不检测，移动时向移动方向检测
        /// </summary>
        WhenMoving = 0,
        /// <summary>
        /// 不移动时全方向检测，移动时向移动方向检测
        /// </summary>
        Continuous = 1,
    }

    public LayerMask CollisionMask = int.MaxValue;
    public CollisionDetectMode DetectMode = CollisionDetectMode.Continuous;
    public int HorizontalRayCount = 3;
    public int VerticalRayCount = 3;
    public RaycastOrigins Origins;
    public CollisionInfo CollisionInfo;

    [HideInInspector]
    public const float ShrinkWidth = 0.020f;
    [HideInInspector]
    public const float ExpandWidth = 0.010f;

    protected float _verticalRaySpace;
    protected float _horizontalRaySpace;
    protected BoxCollider2D _boxCollider2D;
    protected RaycastHit2D[] _hits;

    [Header("Debug")]
    public bool Debugging;
    public bool DrawOrigins;
    public bool DrawCollision;

    public virtual void Awake()
    {
        CollisionInfo = new CollisionInfo();
        _hits = new RaycastHit2D[20];
        _boxCollider2D = GetComponent<BoxCollider2D>();

        CalculateRaySpace();
    }

    public void Move(Vector3 movement)
    {
        UpdateRaycastOrigins();
        CollisionInfo.Reset();
        movement.z = 0f;

        CollisionDetect(ref movement);
        this.transform.position += movement;

        {
            Bounds bounds = _boxCollider2D.bounds;
            bounds.center += movement;
            bounds.Expand(ShrinkWidth * -2);

            Origins.TopLeft.x = bounds.min.x;
            Origins.TopLeft.y = bounds.max.y;

            Origins.TopRight.x = bounds.max.x;
            Origins.TopRight.y = bounds.max.y;

            Origins.BottomLeft.x = bounds.min.x;
            Origins.BottomLeft.y = bounds.min.y;

            Origins.BottomRight.x = bounds.max.x;
            Origins.BottomRight.y = bounds.min.y;
        }
    }

    protected virtual void CollisionDetect(ref Vector3 movement)
    {
        HorizontalCollisionDetect(ref movement);
        VerticalCollisionDetect(ref movement);
    }

    /// <summary>
    /// 当movement.x == 0时，rayDirection为1
    /// </summary>
    private void HorizontalCollisionDetect(ref Vector3 movement)
    {
        int detectionCount = 1;
        if (movement.x == 0f)
        {
            if (DetectMode != CollisionDetectMode.Continuous)
            {
                return;
            }
            detectionCount = 2;
        }

        int rayDirection = movement.x >= 0 ? 1 : -1;
        for (int i = 0; i < detectionCount; i++)
        {
            rayDirection = (i == 0) ? rayDirection : -rayDirection;
            Vector2 rayOrigin = (rayDirection == 1) ? Origins.BottomRight : Origins.BottomLeft;
            float rayLength = ShrinkWidth + MathF.Abs(movement.x);
            if (movement.x == 0f)
            {
                rayLength += ExpandWidth;
            }

            for (int j = 0; j < HorizontalRayCount; j++)
            {
                int hitCount = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.right * rayDirection, _hits, rayLength, CollisionMask);
                for (int k = 0; k < hitCount; k++)
                {
                    var hit = _hits[k];
                    if (hit.collider == this._boxCollider2D)
                    {
                        continue;
                    }

                    HandleHorizontalHitResult(hit, ref movement, rayDirection);
                }
                rayOrigin.y += _horizontalRaySpace;
            }
        }
    }

    /// <summary>
    /// 当movement.y == 0时，rayDirection为-1
    /// </summary>
    private void VerticalCollisionDetect(ref Vector3 movement)
    {
        int detectionCount = 1;
        if (movement.y == 0f)
        {
            if (DetectMode == CollisionDetectMode.WhenMoving)
            {
                return;
            }
            else if (DetectMode == CollisionDetectMode.Continuous)
            {
                detectionCount = 2;
            }
        }

        int rayDirection = movement.y > 0 ? 1 : -1;
        for (int i = 0; i < detectionCount; i++)
        {
            rayDirection = (i == 0) ? rayDirection : -rayDirection;
            Vector2 rayOrigin = (rayDirection == 1) ? Origins.TopLeft : Origins.BottomLeft;
            rayOrigin.x += movement.x;
            float rayLength = ShrinkWidth + MathF.Abs(movement.y);
            if (movement.y == 0f)
            {
                rayLength += ExpandWidth;
            }

            for (int j = 0; j < VerticalRayCount; j++)
            {
                int hitCount = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.up * rayDirection, _hits, rayLength, CollisionMask);
                for (int k = 0; k < hitCount; k++)
                {
                    var hit = _hits[k];
                    if (hit.collider == this._boxCollider2D)
                    {
                        continue;
                    }

                    HandleVerticalHitResult(hit, ref movement, rayDirection);
                }
                rayOrigin.x += _verticalRaySpace;
            }
        }
    }

    public void UpdateRaycastOrigins()
    {
        Bounds bounds = _boxCollider2D.bounds;
        bounds.Expand(ShrinkWidth * -2);

        Origins.TopLeft.x = bounds.min.x;
        Origins.TopLeft.y = bounds.max.y;

        Origins.TopRight.x = bounds.max.x;
        Origins.TopRight.y = bounds.max.y;

        Origins.BottomLeft.x = bounds.min.x;
        Origins.BottomLeft.y = bounds.min.y;

        Origins.BottomRight.x = bounds.max.x;
        Origins.BottomRight.y = bounds.min.y;
    }

    public void CalculateRaySpace()
    {
        Bounds bounds = _boxCollider2D.bounds;
        bounds.Expand(ShrinkWidth * -2);
        Vector2 size = bounds.size;
        _horizontalRaySpace = size.y / (HorizontalRayCount - 1);
        _verticalRaySpace = size.x / (VerticalRayCount - 1);
    }

    protected virtual void HandleHorizontalHitResult(RaycastHit2D hit, ref Vector3 movement, int rayDirection)
    {

    }

    protected virtual void HandleVerticalHitResult(RaycastHit2D hit, ref Vector3 movement, int rayDirection)
    {

    }

    public void OnDrawGizmos()
    {
        if (Application.isPlaying && Debugging)
        {
            if (DrawOrigins)
            {
                DrawRaycastOrigins();
            }

            if (DrawCollision)
            {
                DrawCollisionInfo();
            }
        }
    }

    #region Debug Draw

    public void DrawRaycastOrigins()
    {
        Gizmos.color = Color.green;
        var size = Origins.BottomRight - Origins.TopLeft;
        Gizmos.DrawWireCube(Origins.TopLeft + size / 2, size);
    }

    public void DrawCollisionInfo()
    {
        Gizmos.color = Color.red;
        Vector3 center = (Origins.BottomLeft + Origins.TopRight) / 2f;
        if (CollisionInfo.Above)
        {
            Gizmos.DrawLine(center, (Origins.TopLeft + Origins.TopRight) / 2f);
        }
        if (CollisionInfo.Below)
        {
            Gizmos.DrawLine(center, (Origins.BottomLeft + Origins.BottomRight) / 2f);
        }
        if (CollisionInfo.Left)
        {
            Gizmos.DrawLine(center, (Origins.TopLeft + Origins.BottomLeft) / 2f);
        }
        if (CollisionInfo.Right)
        {
            Gizmos.DrawLine(center, (Origins.TopRight + Origins.BottomRight) / 2f);
        }
    }

    #endregion
}