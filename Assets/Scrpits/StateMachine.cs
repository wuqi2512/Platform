using System.Collections.Generic;
using System;

public class StateMachine
{
    public struct State
    {
        public Func<int> OnUpdate;
        public Action OnEnter;
        public Action OnExit;

        public State(Func<int> onUpdate, Action onEnter, Action onExit)
        {
            OnUpdate = onUpdate;
            OnEnter = onEnter;
            OnExit = onExit;
        }
    }

    public int CurrentState => _currentState;

    private Dictionary<int, State> _states;
    private int _currentState;

    public StateMachine()
    {
        _states = new Dictionary<int, State>();
    }

    public void AddState(int stateId, Func<int> onUpdate, Action onEnter, Action onExit)
    {
        var state = new State(onUpdate, onEnter, onExit);
        _states.Add(stateId, state);
    }
    public void RemoveState(int stateId)
    {
        _states.Remove(stateId);
    }
    public bool Contains(int stateId)
    {
        return _states.ContainsKey(stateId);
    }

    public bool StartState(int stateId)
    {
        if (_currentState == 0 && _states.ContainsKey(stateId))
        {
            _currentState = stateId;
            _states[_currentState].OnEnter?.Invoke();
            return true;
        }
        return false;
    }
    public void Update()
    {
        if (_states[_currentState].OnUpdate != null)
        {
            int stateId = _states[_currentState].OnUpdate.Invoke();
            if (stateId != CurrentState && _states.ContainsKey(stateId))
            {
                _states[_currentState].OnExit?.Invoke();
                _currentState = stateId;
                _states[_currentState].OnEnter?.Invoke();
            }
        }
    }
}