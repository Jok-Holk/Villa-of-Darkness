using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// ─────────────────────────────────────────────────────────
// DATA
// ─────────────────────────────────────────────────────────

[Serializable]
public class DialogueChoice
{
    public string text;
}

[Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 6)]
    public string text;
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Tooltip("Có giọng lồng tiếng (VO) → hiện dạng phụ đề tinh giản. Không tick → nội tâm/ghi chú, hiện dạng popup khung, dừng lại đọc.")]
    public bool hasVoice = true;

    public bool HasChoices => choices != null && choices.Count > 0;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}

// ─────────────────────────────────────────────────────────
// UI CONTROLLER
// ─────────────────────────────────────────────────────────

public class DialogueUI : MonoBehaviour
{
    [Header("References — panel gốc, luôn active khi có hội thoại đang chạy")]
    public GameObject dialoguePanel;

    [Header("Kiểu Phụ đề (VO) — dùng khi DialogueLine.hasVoice = true. Tinh giản, không khung, nền mờ.")]
    public GameObject subtitleView;
    public TextMeshProUGUI subtitleSpeakerText;
    public TextMeshProUGUI subtitleBodyText;
    public GameObject subtitleConfirmArrow;

    [Header("Kiểu Popup khung (nội tâm/ghi chú, không giọng) — hasVoice = false. Nền dimmed, đóng khung, dừng lại đọc.")]
    public GameObject popupView;
    public TextMeshProUGUI popupSpeakerText;
    public TextMeshProUGUI popupBodyText;
    public GameObject popupConfirmArrow;

    [Header("Choices — dùng chung 1 container, tự chuyển sang view đang active")]
    public Transform choiceContainer;        // parent của các dòng choice
    public GameObject choicePrefab;          // prefab: 1 TextMeshProUGUI

    // Gán lại mỗi dòng theo view đang active (xem SetActiveView) — không wire tay trong Inspector.
    TextMeshProUGUI speakerText;
    TextMeshProUGUI bodyText;
    GameObject confirmArrow;

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
    [Tooltip("Thời gian 1 chu kỳ nhấp nháy (giây)")]
    [Range(0.2f, 2f)]
    public float blinkInterval = 0.6f;

    [Header("Events")]
    public UnityEvent OnDialogueEnd;
    [Tooltip("Fired with the 0-based index of the selected choice")]
    public UnityEvent<int> OnChoiceSelected;

    // ── State ──────────────────────────────────────────────
    DialogueAsset _asset;
    int _lineIndex;
    int _choiceIndex;
    bool _typing;
    bool _waitingForNext;
    bool _waitingForChoice;
    Coroutine _typeRoutine;
    Coroutine _blinkRoutine;
    List<TextMeshProUGUI> _choiceRows = new List<TextMeshProUGUI>();

    // Pool các dòng choice — tái sử dụng GameObject thay vì Instantiate/Destroy mỗi lần đổi node thoại.
    List<GameObject> _choicePool = new List<GameObject>();

    // ── Input mapping ──────────────────────────────────────
    // Works with both old and new Input System via polling.
    bool PressAdvance()  => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
    bool PressUp()       => Input.GetKeyDown(KeyCode.W)     || Input.GetKeyDown(KeyCode.UpArrow);
    bool PressDown()     => Input.GetKeyDown(KeyCode.S)     || Input.GetKeyDown(KeyCode.DownArrow);

    // ─────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────

    public void StartDialogue(DialogueAsset asset)
    {
        if (asset == null)
        {
            Debug.LogWarning("DialogueUI: Không thể bắt đầu hội thoại vì DialogueAsset bị null.");
            return;
        }

        _asset      = asset;
        _lineIndex  = -1;
        dialoguePanel.SetActive(true);

        // Thoại = pop-up chặn hẳn gameplay (khác Inventory — Tab không pause).
        // Đứng nói chuyện không nên bị ma bắt/mất tập trung giữa chừng.
        Time.timeScale = 0f;
        PlayerController.Instance?.SetInputEnabled(false);
        InteractionSystem.IsInputBlocked = true;

        AdvanceLine();
    }

