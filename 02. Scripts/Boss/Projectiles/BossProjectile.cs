using Unity.Hierarchy;
using UnityEngine;

// TODO IAttackable 할당
[RequireComponent(typeof(Rigidbody2D))]
public class BossProjectile : MonoBehaviour
{
    #region 필드 & 프로퍼티
    private IAttackable _attacker;
    private float _speed;
    private Vector2 _direction;

    private ProjectilePool _pool;
    private Rigidbody2D _rb;

    private bool _isReturned;
    private bool _hasHit;

    //  발사 위치 저장용
    private Vector2 _spawnPosition;
    [SerializeField] private float maxTravelDistance;

    //  AttackHandler에서 세팅해주는 프로퍼티 
    public int Damage { get; set; }
    public AttackType AttackType { get; set; }
    #endregion

    #region 충돌 시 반환할 레이어
    [Header("충돌 시 반환할 레이어")]
    [SerializeField] private LayerMask _groundMask;

    #endregion

    #region 유니티 콜백
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _isReturned = false;
        _hasHit = false;
        _rb.linearVelocity = _direction * _speed;
    }

    private void OnDisable()
    {
        _rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        //  아직 피격되지 않고, 반환되지 않았다면
        if (!_hasHit && !_isReturned)
        {
            float sqrDistance = ((Vector2)transform.position - _spawnPosition).sqrMagnitude;

            if (sqrDistance > maxTravelDistance * maxTravelDistance)
            {
                ReturnToPool();
            }
        }
    }
    #endregion

    #region 초기화 메서드

    //  풀링용 세팅
    public void SetupPool(ProjectilePool pool)
    {
        _pool = pool;
    }

    public void Initialize(Vector2 direction, IAttackable attacker, float speed)
    {
        _spawnPosition = transform.position;

        _attacker = attacker;
        _direction = direction.normalized;
        _speed = speed;
        _isReturned = false;
        _hasHit = false;
        _rb.linearVelocity = _direction * _speed;

        //  회전 → 머리 부분이 플레이어를 향해서 날아가도록 세팅
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    #endregion

    #region 충돌 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _groundMask) != 0)
        {
            ReturnToPool();
            return;
        }

        if (collision.TryGetComponent<PlayerController>(out PlayerController player))
        {
            TryProcessHit(player);
        }
    }

    private void TryProcessHit(PlayerController player)
    {
        //  이미 처리했거나 반환된 상태면 무시하기
        if (_hasHit || _isReturned)
        {
            return;
        }

        _hasHit = true;

        //  피격 처리
        if (player != null)
        {
            var damage = player.GetComponent<IDamageable>();

            if (damage != null && !damage.IsDead)
            {
                damage.TakeDamage(_attacker);
            }
        }

        ReturnToPool();
    }
    #endregion

    #region 풀 반환
    private void ReturnToPool()
    {
        if (_isReturned)
        {
            return;
        }

        _isReturned = true;

        if (_pool != null)
        {
            _pool.Return(gameObject, ProjectileType.Bullet);
        }

        else
        {
            gameObject.SetActive(false);
        }
    }
    #endregion
}
