using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerParryingStackUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> parryingStack;

    private void OnEnable()
    {
        PlayerUIEvents.OnParryingStackUIUpdate += UpdateStack;
        PlayerUIEvents.OnStrongAttack += ConsumeParryingStack;
    }

    private void OnDisable()
    {
        PlayerUIEvents.OnParryingStackUIUpdate -= UpdateStack;
        PlayerUIEvents.OnStrongAttack -= ConsumeParryingStack;
    }
    
    private void UpdateStack(int stack)
    {
        for (int i = 0; i < stack; i++)
            parryingStack[i].SetActive(true);
    }

    private void ConsumeParryingStack()
    {
        StartCoroutine(ConsumeParryingStackRoutine());
    }

    private IEnumerator ConsumeParryingStackRoutine()
    {
        for (int i = parryingStack.Count - 1; i >= 0; i--)
        {
            parryingStack[i].SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
