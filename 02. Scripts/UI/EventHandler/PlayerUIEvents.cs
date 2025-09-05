using UnityEngine;
using UnityEngine.Events;

public static class PlayerUIEvents
{
    public static UnityAction OnPlayerStatUIUpdate;
    public static UnityAction<int> OnParryingStackUIUpdate;
    public static UnityAction OnStrongAttack;
}
