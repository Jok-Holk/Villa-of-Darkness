using UnityEngine;
using UnityEngine.Events;

public class SanitySystem : MonoBehaviour
{
    public enum SanityLevel { High, Medium, Low, Critical }
    [SerializeField] private float _sanity = 1f;
    public UnityEvent OnSanityChanged;
    public void DecreaseSanity(float amount) { _sanity = Mathf.Clamp(_sanity - amount, 0f, 1f); }
    public void IncreaseSanity(float amount) { _sanity = Mathf.Clamp(_sanity + amount, 0f, 1f); }
    public SanityLevel GetLevel()
    {
        if (_sanity > 0.75f) return SanityLevel.High;
        if (_sanity > 0.40f) return SanityLevel.Medium;
        if (_sanity > 0.10f) return SanityLevel.Low;
        return SanityLevel.Critical;
    }
}
