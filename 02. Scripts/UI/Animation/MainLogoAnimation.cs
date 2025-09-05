using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainLogoAnimation : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private float moveDuration = 2f;
    
    private Image _image;
    private RectTransform _rectTransform;
    private Vector2 _startPos;
    
    public event Action OnAnimationEnd;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
        _startPos = _rectTransform.anchoredPosition;

        Color color = _image.color;
        color.a = 0;
        _image.color = color;
    }

    private void OnEnable()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        float time = 0f;

        while (time < Mathf.Max(fadeDuration, moveDuration))
        {
            time += Time.deltaTime;

            if (time < fadeDuration)
            {
                Color color = _image.color;
                color.a = Mathf.Lerp(0f, 1f, time / fadeDuration);
                _image.color = color;
            }

            float moveT = Mathf.Clamp01(time / moveDuration);
            _rectTransform.anchoredPosition = Vector2.Lerp(_startPos, _startPos + Vector2.down * moveDistance, moveT);

            yield return null;
        }

        Color finalColor = _image.color;
        finalColor.a = 1f;
        _image.color = finalColor;
        _rectTransform.anchoredPosition = _startPos + Vector2.down * moveDistance;
        UIManager.Instance.Open<TitleMenuUI>();
    }
}
