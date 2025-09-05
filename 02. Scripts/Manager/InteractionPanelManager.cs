using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPanelManager : SceneOnlySingleton<InteractionPanelManager>
{
    private Queue<ExpandOpenAnimation> _panelPool = new Queue<ExpandOpenAnimation>();
    private Transform _panelParent;

    protected override void Awake()
    {
        base.Awake();
        _panelParent = GameObject.Find("Canvas - Interaction Panel")?.transform;

        if (_panelParent == null)
        {
            Debug.LogError("Interaction Panel 캔버스 없음");
            return;
        }

        foreach (Transform child in _panelParent)
        {
            _panelPool.Enqueue(child.gameObject.GetComponent<ExpandOpenAnimation>());
        }
    }

    public ExpandOpenAnimation OpenPanel()
    {
        if (_panelPool.Count == 0)
        {
            Debug.LogWarning("풀 수 늘리셈");
            return null;
        }
        
        var panel = _panelPool.Dequeue();
        return panel;
    }

    public void ClosePanel(ExpandOpenAnimation panel)
    {
        _panelPool.Enqueue(panel);
    }
}
