using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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
    [Tooltip("Mặc định CẦM SẴN trên tay ngay từ đầu game -- không phụ thuộc vào việc chạy intro hay skip thẳng tới checkpoint nào, luôn nhất quán 1 chỗ duy nhất thay vì phải tự SetOn(true) rải rác ở từng điểm vào cảnh.")]
    [SerializeField] private bool  _isOn = true;

    [Header("Model — hạ xuống/ẩn khi tắt, đưa lên/hiện khi bật")]
    [SerializeField] private Transform _modelTransform;

    [Header("Hội tụ tia sáng vào giữa màn hình — bù lệch vị trí đèn cầm tay so với mắt camera")]
    [Tooltip("Để trống sẽ tự lấy Camera.main")]
    [SerializeField] private Camera _aimCamera;

    [Header("SFX")]
    [Tooltip("Tiếng lắc đèn pin lúc bấm T — audio dài ~3s nên animation lắc bên dưới cũng kéo dài khớp theo")]
    [SerializeField] private AudioClip _shakeSfx;

    private float _batteryLevel       = 1f;
    private float _drainPauseTimer    = 0f;
    private bool  _eventFired         = false;
    private bool  _isFlickering       = false;
    private bool  _isShaking          = false;
    private bool  _flashlightHintShown = false;
    private bool  _wasBatteryLow       = false;

    // Hint "T lắc pin" chỉ dạy 2 lần đầu là đủ nhớ (Jok yêu cầu) -- lặp lại vô hạn như trước sẽ làm phiền
    // người chơi đã quen. Riêng CHỈ cần ở Chapter1 (chương dạy cơ chế đèn pin), các chương sau người chơi
    // đã biết rồi nên không hiện nữa dù pin có xuống thấp.
    private const string ShakeHintCountKey = "VoD_Hint_t_shake_count";
    private const int    ShakeHintMaxCount = 2;
    private Quaternion _modelBaseLocalRotation;
    private Coroutine _raiseLowerRoutine;
    private Coroutine _shakeAnimRoutine;
    private Coroutine _flickerRoutine;

    public UnityEvent OnBatteryEmpty = new UnityEvent();

    public bool IsOn => _isOn;
    public float BatteryLevel01 => _batteryLevel;

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

        // BUG THẬT: FlashlightController trước đây nghe phím F/T vô điều kiện, không check
        // InteractionSystem.IsInputBlocked -- InventoryUI CŨNG dùng F làm phím tắt bàn phím cho "Sử dụng"
        // (khác nút chuột OnUseButtonClicked), nên mở Inventory bấm F để dùng chìa khoá vô tình BẤM TRÙNG
        // luôn cả Toggle() đèn pin. Giờ chặn hẳn input đèn pin trong lúc input đang bị khoá (Inventory,
        // Dialogue, Piano mode, Examine...).
        if (!InteractionSystem.IsInputBlocked)
        {
            if (Input.GetKeyDown(KeyCode.F)) Toggle();
            if (Input.GetKeyDown(KeyCode.T)) TryShake();

            // Frame ĐẦU TIÊN người chơi thật sự có quyền điều khiển (qua khỏi Intro/thoại mở màn) -- dạy
            // ngay F bật/tắt đèn pin, 1 lần duy nhất là đủ nhớ suốt game.
            if (!_flashlightHintShown)
            {
                _flashlightHintShown = true;
                TutorialHintUI.Instance.ShowOnce("f_flashlight", "F", "Bật / tắt đèn pin");
            }
        }

        // Pin vừa TỤT XUỐNG mức yếu (medium threshold) -- nhắc T lắc hồi pin. Bắt cạnh chuyển trạng thái
        // (không phải mỗi frame) nên chỉ hiện lại đúng lúc pin MỚI xuống yếu, không phải liên tục mỗi frame
        // trong khi vẫn đang yếu -- và hiện LẶP LẠI mỗi lần rơi vào tình huống này (không chỉ 1 lần cho cả game).
        bool isBatteryLow = _isOn && _batteryLevel <= _data.flickerMediumThresh;
        if (isBatteryLow && !_wasBatteryLow)
            TryShowShakeHint();
        _wasBatteryLow = isBatteryLow;

        if (_batteryLevel <= 0f)
        {
            _batteryLevel = 0f;
            _isOn         = false;
            SetLightIntensity(0f);
            SanitySystem.Instance?.SetSafeZone(false); // hết pin — mất luôn hiệu ứng hồi sanity từ đèn
            if (!_eventFired) { _eventFired = true; OnBatteryEmpty.Invoke(); }
            return;
        }

        // SỬA 2026-07-27: Trước đây pin vẫn tụt đều dù đang mở Inventory/Dialogue/Examine/Piano (Time.timeScale
        // không đóng băng những lúc đó) -- người chơi mở túi đồ xem xét lâu 1 chút là mất pin oan, dù không hề
        // đang thật sự "chơi". Chặn drain bằng đúng cờ IsInputBlocked đã dùng để chặn F/T ở trên.
        if (_isOn && _drainPauseTimer <= 0f && !InteractionSystem.IsInputBlocked)
            _batteryLevel -= _data.drainRate * Time.deltaTime;

        // Đang chơi animation lắc (T) thì để ShakeAnimationRoutine() toàn quyền quyết định intensity
        // (ép về 0 trong lúc lắc) — không cho UpdateLightState() đè cường độ thật lên mỗi frame.
        if (!_isShaking) UpdateLightState();
    }

    private void TryShowShakeHint()
    {
        if (SceneManager.GetActiveScene().name != "Chapter1") return;

        int count = PlayerPrefs.GetInt(ShakeHintCountKey, 0);
        if (count >= ShakeHintMaxCount) return;

        PlayerPrefs.SetInt(ShakeHintCountKey, count + 1);
        PlayerPrefs.Save();
        TutorialHintUI.Instance.ShowRepeating("T", "Lắc để hồi pin");
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

        StopFlicker();

        _isOn = !_isOn;
        if (!_isOn) SetLightIntensity(0f);
        SanitySystem.Instance?.SetSafeZone(_isOn);

        if (_modelTransform != null && _data != null)
        {
            if (_raiseLowerRoutine != null) StopCoroutine(_raiseLowerRoutine);
            _raiseLowerRoutine = StartCoroutine(RaiseOrLower(_isOn));
        }
    }

    // ─── SET ON/OFF (dùng khi hệ thống khác cần ép trạng thái, vd tạm tắt khi vào tương tác) ──
    public void SetOn(bool on)
    {
        if (_isOn == on) return;
        if (on && _batteryLevel <= 0f) return; // hết pin thì không ép bật lại được

        StopFlicker();

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

        if (_shakeSfx != null) AudioManager.Instance?.PlaySFX(_shakeSfx);

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

        // shakeCount=16 (thay vì 4 trước đây) để tổng animation ~2.9s (0.15 lên + 16*0.16 lắc + 0.15 xuống),
        // khớp với độ dài audio SFX_FlashlightShake ~3s thay vì kết thúc animation giữa chừng lúc audio còn đang phát.
        const int   shakeCount     = 16;
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
            if (Random.value < _data.mediumFlickerChance) _flickerRoutine = StartCoroutine(Flicker(_data.mediumFlickerCount, _data.mediumFlickerDuration));
        }
        else if (_batteryLevel > _data.flickerCriticalThresh)
        {
            // Low → nhấp nháy mạnh
            SetLightIntensity(max * _data.lowIntensityMult);
            if (Random.value < _data.lowFlickerChance) _flickerRoutine = StartCoroutine(Flicker(_data.lowFlickerCount, _data.lowFlickerDuration));
        }
        else
        {
            // Critical → rất yếu
            SetLightIntensity(max * _data.criticalIntensityMult);
            if (Random.value < _data.criticalFlickerChance) _flickerRoutine = StartCoroutine(Flicker(_data.criticalFlickerCount, _data.criticalFlickerDuration));
        }
    }

    // Huỷ coroutine Flicker() đang chạy dở (nếu có) -- BUG THẬT gây "đèn pin lâu lâu biến mất/tự sáng vô cớ":
    // Flicker() trước đây KHÔNG lưu reference, nên khi hệ thống khác (Piano tắt đèn tạm, v.v.) gọi SetOn()/
    // Toggle() ép đổi trạng thái NGAY GIỮA LÚC đang nhấp nháy (pin yếu), coroutine cũ vẫn tự chạy tiếp vòng
    // lặp riêng của nó và tự ý SetLightIntensity(orig) bật sáng lại, bất kể _isOn vừa bị đổi ở nơi khác.
    private void StopFlicker()
    {
        if (_flickerRoutine != null)
        {
            StopCoroutine(_flickerRoutine);
            _flickerRoutine = null;
        }
        _isFlickering = false;
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
        _flickerRoutine = null;
    }

    // ─── HELPER ────────────────────────────────────────────────────────────────
    private void SetLightIntensity(float intensity)
    {
        if (_light != null) _light.intensity = intensity;
    }
}