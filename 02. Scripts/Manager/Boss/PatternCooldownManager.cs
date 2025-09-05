using System;
using UnityEngine;

public class PatternCooldownManager
{
    #region 인스턴스 변수
    private float _remainingCooldown = 0f;

    #endregion

    #region 프로퍼티
    public bool IsCoolingDown
    {
        get { return _remainingCooldown > 0f; }
    }

    #endregion


    #region 이벤트
    public event Action OnCooldownEnded;

    #endregion

    #region 퍼블릭 메서드
    public void StartCooldown(float seconds)
    {
        _remainingCooldown = Mathf.Max(0f, seconds);
    }

   
    public void Update(float deltaTime)
    {
        if (_remainingCooldown <= 0f)
        {
            return;
        }

        //  매 프레임 호출해서 남은 쿨다운 시간을 갱신
        _remainingCooldown -= deltaTime;

        // 0초 이하가 되면 OnCooldownEnded 이벤트를 발생시킵니다.
        if (_remainingCooldown <= 0f)
        {
            _remainingCooldown = 0f;
            OnCooldownEnded?.Invoke();
        }
    }
    #endregion

}
