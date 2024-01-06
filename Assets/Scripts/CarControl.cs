using AI.API;
using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class CarControl : MonoBehaviour
{
    public float CurrentSpeed { get; private set; }

    public float NitroReloadPercent => Mathf.Clamp01(_nitro ? 1 - (_nitroTimer / _nitroTime) : (_nitroTimer - _nitroTime) / _nitroCooldown);

    public float SteerAngle => EvaluateSteerAngle();

    public bool Occupied => _occupied;

    public float NitroMultipier => _nitroMultipier;

    public float MaxSpeed => _maxSpeed;

    public event Action<Vector2, float> OnCollision;


    [Header("Game design")]
    [SerializeField] private float _nitroTime = 1f;
    [SerializeField] private float _nitroCooldown = 4f;

    [Header("Motion")]
    [SerializeField] private float _nitroMultipier = 1.5f;
    [SerializeField] private float _maxSpeed = 40;
    [SerializeField] private float _acceleration = 0.5f;
    [SerializeField] private float _brakeForce = 1;
    [SerializeField] private float _friction = 0.2f;

    [Header("Rotation")]
    [SerializeField] private float _steerAngle = 40;
    [SerializeField] private float _wheelBase = 2;
    [Header("Collide")]
    [SerializeField] private float _minimalReleaseForce = 30;
    [SerializeField] private float _collisionTimeInterval = 2f;

    public const float velocityThreshold = 1f;
    private const float axisThreshold = 0.01f;
    private float _forwardSpeed = 0;
    private float _speedPedal = 0;
    private float _brakePedal = 0;
    private float _rotationControl = 0;

    private float _collisionTimer = 0;

    private bool _nitro = false;
    private float _nitroTimer = 0;

    private bool _braking = false;
    private bool _occupied = false;
    private bool _collided = false;

    private Rigidbody2D _body;
    private Vector2 _velocity = Vector2.zero;


    private void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _nitroTimer = _nitroTime + _nitroCooldown;
    }

    private void FixedUpdate()
    {
        float currentMaxSpeed = _maxSpeed;
        float accelerationMultiplier = 1;


        if (!_occupied)
        {
            _speedPedal = 0;
            _rotationControl = 0;
            _brakePedal = 0;
        }


        if (_nitro)
        {
            if (_nitroTimer > _nitroTime)
                _nitro = false;

            _nitroTimer += Time.fixedDeltaTime;

            currentMaxSpeed *= _nitroMultipier;
            accelerationMultiplier = _nitroMultipier;
        }
        else
        {
            _nitroTimer += Time.fixedDeltaTime;
        }

        float scaledMaxSpeed = currentMaxSpeed * Time.fixedDeltaTime;

        float acceleration = _speedPedal * _acceleration * accelerationMultiplier;
        float brake = -Mathf.Clamp(_forwardSpeed / scaledMaxSpeed * 10, -1, 1) * _brakePedal * _brakeForce;
        float friction = -_forwardSpeed / scaledMaxSpeed * _friction;

        _forwardSpeed += (acceleration + brake + friction) * Time.fixedDeltaTime;
        _forwardSpeed = Mathf.Clamp(_forwardSpeed, -scaledMaxSpeed, scaledMaxSpeed);

        RaycastHit2D info = Physics2D.BoxCast(transform.position, transform.localScale, _body.rotation, transform.up, 1f);

        Debug.DrawLine(transform.position, transform.position + transform.up * 1f, Color.blue);
        if (info && !_collided && _occupied)
        {
            _collided = true;
            _collisionTimer = 0;

            Debug.DrawLine(transform.position, info.point);

            var gameObject = info.transform.gameObject;

            if (gameObject.CompareTag(AiApi.playerTag))
            {
                var mc = gameObject.GetComponent<MovementControl>();
                mc.RecieveCollision(transform.up, CurrentSpeed);
            }
            else if (gameObject.CompareTag(AiApi.carTag))
            {
                var mc = gameObject.GetComponent<CarControl>();
                mc.RecieveCollision(transform.up, CurrentSpeed);
            }
            _forwardSpeed *= 0.2f;
        }
        else
        {
            if (_collisionTimer > _collisionTimeInterval)
            {
                _collided = false;
            }

            _collisionTimer += Time.fixedDeltaTime;
        }

        _velocity *= 0.99f;
        if (_velocity.sqrMagnitude < 2)
            _velocity = Vector2.zero;

        RotateAndMoveCar(_body.rotation);

        CurrentSpeed = _forwardSpeed / Time.fixedDeltaTime;
    }

    private float EvaluateSteerAngle() => Mathf.Clamp01(1 - Mathf.Abs(Mathf.Pow(_forwardSpeed / Time.fixedDeltaTime / (_maxSpeed + _maxSpeed / 10), 2)));

    private void RotateAndMoveCar(float rotation)
    {
        rotation = Mathf.Deg2Rad * (rotation + 90);

        Vector2 frontWheel = _body.position + _wheelBase / 2 * new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
        Vector2 backWheel = _body.position - _wheelBase / 2 * new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));

        var angle = Mathf.Deg2Rad * -_rotationControl * (_steerAngle * EvaluateSteerAngle());

        backWheel += _forwardSpeed * new Vector2(Mathf.Cos(rotation), Mathf.Sin(rotation));
        frontWheel += _forwardSpeed * new Vector2(Mathf.Cos(rotation + angle), Mathf.Sin(rotation + angle));

        var newRotation = Mathf.Atan2(frontWheel.y - backWheel.y, frontWheel.x - backWheel.x);

        _body.rotation = Mathf.Rad2Deg * newRotation - 90;

        _body.MovePosition((frontWheel + backWheel) / 2 + _velocity * Time.fixedDeltaTime);
    }

    public void RecieveCollision(Vector2 direction, float force)
    {
        if (force < _minimalReleaseForce)
            return;

        if (Vector2.Dot(transform.up, direction) > 0.5f)
        {
            _forwardSpeed *= 0.2f;
            return;
        }

        _velocity = direction * force;

        OnCollision?.Invoke(direction, force);
    }

    public void EnterCar() => _occupied = true;

    public void ExitCar() => _occupied = false;

    public void Accelerate(Vector2 axis)
    {
        if (_forwardSpeed > (velocityThreshold * Time.fixedDeltaTime))
        {
            _rotationControl = axis.x;

            if (axis.y > axisThreshold)
            {
                _speedPedal = axis.y;
                _brakePedal = 0;
                _braking = false;
            }
            else if (Mathf.Abs(axis.y) <= axisThreshold)
            {
                _speedPedal = 0;
                _brakePedal = 0;
                _braking = false;
            }
            else
            {
                _speedPedal = 0;
                _brakePedal = -axis.y;
                _braking = true;
            }
        }
        else
        {
            _rotationControl = -axis.x;

            if (axis.y > axisThreshold)
            {
                _speedPedal = axis.y;
                _brakePedal = 0;
                _braking = false;
            }
            else if (Mathf.Abs(axis.y) <= axisThreshold)
            {
                _speedPedal = 0;
                _brakePedal = 0;
                _braking = false;
            }
            else if (!_braking)
            {
                _speedPedal = axis.y;
                _brakePedal = 0;
                _braking = false;
            }
            else
            {
                _brakePedal = -axis.y;
            }
        }
    }

    public void EnableNitro()
    {
        if (_nitro)
            return;

        if (_nitroTimer > (_nitroCooldown + _nitroTime))
        {
            _nitroTimer = 0;
            _nitro = true;
        }

    }
}
