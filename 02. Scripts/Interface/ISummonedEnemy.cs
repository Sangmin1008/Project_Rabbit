using UnityEngine;

public interface ISummonedEnemy
{
    GameObject Owner { get; }

    void Initialize(GameObject owner);
}
