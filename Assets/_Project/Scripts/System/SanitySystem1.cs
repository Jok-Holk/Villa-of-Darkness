using UnityEngine;
using UnityEngine.Events;

// ĐỔI NAMESPACE NÀY CHO TRÙNG VỚI FILE TEST
namespace Phase1.BuiThanhTan 
{
    public class SanitySystem : MonoBehaviour
    {
        public enum SanityLevel { High, Medium, Low, Critical }

        [SerializeField] private float _sanity = 1f; 

        // Khởi tạo Event để tránh NullReferenceException trong Unit Test
        public UnityEvent OnSanityChanged = new UnityEvent(); 
        public UnityEvent<SanityLevel> OnLevelChanged = new UnityEvent<SanityLevel>();

        private SanityLevel _currentLevel = SanityLevel.High;

        public void DecreaseSanity(float amount)
        {
            if (amount <= 0) return;
            _sanity = Mathf.Clamp(_sanity - amount, 0f, 1f);
            
            // Kích hoạt event
            OnSanityChanged.Invoke(); 
            CheckLevelChange();
        }

        public void IncreaseSanity(float amount)
        {
            if (amount <= 0) return;
            _sanity = Mathf.Clamp(_sanity + amount, 0f, 1f);
            
            // Kích hoạt event
            OnSanityChanged.Invoke();
            CheckLevelChange();
        }

        public SanityLevel GetLevel()
        {
            // Logic threshold chuẩn để pass các bài test boundary
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
                OnLevelChanged.Invoke(_currentLevel);
            }
        }
    }
}