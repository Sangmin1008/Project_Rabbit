using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class BossAudioHandler : MonoBehaviour
{
    #region 필드 세팅
    [Header("사운드 클립")]
    [SerializeField] private AudioClip hitClip;         //  피격음
    [SerializeField] private AudioClip attackClip;      //  기본 공격음
    [SerializeField] private AudioClip chargeClip;      //  돌진 시작음
    [SerializeField] private AudioClip deathClip;       //  사망음
    [SerializeField] private AudioClip phase2Clip;      //  2페이즈 진입 음향

    #endregion

    #region 내부 컴포넌트
    private SceneAudioManager _audioManager;

    #endregion

    #region 초기화
    private void Awake()
    {
        _audioManager = SceneAudioManager.Instance;
    }

    #endregion

    #region 사운드 재생 메서드
    public void PlayHit()
    {
        if (hitClip != null)
        {
            _audioManager.PlaySfx(hitClip);
        }
    }

    //  기본 공격 사운드 
    public void PlayAttack()
    {
        if (attackClip != null)
        {
            _audioManager.PlaySfx(attackClip);
        }
    }

    //  돌진 시작 사운드
    public void PlayCharge()
    {
        if (chargeClip != null)
        {
            _audioManager.PlaySfx(chargeClip);
        }
    }

    //  사망 사운드
    public void PlayDeath()
    {
        if (deathClip != null)
        {
            _audioManager.PlaySfx(deathClip);
        }
    }

    //  2 페이즈 진입 사운드
    public void PlayPhase2()
    {
        if (phase2Clip != null)
        {
            _audioManager.PlaySfx(phase2Clip);
        }
    }
    #endregion
}
