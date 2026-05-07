using UnityEngine;

/// <summary>
/// Gắn lên từng phím đàn (7 object).
/// _note được chọn từ dropdown trong Inspector (nhờ PianoKeyEditor)
/// thay vì gõ tay — tránh nhập sai tên note.
/// </summary>
public class PianoKey : MonoBehaviour, IInteractable
{
    [Header("Note Definition — kéo ScriptableObject vào để dùng dropdown")]
    [SerializeField] private PianoNoteDefinition _noteDefinition;

    [Header("Note của phím này")]
    [SerializeField] private string _note;

    [Header("Piano chứa phím này")]
    [SerializeField] private PianoInteractable _piano;

    // Frame guard — chỉ ở PianoKey, không ảnh hưởng test PianoInteractable
    private int _lastFrame = -1;

    public string Note => _note;

    public void Interact()
    {
        if (_piano == null)
        {
            Debug.LogWarning($"[PianoKey] {gameObject.name} chưa gán _piano!");
            return;
        }

        if (Time.frameCount == _lastFrame) return;
        _lastFrame = Time.frameCount;

        Debug.Log($"[PianoKey] Nhấn phím: {_note}");
        _piano.AddNote(_note);
    }
}