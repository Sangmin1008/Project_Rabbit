using UnityEngine;

public class SummonedEnemy : MonoBehaviour, ISummonedEnemy
{
    [SerializeField] private GameObject _owner;
    public GameObject Owner => _owner;

    public void Initialize(GameObject owner)
    {
        _owner = owner;
    }
}
