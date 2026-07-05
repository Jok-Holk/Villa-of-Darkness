using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gắn vào Camera trong scene MainMenu.
///
/// Mô hình: danh sách các CẢNH (Scene). Mỗi cảnh có 1 điểm BẮT ĐẦU và 1 điểm KẾT THÚC riêng —
/// camera bay MƯỢT (Lerp/Slerp) từ Start sang End trong đúng Travel Duration mong muốn của cảnh đó.
/// KHÔNG có khoảng đứng chờ chết giữa chừng — Fade đen được CHỒNG (overlap) lên đúng đoạn CUỐI
/// của quãng bay (fade ra đen trong lúc vẫn đang bay nốt đoạn cuối), rồi cắt thẳng sang điểm Bắt đầu
/// của cảnh kế tiếp và Fade sáng chồng lên đúng đoạn ĐẦU của quãng bay cảnh đó — liền mạch, không giật.
/// Hết danh sách thì quay vòng lại cảnh đầu nếu Loop=true.
/// </summary>
public class MainMenuCameraFlythrough : MonoBehaviour
{
    [System.Serializable]
    public class FlythroughScene
    {
        public string label = "Cảnh";

        [Header("Điểm BẮT ĐẦU của cảnh")]
        public Vector3 startPosition;
        public Vector3 startRotation;

        [Header("Điểm KẾT THÚC của cảnh")]
        public Vector3 endPosition;
        public Vector3 endRotation;

        [Header("Thời gian (giây)")]
        [Tooltip("Thời gian BAY MƯỢT từ điểm Bắt đầu sang điểm Kết thúc của CẢNH NÀY (tốc độ tự khớp theo khoảng cách)")]
        public float travelDuration = 5f;
        [Tooltip("Thời gian fade đen — CHỒNG lên đúng đoạn CUỐI của quãng bay cảnh này (không có khoảng đứng chờ chết)")]
        public float fadeDuration = 1.5f;
    }

    [Tooltip("Danh sách CẢNH theo thứ tự phát. Mỗi cảnh tự có Start/End riêng, KHÔNG liên quan cảnh khác. Dùng nút +/- để thêm/bớt cảnh.")]
    [SerializeField]
    private List<FlythroughScene> _scenes = new List<FlythroughScene>
    {
        new FlythroughScene
        {
            label = "Cảnh 1 — Đường mòn",
            startPosition = new Vector3(70f, -30f, -30.75f),
            startRotation = new Vector3(0f, -90f, 0f),
            endPosition = new Vector3(70f, -30f, -30.75f),
            endRotation = new Vector3(0f, -90f, 0f),
            travelDuration = 6f,
            fadeDuration = 1.5f,
        },
        new FlythroughScene
        {
            label = "Cảnh 2 — Cam ngang khu rừng",
            startPosition = new Vector3(70f, -30f, -30.75f),
            startRotation = Vector3.zero,
            endPosition = new Vector3(70f, -30f, -30.75f),
            endRotation = Vector3.zero,
            travelDuration = 6f,
            fadeDuration = 1.5f,
        },
    };

    [Tooltip("Hết danh sách thì quay vòng lại Cảnh 1")]
    [SerializeField]
    private bool _loop = true;

    [SerializeField]
    private bool _playOnEnable = true;

    [Tooltip("Dùng ease in/out khi bay mượt trong 1 cảnh, thay vì tốc độ đều tăm tắp")]
    [SerializeField]
    private bool _useEaseInOut = true;

    private Coroutine _routine;

    private void OnEnable()
    {
        if (_playOnEnable)
            Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        if (_scenes == null || _scenes.Count == 0) return;
        Stop();
        transform.position = _scenes[0].startPosition;
        transform.rotation = Quaternion.Euler(_scenes[0].startRotation);
        _routine = StartCoroutine(RunFlythrough());
    }

    public void Stop()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    /// <summary>Tạm dừng (dùng khi mở Settings — camera đứng im tại chỗ, không bay/fade tiếp).</summary>
    public void Pause() => Stop();

    /// <summary>Tiếp tục vòng lặp từ Cảnh 1 (dùng khi đóng Settings).</summary>
    public void Resume() => Play();

    private IEnumerator RunFlythrough()
    {
        int i = 0;
        bool needFadeInAtStart = false;

        while (true)
        {
            var scene = _scenes[i];
            transform.position = scene.startPosition;
            transform.rotation = Quaternion.Euler(scene.startRotation);

            var fader = ScreenFader.Instance;
            if (needFadeInAtStart && fader != null)
                fader.FadeIn(Mathf.Max(0.05f, scene.fadeDuration) * 0.5f);

            yield return FlyWithTailFade(scene);
            needFadeInAtStart = true;

            int nextIndex = i + 1;
            bool isLast = nextIndex >= _scenes.Count;
            if (isLast)
            {
                if (!_loop) yield break;
                nextIndex = 0;
            }
            i = nextIndex;
        }
    }

    /// <summary>Bay mượt Start→End; khi còn lại đúng nửa fadeDuration cuối quãng đường thì bắt đầu Fade đen chồng lên.</summary>
    private IEnumerator FlyWithTailFade(FlythroughScene scene)
    {
        float travelDuration = Mathf.Max(0.01f, scene.travelDuration);
        float fadeHalf = Mathf.Max(0.05f, scene.fadeDuration) * 0.5f;
        float fadeStartTime = Mathf.Max(0f, travelDuration - fadeHalf);

        Vector3 startPos = scene.startPosition;
        Quaternion startRot = Quaternion.Euler(scene.startRotation);
        Vector3 endPos = scene.endPosition;
        Quaternion endRot = Quaternion.Euler(scene.endRotation);

        var fader = ScreenFader.Instance;
        bool fadeTriggered = false;

        float t = 0f;
        while (t < travelDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / travelDuration);
            float easedU = _useEaseInOut ? u * u * (3f - 2f * u) : u;

            transform.position = Vector3.Lerp(startPos, endPos, easedU);
            transform.rotation = Quaternion.Slerp(startRot, endRot, easedU);

            if (!fadeTriggered && t >= fadeStartTime && fader != null)
            {
                fadeTriggered = true;
                fader.FadeOut(fadeHalf);
            }

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        // Đảm bảo màn hình đã đen hẳn trước khi cắt sang cảnh kế (fade vừa trigger có thể chưa kịp chạy xong khung hình cuối).
        if (fadeTriggered)
            yield return new WaitForSeconds(0.02f);
        else if (fader != null)
        {
            fader.FadeOut(0.05f);
            yield return new WaitForSeconds(0.05f);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Đưa Camera Tới Điểm Bắt Đầu Cảnh 1 (Preview)")]
    private void PreviewFirstStart()
    {
        if (_scenes == null || _scenes.Count == 0) return;
        transform.position = _scenes[0].startPosition;
        transform.rotation = Quaternion.Euler(_scenes[0].startRotation);
    }

    private void OnDrawGizmosSelected()
    {
        if (_scenes == null) return;
        foreach (var scene in _scenes)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(scene.startPosition, 0.3f);
            Vector3 startForward = Quaternion.Euler(scene.startRotation) * Vector3.forward;
            Gizmos.DrawLine(scene.startPosition, scene.startPosition + startForward * 2f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(scene.endPosition, 0.3f);
            Vector3 endForward = Quaternion.Euler(scene.endRotation) * Vector3.forward;
            Gizmos.DrawLine(scene.endPosition, scene.endPosition + endForward * 2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(scene.startPosition, scene.endPosition);
        }
    }
#endif
}
