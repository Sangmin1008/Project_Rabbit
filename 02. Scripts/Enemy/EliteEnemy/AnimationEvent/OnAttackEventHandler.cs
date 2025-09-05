using UnityEngine;

public class OnAttackEventHandler : MonoBehaviour
{
    [Tooltip("루트 EliteEnemyController 참조")]
    [SerializeField] private BaseEnemyController _controller;

    public BaseEnemyController Controller
    {
        get => _controller;
        set => _controller = value;
    }

    private bool _hasHitThisAttack = false;

    public void ResetHitFlag()
    {
        _hasHitThisAttack = false;
    }

    //  근접 공격 시 사운드 전용 메서드
    public void OnMeleeSound()
    {
        if (_controller == null)
        {
            Debug.LogError("OnAttackEventHandler: _controller 할당 안 됨");
            return;
        }

        //_controller.PlayAttackSound();
    }

    //  근접 공격 순간 호출하는 메서드 (정예, 일반 공용으로 변경)
    public void OnMeleeHit()
    {
        if (_hasHitThisAttack)
        {
            return;
        }

        _hasHitThisAttack = true;

        if (_controller == null)
        {
            Debug.LogError("OnAttackEventHandler: _controller 할당 안 됨");
            return;
        }

        _controller.PlayAttackSound();

        _controller?.ExecuteMeleeHit();
    }

    public void EnableOutline()
    {
        _controller?.ShowOutline();
    }

    public void DisableOutline()
    {
        _controller?.HideOutline();
    }
}
