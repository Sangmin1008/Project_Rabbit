using UnityEngine;

public class MovementHandler
{
    private readonly Rigidbody2D _rb;
    private readonly IUnitController _unit;

    public MovementHandler(Rigidbody2D rb, IUnitController unit)
    {
        _rb = rb;
        _unit = unit;
    }

    public void Chase()
    {
        if (_unit.Target == null)
        {
            return;
        }

        Vector2 targetPos = _unit.Target.Collider.bounds.center;
        Vector2 myPos = _unit.Collider.bounds.center;

        float speed = (_unit is BossController boss) ? boss.StatHandler.MoveSpeed : _unit.StatManager.GetValueSafe(StatType.MoveSpeed, 0f);

        float direction = Mathf.Sign(targetPos.x - myPos.x);

        _rb.linearVelocity = new Vector2(direction * speed, _rb.linearVelocity.y);
    }

    public void ChaseWithMinDistance(float minDistance)
    {
        if (_unit.Target == null)
        {
            return;
        }

        float distance = Vector2.Distance(_unit.Target.Collider.bounds.center, _unit.Collider.bounds.center);
        float speed = _unit.StatManager.GetValue(StatType.MoveSpeed);

        Vector2 dir;

        if (distance < minDistance)
        {
            dir = (_unit.Collider.bounds.center - _unit.Target.Collider.bounds.center).normalized;
        }

        else
        {
            dir = (_unit.Target.Collider.bounds.center - _unit.Collider.bounds.center).normalized;
        }

        _rb.linearVelocity = new Vector2(dir.x * speed, _rb.linearVelocity.y);
    }

    public void StopXMovement()
    {
        _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }
}
