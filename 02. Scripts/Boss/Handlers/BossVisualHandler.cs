using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BossVisualHandler : MonoBehaviour
{
    #region 필드값 세팅
    [Header("피격 플래시 설정")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    #endregion

    #region 내부 상태값
    private SpriteRenderer _renderer;
    private Material _materialInstance;
    private Coroutine _flashRoutine;
    private BossController _boss;

    #endregion

    #region 프로퍼티
    public bool IsFlipped
    {
        get { return _renderer.flipX; }
    }

    #endregion

    #region 초기화
    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();

        _materialInstance = Instantiate(_renderer.material);

        _renderer.material = _materialInstance;

        _boss = GetComponentInParent<BossController>();
    }

    private void OnEnable()
    {
        //  이벤트 구독
        _boss.OnDamaged += FlashHit;
    }

    private void OnDisable()
    {
        //  이벤트 해제
        _boss.OnDamaged -= FlashHit;
    }

    #endregion

    #region 피격 플래시 
    public void FlashHit()
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }

        _flashRoutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        float elapsed = 0f;

        var color = flashColor;
        color.a = 0.8f;
        _materialInstance.SetColor("_FlashColor", color);

        while (elapsed < flashDuration)
        {
            float strength = 1f - (elapsed / flashDuration);
            _materialInstance.SetFloat("_Flash", strength);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _materialInstance.SetFloat("_Flash", 0f);

        _flashRoutine = null;
    }

    #endregion
}
