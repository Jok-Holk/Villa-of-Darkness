using UnityEngine;
using UnityEngine.Events;
using System.Collections;

// ExecuteAlways để LateUpdate() (canh hướng đèn theo camera) chạy cả lúc Edit Mode — trước đây chỉ chạy
// lúc Play nên ngoài Play rotation nằm y giá trị rác từ lúc import, nhìn Scene view tưởng nhầm là bug hướng.
[ExecuteAlways]
public class FlashlightController : MonoBehaviour
{
    [Header("Data — kéo FlashlightData asset vào")]
    [SerializeField] private FlashlightData _data;

    [Header("Light")]
    [SerializeField] private Light _light;
    [SerializeField] private bool  _isOn = false;

    [Header("Model — hạ xuống/ẩn khi tắt, đưa lên/hiện khi bật")]
    [SerializeField] private Transform _modelTransform;

    [Header("Hội tụ tia sáng vào giữa màn hình — bù lệch vị trí đèn cầm tay so với mắt camera")]
    [Tooltip("Để trống sẽ tự lấy Camera.main")]
    [SerializeField] private Camera _aimCamera;

    [Header("Debug")]
    [Tooltip("Bật lên để in log so sánh rotation Camera vs Light mỗi 0.5s lúc Play — xem Console")]
    [SerializeField] private bool _debugLogAim = false;
    private float _debugLogTimer = 0f;

    private float _batteryLevel       = 1f;
    private float _drainPauseTimer    = 0f;
    private bool  _eventFired         = false;
    private bool  _isFlickering       = false;
    private bool  _isShaking          = false;
    private Quaternion _modelBaseLocalRotation;
    private Coroutine _raiseLowerRoutine;
    private Coroutine _shakeAnimRoutine;

    public UnityEvent OnBatteryEmpty = new UnityEvent();

    public bool IsOn => _isOn;

    // ─── INIT ──────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_light == null) _light = GetComponentInChildren<Light>();
        if (_aimCamera == null) _aimCamera = Camera.main;
        if (_data != null) _batteryLevel = _data.startBatteryLevel;
        SetupLight();

