using AI.API;
using UnityEngine;

public class AIControl : MonoBehaviour
{
    [SerializeField] private MovementControl _controller;

    [SerializeField] private float _p = 1;
    [SerializeField] private float _i = 1;
    [SerializeField] private float _d = 1;

    private PidController _pid;
    private DrivingStates _drivingState = DrivingStates.Forward;
    private MovementControl _target;
    private Vector2 _axis;

    private bool _use;
    private bool _dash;

    public MovementControl Controller => _controller; 

    private void Start()
    {
        _pid = new PidController(_p, _i, _d);
    }

    private void Update()
    {
        _pid.P = _p;
        _pid.I = _i;
        _pid.D = _d;

        _controller.MoveAxis(_axis);
        
        if (_use) _controller.Use();

        if (_dash) _controller.UseDash();

        _use = _dash = false;
    }
    

    public void UpdateAI()
    {
        _use = _dash = false;
        _axis = Vector2.zero;

        var state = _controller.State;

        if (_target == null)
            return;

        switch (state)
        {
            case EntityState.Alive:
                AliveLogic();
                break;
            case EntityState.InCar:
                CarLogic();
                break;
            default:
                break;
        }
    }

    public void SetTarget(MovementControl target)
    {
        _target = target;
    }

    private void CarLogic()
    {
        var position = _target.transform.position - transform.position;
        float angle = Vector2.SignedAngle(transform.up, position);

        float trottle = 0;

        if (Mathf.Abs(angle) < 40f)
        {
            trottle = 1;

            if (Random.Range(0, 100) < 10)
                _dash = true;

            _drivingState = DrivingStates.Forward;
        }
        else if (Mathf.Abs(angle) > 90f)
        {
            if (_drivingState == DrivingStates.Forward && _controller.OccupiedCar.CurrentSpeed < 0)
                _drivingState = DrivingStates.Backwards;

            switch (_drivingState)
            {
                case DrivingStates.Forward:
                    trottle = 0;
                    _drivingState = DrivingStates.Brake;
                    break;
                case DrivingStates.Brake:
                    trottle = -1;

                    if (_controller.OccupiedCar.CurrentSpeed <= CarControl.velocityThreshold)
                    {
                        trottle = 0;
                        _drivingState = DrivingStates.Backwards;
                    }
                    break;
                case DrivingStates.Backwards:
                    trottle = -1;
                    break;
            }
        }
        else
        {
            trottle = 1 - ((Mathf.Abs(angle) - 40) / 90f) * 0.25f;
            _drivingState = DrivingStates.Forward;
        }

        _pid.ProcessVariable = angle / 180f;
        _pid.SetPoint = 0;

        float output = (float)_pid.ControlVariable(AiApi.aiDelta) / 40;
        _axis = new Vector2(output, trottle);
    }

    private void AliveLogic()
    {
        bool result = AiApi.GetClosestCarPosition(transform.position, out Vector2 position, _controller.UseMask);

        if (result)
        {
            position -= (Vector2)transform.position;
            float angle = Mathf.Atan2(position.y, position.x);

            if (position.sqrMagnitude <= (_controller.UseRadius * _controller.UseRadius) * 2)
                _use = true;

            _axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    private enum DrivingStates
    {
        Forward,
        Brake,
        Backwards
    }
}
