using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogSystem : MonoBehaviour
{
    [Header("Component Reference")]
    [SerializeField] private DialogUI dialogUI;
    [SerializeField] private GameObject player;

    [Header("Dialog Data")]
    [SerializeField] private DialogData[] dialogs;

    [Header("Settings")]
    [SerializeField] private bool isAutoStart = false;
    [SerializeField] private float typingSpeed = 0.05f;

    private int _prevChoiceIndex = -1;
    
    private int _currentDialogIndex = 0;
    private bool _isTypingEffect = false;
    private bool _isDialogueActive = false;
    private bool _onClick = false;
    private bool _isWaitingForChoice = false;

    private Coroutine _typingCoroutine;
    private PlayerInputController _playerInputController;

    private List<AudioClip> typingSfxList;

    [Serializable]
    public struct ChoiceData
    {
        public string choiceData;
        public int nextDialogIndex;
        public bool done;
    }

    [Serializable]
    public struct DialogData
    {
        public int dialogIndex;
        public string name;
        [TextArea(3, 5)] public string dialog;
        public bool done;

        public bool isChoice;
        public ChoiceData[] choices;
        public VoidEventChannelSO voidEvent;
        public IntegerEventChannelSO intEvent;
    }

    private void Awake()
    {
        _playerInputController = player.GetComponent<PlayerInputController>();
        dialogUI.Initialize();
    }

    private void OnEnableClick()
    {
        var action = _playerInputController.PlayerActions;
        action.Attack.started += OnClick;
    }

    private void OnDisableClick()
    {
        var action = _playerInputController.PlayerActions;
        action.Attack.started -= OnClick;
    }

    private void Start()
    {
        typingSfxList = dialogUI.TypingSfxList;
        if (isAutoStart)
        {
            StartDialog();
        }
    }
    
    private void Update()
    {
        if (_onClick)
        {
            //Debug.Log("클릭감지됨!!!!!!!");
            _onClick = false;
            AdvanceDialog();
        }
    }
    
    private void OnClick(InputAction.CallbackContext context)
    {
        //Debug.Log("클릭!!!!!!!");
        _onClick = true;
    }
    
    public void AdvanceDialog()
    {
        if (!UIManager.Instance.IsStackEmpty) return;
        if (!_isDialogueActive)
        {
            StartDialog();
            return;
        }
        
        if (_isWaitingForChoice) return;

        if (_isTypingEffect)
        {
            CompleteTyping();
            return;
        }

        if (_currentDialogIndex < dialogs.Length && !dialogs[_currentDialogIndex].done)
        {
            SetNextDialog();
        }
        else
        {
            if (dialogs[_currentDialogIndex - 1].voidEvent != null) 
            {
                dialogs[_currentDialogIndex - 1].voidEvent.Raise();
            }

            if (dialogs[_currentDialogIndex - 1].intEvent != null && _prevChoiceIndex != -1)
            {
                dialogs[_currentDialogIndex - 1].intEvent.Raise(_prevChoiceIndex);
            }
            EndDialog();
        }
    }

    private void StartDialog()
    {
        OnEnableClick();
        dialogUI.Open();
        _isDialogueActive = true;
        _currentDialogIndex = 0;
        SetNextDialog();
    }
    
    private void EndDialog()
    {
        OnDisableClick();
        _isDialogueActive = false;
        dialogUI.Close();
    }
    
    private void SetNextDialog()
    {
        string speakerName = dialogs[_currentDialogIndex].name;

        dialogUI.ShowDialog();
        dialogUI.UpdateName(speakerName);
        dialogUI.UpdateDialog("");
        
        _typingCoroutine = StartCoroutine(OnTypingText(dialogs[_currentDialogIndex].dialog));
    }

    private void CompleteTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _isTypingEffect = false;

        dialogUI.UpdateDialog(dialogs[_currentDialogIndex].dialog);

        if (dialogs[_currentDialogIndex].isChoice)
        {
            ShowChoices(dialogs[_currentDialogIndex].choices);
        }
        else
        {
            dialogUI.SetArrowActive(true);
            _currentDialogIndex++;
        }
    }

    private IEnumerator OnTypingText(string fullText)
    {
        _isTypingEffect = true;
        dialogUI.SetArrowActive(false);

        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogUI.UpdateDialog(fullText.Substring(0, i));
            if (i > 0 && !char.IsWhiteSpace(fullText[i-1]) && i % 2 == 0)
            {
                SceneAudioManager.Instance.PlaySfx(typingSfxList[i % typingSfxList.Count]);
            }
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        CompleteTyping();
    }

    private void ShowChoices(ChoiceData[] choices)
    {
        _isWaitingForChoice = true;
        OnDisableClick();
        dialogUI.SetArrowActive(false);
        dialogUI.ShowChoices(choices, OnChoiceSelected);
    }
    
    private void OnChoiceSelected(int choiceIndex)
    {
        _isWaitingForChoice = false;
        OnEnableClick();
        dialogUI.HideChoices();

        _prevChoiceIndex = choiceIndex;
        ChoiceData selectedChoice = dialogs[_currentDialogIndex].choices[choiceIndex];

        if (selectedChoice.done)
        {
            //Debug.Log("choice done 활성화됨");

            if (dialogs[_currentDialogIndex].voidEvent != null) 
            {
                dialogs[_currentDialogIndex].voidEvent.Raise();
            }
            if (dialogs[_currentDialogIndex].intEvent != null)
            {
                dialogs[_currentDialogIndex].intEvent.Raise(choiceIndex);
            }
        
            EndDialog();
        }
        else
        {
            int nextDialogIndex = selectedChoice.nextDialogIndex;
            //Debug.Log($"{nextDialogIndex} 넥스트 인덱스");
        
            JumpToDialog(nextDialogIndex);
        }
    }
    
    private void JumpToDialog(int dialogIndex)
    {
        if(dialogIndex < 0 || dialogIndex >= dialogs.Length)
        {
            //Debug.LogError($"잘못된 인덱스임: {dialogIndex}");
            EndDialog();
            return;
        }

        _currentDialogIndex = dialogIndex;
        SetNextDialog();
    }
}