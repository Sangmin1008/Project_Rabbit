using System;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private Transform rayTransform;
    
    private readonly List<OutlineInteractable> _detectedTargets = new List<OutlineInteractable>();
    private OutlineInteractable _currentTarget = null;
    private PlayerInputController _playerInputController;
    private bool _interacted = false;

    private void Awake()
    {
        _playerInputController = GetComponent<PlayerInputController>();
    }

    private void Start()
    {
        BindInput();
    }

    private void Update()
    {
        FindBestTarget();
        if (_interacted)
            HandleInteractionInput();
    }

    private void BindInput()
    {
        var action = _playerInputController.PlayerActions;
        action.Interact.started += _ => _interacted = true;
    }

    private void FindBestTarget()
    {
        OutlineInteractable bestTarget = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < _detectedTargets.Count; i++)
        {
            OutlineInteractable target = _detectedTargets[i];

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                _detectedTargets.Remove(target);
                continue;
            }
            
            Vector3 direction = (target.transform.position - rayTransform.position).normalized;
            float distanceToTarget = direction.magnitude;
            
            if (distanceToTarget > interactRange) continue;
            
            RaycastHit2D hit = Physics2D.Raycast(rayTransform.position, direction, distanceToTarget, interactLayer);
            if (hit.collider != null && hit.collider.gameObject == target.gameObject)
            {
                if (distanceToTarget < minDistance)
                {
                    minDistance = distanceToTarget;
                    bestTarget = target;
                }
            }
        }

        if (_currentTarget != bestTarget)
        {
            _currentTarget?.HideOutline();
            _currentTarget?.HidePanel();
            _currentTarget = bestTarget;
            _currentTarget?.ShowOutline();
            _currentTarget?.ShowPanel();
        }
    }
    
    private void HandleInteractionInput()
    {
        _interacted = false;
        if (_currentTarget != null)
        {
            _currentTarget.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<OutlineInteractable>(out var interactable))
        {
            if (!_detectedTargets.Contains(interactable))
            {
                _detectedTargets.Add(interactable);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<OutlineInteractable>(out var interactable))
        {
            _detectedTargets.Remove(interactable);
        }
    }
}
