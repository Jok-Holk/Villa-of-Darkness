using System.Collections;
using UnityEngine;
using TMPro;

// Kiểu Phụ đề (VO) — dùng khi DialogueLine.hasVoice = true. Tinh giản, không khung, nền mờ,
// full-width đáy màn hình. Không hỗ trợ choices (Jok xác nhận không cần lúc này).
//
// Bản copy độc lập của logic typewriter (không dùng chung class với PopupDialogueView) —
// cố ý trùng code, theo đúng yêu cầu "2 script quản lý khác nhau thật sự".
//
// ĐÃ ĐỔI CÁCH LÀM: bản cũ tự tay chỉnh alpha/scale từng vertex ký tự (fade+pop) qua
// UpdateVertexData -- sau nhiều vòng chữ vẫn không lên hình đúng dù đã thử fix nhiều hướng khác
// nhau (font, thời điểm rebuild Canvas...). Chuyển sang dùng thẳng TMP_Text.maxVisibleCharacters
// (cơ chế typewriter CHÍNH THỨC, có sẵn của TMP, Unity tự lo mọi thứ nội bộ) -- đánh đổi mất hiệu
// ứng phồng-to-rồi-co-lại mỗi ký tự, nhưng chắc chắn hiện đúng. Có thể thêm lại hiệu ứng đẹp sau.
public class SubtitleDialogueView : MonoBehaviour, IDialogueView
{
    [Header("References")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI bodyText;
    public GameObject confirmArrow;

    [Header("Typewriter")]
    [Range(0.01f, 0.1f)]
    public float charDelay = 0.03f;

    [Header("SFX")]
    [Tooltip("Tiếng xác nhận/qua dòng khi bấm Space -- KHÔNG có tiếng gõ chữ ở đây vì Subtitle dùng cho line có voice thật, để dành cho PopupDialogueView")]
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

        // GameObject vừa active NGAY DÒNG TRÊN -- Canvas chưa kịp dựng layout lần đầu cho ĐÚNG
        // component này, nên bản render đầu của TypeLine không lên hình kịp (đứng hình tới khi có
        // sự kiện khác ép re-render, VD bấm space). Ép rebuild Canvas ngay bây giờ -- SAU khi active,
        // TRƯỚC khi PlayLine/TypeLine bắt đầu gõ.
        Canvas.ForceUpdateCanvases();
    }

    public void Close()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);
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

        int totalChars = bodyText.textInfo.characterCount;
        for (int i = 1; i <= totalChars; i++)
        {
            bodyText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(charDelay);
        }

        FinishLine();
    }

    public void SkipTypewriter()
    {
        if (!IsTyping) return;

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);

        bodyText.text = _currentLine.text;
        bodyText.maxVisibleCharacters = int.MaxValue;
        bodyText.ForceMeshUpdate();

        FinishLine();
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
