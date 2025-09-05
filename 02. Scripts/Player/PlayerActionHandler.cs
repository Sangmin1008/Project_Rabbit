using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerActionHandler : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private Material dashMaterial;
    [SerializeField] private Gradient dashGradient;
    
    [Header("Strong Attack")]
    [SerializeField] private Material strongAttackMaterial;
    [SerializeField] private Gradient attackGradient;
    
    [Header("Zoom Curve")]
    [SerializeField] private AnimationCurve zoomInCurve;
    private PlayerController _playerController;
    private bool _isDashing;
    private bool _dashTriggered;
    private bool _isDefensing;
    
    private int _playerLayerIndex;
    private int _enemyLayerIndex;
    private int _parryingStack = 0;
    
    
    public bool CanDash { get; set; } = true;
    public bool CanDefense { get; set; } = true;
    public bool IsDashing { get => _isDashing; set => _isDashing = value; }
    public bool DashTriggered { get => _dashTriggered; set => _dashTriggered = value; }
    public bool IsDefensing { get => _isDefensing; set => _isDefensing = value; }
    public bool IsStackFull => _parryingStack >= 3;
    public int ParryingStackCount { get => _parryingStack; set => _parryingStack = value; }


    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _playerLayerIndex = LayerMask.NameToLayer("Player");
        _enemyLayerIndex  = LayerMask.NameToLayer("Enemy");
    }

    private void OnEnable()
    {
        PlayerUIEvents.OnStrongAttack += ResetStack;
    }

    private void OnDisable()
    {
        PlayerUIEvents.OnStrongAttack -= ResetStack;
    }

    public IEnumerator Dash(float duration, bool attackMode, float speed = 1f)
    {
        Physics2D.IgnoreLayerCollision(_playerLayerIndex, _enemyLayerIndex, true);
        _playerController.CanDash = false;
        _isDashing = true;
        
        float originGravity = _playerController.Rigidbody2D.gravityScale;
        _playerController.Rigidbody2D.gravityScale = 0f;
        _playerController.Rigidbody2D.linearVelocity = _playerController.StatManager.GetValue(StatType.DashForce) * speed *
                                                       (_playerController.IsFlipX ? Vector2.left : Vector2.right);
        
        if (!attackMode)
            _playerController.AfterimageController.StartEffect(dashMaterial, dashGradient, _playerController.IsFlipX);
        else
            _playerController.AfterimageController.StartEffect(strongAttackMaterial, attackGradient, _playerController.IsFlipX);

        yield return new WaitForSeconds(duration);
        
        _playerController.Rigidbody2D.gravityScale = originGravity;
        _isDashing = false;
        StartCoroutine(WaitUntilSeparatedAndRestore());

        yield return new WaitForSeconds(0.2f);
        _playerController.AfterimageController.Stop();
        
        if (!attackMode)
            yield return new WaitForSeconds(0.3f);

        _playerController.CanDash = true;
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

    public IEnumerator Defense()
    {
        _playerController.CanDefense = false;
        yield return new WaitForSeconds(0.2f);
        _playerController.CanDefense = true;
    }

    public IEnumerator Parry()
    {
        _playerController.ParryEffect.Play(transform.position, _playerController.IsFlipX);
        AddParryingStack();
        CinemachineCamera camera = CinemachineEffects.Instance.GetCamera();

        CinemachineEffects.Instance.ShakeCamera(0.7f, 0.1f);
        CinemachineEffects.Instance.Zoom(camera.Lens.OrthographicSize,0.9f, transform, 0.05f, 0.24f, false, zoomInCurve);
        yield return new WaitForSeconds(0.04f);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
    }

    public bool TryParrying() => _playerController.IsParryOrDodgeSuccessful;

    private void AddParryingStack()
    {
        _parryingStack = Math.Min(3, _parryingStack + 1);
        PlayerUIEvents.OnParryingStackUIUpdate.Invoke(_parryingStack);
    }

    public void SetParryingStack()
    {
        PlayerUIEvents.OnParryingStackUIUpdate.Invoke(_parryingStack);
    }
    
    private void ResetStack()
    {
        _parryingStack = 0;
    }

    public void Dodge()
    {
        //Debug.Log("Dodge");
    }

    public bool TryDodging() => _playerController.IsParryOrDodgeSuccessful;
}
