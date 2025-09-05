using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadMenuUI : UIBase
{
   [SerializeField] private CanvasGroup canvasGroup;
   [SerializeField] private VerticalSnapScrollView selectScrollView;
   
   [SerializeField] private RectTransform slotsParent;
   
   [SerializeField] private AnimationCurve easeOutCurve;
   [SerializeField] private AnimationCurve easeInCurve;
   [SerializeField] private ExpandOpenAnimation expandAnimation;
   
   private SaveSlotUI[] _slots;
   private ProgressTweener _canvasGroupTweener;
   private UIInputController _uiInputController;
   private bool _isSaveMode = false;
   

   private void Awake()
   {
       base.Awake();
      _canvasGroupTweener = new(this);
      _slots = new SaveSlotUI[slotsParent.childCount];

      for (int i = 0; i < slotsParent.childCount; i++)
      {
          _slots[i] = slotsParent.GetChild(i).GetComponent<SaveSlotUI>();
      }
   }

   private void Start()
   { 
       _uiInputController = UIManager.Instance.UIInputController;
       var saveDatas = SaveManager.Instance.LoadAllSaveData(slotsParent.childCount);
    
       for (int i = 0; i < slotsParent.childCount; i++)
       {
           _slots[i].SetSaveData(saveDatas[i]);
       }
       
       selectScrollView.SnapIndex = 0;
       BindInput();
   }

   private void BindInput()
   {
       var action = _uiInputController.UIActions;
       action.Cancel.started += _ =>
       {
           if (IsOpen && IsTop)
               UIManager.Instance.Close<SaveLoadMenuUI>();
       };
   }

   private void OpenSaveMenu()
   {
       for (int i = 0; i < _slots.Length; i++)
       {
           _slots[i].Initialize(i, true);
       }
   }
   
   private void OpenLoadMenu()
   {
       for (int i = 0; i < _slots.Length; i++)
       {
           _slots[i].Initialize(i, false);
       }
   }


   public override void Open(bool saveMode)
   {
       IsOpen = true;
       _isSaveMode = saveMode;
       if (saveMode)
       {
           if (UIManager.Instance.GameIsPause)
           {
               UIManager.Instance.Close();
               return;
           }
           OpenSaveMenu();
           UIManager.Instance.PauseGame();
       }
       else
           OpenLoadMenu();
       canvasGroup.blocksRaycasts = true;
       canvasGroup.interactable = true;
       canvasGroup.alpha = 1;
       expandAnimation.Open(() =>
       {
           selectScrollView.DirectUpdateItemList(0);
           for (int i = 0; i < _slots.Length; i++)
           {
               _slots[i].Open();
           }
       });
       // _canvasGroupTweener.Play((ratio) => canvasGroup.alpha = Mathf.Lerp(0, 1, ratio), 0.1f).SetCurve(easeOutCurve);
   }

   public override void Close()
   {
       if (_isSaveMode)
       {
           UIManager.Instance.RestartGame();
       }
       IsOpen = false;
       canvasGroup.blocksRaycasts = false;
       canvasGroup.interactable = false;
       canvasGroup.alpha = 0;
       if (!_isSaveMode)
       {
           UIManager.Instance.Open<TitleMenuUI>();
           UIManager.Instance.Open<MainMenuUI>();
       }
       for (int i = 0; i < _slots.Length; i++)
       {
           _slots[i].Close();
       }
   }
}
