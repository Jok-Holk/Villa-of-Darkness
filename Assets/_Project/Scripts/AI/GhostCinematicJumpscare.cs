using System.Collections;
using UnityEngine;

// Cinematic jumpscare "bắt được Player" cảnh 3 (Jok mô tả 2026-07-30) -- 3 yếu tố: tiếng hét to bất ngờ +
// camera shake + đèn chớp tắt lộ mặt enemy, đồng thời tắt đèn pin + freeze animation tấn công ngay trước
// camera. Chạy TRƯỚC JumpscareGameOverUI (đảm nhiệm fade đen + "BẠN ĐÃ CHẾT" + respawn) qua callback
// onComplete -- tách riêng 2 lớp để không phải viết lại toàn bộ JumpscareGameOverUI đã có sẵn.
public static class GhostCinematicJumpscare
{
    private class Runner : MonoBehaviour { }
    private static Runner _runner;

    public static void Trigger(Animator ghostAnim, AudioClip scream, Light jumpscareLight, string attackTriggerName,
        float attackFreezeDelay, int flickerCount, float flickerInterval, float shakeDuration, float shakeMagnitude,
        System.Action onComplete)
    {
        if (_runner == null)
        {
            var go = new GameObject("GhostCinematicJumpscare_Runner");
            Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<Runner>();
        }
        _runner.StartCoroutine(RunSequence(ghostAnim, scream, jumpscareLight, attackTriggerName, attackFreezeDelay,
            flickerCount, flickerInterval, shakeDuration, shakeMagnitude, onComplete));
    }

    private static IEnumerator RunSequence(Animator ghostAnim, AudioClip scream, Light jumpscareLight,
        string attackTriggerName, float attackFreezeDelay, int flickerCount, float flickerInterval,
        float shakeDuration, float shakeMagnitude, System.Action onComplete)
    {
        PlayerController.Instance?.SetInputEnabled(false);

        // Tắt đèn pin đang cầm + HUD -- "model enemy khi player bị bắt sẽ tắt luôn cả đèn pin, HUD tắt hết".
        var flashlight = Object.FindFirstObjectByType<FlashlightController>();
        flashlight?.SetOn(false);
        InteractPromptUI.Instance?.SetDotVisible(false);
        HudMetersUI.Instance?.SetVisible(false);

        if (scream != null)
            AudioManager.Instance?.PlaySFX(scream);

        // Trigger animation tấn công (nếu có param đúng tên) rồi chờ 1 khoảng cho animation kịp chuyển sang
        // đúng tư thế trước khi đóng băng cứng tại đó.
        if (ghostAnim != null)
        {
            if (!string.IsNullOrEmpty(attackTriggerName))
                ghostAnim.SetTrigger(attackTriggerName);

            yield return new WaitForSeconds(attackFreezeDelay);
            ghostAnim.speed = 0f; // freeze cứng animation tại đúng khung hình hiện tại
        }

        // Camera shake + đèn chớp tắt lộ mặt -- chạy song song trong cùng khoảng shakeDuration.
        Camera cam = Camera.main;
        Transform camTransform = cam != null ? cam.transform : null;
        Vector3 originalLocalPos = camTransform != null ? camTransform.localPosition : Vector3.zero;

        float shakeTimer = 0f;
        float flickerTimer = 0f;
        int flickerDone = 0;
        bool lightOn = false;
        if (jumpscareLight != null) jumpscareLight.enabled = false; // bắt đầu từ tối, chớp sáng dần lộ mặt

        while (shakeTimer < shakeDuration)
        {
            shakeTimer   += Time.deltaTime;
            flickerTimer += Time.deltaTime;

            if (camTransform != null)
                camTransform.localPosition = originalLocalPos + Random.insideUnitSphere * shakeMagnitude;

            if (jumpscareLight != null && flickerTimer >= flickerInterval && flickerDone < flickerCount)
            {
                flickerTimer = 0f;
                lightOn = !lightOn;
                jumpscareLight.enabled = lightOn;
                flickerDone++;
            }

            yield return null;
        }

        if (camTransform != null) camTransform.localPosition = originalLocalPos;
        if (jumpscareLight != null) jumpscareLight.enabled = true; // dừng lại ở trạng thái SÁNG -- lộ rõ mặt enemy
        if (ghostAnim != null) ghostAnim.speed = 1f; // trả lại bình thường, JumpscareGameOverUI sẽ fade đen ngay sau

        onComplete?.Invoke();
    }
}
