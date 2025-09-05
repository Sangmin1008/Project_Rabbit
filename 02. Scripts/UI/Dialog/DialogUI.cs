using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{
    [SerializeField] private Speaker[] speakers;
    [SerializeField] private ExpandOpenAnimation expandAnimation;
    
    [Header("Choice UI")]
    [SerializeField] private CanvasGroup choiceCanvasGroup;
    [SerializeField] private List<Button> choiceButtons =  new List<Button>();
    [SerializeField] private List<TextMeshProUGUI> choiceTexts =  new List<TextMeshProUGUI>();
    [SerializeField] private ExpandOpenAnimation choiceUIExpandOpenAnimation;
    
    [Header("Audio")]
    public List<AudioClip> TypingSfxList;

    private int _currentDialogIndex = -1;

    [Serializable]
    public struct Speaker
    {
        public TextMeshProUGUI textName;
        public TextMeshProUGUI textDialog;
        public GameObject objectArrow;
        public CanvasGroup canvasGroup;
    }

    
    public void Initialize()
    {
        foreach (var speaker in speakers)
        {
            if (speaker.canvasGroup != null)
            {
                speaker.canvasGroup.alpha = 0f;
                speaker.canvasGroup.interactable = false;
                speaker.canvasGroup.blocksRaycasts = false;
            }

            if (speaker.objectArrow != null)
            {
                speaker.objectArrow.SetActive(false);
            }
        }
        _currentDialogIndex = -1;
        
        // HideChoices();
    }

    public void ShowDialog()
    {
        speakers[0].canvasGroup.alpha = 1f;
        speakers[0].canvasGroup.interactable = true;
        speakers[0].canvasGroup.blocksRaycasts = true;
        _currentDialogIndex = 0;
    }
    
    public void UpdateName(string name)
    {
        if (_currentDialogIndex != -1)
        {
            speakers[_currentDialogIndex].textName.text = name;
        }
    }
    
    public void UpdateDialog(string dialog)
    {
        if (_currentDialogIndex != -1)
        {
            speakers[_currentDialogIndex].textDialog.text = dialog;
        }
    }
    
    public void SetArrowActive(bool isActive)
    {
        if (_currentDialogIndex != -1)
        {
            speakers[_currentDialogIndex].objectArrow.SetActive(isActive);
        }
    }

    public void Open()
    {
        UIManager.Instance.PauseGame();
        ShowDialog();
        expandAnimation.Open();
    }

    public void Close()
    {
        expandAnimation.Close(() =>
        {
            foreach (var speaker in speakers)
            {
                if (speaker.canvasGroup != null)
                {
                    speaker.canvasGroup.alpha = 0f;
                    speaker.canvasGroup.interactable = false;
                    speaker.canvasGroup.blocksRaycasts = false;
                }

                if (speaker.objectArrow != null)
                {
                    speaker.objectArrow.SetActive(false);
                }
            }
            HideChoices();
        });
        UIManager.Instance.RestartGame();
    }
    
    public void ShowChoices(DialogSystem.ChoiceData[] choices, Action<int> onChoiceSelectedCallback)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            choiceTexts[i].text = choices[i].choiceData;
        }
        
        choiceCanvasGroup.alpha = 1f;
        choiceCanvasGroup.interactable = true;
        choiceCanvasGroup.blocksRaycasts = true;
        
        choiceUIExpandOpenAnimation.Open();

        for (int i = 0; i < choices.Length; i++)
        {
            choiceButtons[i].onClick.RemoveAllListeners();
            int choiceIndex = i;
            choiceButtons[i].onClick.AddListener(() => onChoiceSelectedCallback(choiceIndex));
        }
    }

    public void HideChoices()
    {
        choiceUIExpandOpenAnimation.Close(() =>
        {
            choiceCanvasGroup.alpha = 0f;
            choiceCanvasGroup.interactable = false;
            choiceCanvasGroup.blocksRaycasts = false;
        });
    }
}