using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Rendering;

public class Phase2TransitionEffects : MonoBehaviour
{
    [Header("Freeze Game Time")]
    [Tooltip("게임을 완전 멈춰둘 시간(초)")]
    [SerializeField] private float freezeTime;

    [Header(" Full-Screen Flash")]
    [Tooltip("풀스크린 플래시 반복 횟수")]
    [SerializeField] private int flashCount;
    [Tooltip("플래시 전체 표시 시간(초)")]
    [SerializeField] private float flashDisplayTime;
    [Tooltip("플래시 페이드아웃 시간(초)")]
    [SerializeField] private float flashFadeTime;
    [Tooltip("플래시 후 대기 시간(초)")]
    [SerializeField] private float flashInterval;
    [Tooltip("풀스크린 플래시용 CanvasGroup")]
    [SerializeField] private CanvasGroup flashOverlay;

    [Header(" Camera Shake")]
    [Tooltip("플래시마다 셰이크 실행할지 여부")]
    [SerializeField] private bool shakeOnFlash = true;
    [Tooltip("플래시 시작 후 셰이크 지연 시간(초)")]
    [SerializeField] private float shakeDelay;
    [Tooltip("카메라 흔들기용 ImpulseSource")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (flashOverlay != null)
        {
            flashOverlay.alpha = 0f;
            flashOverlay.gameObject.SetActive(false);
        }
    }

    //  Phase2 전환 연출 코루틴
    public IEnumerator Play()
    {
        //  게임 완전 정지
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        if (freezeTime > 0f)
        {
            yield return new WaitForSecondsRealtime(freezeTime);
        }

        //  풀스크린 플래시 + 셰이크 반복
        for (int i = 0; i < flashCount; i++)
        {
            //  화면 채우기
            flashOverlay.gameObject.SetActive(true);
            flashOverlay.alpha = 1f;

            //  셰이크
            if (shakeOnFlash && impulseSource != null)
            {
                if (shakeDelay > 0f)
                {
                    yield return new WaitForSecondsRealtime(shakeDelay);
                }

                impulseSource.GenerateImpulse();
            }

            //  화면 유지
            if (flashDisplayTime > 0f)
            {
                yield return new WaitForSecondsRealtime(flashDisplayTime);
            }

            //  페이드 아웃
            float time = 0f;

            while (time < flashFadeTime)
            {
                time += Time.unscaledDeltaTime;
                flashOverlay.alpha = Mathf.Lerp(1f, 0f, time / flashFadeTime);
                yield return null;  
            }

            //  완전 끄기
            flashOverlay.alpha = 0f;
            flashOverlay.gameObject.SetActive(false);

            //  다음 플래시 전 간격
            if (i < flashCount - 1 && flashInterval > 0f)
            {
                yield return new WaitForSecondsRealtime(flashInterval);
            }
        }

        //  전체 깜빡임이 끝난 후에 게임 재개하기
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

}
