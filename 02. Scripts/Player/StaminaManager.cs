using System;
using System.Collections;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    [SerializeField] private float staminaRecoveryAmount = 1f;
    [SerializeField] private float dashStaminaCost = 20f;
    [SerializeField] private float defenseStaminaCost = 20f;

    private PlayerController _playerController;
    private float _lastActionTime = Mathf.NegativeInfinity;

    public float DefenseStaminaCost => defenseStaminaCost;
    public float DashStaminaCost => dashStaminaCost;
    
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public void RecoverStamina()
    {
        var x = _playerController.StatManager.GetValue(StatType.MaxStamina);
        if (_playerController.StatManager.GetValue(StatType.MaxStamina) <= _playerController.StatManager.GetValue(StatType.CurStamina)) return;
        
        _playerController.StatManager.Recover(StatType.CurStamina, StatModifierType.Base, staminaRecoveryAmount * (_playerController.IsDefensing ? 0.33f : 1f));
        PlayerUIEvents.OnPlayerStatUIUpdate.Invoke();
    }

    public IEnumerator RecoverStaminaCoroutine()
    {
        while (true)
        {
            if (!_playerController.IsDashing && Time.time - _lastActionTime >= 2f) RecoverStamina();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ConsumeStamina(float amount)
    {
        if (amount > _playerController.StatManager.GetValue(StatType.CurStamina)) return;
        
        _playerController.StatManager.Consume(StatType.CurStamina, StatModifierType.Base, amount);
        PlayerUIEvents.OnPlayerStatUIUpdate.Invoke();

        _lastActionTime = Time.time;
    }
}
