using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingUI : UIBase
{
    [Header("BGM Settings")]
    [SerializeField] private Button bgmUpButton;
    [SerializeField] private Button bgmDownButton;
    [SerializeField] private TextMeshProUGUI bgmValueText;

    [Header("Sound Settings")]
    [SerializeField] private Button sfxUpButton;
    [SerializeField] private Button sfxDownButton;
    [SerializeField] private TextMeshProUGUI sfxValueText;

    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private AnimationCurve easeOutCurve;
    [SerializeField] private AnimationCurve easeInCurve;
    [SerializeField] private ExpandOpenAnimation expandAnimation;
    
    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSfx;
    
    [Space(10)]
    [SerializeField] private float holdStartThreshold = 0.4f;
    [SerializeField] private float holdRepeatDelay = 0.05f;

    private ProgressTweener _canvasGroupTweener;
    private UIInputController _uiInputController;

    private float _bgmVolume;
    private float _sfxVolume;

    private void Awake()
    {
        base.Awake();
        _canvasGroupTweener = new(this);
    }

    private void Start()
    {
        _uiInputController = UIManager.Instance.UIInputController;
        BindInput();
        InitVolumes();
        BindButtonEvents();
    }
    
    private void InitVolumes()
    {
        var audioManager = SceneAudioManager.Instance;
        _bgmVolume = audioManager.GetBgmVolume() * 100f;
        _sfxVolume = audioManager.GetSfxVolume() * 100f;
        UpdateVolumeTexts();
    }

    private void BindButtonEvents()
    {
        bgmUpButton.onClick.AddListener(() => ChangeBgmVolume(+1));
        AddHoldEvents(bgmUpButton, () => ChangeBgmVolume(+1));

        bgmDownButton.onClick.AddListener(() => ChangeBgmVolume(-1));
        AddHoldEvents(bgmDownButton, () => ChangeBgmVolume(-1));

        sfxUpButton.onClick.AddListener(() => ChangeSfxVolume(+1));
        AddHoldEvents(sfxUpButton, () => ChangeSfxVolume(+1));

        sfxDownButton.onClick.AddListener(() => ChangeSfxVolume(-1));
        AddHoldEvents(sfxDownButton, () => ChangeSfxVolume(-1));
    }

    private void ChangeBgmVolume(int delta)
    {
        if (ApproximatelyEqual(_bgmVolume + delta, 101f) || ApproximatelyEqual(_bgmVolume + delta, -1f))
            return;
        _bgmVolume = Mathf.Clamp(_bgmVolume + delta, 0, 100);
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);
        SceneAudioManager.Instance.SetBgmVolume(_bgmVolume / 100f);
        UpdateVolumeTexts();
    }

    private void ChangeSfxVolume(int delta)
    {
        if (ApproximatelyEqual(_sfxVolume + delta, 101f) || ApproximatelyEqual(_sfxVolume + delta, -1f))
            return;
        _sfxVolume = Mathf.Clamp(_sfxVolume + delta, 0, 100);
        SceneAudioManager.Instance.PlaySfx(buttonClickSfx);
        SceneAudioManager.Instance.SetSfxVolume(_sfxVolume / 100f);
        UpdateVolumeTexts();
    }

    private void UpdateVolumeTexts()
    {
        bgmValueText.text = Mathf.RoundToInt(_bgmVolume).ToString();
        sfxValueText.text = Mathf.RoundToInt(_sfxVolume).ToString();
    }

    private void AddHoldEvents(Button button, Action onHoldAction)
    {
        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        Coroutine holdCoroutine = null;

        var downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downEntry.callback.AddListener(_ =>
        {
            holdCoroutine = StartCoroutine(HoldCoroutine(onHoldAction));
        });
        trigger.triggers.Add(downEntry);

        var upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        upEntry.callback.AddListener(_ =>
        {
            if (holdCoroutine != null)
                StopCoroutine(holdCoroutine);
        });
        trigger.triggers.Add(upEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ =>
        {
            if (holdCoroutine != null)
                StopCoroutine(holdCoroutine);
        });
        trigger.triggers.Add(exitEntry);
    }
    
    private IEnumerator HoldCoroutine(Action onHoldAction)
    {
        yield return new WaitForSecondsRealtime(holdStartThreshold);

        while (true)
        {
            onHoldAction?.Invoke();
            yield return new WaitForSecondsRealtime(holdRepeatDelay);
        }
    }

    private void BindInput()
    {
        var action = _uiInputController.UIActions;
        action.Cancel.started += _ =>
        {
            if (IsOpen && IsTop)
                UIManager.Instance.Close<SettingUI>();
        };
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
        UIManager.Instance.Open<TitleMenuUI>();
        UIManager.Instance.Open<MainMenuUI>();
    }
    
    bool ApproximatelyEqual(float a, float b, float epsilon = 0.01f)
    {
        return Mathf.Abs(a - b) < epsilon;
    }
}
