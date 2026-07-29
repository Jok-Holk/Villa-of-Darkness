using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

// Hộp âm nhạc đồng (Thư Phòng) -- E: phát 2 tiếng lên dây chậm rãi (Startup_01 rồi Startup_02, cách nhau
// 1 nhịp), sau đó phát NGUYÊN VẸN 1 file băng ghi âm bà Lan (KHÔNG cắt -- có nhạc nền lồng theo suốt, cắt
// ra sẽ mất tự nhiên) kèm phụ đề tự chạy theo mốc thời gian đã canh sẵn trong "_captions" -- KHÔNG cần bấm
// Space, khác hẳn DialogueUI thường (đây là phụ đề kiểu phim, không phải hội thoại tương tác).
// Nghe xong -> unlock (KHÔNG tự mở) cửa Thư Phòng phía Salon + item Hộp Âm Nhạc trở thành nhặt được.
[RequireComponent(typeof(AudioSource))]
public class MusicBoxInteractable : MonoBehaviour, IInteractable, IInteractableLabel
{
    [SerializeField] private string _interactLabel = "Hộp âm nhạc đồng";

    [Header("Lên dây -- 2 tiếng phát chậm rãi, KHÔNG chồng lên nhau")]
    [SerializeField] private AudioClip _startupSfx01;
    [SerializeField] private AudioClip _startupSfx02;
    [SerializeField] private float _delayBetweenStartupSfx = 1.2f;
    [SerializeField] private float _delayBeforeTape = 1f;

    [Header("Băng ghi âm -- 1 file liền mạch, KHÔNG cắt (có nhạc nền lồng theo)")]
    [SerializeField] private AudioClip _tapeClip;
    [Tooltip("Mốc thời gian (giây) mỗi dòng phụ đề BẮT ĐẦU hiện -- ước lượng ban đầu theo tỉ lệ độ dài câu, Jok tự vừa nghe vừa chỉnh lại cho khớp tai nghe thật, KHÔNG cần chính xác tuyệt đối.")]
    [SerializeField] private TapeCaption[] _captions;

    [Header("Trước khi mở -- 1 câu suy nghĩ tò mò ngắn (\"thứ gì đây?\") TRƯỚC KHI lên dây/nghe băng, KHÔNG phải sau")]
    [SerializeField] private DialogueAsset _curiousThought;

    [Header("Sau khi nghe xong")]
    [Tooltip("Cửa Thư Phòng phía Salon -- unlock im lặng, KHÔNG tự mở, giống piano (Player tự đi tới bấm E)")]
    [SerializeField] private DoorController _doorThuPhongSalon;
    [Tooltip("PickupItem trên CÙNG GameObject này (Reset() tự tìm) -- bắt đầu tắt sẵn, chỉ bật SAU KHI nghe hết băng để nhặt hộp (\"Khoa gấp hộp lại, bỏ vào túi\"). Tự tắt luôn component này để tránh 2 IInteractable cùng tranh nhau trên 1 object.")]
    [SerializeField] private PickupItem _pickupItemAfterListening;
    public UnityEvent OnTapeFinished;

    [Tooltip("Bắn NGAY lúc bắt đầu tương tác (trước cả lên dây) -- dùng cho Chapter1Scene3Manager khoá tạm " +
             "2 cửa Thư Phòng ngay lập tức, KHÔNG đợi nghe xong băng.")]
    public UnityEvent OnSequenceStarted;

    private AudioSource _audioSource;
    private bool _hasPlayed = false;

