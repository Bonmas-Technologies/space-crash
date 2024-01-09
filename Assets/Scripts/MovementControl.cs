using AI.API;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class MovementControl : MonoBehaviour
{
    public EntityState State { get => _state; }
    public CarControl OccupiedCar { get => _car;  }
    public LayerMask UseMask { get => _useMask; }
    public float UseRadius { get => _useRadius; }

    [Header("Setup")]
    [SerializeField] private GameObject _visuals; // TODO: animation controller!!!
    [SerializeField] private LayerMask _useMask;
    [SerializeField] private float _radius = 0.5f;

    [Header("Game design")]
    [SerializeField] private float _entitySpeed = 4f;
    [SerializeField] private float _useRadius = 1f;
    [SerializeField] private float _dashLength = 2f;
    [SerializeField] private float _dashTime = 0.4f;
    [SerializeField] private float _dashCooldownTime = 0.1f;

    [Header("Collide")]
    [SerializeField] private float _minimalDeadSpeed = 10f;
    [SerializeField] private float _afterCollisionTime = 0.5f;

    private const float _skinWidth = 0.02f;
    private const int _maxBounces = 5;

    private EntityState _state = EntityState.Alive;
    private Rigidbody2D _body;
    private Collider2D _collider;
    private CarControl _car;

    private Vector2 _axis;
    private Vector2 _velocity;
    private bool _dashing;
    private float _dashTimer;
    private float _cooldownTimer;
    private float _collisionTimer;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _body = GetComponent<Rigidbody2D>();
        _cooldownTimer = _dashCooldownTime;
    }

    private void FixedUpdate()
    {
        switch (_state)
        {
            case EntityState.Dead:
                break;
            case EntityState.Alive:
                _collisionTimer += Time.fixedDeltaTime;
                _visuals.SetActive(true);

                _body.isKinematic = false;
                _collider.enabled = true;
                _body.rotation = 0;
                WalkControls();
                break;
            case EntityState.InCar:
                _collisionTimer = 0;
                _visuals.SetActive(false);

                _body.isKinematic = true;
                _collider.enabled = false;
                _body.position = _car.transform.position;
                _body.rotation = _car.transform.rotation.eulerAngles.z;
                _car.Accelerate(_axis);
                break;
            default:
                break;
        }
    }

    private void WalkControls()
    {
        Vector2 velocity;

        if (_dashing)
        {
            if (_dashTimer > _dashTime)
                _dashing = false;

            velocity = _dashLength * EvaluateDash(_dashTimer) * _axis;

            _dashTimer += Time.fixedDeltaTime;
        }
        else
        {
            _cooldownTimer += Time.fixedDeltaTime;

            velocity = _axis * _entitySpeed;
        }
        velocity += _velocity;

        _velocity *= 0.99f;
        if (_velocity.sqrMagnitude < 2)
            _velocity = Vector2.zero;

        velocity = CollideAndSlide(velocity * Time.fixedDeltaTime, transform.position);
        _body.MovePosition(_body.position + velocity);
    }

    private float EvaluateDash(float x) => -2 * Mathf.Pow(_dashTime, -2) * (x - _dashTime);

    public void RecieveCollision(Vector2 direction, float speed)
    {
        if (speed < _minimalDeadSpeed)
            return;

        _velocity = direction * speed;

        if (_state == EntityState.InCar)
        {
            OccupiedCar.OnCollision -= RecieveCollision;

            _state = EntityState.Alive;
            _car.ExitCar();
        }
        else if (_state == EntityState.Alive)
        {
            if (_collisionTimer < _afterCollisionTime)
                return;
            _collisionTimer = 0;

            _state = EntityState.Dead;
        }
    }

    public void MoveAxis(Vector2 axis)
    {
        if (!_dashing)
            _axis = Vector2.ClampMagnitude(axis, 1);
    }

    public void Use()
    {
        switch (_state)
        {
            case EntityState.Dead:
                break;

            case EntityState.Alive:
                Collider2D collider = Physics2D.OverlapCircle(transform.position, _useRadius, _useMask);

                if (collider == null)
                    break;

                if (!collider.CompareTag(AiApi.carTag))
                    break;

                _car = collider.GetComponent<CarControl>();

                if (!_car.Occupied)
                {
                    OccupiedCar.OnCollision += RecieveCollision;
                    _state = EntityState.InCar;
                    _car.EnterCar();
                }
                break;

            case EntityState.InCar:
                OccupiedCar.OnCollision -= RecieveCollision;
                _state = EntityState.Alive;
                _car.ExitCar();
                break;

            default:
                break;
        }
    }
    
    public void UseDash()
    {
        switch (_state)
        {
            case EntityState.Dead:
                break;
            case EntityState.Alive:
                if (_cooldownTimer > _dashCooldownTime)
                {
                    _dashing = true;
                    _axis = _axis.normalized;
                    _cooldownTimer = 0;
                    _dashTimer = 0;
                }
                break;
            case EntityState.InCar:
                _car.EnableNitro();
                break;
        }

    }

    private Vector2 CollideAndSlide(Vector2 velocity, Vector2 position, int depth = 0)
    {
        if (_maxBounces <= depth)
            return Vector2.zero;

        float distance = velocity.magnitude + _skinWidth;

        RaycastHit2D info = Physics2D.CircleCast(position, _radius - _skinWidth, velocity, distance);

        if (info)
        {
            Vector2 snapToSurface = velocity.normalized * (info.distance - _skinWidth);
            Vector2 leftover = velocity - snapToSurface;

            if (snapToSurface.magnitude <= _skinWidth)
                snapToSurface = Vector3.zero;

            leftover = Vector3.ProjectOnPlane(leftover, info.normal);

            return snapToSurface + CollideAndSlide(leftover, position + snapToSurface, depth + 1);
        }

        return velocity;
    }
}

public enum EntityState
{
    Dead,
    Alive,
    InCar
}
