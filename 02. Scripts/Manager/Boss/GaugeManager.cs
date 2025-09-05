using UnityEngine;

public class GaugeManager
{
    private float _gauge;
    private readonly float _maxGauge;
    private readonly float _gaugePerAttack;

    #region 외부에서 읽을 수 있도록 하는 프로퍼티
    public float CurrentGauge
    {
        get { return _gauge; }
    }

    public float MaxGauge
    {
        get { return _maxGauge; }
    }
    #endregion

    public GaugeManager(float maxGauge, float perAttack)
    {
        _maxGauge = maxGauge;
        _gaugePerAttack = perAttack;
        _gauge = 0f;
    }

    public void Add()
    {
        _gauge = Mathf.Min(_gauge + _gaugePerAttack, _maxGauge);
    }

    public bool IsFull() => _gauge >= _maxGauge;
    public void Reset() => _gauge = 0f;

    public bool TryConsumeIfFull()
    {
        if (_gauge >= _maxGauge)
        {
            _gauge = 0f;
            return true;
        }

        return false;
    }
}
