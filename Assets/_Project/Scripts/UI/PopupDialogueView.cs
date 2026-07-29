using System.Collections;
using UnityEngine;
using TMPro;

// Kiểu Popup khung (nội tâm/ghi chú, không giọng) — dùng khi DialogueLine.hasVoice = false.
// Nền dimmed, đóng khung, dừng lại đọc. Không hỗ trợ choices (Jok xác nhận không cần lúc này).
//
// Bản copy độc lập của logic typewriter (không dùng chung class với SubtitleDialogueView) —
// cố ý trùng code, theo đúng yêu cầu "2 script quản lý khác nhau thật sự".
//
// ĐÃ ĐỔI CÁCH LÀM: xem comment tương ứng trong SubtitleDialogueView.cs -- dùng thẳng
// TMP_Text.maxVisibleCharacters thay vì tự tay chỉnh vertex, cho chắc chắn hiện đúng.
public class PopupDialogueView : MonoBehaviour, IDialogueView
{
    [Header("References")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI bodyText;
    public GameObject confirmArrow;

    [Header("Typewriter")]
    [Range(0.01f, 0.1f)]
    public float charDelay = 0.03f;

    [Header("SFX — CHỈ Popup (nội tâm/không giọng) mới có tiếng gõ chữ; Subtitle có voice thật nên không cần")]
    [Tooltip("AudioSource riêng để Play()/Stop() được theo ý (không dùng PlayOneShot vì cần cắt giữa chừng lúc skip)")]
    [SerializeField] private AudioSource typingBlipSource;
    [Tooltip("Tiếng xác nhận/qua dòng khi bấm Space")]
    [SerializeField] private AudioClip advanceClickSfx;

    [Header("Confirm Arrow Blink — hiện lâu, tắt ngắn (đỡ chói/giật hơn kiểu 50/50 cũ)")]
    [Tooltip("Thời gian HIỆN mỗi nhịp nháy")]
    [Range(0.2f, 3f)]
    public float blinkOnDuration = 2f;
    [Tooltip("Thời gian TẮT mỗi nhịp nháy")]
    [Range(0.1f, 1f)]
    public float blinkOffDuration = 0.5f;

    public bool IsOpen { get; private set; }
    public bool IsTyping { get; private set; }
    public bool IsWaitingForNext { get; private set; }

    System.Action _onLineFinished;
    DialogueLine _currentLine;

    Coroutine _typeRoutine;
    Coroutine _blinkRoutine;

    public void Open()
    {
        gameObject.SetActive(true);
        IsOpen = true;

        // Cùng bug/fix như SubtitleDialogueView.Open() -- xem comment bên đó.
        Canvas.ForceUpdateCanvases();
    }

    public void Close()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);
        StopTypingBlip();
        IsTyping = false;
        IsWaitingForNext = false;
        IsOpen = false;
        gameObject.SetActive(false);
    }

    public void PlayLine(DialogueLine line, System.Action onLineFinished)
    {
        _currentLine = line;
        _onLineFinished = onLineFinished;

        speakerText.text = line.speakerName;
        SetArrowVisible(false);
        IsWaitingForNext = false;

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        IsTyping = true;

        bodyText.text = line.text;
        bodyText.maxVisibleCharacters = 0;
        bodyText.ForceMeshUpdate();

        PlayTypingBlip();

        int totalChars = bodyText.textInfo.characterCount;
        for (int i = 1; i <= totalChars; i++)
        {
            bodyText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(charDelay);
        }

        StopTypingBlip();
        FinishLine();
    }

    public void SkipTypewriter()
    {
        if (!IsTyping) return;

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        StopTypingBlip(); // cắt ngay lập tức, không để tiếng rít kêu tiếp sau khi chữ đã hiện hết

        bodyText.text = _currentLine.text;
        bodyText.maxVisibleCharacters = int.MaxValue;
        bodyText.ForceMeshUpdate();

        FinishLine();
    }

    void PlayTypingBlip()
    {
        if (typingBlipSource == null) return;
        typingBlipSource.loop = true;
        typingBlipSource.Play();
    }

    void StopTypingBlip()
    {
        if (typingBlipSource == null) return;
        typingBlipSource.Stop();
    }

    void FinishLine()
    {
        IsTyping = false;
        SetArrowVisible(true);
        IsWaitingForNext = true;
    }

    public void AdvanceOrSkip()
    {
        if (advanceClickSfx != null) AudioManager.Instance?.PlaySFX(advanceClickSfx, 0.3f);

        if (IsTyping)
        {
            SkipTypewriter();
        }
        else if (IsWaitingForNext)
        {
            IsWaitingForNext = false;
            SetArrowVisible(false);
            _onLineFinished?.Invoke();
        }
    }

    void SetArrowVisible(bool show)
    {
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);

        if (!show)
        {
            confirmArrow.SetActive(false);
            return;
        }

        confirmArrow.SetActive(true);
        _blinkRoutine = StartCoroutine(BlinkArrow());
    }

    IEnumerator BlinkArrow()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(blinkOnDuration);
            confirmArrow.SetActive(false);
            yield return new WaitForSecondsRealtime(blinkOffDuration);
            confirmArrow.SetActive(true);
        }
    }
}
