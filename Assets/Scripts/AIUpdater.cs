using System.Collections.Generic;
using UnityEngine;
using AI.API;

public class AIUpdater : MonoBehaviour
{
    [SerializeField] private MovementControl _player;

    [SerializeField] private float _updateInterval = 0.05f;
    
    public int CountOfEnemies
    {
        get
        {
            return _enemyTeam.Count;
        }
    }

    public int CountOfAllies
    {
        get
        {
            return _playerTeam.Count + ((_player.State != EntityState.Dead) ? 1 : 0);
        }
    }

    public MovementControl Player => _player; 

    [SerializeField] private List<AIControl> _enemyTeam;
    [SerializeField] private List<AIControl> _playerTeam;

    private float _timer;

    private void Start()
    {
        _timer = _updateInterval;
        AiApi.aiDelta = _updateInterval;
    }

    private void Update()
    {
        if (_timer < _updateInterval)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            _timer = 0;
            UpdateAllAIs();
        }
    }

    private void UpdateAllAIs()
    {
        for (int i = 0; i < _enemyTeam.Count; i++)
            if (_enemyTeam[i].Controller.State == EntityState.Dead)
                _enemyTeam.Remove(_enemyTeam[i]);

        for (int i = 0; i < _playerTeam.Count; i++)
            if (_playerTeam[i].Controller.State == EntityState.Dead)
                _playerTeam.Remove(_playerTeam[i]);

        for (int i = 0; i < _enemyTeam.Count; i++)
        {
            var controller = _enemyTeam[i];

            if (_playerTeam.Count > 0 && (i % 2) == 0)
            {
                AIControl enemy = _playerTeam[i % _playerTeam.Count];

                if (enemy.Controller.State == EntityState.Dead)
                    _playerTeam.Remove(enemy);

                controller.SetTarget(enemy.Controller);
            }
            else if (_player.State != EntityState.Dead)
            {
                controller.SetTarget(_player);
            }
            else
            {
                controller.SetTarget(null);
            }

            controller.UpdateAI();
        }

        for (int i = 0; i < _playerTeam.Count; i++)
        {
            var controller = _playerTeam[i];

            if (_enemyTeam.Count > 0)
            {
                AIControl enemy = _enemyTeam[i % _enemyTeam.Count];

                if (enemy.Controller.State == EntityState.Dead)
                    _enemyTeam.Remove(enemy);

                controller.SetTarget(enemy.Controller);
            }

            controller.UpdateAI();
        }
    }
}
