using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : BasicController
{
    private Vector3 _boost;
    // when grabbing wall, is facing the wall
    private int _facing = 1;
    private InputData _inputData;
    private StateMachine _stateMachine;
    private Vector3 _velocity;
    private int _jumpCounter;
    private int _dashCounter;
    private bool _wasOnGround;
    private MovingBlock _standingPlatform;
    private MovingBlock _grabbedBlock;
    private SimpleAnimator _simpleAnimator;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private int GrabCheckMask;
    private int GroundCheckMask;

    [Header("Move")]
    public float GroundSpeed = 8f;
    public float Acceleration = 50f;
    public float Deceleration = 80f;
    public float GroundCheckRayLength = 0.03f;

    [Header("Gravity")]
    public const float Gravity = 40f;
    public float GravityScale = 1f;

    [Header("Dash")]
    public float DashSpeed = 20f;
    public float DashTime = 0.15f;
    public float DashTimer;
    public int MaxDash = 1;

    [Header("DownJump")]
    public float DownJumpTimer;
    public float DownJumpTime = 0.25f;
    public bool IsDownJumping;

    [Header("Jump")]
    public float JumpSpeed = 14f;
    public float WallJumpHSpeed = 6f;
    public int MaxJump = 2;

    [Header("Climb")]
    public Transform GrabCheckOrigin;
    public float GrabCheckLength = 0.50f;
    public float GrabCheckRayLength = 0.15f;
    public float ClimbSpeed = 3f;
    public float ClimbAccel = 10f;
    public float ClimbHopYBoost = 10f;

    public TextAsset AnimInfos_Text;

    public const int Normal_State = 1;
    public const int Dash_State = 2;
    public const int Climb_State = 3;

    public override void Awake()
    {
        base.Awake();

        GrabCheckMask = Constant.Layers.TileLayer | Constant.Layers.BlockLayer;
        GroundCheckMask = Constant.Layers.TileLayer | Constant.Layers.BlockLayer;

        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _inputData = new InputData();

        _stateMachine = new StateMachine();
        _stateMachine.AddState(Normal_State, OnNormalUpdate, null, null);
        _stateMachine.AddState(Dash_State, OnDashUpdate, OnDashEnter, null);
        _stateMachine.AddState(Climb_State, OnClimbUpdate, OnClimbEnter, OnClimbExit);
        _stateMachine.StartState(Normal_State);

        _simpleAnimator = new SimpleAnimator(_animator);
        var anims = JsonConvert.DeserializeObject<Dictionary<string, AnimInfo>>(AnimInfos_Text.text);
        _simpleAnimator.SetAnims(anims);
    }

    private void FixedUpdate()
    {
        _simpleAnimator.Update(Time.fixedDeltaTime);
        _stateMachine.Update();
        _wasOnGround = CollisionInfo.Below;
        base.Move(_velocity * Time.deltaTime);

        // down jump timer
        if (IsDownJumping)
        {
            DownJumpTimer += Time.deltaTime;
            if (DownJumpTimer >= DownJumpTime)
            {
                IsDownJumping = false;
                DownJumpTimer = 0f;
            }
        }

        _boost = Vector3.zero;
    }

    #region States

    public bool CanDash => _dashCounter < MaxDash;
    public bool CanJump => _jumpCounter < MaxJump;

    public void DownJump()
    {
        IsDownJumping = true;
        DownJumpTimer = 0f;
    }

    private void Jump()
    {
        _jumpCounter++;
        _velocity.y = JumpSpeed;

        // animation
        if (CollisionInfo.Below)
        {
            _simpleAnimator.Play(Constant.AnimHash.Jump_AnimName);
        }
        else
        {
            _simpleAnimator.Play(Constant.AnimHash.DoubleJump_AnimName);
        }
    }

    private void WallJump(int dir)
    {
        _jumpCounter++;
        _velocity.x = WallJumpHSpeed * dir;
        _velocity.y = JumpSpeed;

        _simpleAnimator.Play(Constant.AnimHash.Jump_AnimName);
    }

    private int OnNormalUpdate()
    {
        _velocity += _boost;

        if (_velocity.y <= 0f)
        {
            CollisionInfo.Below = GroundCheck();
            if (CollisionInfo.Below && !_wasOnGround)
            {
                _standingPlatform = GetStandingBlock();
                if (_standingPlatform != null)
                {
                    _standingPlatform.Add(this);
                }
            }
        }

        // jump counter and dash counter
        if (CollisionInfo.Below)
        {
            _jumpCounter = 0;
            _dashCounter = 0;
        }

        if (_standingPlatform != null && !CollisionInfo.Below)
        {
            _standingPlatform.Remove(this);
            _standingPlatform = null;
        }

        // switch state
        {
            // dash
            if (_inputData.Dash && CanDash)
            {
                _inputData.Dash = false;
                return Dash_State;
            }
            // climb
            if (_inputData.Grab && GrabCheck())
            {
                return Climb_State;
            }
        }

        // running and friction
        if (MathF.Abs(_velocity.x) > GroundSpeed && MathF.Sign(_velocity.x) == MathF.Sign(_inputData.X))
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, _inputData.X * GroundSpeed, Time.deltaTime * Deceleration);
        }
        else
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, _inputData.X * GroundSpeed, Time.deltaTime * Acceleration);
        }

        // gravity
        if ((CollisionInfo.Above && _velocity.y > 0f) || CollisionInfo.Below)
        {
            _velocity.y = 0f;
        }
        else
        {
            _velocity.y = Mathf.MoveTowards(_velocity.y, -30f, Time.deltaTime * GravityScale * Gravity);
        }

        // jump
        if (_inputData.Jump && CanJump)
        {
            _inputData.Jump = false;
            Jump();
        }

        // down jump
        if (_inputData.Y < 0f && CollisionInfo.BelowOneway)
        {
            DownJump();
        }

        // animation
        if (_velocity.y < 0)
        {
            _simpleAnimator.Play(Constant.AnimHash.Fall_AnimName);
        }
        else
        {
            if (MathF.Abs(_velocity.x) > 0)
            {
                _simpleAnimator.Play(Constant.AnimHash.Run_AnimName);
            }
            else
            {
                _simpleAnimator.Play(Constant.AnimHash.Idle_AnimName);
            }
        }
        if (_velocity.x > 0)
        {
            _facing = 1;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (_velocity.x < 0)
        {
            _facing = -1;
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        return Normal_State;
    }

    private void OnDashEnter()
    {
        Vector3 dashDir = new Vector3(MathF.Sign(_inputData.X), MathF.Sign(_inputData.Y));
        if (dashDir == Vector3.zero)
        {
            dashDir = new Vector3(_facing, 0f);
        }
        DashTimer = 0f;
        _velocity = dashDir * DashSpeed;
        _dashCounter++;
    }

    private int OnDashUpdate()
    {
        DashTimer += Time.deltaTime;
        if (DashTimer >= DashTime)
        {
            DashTimer = 0f;
            _velocity = _velocity.normalized * GroundSpeed;
            return Normal_State;
        }

        return Dash_State;
    }

    private void OnClimbEnter()
    {
        _velocity = Vector3.zero;
        _jumpCounter = 0;
        _dashCounter = 0;

        // get grabbed block
        _grabbedBlock = GetGrebbedBlock();
        if (_grabbedBlock != null)
        {
            _grabbedBlock.Add(this);
        }
    }

    private int OnClimbUpdate()
    {
        // switch state
        {
            // normal
            if (!_inputData.Grab)
            {
                return Normal_State;
            }

            // dash
            if (_inputData.Dash && CanDash)
            {
                _inputData.Dash = false;
                return Dash_State;
            }
        }

        // no wall to hold
        if (!GrabCheck())
        {
            // climbe hop
            if (_velocity.y > 0f)
            {
                _velocity.y = MathF.Max(_velocity.y, ClimbHopYBoost);
            }

            return Normal_State;
        }

        // jump
        if (_inputData.Jump && CanJump)
        {
            _inputData.Jump = false;
            if (CollisionInfo.Left && _inputData.X > 0)
            {
                WallJump(1);
                return Normal_State;
            }
            else if (CollisionInfo.Right && _inputData.X < 0)
            {
                WallJump(-1);
                return Normal_State;
            }
        }

        _velocity.y = Mathf.MoveTowards(_velocity.y, _inputData.Y * ClimbSpeed, Time.deltaTime * ClimbAccel);

        // animation
        if (_velocity.y == 0f)
        {
            _simpleAnimator.Play(Constant.AnimHash.WallGrab_AnimName);
        }
        else
        {
            _simpleAnimator.Play(Constant.AnimHash.WallClimb_AnimName);
        }

        return Climb_State;
    }

    private void OnClimbExit()
    {
        if (_grabbedBlock != null)
        {
            _grabbedBlock.Remove(this);
            _grabbedBlock = null;
        }
    }

    #endregion

    public InputData GetInputData()
    {
        return _inputData;
    }

    protected override void HandleHorizontalHitResult(RaycastHit2D hit, ref Vector3 movement, int rayDirection)
    {
        // collision direction
        if (rayDirection == 1)
        {
            CollisionInfo.Right = true;
        }
        else
        {
            CollisionInfo.Left = true;
        }
        // fix movement
        if (movement.x != 0f)
        {
            if (MathF.Abs(hit.distance - ShrinkWidth) < MathF.Abs(movement.x))
            {
                movement.x = (hit.distance - ShrinkWidth) * rayDirection;
            }
        }
    }

    protected override void HandleVerticalHitResult(RaycastHit2D hit, ref Vector3 movement, int rayDirection)
    {
        if (IsDownJumping)
        {
            DownJumpTimer = 0f;
            IsDownJumping = false;
        }

        // collision direction
        if (rayDirection == 1)
        {
            CollisionInfo.Above = true;
        }
        else
        {
            CollisionInfo.Below = true;
        }
        // fix movement
        if (movement.y != 0f)
        {
            if (MathF.Abs(hit.distance - ShrinkWidth) < MathF.Abs(movement.y))
            {
                movement.y = (hit.distance - ShrinkWidth) * rayDirection;
            }
        }
    }

    private bool GrabCheck()
    {
        Vector2 rayOrigin = GrabCheckOrigin.position;
        float space = GrabCheckLength / HorizontalRayCount;
        Vector2 rayDirection = _facing == 1 ? Vector2.right : Vector2.left;
        for (int i = 0; i < HorizontalRayCount; i++)
        {
            if (Physics2D.Raycast(rayOrigin, rayDirection, GrabCheckRayLength, GrabCheckMask))
            {
                // Debug.DrawRay(rayOrigin, rayDirection * GrabCheckRayLength, Color.red);
                return true;
            }
            // Debug.DrawRay(rayOrigin, rayDirection * GrabCheckRayLength, Color.yellow);
            rayOrigin.y -= space;
        }

        return false;
    }

    private MovingBlock GetGrebbedBlock()
    {
        Vector2 rayOrigin = GrabCheckOrigin.position;
        float space = GrabCheckLength / HorizontalRayCount;
        Vector2 rayDirection = _facing == 1 ? Vector2.right : Vector2.left;
        for (int i = 0; i < HorizontalRayCount; i++)
        {
            var hit = Physics2D.Raycast(rayOrigin, rayDirection, GrabCheckRayLength, GrabCheckMask);
            if (hit)
            {
                return hit.collider.GetComponentInChildren<MovingBlock>();
            }
            rayOrigin.y -= space;
        }

        return null;
    }

    private bool GroundCheck()
    {
        Vector2 rayOrigin = Origins.BottomLeft;
        for (int i = 0; i < VerticalRayCount; i++)
        {
            if (Physics2D.Raycast(rayOrigin, Vector2.down, GroundCheckRayLength, GroundCheckMask))
            {
                Debug.DrawRay(rayOrigin, Vector2.down * GroundCheckRayLength, Color.red);
                return true;
            }
            Debug.DrawRay(rayOrigin, Vector2.down * GroundCheckRayLength, Color.yellow);
            rayOrigin.x += _verticalRaySpace;
        }

        return false;
    }

    private MovingBlock GetStandingBlock()
    {
        Vector2 rayOrigin = Origins.BottomLeft;
        for (int i = 0; i < VerticalRayCount; i++)
        {
            var hit = Physics2D.Raycast(rayOrigin, Vector2.down, GroundCheckRayLength, GroundCheckMask);
            if (hit)
            {
                return hit.collider.GetComponentInChildren<MovingBlock>();
            }
            rayOrigin.x += _verticalRaySpace;
        }

        return null;
    }

    public void AddBoost(Vector3 boost)
    {
        _boost += boost;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(GrabCheckOrigin.position, Vector3.down * GrabCheckLength);
    }
}