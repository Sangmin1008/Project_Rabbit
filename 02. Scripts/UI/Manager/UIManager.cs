using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    private readonly Dictionary<Type, UIBase> _uiDict = new();
    private Stack<UIBase> _openedUIStack = new Stack<UIBase>();
    private UIInputController _uiInputController;
    
    public UIInputController UIInputController => _uiInputController;
    public UIBase TopUI => _openedUIStack.Count == 0 ? null : _openedUIStack.Peek() as UIBase;
    public bool IsStackEmpty => _openedUIStack.Count == 0;
    public bool GameIsPause;

    protected override void Awake()
    {
        base.Awake();
        _uiInputController = GetComponent<UIInputController>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeUIRoot();
    }

    private void InitializeUIRoot()
    {
        _uiDict.Clear();
        _openedUIStack.Clear();

        Transform uiRoot = GameObject.Find("UIRoot")?.transform;
        if (uiRoot == null)
        {
            Debug.LogWarning("[UIManager] UIRoot를 찾을 수 없습니다.");
            return;
        }

        UIBase[] uiComponents = uiRoot.GetComponentsInChildren<UIBase>(true);
        foreach (UIBase uiComponent in uiComponents)
        {
            _uiDict[uiComponent.GetType()] = uiComponent;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Open<T>(bool flag) where T : UIBase
    {
        if (_uiDict.TryGetValue(typeof(T), out UIBase ui) && !_openedUIStack.Contains(ui))
        {
            _openedUIStack.Push(ui);
            ui.Open(flag);
        }
    }
    
    public void Open<T>() where T : UIBase
    {
        if (_uiDict.TryGetValue(typeof(T), out UIBase ui) && !_openedUIStack.Contains(ui))
        {
            _openedUIStack.Push(ui);
            ui.Open();
        }
    }

    public void Close<T>() where T : UIBase
    {
        if (_uiDict.TryGetValue(typeof(T), out UIBase ui))
        {
            while (_openedUIStack.Contains(ui))
            {
                _openedUIStack.Pop();
            }
            ui.Close();
        }
        else
        {
            Debug.LogWarning("해당 UI를 닫을 수 없음");
        }
    }

    public void Close()
    {
        if (_openedUIStack.Count > 0) _openedUIStack.Pop();
    }

    public T GetUIComponent<T>() where T : UIBase
    {
        return _uiDict.TryGetValue(typeof(T), out var ui) ? ui as T : null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestartGame();
        _openedUIStack.Clear();
        InitializeUIRoot();
    }
    
    public void CloseAllOpenUI()
    {
        while (_openedUIStack.Count > 0)
        {
            _openedUIStack.Pop().Close();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameIsPause = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        GameIsPause = false;
    }
}

public class UIBase : MonoBehaviour
{
    public bool IsOpen;
    public bool IsTop
    {
        get
        {
            if (UIManager.Instance.TopUI != null)
            {
                return UIManager.Instance.TopUI == this;
            }

            return false;
        }
    }
    public Action OnClose;
    
    protected virtual void Awake()
    {
        IsOpen = false;
        OnClose = null;
    }
    
    public virtual void Open(bool flag)
    {
        
    }
    
    public virtual void Open()
    {
        
    }

    public virtual void Close()
    {
        
    }
}