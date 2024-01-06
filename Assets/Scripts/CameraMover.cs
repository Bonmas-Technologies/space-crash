using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private MovementControl _player;
    [SerializeField] private float _sizeSwitchTime = 0.1f;
    [SerializeField] private float _minSize = 5;
    [SerializeField] private float _medSize = 10;
    [SerializeField] private float _maxSize = 15;

    private float RequiredSize
    {
        get
        {
            return _requiredSize;
        }
        set
        {
            _switching = true;
            _switchTimer = 0;

            _startSize = _cam.orthographicSize;
            _requiredSize = value;
        }
    }

    private float _startSize;
    private float _requiredSize;

    private bool _switching;
    private float _switchTimer;

    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        switch (_player.State)
        {
            case EntityState.Dead:
                transform.position = Vector3.zero;
                if (!_switching && RequiredSize != _maxSize)
                    RequiredSize = _maxSize;
                break;
            case EntityState.Alive:
                transform.position = _player.transform.position;
                if (!_switching && RequiredSize != _minSize)
                    RequiredSize = _minSize;
                break;
            case EntityState.InCar:
                transform.position = _player.OccupiedCar.transform.position;
                if (!_switching && RequiredSize != _medSize)
                    RequiredSize = _medSize;
                break;
        }

        if (!_switching)
            return;

        if (_switchTimer > _sizeSwitchTime)
        {
            _switching = false;
            _cam.orthographicSize = RequiredSize;
            return;
        }

        _switchTimer += Time.deltaTime;
        _cam.orthographicSize = Mathf.Lerp(_startSize, RequiredSize, _switchTimer / _sizeSwitchTime);
    }
}
