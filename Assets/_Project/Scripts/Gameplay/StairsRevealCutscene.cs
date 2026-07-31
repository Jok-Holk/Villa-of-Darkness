using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Cutscene "lia cầu thang" -- 1 trong 3 cutscene còn thiếu của Cảnh 3 (Tân báo cáo 2026-07-31, Jok xác
// nhận "đương nhiên phải làm"). Player đi qua trigger gần cầu thang, camera bị ép quay nhìn về phía
// _lookTarget (VD đầu cầu thang, chỗ bóng ma thoáng hiện), giữ 1 nhịp, tuỳ chọn hiện rồi ẩn _revealObject
// (glimpse ma), rồi trả lại quyền điều khiển. TẤT CẢ field để trống -- Jok tự kéo Transform/GameObject
// thật vào Inspector, không đoán mù vị trí scene thật (đúng pattern Chapter1Scene3Manager đã dùng).
public class StairsRevealCutscene : MonoBehaviour
{
    [Tooltip("Điểm camera bị ép quay nhìn tới -- VD đầu cầu thang, chỗ bóng ma thoáng hiện.")]
    [SerializeField] private Transform _lookTarget;

    [Tooltip("Model/silhouette ma thoáng hiện rồi biến mất -- để trống nếu chỉ cần lia camera, không cần hiện gì.")]
    [SerializeField] private GameObject _revealObject;

    [SerializeField] private float _turnDuration = 0.8f;
    [Tooltip("Chờ bao lâu sau khi camera quay xong rồi mới hiện _revealObject -- tạo nhịp hồi hộp trước khi lộ ra.")]
    [SerializeField] private float _revealDelay = 0.3f;
    [SerializeField] private float _revealHoldDuration = 0.5f;
    [SerializeField] private float _extraHoldDuration = 0.4f;
    [SerializeField] private AudioClip _stingSfx;
    [Tooltip("Quay camera trả về đúng hướng cũ trước khi trả quyền điều khiển -- tắt nếu muốn giữ nguyên hướng mới sau cutscene.")]
    [SerializeField] private bool _returnToOriginalFacing = false;

    public UnityEvent OnCutsceneComplete;

    private bool _hasPlayed = false;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasPlayed) return;
        if (!other.CompareTag("Player")) return;
        // THÊM (Jok hỏi "có check hiện đang cảnh 3 không"): cutscene này CHỈ thuộc cảnh 3 -- tránh trigger
        // sai ngữ cảnh nếu Player debug-nhảy cảnh khác (CheckpointDebugTool) mà lỡ đi ngang đúng vùng này.
        if (!Chapter1Scene3Manager.IsActive) return;
        _hasPlayed = true;
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        var player = PlayerController.Instance;
        Camera cam = Camera.main;
        if (player == null || cam == null) { OnCutsceneComplete?.Invoke(); yield break; }

        Quaternion originalCamWorldRot = cam.transform.rotation;

        player.SetInputEnabled(false);

        if (_lookTarget != null)
            yield return CutsceneCameraUtil.LookAt(player, cam.transform, _lookTarget, _turnDuration);

        if (_revealObject != null)
        {
            yield return new WaitForSeconds(_revealDelay);
            if (_stingSfx != null) AudioManager.Instance?.PlaySFX(_stingSfx);
            _revealObject.SetActive(true);
            yield return new WaitForSeconds(_revealHoldDuration);
            _revealObject.SetActive(false);
        }
        else if (_stingSfx != null)
        {
            AudioManager.Instance?.PlaySFX(_stingSfx);
        }

        yield return new WaitForSeconds(_extraHoldDuration);

        if (_returnToOriginalFacing)
        {
            // Tạo target ảo tạm thời tại đúng hướng nhìn ban đầu (yaw + pitch) để tái dùng LookAt() --
            // dùng cam.transform.rotation (world) chứ không phải root.rotation, vì phải khôi phục cả pitch.
            var tempTargetGO = new GameObject("StairsReveal_ReturnTarget");
            tempTargetGO.transform.position = cam.transform.position + originalCamWorldRot * Vector3.forward * 5f;
            yield return CutsceneCameraUtil.LookAt(player, cam.transform, tempTargetGO.transform, _turnDuration);
            Object.Destroy(tempTargetGO);
        }

        player.SetInputEnabled(true);
        OnCutsceneComplete?.Invoke();
    }
}
