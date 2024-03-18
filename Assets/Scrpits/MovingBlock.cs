using System;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : BasicController
{
    public Vector3[] GlobalWaypoints;
    public float MoveSpeed;

    private int _beforeWaypointIndex;
    private float _percentageBetweenWaypoints;
    private List<Passenger> _passengerList;
    private HashSet<Transform> _hashSet;
    private List<CharacterController2D> _characterList;

    public override void Awake()
    {
        base.Awake();

        _passengerList = new List<Passenger>();
        _hashSet = new HashSet<Transform>();
        _characterList = new List<CharacterController2D>();
    }
    public void FixedUpdate()
    {
        Vector3 movement = CalculateMovement(Time.fixedDeltaTime);
        _hashSet.Clear();
        _passengerList.Clear();
        Move(movement);
    }

    protected override void CollisionDetect(ref Vector3 movement)
    {
        foreach (CharacterController2D character in _characterList)
        {
            if (!_hashSet.Contains(character.transform))
            {
                _hashSet.Add(character.transform);
                _passengerList.Add(new Passenger(character, movement));
            }
        }
        base.CollisionDetect(ref movement);
        foreach (Passenger passenger in _passengerList)
        {
            passenger.Character.Move(passenger.Movement);
        }
    }

    private Vector3 CalculateMovement(float deltaTime)
    {
        _beforeWaypointIndex %= GlobalWaypoints.Length;
        int nextWaypointIndex = (_beforeWaypointIndex + 1) % GlobalWaypoints.Length;
        float distance = Vector3.Distance(GlobalWaypoints[_beforeWaypointIndex], GlobalWaypoints[nextWaypointIndex]);
        _percentageBetweenWaypoints += MoveSpeed * deltaTime / distance;
        _percentageBetweenWaypoints = Mathf.Clamp01(_percentageBetweenWaypoints);
        Vector3 vector = Vector3.Lerp(GlobalWaypoints[_beforeWaypointIndex], GlobalWaypoints[nextWaypointIndex], _percentageBetweenWaypoints);
        if (_percentageBetweenWaypoints >= 1f)
        {
            _percentageBetweenWaypoints = 0f;
            _beforeWaypointIndex++;
        }
        return vector - transform.position;
    }

    protected override void HandleHorizontalHitResult(RaycastHit2D hit, ref Vector3 movement, int rayDirection)
    {
        if (hit.collider != null && !_hashSet.Contains(hit.transform))
        {
            float x = movement.x - (hit.distance - ShrinkWidth) * rayDirection;
            if (hit.transform.TryGetComponent<CharacterController2D>(out CharacterController2D character))
            {
                _hashSet.Add(hit.transform);
                Passenger passenger = new Passenger(character, new Vector3(x, 0f));
                _passengerList.Add(passenger);
            }
        }
    }

    protected override void HandleVerticalHitResult(RaycastHit2D hit, ref Vector3 movement, int rayDirection)
    {
        if (hit.collider != null && !_hashSet.Contains(hit.transform))
        {
            float x = rayDirection == 1f ? movement.x : 0f;
            float y = movement.y - (hit.distance - ShrinkWidth) * rayDirection;
            if (hit.transform.TryGetComponent<CharacterController2D>(out CharacterController2D character))
            {
                _hashSet.Add(hit.transform);
                Passenger passenger = new Passenger(character, new Vector3(x, y));
                _passengerList.Add(passenger);
            }
        }
    }

    public void Add(CharacterController2D character)
    {
        _characterList.Add(character);
    }

    public void Remove(CharacterController2D character)
    {
        if (_characterList.Contains(character))
        {
            _characterList.Remove(character);
        }
    }

    [Serializable]
    private struct Passenger
    {
        public CharacterController2D Character;
        public Vector3 Movement;

        public Passenger(CharacterController2D character, Vector3 movement)
        {
            Character = character;
            Movement = movement;
        }
    }
}