    public bool IsDialogueOpen() => dialoguePanel != null && dialoguePanel.activeSelf;

    public void AdvanceOrStartDialogue(DialogueAsset asset)
    {
        if (asset == null)
        {
            Debug.LogWarning("DialogueUI: Không thể tiến hành hội thoại vì DialogueAsset bị null.");
            return;
        }

        if (!IsDialogueOpen() || _asset != asset)
        {
            StartDialogue(asset);
            return;
        }

        if (_typing)
        {
            SkipTypewriter();
        }
        else if (_waitingForChoice)
        {
            ConfirmChoice();
        }
        else
        {
            AdvanceLine();
        }
    }

    public void CloseDialogue()
    {
        StopAllCoroutines();
        _asset = null;
        _lineIndex = -1;
        _typing = false;
        _waitingForNext = false;
        _waitingForChoice = false;
        dialoguePanel.SetActive(false);
        ClearChoices();
        SetArrowVisible(false);

        Time.timeScale = 1f;
        PlayerController.Instance?.SetInputEnabled(true);
        InteractionSystem.IsInputBlocked = false;
    }

    // ─────────────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────────────

    void Update()
    {
        if (_asset == null || !dialoguePanel.activeSelf) return;

        if (PressAdvance())
        {
            if (_typing)
            {
                SkipTypewriter();
            }
            else if (_waitingForChoice)
            {
                ConfirmChoice();
            }
            else if (_waitingForNext)
            {
                AdvanceLine();
            }
        }

        if (_waitingForChoice)
        {
            if (PressUp())   NavigateChoice(-1);
            if (PressDown()) NavigateChoice(+1);
        }
    }

    // ─────────────────────────────────────────────────────
    // LINE FLOW
    // ─────────────────────────────────────────────────────

