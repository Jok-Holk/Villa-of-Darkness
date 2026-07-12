using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Kiểu Popup khung (nội tâm/ghi chú, không giọng) — dùng khi DialogueLine.hasVoice = false.
// Nền dimmed, đóng khung, dừng lại đọc. Không hỗ trợ choices (Jok xác nhận không cần lúc này).
//
// Bản copy độc lập của logic typewriter (không dùng chung class với SubtitleDialogueView) —
// cố ý trùng code, theo đúng yêu cầu "2 script quản lý khác nhau thật sự".
public class PopupDialogueView : MonoBehaviour, IDialogueView
{
    [Header("References")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI bodyText;
    public GameObject confirmArrow;

    [Header("Typewriter")]
    [Range(0.01f, 0.1f)]
    public float charDelay = 0.03f;
    [Tooltip("Thời gian (giây) để 1 ký tự hoàn thành fade-in + pop-in")]
    [Range(0.05f, 0.5f)]
    public float charRevealDuration = 0.18f;
    [Tooltip("Ký tự bắt đầu phồng to bao nhiêu lần kích thước gốc trước khi co lại đúng size")]
    [Range(1f, 2f)]
    public float popOvershoot = 1.35f;

    [Header("Confirm Arrow Blink")]
    [Range(0.2f, 2f)]
    public float blinkInterval = 0.6f;

    public bool IsOpen { get; private set; }
    public bool IsTyping { get; private set; }
    public bool IsWaitingForNext { get; private set; }

    System.Action _onLineFinished;
    DialogueLine _currentLine;

    Coroutine _typeRoutine;
    Coroutine _vertexAnimRoutine;
    Coroutine _blinkRoutine;

    class CharReveal
    {
        public int charIndex;
        public float startTime;
    }
    List<CharReveal> _activeReveals = new List<CharReveal>();

    public void Open()
    {
        gameObject.SetActive(true);
        IsOpen = true;
    }

    public void Close()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        if (_vertexAnimRoutine != null) StopCoroutine(_vertexAnimRoutine);
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);
        _activeReveals.Clear();
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
        _activeReveals.Clear();

        bodyText.text = line.text;
        bodyText.ForceMeshUpdate();
        HideAllCharacters();

        if (_vertexAnimRoutine != null) StopCoroutine(_vertexAnimRoutine);
        _vertexAnimRoutine = StartCoroutine(AnimateRevealedChars());

        int totalChars = bodyText.textInfo.characterCount;
        for (int i = 0; i < totalChars; i++)
        {
            if (!bodyText.textInfo.characterInfo[i].isVisible)
                continue;

            _activeReveals.Add(new CharReveal { charIndex = i, startTime = Time.unscaledTime });
            yield return new WaitForSecondsRealtime(charDelay);
        }

        yield return new WaitForSecondsRealtime(charRevealDuration);

        FinishLine();
    }

    void HideAllCharacters()
    {
        var textInfo = bodyText.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
            SetCharAlpha(i, 0f);
            SetCharScale(i, 0f);
        }
        bodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    IEnumerator AnimateRevealedChars()
    {
        while (true)
        {
            bool dirty = false;

            for (int r = _activeReveals.Count - 1; r >= 0; r--)
            {
                var reveal = _activeReveals[r];
                float t = (Time.unscaledTime - reveal.startTime) / charRevealDuration;

                if (t >= 1f)
                {
                    SetCharAlpha(reveal.charIndex, 1f);
                    SetCharScale(reveal.charIndex, 1f);
                    _activeReveals.RemoveAt(r);
                    dirty = true;
                    continue;
                }

                float alpha = Mathf.Clamp01(t * 1.6f);
                float scale = EvaluatePopScale(t);

                SetCharAlpha(reveal.charIndex, alpha);
                SetCharScale(reveal.charIndex, scale);
                dirty = true;
            }

            if (dirty)
                bodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            yield return null;
        }
    }

    float EvaluatePopScale(float t)
    {
        if (t < 0.6f)
        {
            float local = t / 0.6f;
            return Mathf.Lerp(0f, popOvershoot, EaseOutQuad(local));
        }
        else
        {
            float local = (t - 0.6f) / 0.4f;
            return Mathf.Lerp(popOvershoot, 1f, EaseOutQuad(local));
        }
    }

    float EaseOutQuad(float x) => 1f - (1f - x) * (1f - x);

    void SetCharAlpha(int charIndex, float alpha01)
    {
        var textInfo = bodyText.textInfo;
        if (charIndex < 0 || charIndex >= textInfo.characterCount) return;

        var charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex   = charInfo.vertexIndex;
        Color32[] colors  = textInfo.meshInfo[materialIndex].colors32;
        byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha01) * 255f);

        for (int v = 0; v < 4; v++)
        {
            Color32 c = colors[vertexIndex + v];
            c.a = a;
            colors[vertexIndex + v] = c;
        }
    }

    void SetCharScale(int charIndex, float scale)
    {
        var textInfo = bodyText.textInfo;
        if (charIndex < 0 || charIndex >= textInfo.characterCount) return;

        var charInfo = textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex   = charInfo.vertexIndex;
        Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

        Vector3 bl = vertices[vertexIndex + 0];
        Vector3 tl = vertices[vertexIndex + 1];
        Vector3 tr = vertices[vertexIndex + 2];
        Vector3 br = vertices[vertexIndex + 3];

        Vector3 center = (bl + tl + tr + br) / 4f;

        vertices[vertexIndex + 0] = center + (bl - center) * scale;
        vertices[vertexIndex + 1] = center + (tl - center) * scale;
        vertices[vertexIndex + 2] = center + (tr - center) * scale;
        vertices[vertexIndex + 3] = center + (br - center) * scale;
    }

    public void SkipTypewriter()
    {
        if (!IsTyping) return;

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        if (_vertexAnimRoutine != null) StopCoroutine(_vertexAnimRoutine);
        _activeReveals.Clear();

        bodyText.text = _currentLine.text;
        bodyText.ForceMeshUpdate();

        var textInfo = bodyText.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
            SetCharAlpha(i, 1f);
            SetCharScale(i, 1f);
        }
        bodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

        FinishLine();
    }

    void FinishLine()
    {
        IsTyping = false;
        if (_vertexAnimRoutine != null) StopCoroutine(_vertexAnimRoutine);

        SetArrowVisible(true);
        IsWaitingForNext = true;
    }

    public void AdvanceOrSkip()
    {
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
            yield return new WaitForSecondsRealtime(blinkInterval * 0.5f);
            confirmArrow.SetActive(!confirmArrow.activeSelf);
        }
    }
}
