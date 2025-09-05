using System;
using UnityEngine;

[RequireComponent(typeof(SpriteOutline))]
public abstract class OutlineInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private float offsetY;
    private SpriteOutline _spriteOutline;
    private Vector2 _panelPosition;
    private ExpandOpenAnimation _expandOpenAnimation;

    protected void Awake()
    {
        _spriteOutline = GetComponent<SpriteOutline>();
        if (TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            _panelPosition = new Vector2(0, spriteRenderer.bounds.size.y);
        }
    }

    public void ShowOutline()
    {
        _spriteOutline?.UpdateOutline(true);
    }

    public void HideOutline()
    {
        _spriteOutline?.UpdateOutline(false);
    }

    public void ShowPanel()
    {
        _expandOpenAnimation = InteractionPanelManager.Instance.OpenPanel();
        
        Vector3 worldPosition = transform.position + (Vector3)_panelPosition;
        worldPosition.y += offsetY;
        _expandOpenAnimation.transform.position = worldPosition;

        _expandOpenAnimation?.Open();
    }

    public void HidePanel()
    {
        _expandOpenAnimation?.Close();
        InteractionPanelManager.Instance.ClosePanel(_expandOpenAnimation);
        _expandOpenAnimation = null;
    }

    public abstract string GetInteractPrompt();

    public abstract void Interact();
}
