using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider staminaSlider;
    
    [SerializeField] private StatManager playerStatManager;

    private void OnEnable()
    {
        PlayerUIEvents.OnPlayerStatUIUpdate += UpdateStatUI;
    }

    private void OnDisable()
    {
        PlayerUIEvents.OnPlayerStatUIUpdate -= UpdateStatUI;
    }

    private void UpdateStatUI()
    {
        float curHp = playerStatManager.GetValue(StatType.CurHp);
        float maxHp = playerStatManager.GetValue(StatType.MaxHp);
        float curStamina = playerStatManager.GetValue(StatType.CurStamina);
        float maxStamina = playerStatManager.GetValue(StatType.MaxStamina);
        
        hpSlider.value = curHp / maxHp;
        staminaSlider.value = curStamina / maxStamina;
    }
}
