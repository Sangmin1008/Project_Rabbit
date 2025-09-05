using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    [Header("Event Channels")]
    [SerializeField] private VoidEventChannelSO onStartLoadMainScene;
    [SerializeField] private VoidEventChannelSO onStartLoadTitleScene;
    [SerializeField] private IntegerEventChannelSO onSelectLoadScene;
    
    [Header("Fade Effect")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private void OnEnable()
    {
        onStartLoadMainScene.RegisterListener(LoadMainScene);
        onStartLoadTitleScene.RegisterListener(LoadTitleScene);
        onSelectLoadScene.RegisterListener(LoadSceneByIndex);
    }

    private void OnDisable()
    {
        onStartLoadMainScene.UnregisterListener(LoadMainScene);
        onStartLoadTitleScene.UnregisterListener(LoadTitleScene);
        onSelectLoadScene.UnregisterListener(LoadSceneByIndex);
    }

    public void LoadMainScene()
    {
        StartCoroutine(LoadSceneWithFade("01. Scenes/MapScene"));
    }

    public void LoadTitleScene()
    {
        StartCoroutine(LoadSceneWithFade("01. Scenes/TitleScene"));
    }

    private void LoadSceneByIndex(int index)
    {
        string sceneName = "";
        switch (index)
        {
            case 0:
                sceneName = "01. Scenes/MapScene";
                break;
            case 1:
                sceneName = "01. Scenes/TutorialScene";
                break;
            default:
                Debug.LogError($"잘못된 인덱스 씬 {index}");
                return;
        }
        
        StartCoroutine(LoadSceneWithFade(sceneName));
    }
    
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        yield return StartCoroutine(Fade(1f));

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.blocksRaycasts = true;
        
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = (targetAlpha == 1f);
    }
}
