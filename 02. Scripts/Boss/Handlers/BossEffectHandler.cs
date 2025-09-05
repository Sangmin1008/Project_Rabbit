using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BossController))]
public class BossEffectHandler : MonoBehaviour
{
    #region 필드 세팅
    [FormerlySerializedAs("dashDustEffect")]
    [Header("돌진 먼지 이펙트")]
    [SerializeField] private EffectController dashEffect;
    [SerializeField] private Transform dashEffectPoint;

    [Header("돌진 애프터이미지")]
    [SerializeField] public AfterimageController afterimageController;
    [SerializeField] private Material dashMaterial;
    [SerializeField] private Gradient dashGradient;

    [Header("돌진 히트박스")]
    [SerializeField] private GameObject chargeHitbox;

    [Header("TNT 발사지점")]
    [SerializeField] private Transform tntFirePoint;

    private SpriteRenderer _spriteRenderer;

    #endregion

    #region 초기화
    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    #endregion

    #region 외부 공개 메서드들
    public bool ChargeHitboxActive
    {
        get { return chargeHitbox != null && chargeHitbox.activeSelf; }
    }

    public void StartDashEffects()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        //  스스로 flipX 상태를 읽어보기
        bool flipX = _spriteRenderer.flipX;

        // 먼지 이펙트
        if (dashEffect != null)
        {
            dashEffect.Play(dashEffectPoint.position, flipX);
        }

        // 애프터이미지
        if (afterimageController != null)
        {
            afterimageController.StartEffect(dashMaterial, dashGradient, flipX);
        }

        // 히트박스 켜기
        if (chargeHitbox != null)
        {
            chargeHitbox.SetActive(true);
        }
    }

    public void StopDashEffects()
    {
        if (afterimageController != null)
        {
            afterimageController.Stop();
        }

        if (chargeHitbox != null)
        {
            chargeHitbox.SetActive(false);
        }
    }

    public Transform GetTntFirePoint()
    {
        return tntFirePoint;
    }
    #endregion
}
