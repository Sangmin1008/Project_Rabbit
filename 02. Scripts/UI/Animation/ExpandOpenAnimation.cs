using System;
using System.Collections;
using UnityEngine;

public class ExpandOpenAnimation : MonoBehaviour
{
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private AnimationCurve easeOutCurve;
    [SerializeField] private AnimationCurve easeInCurve;
    
    private RectTransform _rectTransform;
    private Coroutine _coroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.localScale = new Vector3(1, 0, 1);
    }

    public void Open(Action onComplete = null)
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(Animate(0f, 1f, duration, easeOutCurve, onComplete));
    }
    
    public void Close(Action onComplete = null)
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(Animate(1f, 0f, duration, easeInCurve, onComplete));
    }

    private IEnumerator Animate(float start, float end, float time, AnimationCurve curve, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            float t = elapsed / time;
            float evaluated = Mathf.Lerp(start, end, curve.Evaluate(t));
            _rectTransform.localScale = new Vector3(1f, evaluated, 1f);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        _rectTransform.localScale = new Vector3(1f, end, 1f);
        onComplete?.Invoke();
        _coroutine = null;
    }
}
