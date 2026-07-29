using System.Collections;
using UnityEngine;

// Jok yêu cầu 2026-07-30: "Nếu để sanity dưới 15% hay mốc đỏ cuối do không bật đèn pin sẽ dẫn tới chạy
// tuần tự 2 file này... nếu tới ngưỡng thì 10s chạy 1 lần 1 file audio." -- SFX_Sanity_Warning_01/02 phát
// XEN KẼ (không random) mỗi _repeatInterval giây trong lúc sanity dưới ngưỡng, tự dừng khi hồi lại trên
// ngưỡng (VD bật đèn pin lại). Gắn script này lên bất kỳ object nào trong scene (1 bản duy nhất).
public class SanityThresholdAudioLoop : MonoBehaviour
{
    [SerializeField] private SanitySystem _sanitySystem;

    [Tooltip("Sanity (0-1) dưới mức này thì bắt đầu lặp cảnh báo -- mặc định 0.15 = 15%.")]
    [SerializeField] private float _threshold = 0.15f;

    [Tooltip("SFX_Sanity_Warning_01/02 -- phát TUẦN TỰ xen kẽ 01,02,01,02... mỗi lần tới lượt, không random.")]
    [SerializeField] private AudioClip[] _warningClips;

    [SerializeField] private float _repeatInterval = 10f;

    private Coroutine _loopRoutine;
    private int _clipIndex = 0;

    private void Start()
    {
        if (_sanitySystem == null) _sanitySystem = SanitySystem.Instance;
    }

    private void Update()
    {
        if (_sanitySystem == null) return;

        bool belowThreshold = _sanitySystem.GetSanity() < _threshold;

        if (belowThreshold && _loopRoutine == null)
        {
            _loopRoutine = StartCoroutine(WarningLoop());
        }
        else if (!belowThreshold && _loopRoutine != null)
        {
            StopCoroutine(_loopRoutine);
            _loopRoutine = null;
        }
    }

    private IEnumerator WarningLoop()
    {
        while (true)
        {
            if (_warningClips != null && _warningClips.Length > 0)
            {
                AudioManager.Instance?.PlaySFX(_warningClips[_clipIndex % _warningClips.Length]);
                _clipIndex = (_clipIndex + 1) % _warningClips.Length;
            }
            yield return new WaitForSeconds(_repeatInterval);
        }
    }
}
