using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class FlashlightController : MonoBehaviour
{
    [Header("Battery")]
    [SerializeField] private float _batteryLevel = 1f;
    [SerializeField] private float _drainRate    = 0.02f;

    [Header("Light")]
    [SerializeField] private Light _light;
    [SerializeField] private float _maxIntensity    = 1.425f;
    [SerializeField] private float _spotAngle       = 90f;
    [SerializeField] private float _innerSpotAngle  = 80f;
    [SerializeField] private float _range           = 30f;
    [SerializeField] private bool  _isOn            = false;

    [Header("Shake to Recover (phím F)")]
    [SerializeField] private float _shakeRecoverAmount  = 0.1f;  // phục hồi 10%
    [SerializeField] private float _shakeCooldown       = 3f;    // cooldown lắc tiếp
    [SerializeField] private float _shakeDrainPause     = 2f;    // tạm dừng drain bao lâu sau khi lắc
    [SerializeField] private float _shakeBatteryThresh  = 0.5f;  // chỉ lắc khi pin < 50%

    private float _shakeCooldownTimer  = 0f;
    private float _drainPauseTimer     = 0f; // đếm ngược thời gian pause drain
    private bool  _eventFired          = false;
    private bool  _isFlickering        = false;

    public UnityEvent OnBatteryEmpty = new UnityEvent();

    // ─── INIT ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_light == null) _light = GetComponentInChildren<Light>();
        SetupLight();
    }

    private void SetupLight()
    {
        if (_light == null) return;
        _light.type            = LightType.Spot;
        _light.spotAngle       = _spotAngle;
        _light.innerSpotAngle  = _innerSpotAngle;
        _light.range           = _range;
        _light.color           = new Color(1f, 0.97f, 0.88f);
        _light.intensity       = 0f;
        _light.shadows         = LightShadows.Soft;
    }

    // ─── UPDATE ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (_shakeCooldownTimer > 0f) _shakeCooldownTimer -= Time.deltaTime;
        if (_drainPauseTimer    > 0f) _drainPauseTimer    -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.T)) Toggle();
        if (Input.GetKeyDown(KeyCode.F)) TryShake();

        // Hết pin
        if (_batteryLevel <= 0f)
        {
            _batteryLevel = 0f;
            _isOn         = false;
            SetLightIntensity(0f);
            if (!_eventFired) { _eventFired = true; OnBatteryEmpty.Invoke(); }
            return;
        }

        // Drain pin — tạm dừng khi vừa lắc xong
        if (_isOn && _drainPauseTimer <= 0f)
            _batteryLevel -= _drainRate * Time.deltaTime;

        UpdateLightState();
    }

    // ─── TOGGLE ────────────────────────────────────────────────────────────────
    public void Toggle()
    {
        if (_batteryLevel <= 0f) return;
        _isOn = !_isOn;
        if (!_isOn) SetLightIntensity(0f);
        Debug.Log($"[Đèn Pin] {(_isOn ? "BẬT" : "TẮT")} — Pin: {_batteryLevel * 100f:F0}%");
    }

    // ─── ADD BATTERY ───────────────────────────────────────────────────────────
    public void AddBattery(float amount)
    {
        _batteryLevel = Mathf.Clamp01(_batteryLevel + amount);
        if (_batteryLevel > 0f) _eventFired = false;
    }

    // ─── SHAKE ─────────────────────────────────────────────────────────────────
    private void TryShake()
    {
        if (_batteryLevel >= _shakeBatteryThresh)
        {
            Debug.Log("[Đèn Pin] Pin còn đủ, không cần lắc.");
            return;
        }
        if (_shakeCooldownTimer > 0f)
        {
            Debug.Log($"[Đèn Pin] Chờ {_shakeCooldownTimer:F1}s nữa mới lắc được.");
            return;
        }

        AddBattery(_shakeRecoverAmount);
        _shakeCooldownTimer = _shakeCooldown;

        // Tạm dừng drain sau khi lắc để pin không bị ăn ngay lập tức
        _drainPauseTimer = _shakeDrainPause;

        Debug.Log($"[Đèn Pin] Lắc! +10% → {_batteryLevel * 100f:F0}% (drain tạm dừng {_shakeDrainPause}s)");
    }

    // ─── LIGHT STATE ───────────────────────────────────────────────────────────
    private void UpdateLightState()
    {
        if (!_isOn || _isFlickering)
        {
            if (!_isFlickering) SetLightIntensity(0f);
            return;
        }

        if (_batteryLevel > 0.75f)
        {
            SetLightIntensity(_maxIntensity);
        }
        else if (_batteryLevel > 0.5f)
        {
            float t = (_batteryLevel - 0.5f) / 0.25f;
            SetLightIntensity(Mathf.Lerp(_maxIntensity * 0.7f, _maxIntensity, t));
        }
        else if (_batteryLevel > 0.3f)
        {
            SetLightIntensity(_maxIntensity * 0.65f);
            if (Random.value < 0.003f) StartCoroutine(Flicker(2, 0.05f));
        }
        else if (_batteryLevel > 0.15f)
        {
            SetLightIntensity(_maxIntensity * 0.4f);
            if (Random.value < 0.005f) StartCoroutine(Flicker(1, 0.15f));
        }
        else
        {
            SetLightIntensity(_maxIntensity * 0.2f);
            if (Random.value < 0.01f) StartCoroutine(Flicker(2, 0.2f));
        }
    }

    // ─── FLICKER ───────────────────────────────────────────────────────────────
    private IEnumerator Flicker(int count, float duration)
    {
        if (_isFlickering) yield break;
        _isFlickering = true;
        float orig = _light != null ? _light.intensity : 0f;
        for (int i = 0; i < count; i++)
        {
            SetLightIntensity(0f);
            yield return new WaitForSeconds(duration);
            SetLightIntensity(orig);
            yield return new WaitForSeconds(0.05f);
        }
        _isFlickering = false;
    }

    // ─── HELPER ────────────────────────────────────────────────────────────────
    private void SetLightIntensity(float intensity)
    {
        if (_light != null) _light.intensity = intensity;
    }
}