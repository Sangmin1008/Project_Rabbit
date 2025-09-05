using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ProgressTweener
{
    private float _progressRatio;
    private MonoBehaviour _runner;
    private Coroutine _tweenCoroutine;
    private AnimationCurve _runningCurve;

    public ProgressTweener(MonoBehaviour runner)
    {
        _runner = runner;
    }
    
    
    public ProgressTweener SetCurve(AnimationCurve curve)
    {
        _runningCurve = curve;
        return this;
    }

    public ProgressTweener Play( UnityAction<float> onUpdateToProgressRatio, float duration, UnityAction onComplete = null)
    {
        if (_tweenCoroutine != null)
        {
            _runner.StopCoroutine(_tweenCoroutine);
        }

        _tweenCoroutine = _runner.StartCoroutine(TweenCoroutine(onUpdateToProgressRatio, duration, onComplete));
        return this;
    }

    private IEnumerator TweenCoroutine(UnityAction<float> onUpdateToProgressRatio , float duration, UnityAction onComplete)
    {
        float time = _progressRatio * duration;
        while (time < duration)
        {
            yield return null;
            time += Time.unscaledDeltaTime;
            _progressRatio = _runningCurve != null? _runningCurve.Evaluate(time / duration) : time / duration;
            onUpdateToProgressRatio?.Invoke(_progressRatio);
        }

        _progressRatio = 1;
        onUpdateToProgressRatio?.Invoke(_progressRatio); 
        _runningCurve = null;
        _progressRatio = 0;
        
        onComplete?.Invoke(); 
    }
}