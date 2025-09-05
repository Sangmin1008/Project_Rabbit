using System;
using UnityEngine;

public class DustEffectHandler : MonoBehaviour
{
    private Action<GameObject> onEffectComplete;

    public void Init(Action<GameObject> onComplete)
    {
        onEffectComplete = onComplete;
    }

    public void OnAnimationEnd()
    {
        onEffectComplete?.Invoke(gameObject);
    }
}
