using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    public enum SanityLevel { High, Medium, Low, Critical }

    [SerializeField] private float _sanity = 1f;
    [SerializeField] private SanityData _data;

    public UnityEvent OnSanityChanged = new UnityEvent();
    public UnityEvent<SanityLevel> OnLevelChanged = new UnityEvent<SanityLevel>();

    private SanityLevel _currentLevel = SanityLevel.High;
    private bool _isInSafeZone = false;
    private float _checkTimer = 0f;
    private const float CHECK_INTERVAL = 0.2f;  // check mỗi 200ms

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

    public SanityLevel GetLevel()
    {
        if (_sanity > 0.75f) return SanityLevel.High;
        if (_sanity > 0.40f) return SanityLevel.Medium;
        if (_sanity > 0.10f) return SanityLevel.Low;
        return SanityLevel.Critical;
    }

    public float GetSanity() => _sanity;

    public SanityLevelSettings GetCurrentSettings()
    {
        if (_data == null || _data.levels.Length == 0) return default;

        float sanityPercent = _sanity * 100f;
        foreach (var level in _data.levels)
        {
            if (sanityPercent <= level.sanityMax && sanityPercent >= level.sanityMin)
                return level;
        }
        return _data.levels[_data.levels.Length - 1];
    }

    private void CheckLevelChange()
    {
        SanityLevel newLevel = GetLevel();
        if (newLevel != _currentLevel)
        {
            _currentLevel = newLevel;
            OnLevelChanged?.Invoke(_currentLevel);
        }
    }
}