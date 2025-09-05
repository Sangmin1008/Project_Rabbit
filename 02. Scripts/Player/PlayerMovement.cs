using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float gravityMultiplier = 3.0f;
    private PlayerController _playerController;
    private Vector2 _moveInput;
    private bool _jumpTriggered;
    private bool _canDoubleJump;
    private Vector3 _originalLocalScale;
    private Vector3 _reverseLocalScale;

    public Vector2 MoveInput {get => _moveInput; set => _moveInput = value;}
    public bool JumpTriggered { get => _jumpTriggered; set => _jumpTriggered = value; }
    public bool CanDoubleJump { get => _canDoubleJump; set => _canDoubleJump = value; }

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _originalLocalScale = new Vector3(1, 1, 1);
        _reverseLocalScale = new Vector3(-1, 1, 1);
    }

    public void Movement()
    {
        float speed = _playerController.StatManager.GetValue(StatType.MoveSpeed);
        _playerController.Rigidbody2D.linearVelocityX = _moveInput.x * speed;
    }

    public void Rotate()
    {
        const float moveThreshold = 0.01f;
        if (_moveInput.x > moveThreshold)
        {
            _playerController.transform.localScale = _originalLocalScale;
        }
        else if (_moveInput.x < -moveThreshold)
        {
            _playerController.transform.localScale = _reverseLocalScale;
        }
    }

    public void ForceRotate()
    {
        if (_playerController.transform.localScale == _originalLocalScale)
            _playerController.transform.localScale = _reverseLocalScale;
        else
            _playerController.transform.localScale = _originalLocalScale;
    }

    public void Fall()
    {
        if (_playerController.VelocityY < 0)
        {
            _playerController.Rigidbody2D.linearVelocityY += Physics2D.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
        }
        else
        {
            _playerController.Rigidbody2D.linearVelocityY += Physics2D.gravity.y * (gravityMultiplier) * Time.fixedDeltaTime;
        }
    }

    public void Jump()
    {
        _jumpTriggered = false;
        if (_playerController.IsGrounded)
        {
            _playerController.JumpEffect.Play(transform.position, _playerController.IsFlipX);
            _playerController.Rigidbody2D.AddForceY(_playerController.StatManager.GetValue(StatType.JumpForce), ForceMode2D.Impulse);
        } 
        else if (_canDoubleJump)
        {
            _playerController.LandingEffect.Play(_playerController.GroundDetector.GroundCheckBlock.position, _playerController.IsFlipX);
            _playerController.Rigidbody2D.linearVelocityY = 0f;
            _playerController.Rigidbody2D.AddForceY(_playerController.StatManager.GetValue(StatType.JumpForce), ForceMode2D.Impulse);
            _canDoubleJump = false;
        }
        _playerController.GroundDetector.GroundCheckBlockTime = _playerController.GroundDetector.GroundCheckDisableDuration;
    }

    public void StopMove()
    {
        _playerController.Rigidbody2D.linearVelocityX = 0f;
    }
}
