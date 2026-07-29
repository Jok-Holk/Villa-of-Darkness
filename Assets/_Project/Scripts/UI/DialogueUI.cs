using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// DialogueLine / DialogueAsset đã tách ra DialogueAsset.cs (file riêng đúng tên class --
// Unity không tự resolve MonoScript cho ScriptableObject tạo bằng CreateInstance khi
// class không trùng tên file chứa nó, khiến m_Script serialize ra {fileID: 0} và asset
// không hiện trong Object Picker dù vẫn đọc/ghi field bình thường qua YAML).

// ─────────────────────────────────────────────────────────
// UI ORCHESTRATOR — quản lý state/thứ tự dòng thoại, KHÔNG tự vẽ.
// Mỗi dòng thoại giao thẳng cho đúng 1 trong 2 view (Subtitle/Popup) theo line.hasVoice —
// mỗi view tự có bản copy logic typewriter riêng (xem SubtitleDialogueView / PopupDialogueView).
// ─────────────────────────────────────────────────────────

public class DialogueUI : MonoBehaviour
{
    // Singleton theo đúng pattern GameManager/AudioManager/PlayerController đã dùng khắp codebase —
    // cho phép bất kỳ script nào (IntroManager, trigger mới...) gọi DialogueUI.Instance.StartDialogue(asset)
    // thẳng, khỏi phải kéo tay reference vào từng DialogueTrigger như trước.
    public static DialogueUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("References — panel gốc, luôn active khi có hội thoại đang chạy")]
    public GameObject dialoguePanel;

    [Header("Views — kéo đúng component implement IDialogueView (SubtitleDialogueView / PopupDialogueView)")]
    [Tooltip("Dùng khi DialogueLine.hasVoice = true")]
    [SerializeField] MonoBehaviour subtitleViewComp;
    [Tooltip("Dùng khi DialogueLine.hasVoice = false")]
    [SerializeField] MonoBehaviour popupViewComp;

    IDialogueView SubtitleView => subtitleViewComp as IDialogueView;
    IDialogueView PopupView => popupViewComp as IDialogueView;

    [Header("Events")]
    public UnityEvent OnDialogueEnd;

    [Header("HUD ẩn khi có hội thoại đang chạy — VD: thanh Stamina, icon pin đèn")]
    [SerializeField] private GameObject[] hudToHideDuringDialogue;

    // ── State ──────────────────────────────────────────────
    DialogueAsset _asset;
    int _lineIndex;
    IDialogueView _activeView;

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
        SetHudVisible(false);

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

        _activeView?.AdvanceOrSkip();
    }

    public void CloseDialogue()
    {
        AudioManager.Instance?.StopVoice();
        _activeView?.Close();
        _activeView = null;
        _asset = null;
        _lineIndex = -1;
        dialoguePanel.SetActive(false);
        SetHudVisible(true);

        Time.timeScale = 1f;
        PlayerController.Instance?.SetInputEnabled(true);
        InteractionSystem.IsInputBlocked = false;
    }

    private void SetHudVisible(bool visible)
    {
        if (hudToHideDuringDialogue == null) return;
        foreach (var go in hudToHideDuringDialogue)
            if (go != null) go.SetActive(visible);
    }

    // ─────────────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────────────

    void Update()
    {
        if (_asset == null || !dialoguePanel.activeSelf || _activeView == null) return;

        bool pressAdvance = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
        if (pressAdvance)
        {
            _activeView.AdvanceOrSkip();
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
        IDialogueView nextView = line.hasVoice ? SubtitleView : PopupView;

        if (_activeView != null && _activeView != nextView)
            _activeView.Close();

        _activeView = nextView;
        _activeView.Open();
        _activeView.PlayLine(line, AdvanceLine);

        // Play() trên cùng AudioSource tự cắt tiếng dòng trước nếu còn đang phát dở khi chuyển dòng nhanh
        // -- không cần tự Stop() tay trước khi gọi PlayVoice() dòng mới.
        if (line.hasVoice && line.voiceClip != null)
            AudioManager.Instance?.PlayVoice(line.voiceClip);
        else
            AudioManager.Instance?.StopVoice();
    }
}
