// Interface mỏng để DialogueUI (orchestrator) gọi thống nhất 2 kiểu view
// (SubtitleDialogueView / PopupDialogueView) mà không cần biết chi tiết bên trong.
public interface IDialogueView
{
    bool IsOpen { get; }
    bool IsTyping { get; }
    bool IsWaitingForNext { get; }

    void Open();
    void Close();

    // Bắt đầu hiện 1 dòng thoại (set speaker + chạy typewriter). onLineFinished được
    // gọi khi dòng đã đọc xong VÀ người chơi đã bấm confirm (không có choices ở view này).
    void PlayLine(DialogueLine line, System.Action onLineFinished);

    // Nhấn Space lúc đang gõ chữ — hiện hết ngay lập tức.
    void SkipTypewriter();

    // Nhấn Space lúc đã gõ xong — view tự quyết: gọi callback đã lưu từ PlayLine.
    void AdvanceOrSkip();
}