    public string InteractLabel => _interactLabel;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_pickupItemAfterListening != null) _pickupItemAfterListening.enabled = false;

        // THÊM 2026-07-27: Nhớ đã nghe băng xong theo checkpoint -- tránh Retry (chết) sau khi đã nghe xong
        // bắt nghe lại từ đầu. Đăng ký ở Awake() vì CheckpointManager.Restore() chạy trước Start().
        string id = "MusicBox." + CheckpointManager.GetHierarchyPath(transform);
        CheckpointManager.RegisterFlag(id, () => _hasPlayed, ApplyCheckpointState);
    }

    // Áp lại trạng thái đã nghe/chưa nghe NGAY LẬP TỨC (không phát lại audio/phụ đề) -- dùng riêng cho
    // checkpoint restore, khác hẳn Interact() bình thường của người chơi.
    private void ApplyCheckpointState(bool hasPlayed)
    {
        _hasPlayed = hasPlayed;

        if (_pickupItemAfterListening != null) _pickupItemAfterListening.enabled = hasPlayed;
        this.enabled = !hasPlayed; // đã nghe xong -> tắt hẳn MusicBox interact, để PickupItem một mình xử lý raycast
    }

    public void Interact()
    {
        if (_hasPlayed) return;
        _hasPlayed = true;

        // SỬA 2026-07-27 (Jok chỉnh lại đúng thứ tự): thoại tò mò ("thứ gì đây?") phải chạy TRƯỚC khi lên
        // dây/nghe băng, không phải sau khi nghe xong -- tự nhiên hơn (thấy vật lạ -> tò mò -> thử mở nghe
        // -> nghe xong mới soi kỹ + nhặt).
        if (_curiousThought != null && DialogueUI.Instance != null)
            StartCoroutine(RunCuriousThoughtThenSequence());
        else
            StartCoroutine(RunSequence());
    }

    private IEnumerator RunCuriousThoughtThenSequence()
    {
        DialogueUI.Instance.StartDialogue(_curiousThought);
        while (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen())
            yield return null;

        yield return StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        PlayerController.Instance?.SetInputEnabled(false);
        OnSequenceStarted?.Invoke();

        if (_startupSfx01 != null)
        {
            AudioManager.Instance?.PlaySFX(_startupSfx01);
            yield return new WaitForSeconds(_delayBetweenStartupSfx);
        }
        if (_startupSfx02 != null)
        {
            AudioManager.Instance?.PlaySFX(_startupSfx02);
            yield return new WaitForSeconds(_delayBeforeTape);
        }

        yield return StartCoroutine(PlayTapeWithCaptions());

        if (_doorThuPhongSalon != null) _doorThuPhongSalon.SetLocked(false);
        if (_pickupItemAfterListening != null)
        {
            _pickupItemAfterListening.enabled = true;
            this.enabled = false; // tránh 2 IInteractable (MusicBox + Pickup) cùng tranh nhau trên raycast

            // THÊM 2026-07-27: Nghe băng xong -> TỰ mở luôn chuỗi "nêu cảm nghĩ -> soi 360 độ -> nhặt" của
            // hộp âm nhạc, KHÔNG bắt Player phải tự bấm E lại lần nữa. PickupItem._requireExamineFirst đã
            // có sẵn cơ chế này (dùng chung với các item lore khác) -- chỉ cần gọi Interact() hộ Player.
            // KHÔNG SetInputEnabled(true) trước dòng này -- ExamineItem/DialogueUI tự quản lý input riêng.
            _pickupItemAfterListening.Interact();
        }
        else
        {
            PlayerController.Instance?.SetInputEnabled(true);
        }

        OnTapeFinished?.Invoke();
    }

    private IEnumerator PlayTapeWithCaptions()
    {
        if (_tapeClip == null) yield break;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject captionGO = null;
        TextMeshProUGUI captionText = null;
        if (canvas != null)
        {
            captionGO = new GameObject("TapeCaption");
            captionGO.transform.SetParent(canvas.transform, false);
            captionText = captionGO.AddComponent<TextMeshProUGUI>();
            captionText.alignment = TextAlignmentOptions.Center;
            captionText.fontSize = 32;
            captionText.color = new Color(0.92f, 0.9f, 0.85f);
            var rt = captionText.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.12f);
            rt.anchorMax = new Vector2(0.5f, 0.12f);
            rt.sizeDelta = new Vector2(1400, 220);
            rt.anchoredPosition = Vector2.zero;
        }

        _audioSource.clip = _tapeClip;
        _audioSource.loop = false;
        _audioSource.Play();

        int captionIndex = -1;
        while (_audioSource.isPlaying)
        {
            float t = _audioSource.time;
            while (captionIndex < _captions.Length - 1 && t >= _captions[captionIndex + 1].startTime)
                captionIndex++;

            if (captionText != null)
                captionText.text = (captionIndex >= 0 && captionIndex < _captions.Length) ? _captions[captionIndex].text : "";

            yield return null;
        }

        if (captionGO != null) Object.Destroy(captionGO);
    }
}

[System.Serializable]
public struct TapeCaption
{
    public float startTime;
    [TextArea(1, 3)] public string text;
}
