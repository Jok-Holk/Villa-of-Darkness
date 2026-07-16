using UnityEngine;

public abstract class StaminaBarUI : MonoBehaviour
{
    [SerializeField] protected PlayerController player;
    [SerializeField] protected float smoothSpeed = 8f;
    protected float displayedValue = 1f;

    void Update()
    {
        if (player == null) return;
        float target = player.Stamina01;
        displayedValue = Mathf.Lerp(displayedValue, target, Time.deltaTime * smoothSpeed);
        UpdateVisual(displayedValue, target);
    }

    protected abstract void UpdateVisual(float smoothed, float raw);
}