    void AdvanceLine()
    {
        _lineIndex++;

        if (_lineIndex >= _asset.lines.Count)
        {
            CloseDialogue();
            OnDialogueEnd?.Invoke();
            return;
        }

        var line = _asset.lines[_lineIndex];
        SetActiveView(line.hasVoice);
        speakerText.text = line.speakerName;
        SetArrowVisible(false);
        ClearChoices();

        _waitingForNext   = false;
        _waitingForChoice = false;

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeLine(line));
    }

    // Chuyển đổi giữa 2 kiểu hiển thị theo DialogueLine.hasVoice — cho phép 1 cuộc hội
    // thoại trộn cả dòng có giọng (phụ đề) và dòng nội tâm/ghi chú (popup khung).
    void SetActiveView(bool hasVoice)
    {
        subtitleView.SetActive(hasVoice);
        popupView.SetActive(!hasVoice);

        speakerText  = hasVoice ? subtitleSpeakerText  : popupSpeakerText;
        bodyText     = hasVoice ? subtitleBodyText     : popupBodyText;
        confirmArrow = hasVoice ? subtitleConfirmArrow : popupConfirmArrow;

        Transform targetParent = hasVoice ? subtitleView.transform : popupView.transform;
        if (choiceContainer.parent != targetParent)
            choiceContainer.SetParent(targetParent, false);
    }

    // ─────────────────────────────────────────────────────
    // TYPEWRITER — fade + pop per character (TMP vertex anim)
    // ─────────────────────────────────────────────────────
    //
    // Cách hoạt động:
    // 1. Gán toàn bộ text vào bodyText NGAY LẬP TỨC (để TMP build layout/word-wrap đúng),
    //    nhưng set alpha = 0 cho mọi ký tự (ẩn hết).
    // 2. Mỗi charDelay giây, "mở khóa" thêm 1 ký tự mới: ký tự đó bắt đầu 1 tween riêng
    //    (chạy song song, không chặn vòng lặp) gồm:
    //      - alpha: 0 → 255 (fade in)
    //      - scale: 0 → popOvershoot → 1 (pop in, hơi phồng quá rồi co lại)
    // 3. Vì nhiều ký tự có thể đang tween cùng lúc (ký tự sau bắt đầu trước khi ký tự
    //    trước animation xong), ta lưu danh sách "active reveals" và update tất cả
    //    mỗi frame trong 1 coroutine phụ (AnimateRevealedChars).

    class CharReveal
    {
        public int charIndex;
        public float startTime;
    }

    List<CharReveal> _activeReveals = new List<CharReveal>();
    Coroutine _vertexAnimRoutine;

    IEnumerator TypeLine(DialogueLine line)
    {
        _typing = true;
        _activeReveals.Clear();

        // Set toàn bộ text ngay để TMP tính layout/wrap, rồi ẩn hết ký tự (alpha 0).
        bodyText.text = line.text;
        bodyText.ForceMeshUpdate();
        HideAllCharacters();

        // Coroutine phụ chạy mỗi frame để animate các ký tự đang trong quá trình reveal.
        if (_vertexAnimRoutine != null) StopCoroutine(_vertexAnimRoutine);
        _vertexAnimRoutine = StartCoroutine(AnimateRevealedChars());

        int totalChars = bodyText.textInfo.characterCount;
        for (int i = 0; i < totalChars; i++)
        {
            // Bỏ qua khoảng trắng/ký tự không hiển thị (TMP vẫn tính vào characterCount).
            if (!bodyText.textInfo.characterInfo[i].isVisible)
                continue;

            _activeReveals.Add(new CharReveal { charIndex = i, startTime = Time.unscaledTime });
            yield return new WaitForSecondsRealtime(charDelay);
        }

        // Đợi animation của ký tự cuối cùng hoàn tất trước khi coi như "xong".
        yield return new WaitForSecondsRealtime(charRevealDuration);

        FinishLine(line);
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
        bodyText.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.All);
    }

    IEnumerator AnimateRevealedChars()
    {
        // Chạy liên tục trong lúc đang typing; tự dừng khi TypeLine gọi FinishLine
        // (coroutine bị Stop ở SkipTypewriter / AdvanceLine tiếp theo).
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

                // Fade: tuyến tính là đủ, mắt người không phân biệt được easing trên alpha.
                float alpha = Mathf.Clamp01(t * 1.6f); // alpha xong sớm hơn scale 1 chút, cảm giác "nét" hơn

                // Pop: overshoot rồi co lại — dùng easing back-out đơn giản viết tay (không cần DOTween).
                float scale = EvaluatePopScale(t);

                SetCharAlpha(reveal.charIndex, alpha);
                SetCharScale(reveal.charIndex, scale);
                dirty = true;
            }

            if (dirty)
                bodyText.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.All);

            yield return null;
        }
    }

    // Easing back-out viết tay: 0 → vượt quá popOvershoot ở giữa → 1.
    // Tránh phụ thuộc thư viện tween ngoài (DOTween/LitMotion) để script chạy được ngay,
    // không cần cài thêm package.
    float EvaluatePopScale(float t)
    {
        if (t < 0.6f)
        {
            // Giai đoạn 1: 0 → overshoot, nhanh
            float local = t / 0.6f;
            return Mathf.Lerp(0f, popOvershoot, EaseOutQuad(local));
        }
        else
        {
            // Giai đoạn 2: overshoot → 1, chậm lại, "lắng" xuống đúng kích thước
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

        // Tâm scale = điểm giữa baseline của ký tự (để chữ phồng to/nhỏ quanh chính nó,
        // không bị lệch vị trí khi scale != 1).
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

    void SkipTypewriter()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        if (_vertexAnimRoutine != null) StopCoroutine(_vertexAnimRoutine);
        _activeReveals.Clear();

        var line = _asset.lines[_lineIndex];
        // Đảm bảo text đúng và hiện toàn bộ ký tự ở alpha=1, scale=1 ngay lập tức.
        bodyText.text = line.text;
        bodyText.ForceMeshUpdate();

        var textInfo = bodyText.textInfo;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
            SetCharAlpha(i, 1f);
            SetCharScale(i, 1f);
        }
        bodyText.UpdateVertexData(TMPro.TMP_VertexDataUpdateFlags.All);

        FinishLine(line);
    }

    void FinishLine(DialogueLine line)
    {
        _typing = false;
        if (_vertexAnimRoutine != null) StopCoroutine(_vertexAnimRoutine);

        if (line.HasChoices)
        {
            BuildChoices(line.choices);
            _waitingForChoice = true;
        }
        else
        {
            SetArrowVisible(true);
            _waitingForNext = true;
        }
    }

    // ─────────────────────────────────────────────────────
    // CONFIRM ARROW — nhấp nháy (blink)
    // ─────────────────────────────────────────────────────

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
        // Toggle SetActive theo nhịp — đơn giản, hiệu suất tốt, đúng với hiệu ứng "nhấp nháy"
        // (không cần fade mượt cho icon mũi tên, ảnh tham khảo cũng là nhấp nháy dạng on/off).
        while (true)
        {
            yield return new WaitForSecondsRealtime(blinkInterval * 0.5f);
            confirmArrow.SetActive(!confirmArrow.activeSelf);
        }
    }

    // ─────────────────────────────────────────────────────
    // CHOICES
    // ─────────────────────────────────────────────────────

    void BuildChoices(List<DialogueChoice> choices)
    {
        ClearChoices();
        _choiceIndex = 0;

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject go;
            if (i < _choicePool.Count)
            {
                go = _choicePool[i];
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(choicePrefab, choiceContainer);
                _choicePool.Add(go);
            }

            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            _choiceRows.Add(tmp);

            // Pop-in nhẹ cho cả dòng choice khi nó xuất hiện (dùng scale RectTransform,
            // không cần animate ký tự — choices xuất hiện cùng lúc nên hiệu ứng theo dòng là đủ).
            StartCoroutine(PopInRow(go.transform, i * 0.05f));
        }

        RefreshChoiceHighlight(choices);
    }

    IEnumerator PopInRow(Transform row, float extraDelay)
    {
        row.localScale = Vector3.zero;
        if (extraDelay > 0f) yield return new WaitForSecondsRealtime(extraDelay);

        float duration = 0.15f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float s = EaseOutQuad(Mathf.Clamp01(t / duration));
            row.localScale = Vector3.one * s;
            yield return null;
        }
        row.localScale = Vector3.one;
    }

    void NavigateChoice(int delta)
    {
        _choiceIndex = Mathf.Clamp(
            _choiceIndex + delta, 0,
            _asset.lines[_lineIndex].choices.Count - 1);
        RefreshChoiceHighlight(_asset.lines[_lineIndex].choices);
    }

    void RefreshChoiceHighlight(List<DialogueChoice> choices)
    {
        for (int i = 0; i < _choiceRows.Count; i++)
        {
            string prefix = (i == _choiceIndex) ? "▶ " : "  ";
            _choiceRows[i].text = prefix + choices[i].text;
            _choiceRows[i].color = (i == _choiceIndex)
                ? Color.white
                : new Color(0.6f, 0.6f, 0.6f);
        }
    }

    void ConfirmChoice()
    {
        _waitingForChoice = false;
        int selected = _choiceIndex;
        OnChoiceSelected?.Invoke(selected);
        AdvanceLine();
    }

    void ClearChoices()
    {
        // Ẩn thay vì Destroy — pool giữ lại để dùng cho lượt thoại kế tiếp (tránh Instantiate/Destroy liên tục).
        foreach (var go in _choicePool)
            go.SetActive(false);
        _choiceRows.Clear();
    }
}