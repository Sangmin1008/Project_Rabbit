using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody2D), typeof(Collider2D))]
public class ShellingProjectile : MonoBehaviour
{
    #region 필드 & 프로퍼티
    private float _speed;
    private Vector2 _direction = Vector2.zero;
    private Vector2 _spawnPosition;

    private IAttackable _attacker;
    private ProjectilePool _pool;

    private bool _hasExploded;

    public int Damage { get; set; }
    public AttackType AttackType { get; set; }

    [SerializeField] private float maxTravelDistance = 20f;

    private SceneAudioManager _audioObject;

    #endregion

    #region 인스펙터 설정
    [Header("폭발 VFX")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [Header("폭발 반경")]
    [SerializeField] private float explosionRadius = 2f;
    [Header("지면 레이어 마스크 (폭발 시)")]
    [SerializeField] private LayerMask groundMask;
    [Header("피격 레이어 마스크")]
    [SerializeField] private LayerMask hitMask;

    [Header("사운드")]
    [SerializeField] private AudioClip explosionClip;

    #endregion

    #region 유니티 콜백
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 0f;
        _audioObject = SceneAudioManager.Instance;
    }

    private void OnEnable()
    {
        _hasExploded = false;
        _spawnPosition = transform.position;
        _rigidbody.linearVelocity = _direction * _speed;
    }

    private void OnDisable()
    {
        _rigidbody.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        // 최대 거리 초과 시 폭발
        if (!_hasExploded && (transform.position - (Vector3)_spawnPosition).sqrMagnitude >= maxTravelDistance * maxTravelDistance)
        {
            StartCoroutine(Explode());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasExploded)
        {
            return;
        }

        //  플레이어에 맞았을 때
        if (collision.TryGetComponent<IDamageable>(out var dmg)&& !dmg.IsDead)
        {
            StartCoroutine(Explode());
            return;
        }

        //  지면에 맞았을 때 
        if (((1 << collision.gameObject.layer) & groundMask) != 0)
        {
            StartCoroutine(Explode());
        }
    }
    #endregion

    #region 풀 반환 세팅
    public void Initialize(ProjectilePool pool, IAttackable attacker, Vector2 direction, float speed)
    {
        _pool = pool;
        _attacker = attacker;
        _direction = direction.normalized;
        _speed = speed;
        _hasExploded = false;

        // Damage, AttackType은 AttackHandler에서 할당해 주세요
        gameObject.SetActive(true);
    }
    #endregion

    #region 폭발 처리
    private IEnumerator Explode()
    {
        _hasExploded = true;

        if (explosionClip != null)
        {
            _audioObject.PlaySfx(explosionClip);
        }

        //  폭발 VFX
        if (explosionEffectPrefab != null)
        {
            var vfx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }

        //  반경 내 모든 IDamageable에 데미지 적용
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitMask);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<IDamageable>(out var target) && !target.IsDead)
            {
                target.TakeDamage(_attacker);
            }
        }

        // 짧게 대기 후 풀로 반환
        yield return new WaitForSeconds(0.1f);

        ReturnToPool();
    }
    #endregion

    #region 풀 반환
    private void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Return(gameObject, ProjectileType.Missile);
        }

        else
        {
            gameObject.SetActive(false);
        }
    }
    #endregion
}
