using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveData
{
    [Header("Save Data Info")]
    public int SlotIndex;
    public string SavedTimeString;
    public DateTime SavedTime
    {
        get => DateTime.TryParse(SavedTimeString, out var dt) ? dt : DateTime.MinValue;
        set => SavedTimeString = value.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    [Header("Stage Data Info")]
    public string StageName;

    [Header("Player Data Info")]
    public float PlayerCurrHealth;
    public float PlayerMaxHealth;
    public float PlayerCurrStamina;
    public float PlayerMaxStamina;
    public int ParryStack;
    public Vector2 PlayerPosition;
}

public class SaveManager : Singleton<SaveManager>
{
    private GameObject _player;
    
    protected override void Awake()
    {
        base.Awake();
        FindPlayer();
    }

    private void FindPlayer()
    {
        _player = GameObject.Find("Player");
    }

    private string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }

    public void SaveGame(SaveData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slot), json);
        Debug.Log($"게임 저장됨 -> {GetSavePath(slot)}");
    }

    public SaveData LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("저장된 파일 없음");
            return null;
        }
        
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public List<SaveData> LoadAllSaveData(int size)
    {
        List<SaveData> allData = new List<SaveData>();
        for (int i = 0; i < size; i++)
        {
            allData.Add(LoadGame(i));
        }

        return allData;
    }

    public void DeleteSaveData(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"파일 삭제됨 -> {path}");
        }
    }

    public SaveData GenerateSaveDataFromGame(int slot)
    {
        SaveData data = new SaveData();
        
        data.SlotIndex = slot;
        data.SavedTime = DateTime.Now;
        
        data.StageName = SceneManager.GetActiveScene().name;
        
        if (_player == null)
            FindPlayer();
        if (_player != null && _player.TryGetComponent<PlayerController>(out var pc))
        {
            data.PlayerPosition = _player.transform.position;
            data.PlayerCurrHealth = pc.StatManager.GetValue(StatType.CurHp);
            data.PlayerMaxHealth = pc.StatManager.GetValue(StatType.MaxHp);
            data.PlayerCurrStamina = pc.StatManager.GetValue(StatType.CurStamina);
            data.PlayerMaxStamina = pc.StatManager.GetValue(StatType.MaxStamina);
            data.ParryStack = pc.PlayerActionHandler.ParryingStackCount;
        }
        return data;
    }

    public void ApplySaveDataToGame(SaveData data)
    {
        UnityAction<Scene, LoadSceneMode> onLoaded = null;
        onLoaded = (scene, mode) =>
        {
            ApplyPlayerData(data);
            FindPlayer();
            SceneManager.sceneLoaded -= onLoaded;
        };
        SceneManager.sceneLoaded += onLoaded;
        SceneLoadManager.Instance.LoadMainScene();
    }

    private void ApplyPlayerData(SaveData data)
    {
        if (_player == null)
            FindPlayer();
        if (_player != null && _player.TryGetComponent<PlayerController>(out var pc))
        {
            _player.transform.position = data.PlayerPosition;
            pc.StatManager.SetValue(StatType.CurHp, data.PlayerCurrHealth);
            pc.StatManager.SetValue(StatType.MaxHp, data.PlayerMaxHealth);
            pc.StatManager.SetValue(StatType.CurStamina, data.PlayerCurrStamina);
            pc.StatManager.SetValue(StatType.MaxStamina, data.PlayerMaxStamina);
            pc.PlayerActionHandler.ParryingStackCount = data.ParryStack;
            pc.PlayerActionHandler.SetParryingStack();
        }

        if (_player != null)
        {
            
        }
    }
}
