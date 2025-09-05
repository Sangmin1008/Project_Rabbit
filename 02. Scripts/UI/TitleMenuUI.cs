using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuUI : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button exitButton;
    
    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AnimationCurve easeOutCurve;
    [SerializeField] private AnimationCurve easeInCurve;
    [SerializeField] private ExpandOpenAnimation expandAnimation;
    
    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSfx;
    
    [Space(10)]
    [SerializeField] DialogSystem dialogSystem;

    
    private ProgressTweener _canvasGroupTweener;

    private void Awake()
    {
        base.Awake();
        _canvasGroupTweener = new(this);
    }

    private void OnEnable()
    {
        startButton.onClick.AddListener(OnStartClicked);
        loadButton.onClick.AddListener(OnLoadClicked);
        settingButton.onClick.AddListener(OnSettingClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(OnStartClicked);
        loadButton.onClick.RemoveListener(OnLoadClicked);
        settingButton.onClick.RemoveListener(OnSettingClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
    }

    private void Start()
    {
        
    }

    private void OnStartClicked()
    {
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);
        // Debug.Log("게임 시작");
        // SceneManager.LoadScene("01. Scenes/TutorialScene");
        UIManager.Instance.Close<TitleMenuUI>();
        dialogSystem.AdvanceDialog();
    }

    private void OnLoadClicked()
    {
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);
        UIManager.Instance.Close<TitleMenuUI>();
        UIManager.Instance.Open<SaveLoadMenuUI>(false);
    }

    private void OnSettingClicked()
    {
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);
        UIManager.Instance.Close<TitleMenuUI>();
        UIManager.Instance.Open<SettingUI>();
    }

    private void OnExitClicked()
    {
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);
        //Debug.Log("게임 종료");
        Application.Quit();
    }
    
    public override void Open()
    {
        IsOpen = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        // _canvasGroupTweener.Play((ratio) => canvasGroup.alpha = Mathf.Lerp(0, 1, ratio), 0.1f).SetCurve(easeOutCurve);
        canvasGroup.alpha = 1;
        expandAnimation.Open();
    }

    public override void Close()
    {
        IsOpen = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}
