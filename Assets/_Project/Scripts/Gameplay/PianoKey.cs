using UnityEngine;

/// <summary>
/// Gắn lên từng phím đàn (7 object: Si La Đô Rê Mi Fa Sol).
/// Khi InteractionSystem raycast hit vào phím → Interact() → gọi PianoInteractable.AddNote()
/// </summary>
public class PianoKey : MonoBehaviour, IInteractable
{
    [Header("Note của phím này")]
    [SerializeField] private string _note; // ví dụ: "Do", "Re", "Mi", "Fa", "Sol", "La", "Si"

    [Header("Piano chứa phím này")]
    [SerializeField] private PianoInteractable _piano;

    public void Interact()
    {
        if (_piano == null)
        {
            Debug.LogWarning($"[PianoKey] {gameObject.name} chưa gán _piano!");
            return;
        }

        Debug.Log($"[PianoKey] Nhấn phím: {_note}");
        _piano.AddNote(_note);
    }
}