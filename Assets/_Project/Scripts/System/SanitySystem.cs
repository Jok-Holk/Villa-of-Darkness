using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    public static SanitySystem Instance { get; private set; }

    [SerializeField] private float _sanity = 1f;
    [SerializeField] private SanityData _data;

    public UnityEvent OnSanityChanged = new UnityEvent();
    // Bắn ra INDEX trong SanityData.levels (nguồn duy nhất định nghĩa số nấc) — trước đây bắn enum
    // SanityLevel 4 giá trị cứng trong khi SanityData có 5 nấc, ép (int)level làm index khiến nấc nặng
    // nhất (index 4) không bao giờ chọn tới được + ngưỡng chuyển nấc lệch hẳn so với ngưỡng drain rate
    // thật. Giờ chỉ còn 1 nguồn: GetCurrentLevelIndex() quét thẳng _data.levels.
    public UnityEvent<int> OnLevelChanged = new UnityEvent<int>();

    private int  _currentLevelIndex = 0;
    private bool _isInSafeZone = false;
    private float _checkTimer = 0f;
    private const float CHECK_INTERVAL = 0.2f;  // check mỗi 200ms

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (_data == null) return;

        _checkTimer += Time.deltaTime;
        if (_checkTimer < CHECK_INTERVAL) return;
        _checkTimer = 0f;

        if (_isInSafeZone)
        {
            // Hồi sanity — không bị trừ
            IncreaseSanity(_data.recoveryRate * CHECK_INTERVAL);
        }
        else
        {
            // Tiêu hao theo nấc hiện tại
            SanityLevelSettings current = GetCurrentSettings();
            if (current.drainRate > 0)
                DecreaseSanity(current.drainRate * CHECK_INTERVAL);
        }
    }

    // Dùng bool thay vì event để tránh spam bug
    public void SetSafeZone(bool isSafe)
    {
        _isInSafeZone = isSafe;
    }

    public void DecreaseSanity(float amount)
    {
        if (amount <= 0) return;
        _sanity = Mathf.Clamp(_sanity - amount, 0f, 1f);
        OnSanityChanged?.Invoke();
        CheckLevelChange();
    }

    public void IncreaseSanity(float amount)
    {
        if (amount <= 0) return;
        _sanity = Mathf.Clamp(_sanity + amount, 0f, 1f);
        OnSanityChanged?.Invoke();
        CheckLevelChange();
    }

    public float GetSanity() => _sanity;

    /// <summary>Index trong _data.levels khớp % sanity hiện tại — nguồn duy nhất cho cả drain rate lẫn visual.</summary>
    public int GetCurrentLevelIndex()
    {
        if (_data == null || _data.levels.Length == 0) return 0;

        float sanityPercent = _sanity * 100f;
        for (int i = 0; i < _data.levels.Length; i++)
        {
            var level = _data.levels[i];
            if (sanityPercent <= level.sanityMax && sanityPercent >= level.sanityMin)
                return i;
        }
        return _data.levels.Length - 1; // fallback: rơi ra ngoài mọi khoảng (lỗi làm tròn ở biên) → coi là nấc nặng nhất
    }

    public SanityLevelSettings GetCurrentSettings()
    {
        if (_data == null || _data.levels.Length == 0) return default;
        return _data.levels[GetCurrentLevelIndex()];
    }

    public string GetCurrentLevelName() => GetCurrentSettings().levelName;

    private void CheckLevelChange()
    {
        int newIndex = GetCurrentLevelIndex();
        if (newIndex != _currentLevelIndex)
        {
            _currentLevelIndex = newIndex;
            OnLevelChanged?.Invoke(_currentLevelIndex);
        }
    }
}
