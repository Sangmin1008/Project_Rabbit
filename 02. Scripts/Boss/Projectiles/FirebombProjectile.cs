using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class FirebombProjectile : MonoBehaviour
{
    #region 필드 & 프로퍼티
    private Vector3 _start;
    private Vector3 _end;
    private float _height;
    private float _duration;
    private ProjectilePool _pool;
    private Coroutine _flightRoutine;
    private bool _hasExploded;

    //  데미지 적용용
    private IAttackable _attacker;
    private int _damage;

    public int Damage
    {
        get { return _damage; }
        set { _damage = value; }
    }

    [Header("사운드")]
    [SerializeField] private AudioClip explosionClip;   //  폭발음

    private SceneAudioManager _audioObject;

    #endregion

    #region 인스펙터 세팅
    [Header("회전 속도")]
    public float rotationSpeed;

    [Header("폭발 VFX")]
    public GameObject explosionEffectPrefab;

    [Header("폭발 반경")]
    public float explosionRadius = 2f;

    [Header("피격 레이어 마스크")]
    public LayerMask hitMask;
    #endregion

    #region 유니티 콜백

    private void Awake()
    {
        _audioObject = SceneAudioManager.Instance;
    }

    private void OnEnable()
    {
        _hasExploded = false;
        transform.rotation = Quaternion.identity;
    }

    private void OnDisable()
    {
        //  코루틴 정리
        if (_flightRoutine != null)
        {
            StopCoroutine(_flightRoutine);
            _flightRoutine = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasExploded)
        {
            return;
        }

        //  플레이어에 직접 맞았을 때
        if (!_hasExploded && collision.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
        {
            StartCoroutine(Explode());
        }
    }
    #endregion

    #region 풀링 세팅
    public void SetPool(ProjectilePool pool)
    {
        _pool = pool;
    }
    #endregion

    #region 발사/초기화
    public void Launch(Vector3 start, Vector3 end, float arcHeight, float duration, HitData hitInfo, IAttackable attacker)
    {
        _start = start;
        _end = end;
        _height = arcHeight;
        _duration = duration;
        _damage = hitInfo.Damage;
        _attacker = attacker;
        _hasExploded = false;

        //  기존 비행 루틴 재시작
        if (_flightRoutine != null)
        {
            StopCoroutine(_flightRoutine);
        }

        _flightRoutine = StartCoroutine(ParabolaFlight());
    }

    private IEnumerator ParabolaFlight()
    {
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / _duration);

            //  선형 보간 + Sin 아크
            Vector3 pos = Vector3.Lerp(_start, _end, time);
            pos.y += _height * Mathf.Sin(Mathf.PI * time);
            transform.position = pos;

            //  회전
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            yield return null;
        }

        //  비행 끝나면 폭발
        yield return Explode();
    }
    #endregion

    #region 폭발 처리
    private IEnumerator Explode()
    {
        if (_hasExploded)
        {
            yield break;
        }

        _hasExploded = true;

        if (explosionClip != null)
        {
            _audioObject.PlaySfx(explosionClip);
        }

        //  폭발 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            GameObject vfx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 1.0f); //  이펙트 클립 길이에 맞게 0.8 ~ 1.2초 정도 조절
        }

        //  반경 내 IDamageable 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitMask);

        foreach (var col in hits)
        {
            if (col.TryGetComponent<IDamageable>(out var target) && !target.IsDead)
            {
                target.TakeDamage(_attacker);
            }
        }

        yield return new WaitForSeconds(0.1f);

        ReturnToPool();
    }
    #endregion

    #region 풀 반환
    private void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Return(gameObject, ProjectileType.Firebomb);
        }

        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
}
