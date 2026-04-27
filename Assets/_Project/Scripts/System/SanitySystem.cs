using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    public enum SanityLevel { High, Medium, Low, Critical }

    [SerializeField] private float _sanity = 1f; 
    public UnityEvent OnSanityChanged = new UnityEvent(); 
    public UnityEvent<SanityLevel> OnLevelChanged = new UnityEvent<SanityLevel>();

    private SanityLevel _currentLevel = SanityLevel.High;

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
        // SỬA LẠI LOGIC SO SÁNH ĐỂ KHỚP VỚI TEST:
        // Mốc biên (0.75, 0.4, 0.1) phải dùng dấu > thì mốc biên mới rơi vào nhóm dưới
        if (_sanity > 0.75f) return SanityLevel.High;
        if (_sanity > 0.40f) return SanityLevel.Medium;
        if (_sanity > 0.10f) return SanityLevel.Low;
        return SanityLevel.Critical;
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