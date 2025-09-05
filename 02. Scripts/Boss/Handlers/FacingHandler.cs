using UnityEngine;

public class FacingHandler
{
    #region 필드 선언
    private readonly SpriteRenderer _spriteRenderer;
    private readonly ColliderFlipHandler _flipCollider;
    private readonly Transform _firePoint;
    private readonly Transform _tntFirePoint;
    private readonly Vector3 _firePointOriginLocal;
    private readonly Vector3 _tntPointOriginLocal;

    #endregion

    #region 생성자
    public FacingHandler(SpriteRenderer spriteRenderer, ColliderFlipHandler flipCollider, Transform firePoint, Transform tntFirePoint)
    {
        _spriteRenderer = spriteRenderer;
        _flipCollider = flipCollider;
        _firePoint = firePoint;
        _tntFirePoint = tntFirePoint;

        _firePointOriginLocal = firePoint != null ? firePoint.localPosition : Vector3.zero;
        _tntPointOriginLocal = tntFirePoint != null ? tntFirePoint.localPosition : Vector3.zero;
    }

    #endregion

    #region 대상 바라보기 처리
    public void FaceToTarget(IDamageable target)
    {
        if (target == null)
        {
            return;
        }

        bool flip = target.Collider.bounds.center.x < _spriteRenderer.transform.position.x;

        _spriteRenderer.flipX = flip;

        _flipCollider.MirrorX(flip);

        if (_firePoint != null)
        {
            _firePoint.localPosition = new Vector3(_firePointOriginLocal.x * (flip ? -1 : 1), _firePointOriginLocal.y, _firePointOriginLocal.z);
        }

        if (_tntFirePoint != null)
        {
            _tntFirePoint.localPosition = new Vector3(_tntPointOriginLocal.x * (flip ? -1 : 1), _tntPointOriginLocal.y, _tntPointOriginLocal.z);
        }
    }

    #endregion
}
