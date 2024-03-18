using System.Collections.Generic;
using UnityEngine;

public class SimpleAnimator
{
    /// <summary>
    /// Key: AnimName
    /// </summary>
    private Dictionary<string, AnimInfo> _anims;
    private float _timeSacle = 1f;
    private string _currentAnimName;
    private int _currentAnimPriority;
    private float _timer;
    private Animator _animator;

    public SimpleAnimator(Animator animator)
    {
        _animator = animator;
        _currentAnimPriority = int.MinValue;
    }

    public void Update(float deltaTime)
    {
        if (_timer > 0f)
        {
            _timer -= deltaTime * _timeSacle;
        }
    }

    public void Play(string animName)
    {
        if (_currentAnimName != null && _currentAnimName == animName)
        {
            return;
        }
        if (!_anims.ContainsKey(animName))
        {
            return;
        }

        AnimInfo toAnimInfo = _anims[animName];
        int currentPriority = _currentAnimName == null ? 0 : _currentAnimPriority;
        if (_timer <= 0f)
        {
            currentPriority = 0;
        }

        if (toAnimInfo.Priority >= currentPriority)
        {
            _currentAnimName = animName;
            _timer = toAnimInfo.Duration;
            _currentAnimPriority = toAnimInfo.Priority;
            _animator.Play(toAnimInfo.AnimHash);
        }
    }

    public void SetTimeScale(float timeScale)
    {
        _timeSacle = timeScale;
    }

    public void SetAnims(Dictionary<string, AnimInfo> anims)
    {
        _anims = anims;
    }
}

/// 可能的需求：动画有多个可能性，最常见的比如受伤动画可能有3种
public class AnimInfo
{
    public int Priority;
    public float Duration;
    public int AnimHash;

    public AnimInfo(int priority, float duration, int animHash)
    {
        AnimHash = animHash;
        Priority = priority;
        Duration = duration;
    }
}