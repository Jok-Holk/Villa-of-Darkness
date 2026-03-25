using UnityEngine;
using UnityEngine.Events;

namespace Phase1.VoVanThuan
{
    public class FlashlightController : MonoBehaviour
    {
        [SerializeField] private float _batteryLevel = 1f;
        [SerializeField] private bool _isOn = false; 
        private bool _eventFired = false; // Biến phụ để kiểm soát event

        public UnityEvent OnBatteryEmpty = new UnityEvent();

        public void Toggle() 
        {
            _isOn = !_isOn;
        }

        public void AddBattery(float amount) 
        {
            _batteryLevel = Mathf.Clamp01(_batteryLevel + amount);
            if (_batteryLevel > 0) _eventFired = false; // Reset nếu sạc lại pin
        }

        private void Update()
        {
            // Nếu pin hết (dù đèn đang bật hay vừa bị ép về 0 bằng R.Set)
            if (_batteryLevel <= 0)
            {
                _batteryLevel = 0;
                _isOn = false;

                // CHỐT CHẶN: Chỉ gọi Invoke 1 lần duy nhất khi hết pin
                if (!_eventFired)
                {
                    _eventFired = true;
                    OnBatteryEmpty.Invoke();
                }
                return;
            }

            // Giảm pin bình thường khi đèn bật
            if (_isOn)
            {
                _batteryLevel -= 0.1f * Time.deltaTime;
            }
        }
    }
}