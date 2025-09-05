using System.Collections;
using UnityEngine;

public class PlayerHitHandler : MonoBehaviour
{
    [Header("피격 효과음"), SerializeField] private AudioClip hitClip;
    private PlayerController _playerController;
    private int _playerLayerIndex;
    private int _enemyLayerIndex;
    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerLayerIndex = LayerMask.NameToLayer("Player");
        _enemyLayerIndex  = LayerMask.NameToLayer("Enemy");
    }

    public PlayerState ResolveHitState(AttackType attackType)
    {
        return attackType switch
        {
            AttackType.Light => PlayerState.NormalHit,
            AttackType.Heavy => PlayerState.StrongHit,
            AttackType.Knockback => PlayerState.NormalKnockback,
            AttackType.Launch => PlayerState.StrongKnockback,
            _ => PlayerState.NormalHit,
        };
    }

    public IEnumerator InvincibilityCoroutine(float timer)
    {
        _playerController.IsInvincible = true;
        
        float currentTime = 0f;
        float interval = 0.05f;

        SpriteRenderer spriteRenderer = _playerController.SpriteRenderer;
        Color originalColor = spriteRenderer.color;
        Color transparentColor = spriteRenderer.color;
        transparentColor.r = Mathf.Lerp(transparentColor.r, 1f, 0.3f);
        transparentColor.g = Mathf.Lerp(transparentColor.g, 1f, 0.3f);
        transparentColor.b = Mathf.Lerp(transparentColor.b, 1f, 0.3f);
        transparentColor.a = 0.3f;
        bool isTransparent = false;
        
        while (currentTime < timer)
        {
            if (isTransparent)
            {
                spriteRenderer.color = originalColor;
            }
            else
            {
                spriteRenderer.color = transparentColor;
            }
            isTransparent = !isTransparent;
            
            yield return new WaitForSeconds(interval);
            currentTime += interval;
        }
        
        spriteRenderer.color = originalColor;
        _playerController.IsInvincible = false;
    }
    
    public IEnumerator HitPauseCoroutine(float pauseTime)
    {
        var originalColor = _playerController.SpriteRenderer.color;
        _playerController.SpriteRenderer.color = Color.red;
        
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(pauseTime);
        Time.timeScale = 1f;
        
        _playerController.SpriteRenderer.color = originalColor;
    }

    public void TakeDamage(IAttackable attacker)
    {
        if (_playerController.IsInvincible) return;
        if (_playerController.IsParryingOrDodging)
        {
            _playerController.IsParryOrDodgeSuccessful = true;
            return;
        }
        
        var attackerMonoBehaviour = attacker as MonoBehaviour;
        if (attackerMonoBehaviour != null)
        {
            bool isEnemyOnRight = attackerMonoBehaviour.transform.position.x > _playerController.transform.position.x;
            if (isEnemyOnRight == _playerController.IsFlipX)
                _playerController.PlayerMovement.ForceRotate();
        }
        
        _playerController.StatManager.Consume(StatType.CurHp, StatModifierType.Base, attacker.AttackStat.Value * (_playerController.IsDefensing ? 0.33f : 1f));
        if (_playerController.IsDefensing)
        {
            _playerController.StatManager.Consume(StatType.CurStamina, StatModifierType.Base, _playerController.StaminaManager.DefenseStaminaCost);
        }
        else
        {
            Physics2D.IgnoreLayerCollision(_playerLayerIndex, _enemyLayerIndex, true);
            StartCoroutine(WaitUntilSeparatedAndRestore());
            _playerController.ReceivedAttackType = attacker.AttackType;
            _playerController.TookDamage = true;
            // Palyer 피격 시 효과음 재생
            SceneAudioManager.Instance.PlaySfx(hitClip);
        }
        PlayerUIEvents.OnPlayerStatUIUpdate.Invoke();
    }
    
    private IEnumerator WaitUntilSeparatedAndRestore()
    {
        int enemyMask = 1 << _enemyLayerIndex;
        while (Physics2D.OverlapCircle(_playerController.transform.position, 0.1f, enemyMask))
        {
            yield return null;
        }

        Physics2D.IgnoreLayerCollision(_playerLayerIndex, _enemyLayerIndex, false);
    }
}
