using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EliteProjectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private float _speed;
    private float _damage;
    private IAttackable _owner;
    private float _spawnTime;
    private float _lifeTime = 5f;

    private ProjectilePool _pool;
    private ProjectileType _type;

    private SceneAudioManager _audioObject;

    private Vector2 _spawnPosition;
    [SerializeField] private float maxTravelDistance;

    [Header("폭발 이펙트")]
    [SerializeField] private GameObject _explosionPrefab;

    [Header("사운드")]
    [SerializeField] private AudioClip _explosionClip;

    public float Damage { get; set; }
    public AttackType AttackType { get; set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _audioObject = SceneAudioManager.Instance;
    }

    public void SetPool(ProjectilePool pool)
    {
        _pool = pool;
    }

    public void SetPool(ProjectilePool pool, ProjectileType type)
    {
        _pool = pool;
        _type = type;
    }

    public void Initialize(Vector2 direction, float speed, float damage, IAttackable owner)
    {
        _spawnPosition = transform.position;

        _speed = speed;
        _damage = damage;
        _owner = owner;
        _spawnTime = Time.time;

        //  Transform 리셋
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        //  콜라이더 활성화
        if (_collider != null)
        {
            _collider.enabled = true;
        }

        //  물리 속도 세팅
        _rb.linearVelocity = direction * _speed;

        //  투사체 머리가 날아갈 방향을 가리키게 회전시킬 것
        transform.rotation = Quaternion.FromToRotation(Vector3.down, direction);
    }

    private void Update()
    {
        if (Time.time - _spawnTime >= _lifeTime)
        {
            SpawnExplosion();
            ReturnToPool();
        }

        float sqrDistance = ((Vector2)transform.position - _spawnPosition).sqrMagnitude;

        if (sqrDistance > maxTravelDistance * maxTravelDistance)
        {
            SpawnExplosion();
            ReturnToPool();
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //  패링 우선 처리
        if (collision.TryGetComponent<PlayerController>(out var player))
        {
            //  실제 데미지 대상 찾기
            if (!player.IsDead)
            {
                player.TakeDamage(_owner);

                //  폭발 이펙트 재생
                SpawnExplosion();

                ReturnToPool();
            }
        }
    }

    private void SpawnExplosion()
    {
        if (_explosionPrefab == null)
        {
            return;
        }

        if (_explosionClip != null)
        {
            _audioObject.PlaySfx(_explosionClip);
        }

        // 폭발 이펙트 생성
        var fx = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);

        Destroy(fx, 1f);
    }

    private void ReturnToPool()
    {
        //  콜라이더 다시 비활성화
        if (_collider != null)
        {
            _collider.enabled = false;
        }

        //  풀로 반환
        if (_pool != null)
        {
            _pool.Return(gameObject, _type);
        }

        else
        {
            Destroy(gameObject);
        }
    }
}
