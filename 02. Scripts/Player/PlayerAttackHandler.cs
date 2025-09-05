using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHandler : MonoBehaviour
{
    [Header("타격 효과음"), SerializeField] private AudioClip AttackClip;
     
    [SerializeField] private float strongAttackCoolTimer = 0.5f;
    private PlayerController _playerController;
    private List<IDamageable> _targets = new List<IDamageable>();
    private IDamageable _target;
    private BoxCollider2D _collider;
    private DamageableSensor _damageableSensor;
    private bool _attackTriggered;
    private bool _isAttacking;
    private float _attackPerformedTimer = 0f;
    
    public bool GroundAttackTriggered { get => _attackTriggered && _playerController.IsGrounded; set => _attackTriggered = value; }
    public bool IsGroundAttacking { get => _isAttacking && _playerController.IsGrounded; set => _isAttacking = value; }
    public bool AirAttackTriggered { get => _attackTriggered && !_playerController.IsGrounded; set => _attackTriggered = value; }
    public bool IsAirAttacking { get => _isAttacking && !_playerController.IsGrounded; set => _isAttacking = value; }
    public bool AttackTriggered { get => _attackTriggered; set => _attackTriggered = value; }
    public bool IsAttacking { get => _isAttacking; set => _isAttacking = value; }
    public bool AttackPerformed = false;

    public bool CanStrongAttack
    {
        get => AttackPerformed && (_attackPerformedTimer > strongAttackCoolTimer) && _playerController.PlayerActionHandler.IsStackFull;
    }
    
    public int ComboIndex = 0;
    public GameObject HitBox;
    public AttackInfoData CurrentAttackInfoData;
    [SerializeField] public List<AttackInfoData> ComboAttackInfoDatas;
    [SerializeField] public AttackInfoData AirAttackInfoData;
    [SerializeField] public AttackInfoData MoveAttackInfoData;
    [SerializeField] public AttackInfoData StrongAttackInfoData;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _collider = HitBox.GetComponent<BoxCollider2D>();
        _damageableSensor = HitBox.GetComponent<DamageableSensor>();
    }

    private void Update()
    {
        if (AttackPerformed)
        {
            _attackPerformedTimer += Time.deltaTime;
        }
        else
        {
            _attackPerformedTimer = 0f;
        }
    }
    
    public IEnumerator AttackCounter()
    {
        yield return new WaitForSeconds(0.3f);
        ComboIndex = 0;
    }

    public void Attack()
    {
        if (_target == null) return;
        if (CurrentAttackInfoData == null)
        {
            Debug.LogError("왜 공격 데이터 없음??");
        }
        
        _target.TakeDamage(_playerController);
        CinemachineEffects.Instance.ShakeCamera(0.6f, 0.08f);
        _target = null;
    }

    public void AttackAllTargets()
    {
        //Debug.Log("Attack All Targets");
        GroundAttackTriggered = false;
        FindTarget();
        AddExtraDamage();
        // _playerController.AttackStat = _playerController.StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
        //Debug.Log(_playerController.AttackStat.Value);
        foreach (var currTarget in _targets)
        {
            _target = currTarget;
            Attack();
            // 공격성공 시 효과음 처리
            SceneAudioManager.Instance.PlaySfx(AttackClip);
        }
        ConsumeExtraDamage();
        // _playerController.AttackStat = _playerController.StatManager.GetStat<CalculatedStat>(StatType.AttackPow);
    }

    private void AddExtraDamage()
    {
        _playerController.StatManager.ApplyStatEffect(StatType.AttackPow, StatModifierType.Base, CurrentAttackInfoData.ExtraDamage);
    }

    private void ConsumeExtraDamage()
    {
        _playerController.StatManager.ApplyStatEffect(StatType.AttackPow, StatModifierType.Base, -CurrentAttackInfoData.ExtraDamage);
    }

    public void FindTarget()
    {
        _targets = _damageableSensor.Damageables;
    }
    
    
    public void SetHitBox(Vector2 center, Vector2 size)
    {
        _collider.offset = center;
        _collider.size = size;
    }
}
