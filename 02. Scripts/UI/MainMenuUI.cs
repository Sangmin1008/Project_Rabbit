using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : UIBase
{
    [Header("Buttons")]
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

    private ProgressTweener _canvasGroupTweener;
    private UIInputController _uiInputController;
    
    private void Awake()
    {
        base.Awake();
        _canvasGroupTweener = new(this);
    }
    
    private void OnEnable()
    {
        _uiInputController = UIManager.Instance.UIInputController;
        loadButton.onClick.AddListener(OnLoadClicked);
        settingButton.onClick.AddListener(OnSettingClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        var action = _uiInputController.UIActions;
        action.Cancel.started += OnCancel;
    }

    private void OnDisable()
    {
        loadButton.onClick.RemoveListener(OnLoadClicked);
        settingButton.onClick.RemoveListener(OnSettingClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
        var action = _uiInputController.UIActions;
        action.Cancel.started -= OnCancel;
    }
    
    
    private void OnCancel(InputAction.CallbackContext _)
    {
        if (IsOpen && IsTop)
        {
            UIManager.Instance.Close<MainMenuUI>();
            UIManager.Instance.RestartGame();
        }
        else if (!IsOpen && UIManager.Instance.IsStackEmpty)
        {
            if (UIManager.Instance.GameIsPause) return;
            UIManager.Instance.Open<MainMenuUI>();
            UIManager.Instance.PauseGame();
        }
    }
    
    private void OnLoadClicked()
    {
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);

        UIManager.Instance.Close<MainMenuUI>();
        UIManager.Instance.Open<SaveLoadMenuUI>(false);
    }

    private void OnSettingClicked()
    {
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);

        UIManager.Instance.Close<MainMenuUI>();
        UIManager.Instance.Open<SettingUI>();
    }

    private void OnExitClicked()
    {
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);

        SceneLoadManager.Instance.LoadTitleScene();
    }
    
    public override void Open()
    {
        IsOpen = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1;
        expandAnimation.Open();
        // _canvasGroupTweener.Play((ratio) => canvasGroup.alpha = Mathf.Lerp(0, 1, ratio), 0.1f).SetCurve(easeOutCurve);
    }

    public override void Close()
    {
        IsOpen = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0;
    }
}
