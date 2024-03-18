using UnityEngine;

public class Fan : MonoBehaviour
{
    private Vector2 _right;
    private Vector2 _up;
    private float _raySpace;
    private RaycastHit2D[] _hits;
    private BoxCollider2D _boxCollider;
    private Animator _animator;


    public Transform RayOrigin;
    public float AreaLength;
    public int RayCount = 3;
    public float BoostSpeed = 5f;
    public float AreaMaxLength = 10f;

    public void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();

        _hits = new RaycastHit2D[16];
        _up = transform.up;
        _right = transform.right;
        _raySpace = _boxCollider.size.x / (RayCount - 1);
        AreaLength = Mathf.Min(AreaLength, AreaMaxLength);
        _animator.Play(Constant.AnimHash.On_Hash);
    }

    public void FixedUpdate()
    {
        Vector2 rayOrigin = RayOrigin.position;
        for (int i = 0; i < RayCount; i++)
        {
            int count = Physics2D.RaycastNonAlloc(rayOrigin, _up, _hits, AreaLength, Constant.Layers.EntityLayer);
            for (int j = 0; j < count; j++)
            {
                CharacterController2D character = _hits[j].collider.GetComponent<CharacterController2D>();
                character.AddBoost(_up * BoostSpeed);
            }
            Debug.DrawRay(rayOrigin, _up * AreaLength, Color.yellow);
            rayOrigin += _right * _raySpace;
        }
    }
}
