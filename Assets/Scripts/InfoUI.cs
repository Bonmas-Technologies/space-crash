using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{

    [SerializeField] private AIUpdater _updater;
    [SerializeField] private TextMeshProUGUI _mesh;
    [SerializeField] private Image _speed;
    [SerializeField] private Image _nitro;

    void LateUpdate()
    {
        _mesh.text = $"{_updater.CountOfEnemies}/{_updater.CountOfAllies}";

        _speed.fillAmount = 0;
        _nitro.fillAmount = 0;

        if (_updater.Player.State == EntityState.InCar)
        {
            var car = _updater.Player.OccupiedCar;

            _speed.fillAmount = Mathf.Abs(car.CurrentSpeed) / car.MaxSpeed;
            _nitro.fillAmount = car.NitroReloadPercent;
        }
    }
}