        if (_modelTransform != null && _data != null)
        {
            _modelTransform.localPosition = _isOn ? _data.raisedLocalPos : _data.loweredLocalPos;
            _modelTransform.gameObject.SetActive(_isOn);
        }
        if (_modelTransform != null) _modelBaseLocalRotation = _modelTransform.localRotation;
    }

    private void SetupLight()
    {
        if (_light == null || _data == null) return;
        _light.type            = LightType.Spot;
        _light.spotAngle       = _data.spotAngle;
        _light.innerSpotAngle  = _data.innerSpotAngle;
        _light.range           = _data.range;
        _light.color           = _data.lightColor;
        _light.shadows         = _data.shadowType;
        _light.shadowStrength  = _data.shadowStrength;
        _light.shadowBias      = _data.shadowBias;
        _light.shadowNormalBias = _data.shadowNormalBias;

        // Update() (chỗ tính intensity thật theo pin/flicker) chỉ chạy lúc Play — Edit Mode không có gì
        // chạy để cập nhật, nên preview trực tiếp theo _isOn ở đây để Jok bật/tắt checkbox là thấy ngay.
        _light.intensity = (Application.isPlaying || !_isOn) ? 0f : _data.maxIntensity;
    }

    // Gọi lại SetupLight() mỗi khi đổi field trong Inspector lúc Edit Mode (Awake chỉ chạy 1 lần lúc
    // Enable/reload script, không tự bắt được lúc Jok tick/untick _isOn hay đổi giá trị trong FlashlightData).
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (_light == null) _light = GetComponentInChildren<Light>();
        SetupLight();
    }

    // ─── UPDATE ────────────────────────────────────────────────────────────────
    private void Update()
    {
        // Pin/input/flicker chỉ có ý nghĩa lúc Play — Edit Mode chỉ cần LateUpdate() canh hướng chạy thôi.
        if (!Application.isPlaying) return;
        if (_data == null) return;

        if (_drainPauseTimer > 0f) _drainPauseTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.F)) Toggle();
        if (Input.GetKeyDown(KeyCode.T)) TryShake();

        if (_batteryLevel <= 0f)
        {
            _batteryLevel = 0f;
            _isOn         = false;
            SetLightIntensity(0f);
            SanitySystem.Instance?.SetSafeZone(false); // hết pin — mất luôn hiệu ứng hồi sanity từ đèn
            if (!_eventFired) { _eventFired = true; OnBatteryEmpty.Invoke(); }
            return;
        }

        if (_isOn && _drainPauseTimer <= 0f)
            _batteryLevel -= _data.drainRate * Time.deltaTime;

        // Đang chơi animation lắc (T) thì để ShakeAnimationRoutine() toàn quyền quyết định intensity
        // (ép về 0 trong lúc lắc) — không cho UpdateLightState() đè cường độ thật lên mỗi frame.
        if (!_isShaking) UpdateLightState();
    }

    // ─── AIM (chạy sau Update của camera để không giật 1 frame) ─────────────────
    private void LateUpdate()
    {
        if (_light == null || _aimCamera == null || _data == null) return;

        // Trước đây hội tụ về 1 điểm CỐ ĐỊNH cách camera đúng aimConvergeDistance — nên chỉ đúng tâm màn
        // hình ở đúng khoảng cách đó, gần/xa hơn là lệch tâm rõ. Giờ raycast thật mỗi frame để hội tụ đúng
        // vào điểm player đang thực sự nhìn (vật cản gần thì hội tụ gần, trống trải thì lùi về khoảng cách
        // mặc định) — luôn giữa màn hình bất kể khoảng cách. Loại trừ layer Player để không tự trúng người
        // chơi (giống lỗi raycast tự đâm vào chính mình từng gặp lúc đo chiều cao camera).
        float targetDistance = _data.aimConvergeDistance;
        if (Application.isPlaying)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            int mask = playerLayer >= 0 ? ~(1 << playerLayer) : ~0;
            if (Physics.Raycast(_aimCamera.transform.position, _aimCamera.transform.forward, out RaycastHit hit, _light.range, mask, QueryTriggerInteraction.Ignore))
                targetDistance = hit.distance;
        }

        Vector3 targetPoint = _aimCamera.transform.position + _aimCamera.transform.forward * targetDistance;
        Vector3 dir = targetPoint - _light.transform.position;
        if (dir.sqrMagnitude <= 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        // Đèn đặt lệch vị trí so với mắt camera nên hội tụ về 1 điểm gần rất nhạy — camera lắc nhẹ (head bob,
        // mouse look nhanh) là góc nhắm nhảy theo ngay, nhìn như đèn bị rung/giật. Làm mượt bằng Slerp lúc
        // Play (Edit Mode Time.deltaTime không đáng tin nên giữ snap thẳng, đủ dùng để preview tĩnh).
        if (Application.isPlaying && _data.aimSmoothSpeed > 0f)
            _light.transform.rotation = Quaternion.Slerp(_light.transform.rotation, targetRot, Time.deltaTime * _data.aimSmoothSpeed);
        else
            _light.transform.rotation = targetRot;

        if (_debugLogAim && Application.isPlaying)
        {
            _debugLogTimer += Time.deltaTime;
            if (_debugLogTimer >= 0.5f)
            {
                _debugLogTimer = 0f;
                float angle = Vector3.Angle(_light.transform.forward, _aimCamera.transform.forward);
                Debug.Log(
                    $"[Đèn Pin Debug] Camera euler={_aimCamera.transform.eulerAngles} forward={_aimCamera.transform.forward}\n" +
                    $"[Đèn Pin Debug] Light  euler={_light.transform.eulerAngles} forward={_light.transform.forward}\n" +
                    $"[Đèn Pin Debug] Góc lệch={angle:F1}°  |  Light pos={_light.transform.position} intensity={_light.intensity:F2} range={_light.range} spotAngle={_light.spotAngle} isOn={_isOn} battery={_batteryLevel * 100f:F0}%");
            }
        }
    }

    // ─── TOGGLE ────────────────────────────────────────────────────────────────
    public void Toggle()
    {
        if (_batteryLevel <= 0f) return;

        // Đang giữa animation lắc (T) mà bấm F — ngắt animation ngay, snap model về hướng bình thường
        // rồi mở đèn theo đúng chiều cũ, không đợi animation lắc chạy hết.
        if (_isShaking)
        {
            if (_shakeAnimRoutine != null) StopCoroutine(_shakeAnimRoutine);
            _isShaking = false;
            _shakeAnimRoutine = null;
            if (_modelTransform != null) _modelTransform.localRotation = _modelBaseLocalRotation;
        }

        _isOn = !_isOn;
        if (!_isOn) SetLightIntensity(0f);
        SanitySystem.Instance?.SetSafeZone(_isOn);

        if (_modelTransform != null && _data != null)
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
        SanitySystem.Instance?.SetSafeZone(_isOn);

        if (_modelTransform != null && _data != null)
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
        Vector3 to   = raising ? _data.raisedLocalPos : _data.loweredLocalPos;
        float duration = _data.raiseLowerDuration;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _modelTransform.localPosition = Vector3.Lerp(from, to, t / duration);
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
    // Bỏ hết giới hạn cũ (ngưỡng pin tối thiểu, cooldown) — lắc được bất cứ lúc nào, kể cả pin đầy
    // (AddBattery tự Clamp01 nên không lố). Chỉ chặn KHÔNG cho chồng animation nếu đang lắc dở.
    private void TryShake()
    {
        if (_data == null || _isShaking) return;

        AddBattery(_data.shakeRecoverAmount);
        _drainPauseTimer = _data.shakeDrainPause;
        Debug.Log($"[Đèn Pin] Lắc! +{_data.shakeRecoverAmount * 100f:F0}% → {_batteryLevel * 100f:F0}%");

        if (_modelTransform != null)
        {
            if (_shakeAnimRoutine != null) StopCoroutine(_shakeAnimRoutine);
            _shakeAnimRoutine = StartCoroutine(ShakeAnimationRoutine());
        }
    }

    // Quay hếch đèn lên (giả động tác lắc), rung lên-xuống vài cái, rồi trả về đúng hướng chĩa trước cũ.
    // Tắt đèn tạm trong lúc lắc (nhìn tự nhiên hơn — đèn chĩa lung tung không nên vẫn sáng rọi vào mắt),
    // bật lại đúng độ sáng cũ khi model đã về hướng bình thường (UpdateLightState() lo phần cường độ).
    private IEnumerator ShakeAnimationRoutine()
    {
        _isShaking = true;
        bool wasOn = _isOn;
        bool wasModelActive = _modelTransform.gameObject.activeSelf;

        // Đèn đang tắt/ẩn thì tạm hiện ra để chơi animation cho có, sau đó trả lại trạng thái ẩn nếu cần.
        if (!wasModelActive) _modelTransform.gameObject.SetActive(true);
        SetLightIntensity(0f); // tắt tạm dù trước đó đang bật

        Quaternion baseRot = _modelBaseLocalRotation;
        Quaternion upRot   = baseRot * Quaternion.Euler(-55f, 0f, 0f); // hếch lên trời

        yield return RotateOverTime(baseRot, upRot, 0.15f);

        const int   shakeCount     = 4;
        const float shakeAmplitude = 15f;
        const float shakeStepTime  = 0.08f;
        for (int i = 0; i < shakeCount; i++)
        {
            Quaternion a = upRot * Quaternion.Euler(0f, 0f, 0f);
            Quaternion b = upRot * Quaternion.Euler(shakeAmplitude, 0f, 0f);
            yield return RotateOverTime(i == 0 ? upRot : a, b, shakeStepTime);
            yield return RotateOverTime(b, a, shakeStepTime);
        }

        yield return RotateOverTime(_modelTransform.localRotation, baseRot, 0.15f);
        _modelTransform.localRotation = baseRot;

        if (!wasModelActive) _modelTransform.gameObject.SetActive(false);

        _isShaking = false;
        _shakeAnimRoutine = null;
        // Không cần tự bật lại đèn thủ công — Update()/UpdateLightState() chạy ngay frame sau sẽ tự tính
        // đúng cường độ theo _isOn + pin hiện tại (wasOn không đổi trong suốt animation).
    }

    private IEnumerator RotateOverTime(Quaternion from, Quaternion to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _modelTransform.localRotation = Quaternion.Slerp(from, to, t / duration);
            yield return null;
        }
        _modelTransform.localRotation = to;
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
            // Ổn định → sáng lerp từ stableIntensityMultMin lên full theo % pin còn lại
            float t = (_batteryLevel - _data.flickerMediumThresh) / (1f - _data.flickerMediumThresh);
            SetLightIntensity(Mathf.Lerp(max * _data.stableIntensityMultMin, max, t));
        }
        else if (_batteryLevel > _data.flickerLowThresh)
        {
            // Medium → nhấp nháy nhẹ
            SetLightIntensity(max * _data.mediumIntensityMult);
            if (Random.value < _data.mediumFlickerChance) StartCoroutine(Flicker(_data.mediumFlickerCount, _data.mediumFlickerDuration));
        }
        else if (_batteryLevel > _data.flickerCriticalThresh)
        {
            // Low → nhấp nháy mạnh
            SetLightIntensity(max * _data.lowIntensityMult);
            if (Random.value < _data.lowFlickerChance) StartCoroutine(Flicker(_data.lowFlickerCount, _data.lowFlickerDuration));
        }
        else
        {
            // Critical → rất yếu
            SetLightIntensity(max * _data.criticalIntensityMult);
            if (Random.value < _data.criticalFlickerChance) StartCoroutine(Flicker(_data.criticalFlickerCount, _data.criticalFlickerDuration));
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