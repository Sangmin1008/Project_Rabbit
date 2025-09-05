using System;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : SceneOnlySingleton<BackgroundManager>
{
    [SerializeField] private int backgroundID = 0;

    private BackgroundTable _backgroundTable;
    private BackgroundSO _backgroundData;

    public Transform BottomTarget;
    public float GroundOffset;

    public List<List<GameObject>> Layers { get; private set; }
    public List<float> Weights { get; private set; }

    public Action OnStartCompleted;

    private void Start()
    {
        Initialize();
        OnStartCompleted?.Invoke();
    }

    private void Initialize()
    {
        _backgroundTable = TableManager.Instance.GetTable<BackgroundTable>();
        _backgroundData = _backgroundTable.GetDataByID(backgroundID);

        if (_backgroundData == null)
        {
            Debug.LogError("백그라운드 없음");
            return;
        }

        Layers = new List<List<GameObject>>();
        Weights = new List<float>();

        foreach (var layer in _backgroundData.layers)
        {
            Layers.Add(layer.objects);
            Weights.Add(layer.weight);
        }
    }
}