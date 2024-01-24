using AI.API;
using System;
using UnityEngine;

public class HudTargeter : MonoBehaviour
{
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _deadScreen;
    [SerializeField] private MovementControl _player;

    [SerializeField] private LayerMask _useLayer;

    void Start()
    {
        _deadScreen.SetActive(false);
        _arrow.SetActive(false);
    }

    void Update()
    {
        switch (_player.State)
        {
            case EntityState.Dead:
                _deadScreen.SetActive(true);
                _arrow.SetActive(false);
                break;
            case EntityState.Alive:
                if (!_arrow.activeSelf)
                    _arrow.SetActive(true);

                SetAngleForArrow();
                break;
            case EntityState.InCar:
                if (_arrow.activeSelf)
                    _arrow.SetActive(false);
                break;
        }


    }

    private void SetAngleForArrow()
    {
        if (AiApi.GetClosestCarPosition(_player.transform.position, out Vector2 position, _useLayer.value))
        {
            position -= (Vector2)_player.transform.position;

            float rotation = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;

            _arrow.transform.rotation = Quaternion.Euler(0, 0, rotation - 90f);
        }
        else
        {
            _arrow.SetActive(false);
        }
    }
}
