using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class FlashlightController : MonoBehaviour
{
    [Header("Data — kéo FlashlightData asset vào")]
    [SerializeField] private FlashlightData _data;

    [Header("Light")]
    [SerializeField] private Light _light;
    [SerializeField] private bool  _isOn = false;

    [Header("Model — hạ xuống/ẩn khi tắt, đưa lên/hiện khi bật")]
    [SerializeField] private Transform _modelTransform;
    [SerializeField] private Vector3   _raisedLocalPos  = new Vector3(0.35f, -0.25f, 0.5f);
    [SerializeField] private Vector3   _loweredLocalPos = new Vector3(0.35f, -0.7f, 0.5f);
    [SerializeField] private float     _raiseLowerDuration = 0.25f;

    [Header("Hội tụ tia sáng vào giữa màn hình — bù lệch vị trí đèn cầm tay so với mắt camera")]
    [Tooltip("Để trống sẽ tự lấy Camera.main")]
    [SerializeField] private Camera _aimCamera;
    [Tooltip("Khoảng cách phía trước camera mà tia đèn sẽ hội tụ đúng vào giữa màn hình")]
    [SerializeField] private float  _aimConvergeDistance = 3f;

    private float _batteryLevel       = 1f;
    private float _shakeCooldownTimer = 0f;
    private float _drainPauseTimer    = 0f;
    private bool  _eventFired         = false;
    private bool  _isFlickering       = false;
    private Coroutine _raiseLowerRoutine;

    public UnityEvent OnBatteryEmpty = new UnityEvent();

    public bool IsOn => _isOn;

    // ─── INIT ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_light == null) _light = GetComponentInChildren<Light>();
        if (_aimCamera == null) _aimCamera = Camera.main;
        SetupLight();

        if (_modelTransform != null)
        {
            _modelTransform.localPosition = _isOn ? _raisedLocalPos : _loweredLocalPos;
            _modelTransform.gameObject.SetActive(_isOn);
        }
    }

    private void SetupLight()
    {
        if (_light == null || _data == null) return;
        _light.type           = LightType.Spot;
        _light.spotAngle      = _data.spotAngle;
        _light.innerSpotAngle = _data.innerSpotAngle;
        _light.range          = _data.range;
        _light.color          = new Color(1f, 0.97f, 0.88f);
        _light.intensity      = 0f;
        _light.shadows        = LightShadows.Soft;
    }

    // ─── UPDATE ────────────────────────────────────────────────────────────────
    private void Update()
    {
        if (_data == null) return;

        if (_shakeCooldownTimer > 0f) _shakeCooldownTimer -= Time.deltaTime;
        if (_drainPauseTimer    > 0f) _drainPauseTimer    -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.F)) Toggle();
        if (Input.GetKeyDown(KeyCode.T)) TryShake();

        if (_batteryLevel <= 0f)
        {
            _batteryLevel = 0f;
            _isOn         = false;
            SetLightIntensity(0f);
            if (!_eventFired) { _eventFired = true; OnBatteryEmpty.Invoke(); }
            return;
        }

        if (_isOn && _drainPauseTimer <= 0f)
            _batteryLevel -= _data.drainRate * Time.deltaTime;

        UpdateLightState();
    }

    // ─── AIM (chạy sau Update của camera để không giật 1 frame) ─────────────────
    private void LateUpdate()
    {
        if (_light == null || _aimCamera == null) return;

        Vector3 targetPoint = _aimCamera.transform.position + _aimCamera.transform.forward * _aimConvergeDistance;
        Vector3 dir = targetPoint - _light.transform.position;
        if (dir.sqrMagnitude > 0.0001f)
            _light.transform.rotation = Quaternion.LookRotation(dir);
    }

    // ─── TOGGLE ────────────────────────────────────────────────────────────────
    public void Toggle()
    {
        if (_batteryLevel <= 0f) return;
        _isOn = !_isOn;
        if (!_isOn) SetLightIntensity(0f);

        if (_modelTransform != null)
        {
            if (_raiseLowerRoutine != null) StopCoroutine(_raiseLowerRoutine);
            _raiseLowerRoutine = StartCoroutine(RaiseOrLower(_isOn));
        }

        Debug.Log($"[Đèn Pin] {(_isOn ? "BẬT" : "TẮT")} — Pin: {_batteryLevel * 100f:F0}%");
    }

    // ─── SET ON/OFF (dùng khi hệ thống khác cần ép trạng thái, vd tạm tắt khi vào tương tác) ──
    public void SetOn(bool on)
    {
        if (_isOn == on) return;
        if (on && _batteryLevel <= 0f) return; // hết pin thì không ép bật lại được
        _isOn = on;
        if (!_isOn) SetLightIntensity(0f);

        if (_modelTransform != null)
        {
            if (_raiseLowerRoutine != null) StopCoroutine(_raiseLowerRoutine);
            _raiseLowerRoutine = StartCoroutine(RaiseOrLower(_isOn));
        }
    }

    // ─── MODEL SHOW/HIDE ───────────────────────────────────────────────────────
    private IEnumerator RaiseOrLower(bool raising)
    {
        // Bật lên: hiện model NGAY (đã có nguồn sáng sẵn) rồi lerp lên vị trí cầm.
        // Tắt đi: lerp xuống rồi mới ẩn hẳn.
        if (raising) _modelTransform.gameObject.SetActive(true);

        Vector3 from = _modelTransform.localPosition;
        Vector3 to   = raising ? _raisedLocalPos : _loweredLocalPos;
        float t = 0f;
        while (t < _raiseLowerDuration)
        {
            t += Time.deltaTime;
            _modelTransform.localPosition = Vector3.Lerp(from, to, t / _raiseLowerDuration);
            yield return null;
        }
        _modelTransform.localPosition = to;

        if (!raising) _modelTransform.gameObject.SetActive(false);
        _raiseLowerRoutine = null;
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
        if (_data == null) return;

        if (_batteryLevel >= _data.shakeBatteryThresh)
        {
            Debug.Log("[Đèn Pin] Pin còn đủ, không cần lắc.");
            return;
        }
        if (_shakeCooldownTimer > 0f)
        {
            Debug.Log($"[Đèn Pin] Chờ {_shakeCooldownTimer:F1}s nữa mới lắc được.");
            return;
        }

        AddBattery(_data.shakeRecoverAmount);
        _shakeCooldownTimer = _data.shakeCooldown;
        _drainPauseTimer    = _data.shakeDrainPause;
        Debug.Log($"[Đèn Pin] Lắc! +{_data.shakeRecoverAmount * 100f:F0}% → {_batteryLevel * 100f:F0}%");
    }

    // ─── LIGHT STATE ───────────────────────────────────────────────────────────
    private void UpdateLightState()
    {
        if (_data == null) return;
        if (!_isOn || _isFlickering)
        {
            if (!_isFlickering) SetLightIntensity(0f);
            return;
        }

        float max = _data.maxIntensity;

        if (_batteryLevel > _data.flickerMediumThresh)
        {
            // 50-100% → sáng ổn định, giảm dần
            float t = (_batteryLevel - _data.flickerMediumThresh) / (1f - _data.flickerMediumThresh);
            SetLightIntensity(Mathf.Lerp(max * 0.7f, max, t));
        }
        else if (_batteryLevel > _data.flickerLowThresh)
        {
            // 30-50% → nhấp nháy nhẹ
            SetLightIntensity(max * 0.65f);
            if (Random.value < 0.003f) StartCoroutine(Flicker(2, 0.05f));
        }
        else if (_batteryLevel > _data.flickerCriticalThresh)
        {
            // 15-30% → nhấp nháy mạnh
            SetLightIntensity(max * 0.4f);
            if (Random.value < 0.005f) StartCoroutine(Flicker(1, 0.15f));
        }
        else
        {
            // <15% → rất yếu
            SetLightIntensity(max * 0.2f);
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