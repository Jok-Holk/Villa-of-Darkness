using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Cutscene "twist khoá cửa sau lưng" -- 1 trong 3 cutscene còn thiếu của Cảnh 3. Player vừa đi qua cửa
// ngoại lệ sân sau (xem Chapter1Scene3Manager._backyardUnlockableDoor -- cửa duy nhất mở lại được bằng
// chìa lúc vào cảnh 3), cửa tự đóng sập + khoá cứng vĩnh viễn đúng lúc Player vừa lọt qua -- không quay
// đầu lại được nữa dù có đúng chìa (SetForceJammed(true), khác Chapter1Scene3Manager chỉ khoá thường lúc
// đầu cảnh 3). Đặt trigger NGAY SAU cửa (phía Player vừa bước tới). Field để trống hết, Jok tự kéo
// DoorController + Transform cửa thật vào.
public class DoorRelockTwistCutscene : MonoBehaviour
{
    [Tooltip("Đúng cửa vừa đi qua -- sẽ bị Close() + SetLocked(true) + SetForceJammed(true) (kẹt cứng vĩnh viễn).")]
    [SerializeField] private DoorController _door;
    [Tooltip("Điểm camera bị ép quay lại nhìn -- thường là chính vị trí cửa.")]
    [SerializeField] private Transform _lookTarget;

    [SerializeField] private float _delayBeforeSlam = 0.4f;
    [SerializeField] private float _turnDuration = 0.6f;
    [SerializeField] private AudioClip _slamSfx;
    [SerializeField] private float _holdAfterSlam = 0.8f;
    [SerializeField] private float _shakeDuration = 0.25f;
    [SerializeField] private float _shakeMagnitude = 0.05f;

    public UnityEvent OnTwistComplete;

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

        if (player != null) player.SetInputEnabled(false);

        yield return new WaitForSeconds(_delayBeforeSlam);

        if (player != null && cam != null && _lookTarget != null)
            yield return CutsceneCameraUtil.LookAt(player, cam.transform, _lookTarget, _turnDuration);

        if (_door != null)
        {
            _door.Close();
            _door.SetLocked(true);
            _door.SetForceJammed(true); // twist thật sự -- kẹt cứng vĩnh viễn, không còn đường lùi
        }
        if (_slamSfx != null) AudioManager.Instance?.PlaySFX(_slamSfx);

        if (cam != null && _shakeDuration > 0f)
            yield return ShakeCamera(cam.transform, _shakeDuration, _shakeMagnitude);

        yield return new WaitForSeconds(_holdAfterSlam);

        if (player != null) player.SetInputEnabled(true);
        OnTwistComplete?.Invoke();
    }

    private IEnumerator ShakeCamera(Transform camTransform, float duration, float magnitude)
    {
        Vector3 original = camTransform.localPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            camTransform.localPosition = original + Random.insideUnitSphere * magnitude;
            yield return null;
        }
        camTransform.localPosition = original;
    }
}
