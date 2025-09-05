using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI slotIndexText;
    [SerializeField] private TextMeshProUGUI slotTimeText;
    [SerializeField] private TextMeshProUGUI stageNameText;
    [SerializeField] private Button deleteButton;
    
    [SerializeField] private AudioClip slotClickSfx;
    [SerializeField] private AudioClip deleteButtonClickSfx;


    private SaveData _saveData = null;
    private int _slotIndex = 0;
    private bool _isSaveMode;
    
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Initialize(int slotIndex, bool saveMode)
    {
        _slotIndex = slotIndex;
        slotIndexText.text = (_slotIndex + 1).ToString();
        _isSaveMode = saveMode;
    
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(ResetData);
        RefreshUI();
    }
    
    public void SetSaveData(SaveData data)
    {
        _saveData = data;
        RefreshUI();
    }

    public void SetInteractable(bool enable)
    {
        deleteButton.interactable = enable;
    }
    
    private void SaveData()
    {
        if (_saveData != null)
        {
            // TODO 한 번 다시 확인
        }

        _saveData = SaveManager.Instance.GenerateSaveDataFromGame(_slotIndex);
        SaveManager.Instance.SaveGame(_saveData, _slotIndex);
        RefreshUI();
    }

    private void LoadData()
    {
        if (_saveData == null)
        {
            //Debug.Log("데이터 없는데요");
            //TODO 데이터 없다는 창 띄우기
            return;
        }

        UIManager.Instance.Close<SaveLoadMenuUI>();
        UIManager.Instance.Close<MainMenuUI>();
        UIManager.Instance.CloseAllOpenUI();
        SaveManager.Instance.ApplySaveDataToGame(_saveData);
    }

    private void ResetData()
    {
        SceneAudioManager.Instance.PlaySfx(deleteButtonClickSfx);
        _saveData = null;
        SaveManager.Instance.DeleteSaveData(_slotIndex);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_saveData != null)
        {
            slotTimeText.text = _saveData.SavedTime.ToString("yyyy.MM.dd HH:mm");
            stageNameText.text = _saveData.StageName;
            if (deleteButton != null)
                deleteButton.gameObject.SetActive(true);
            slotIndexText.color = Color.white;
        }
        else
        {
            slotTimeText.text = _isSaveMode ? "New Game Save" : "Empty Slot";
            stageNameText.text = "";
            if (deleteButton != null)
                deleteButton.gameObject.SetActive(false);
            slotIndexText.color = Color.gray;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SceneAudioManager.Instance.PlaySfx(slotClickSfx);
        if (_isSaveMode) SaveData();
        else LoadData();
    }

    public void Open()
    {
        _canvasGroup.alpha = 1f;
    }

    public void Close()
    {
        _canvasGroup.alpha = 0f;
    }
}
