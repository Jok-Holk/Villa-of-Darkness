using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] private float _batteryLevel = 1f;
    [SerializeField] private float _drainRate = 0.01f;
    private void Update() { _batteryLevel = Mathf.Clamp(_batteryLevel - _drainRate * Time.deltaTime, 0f, 1f); }
    public void AddBattery(float amount) { _batteryLevel = Mathf.Clamp(_batteryLevel + amount, 0f, 1f); }
}
