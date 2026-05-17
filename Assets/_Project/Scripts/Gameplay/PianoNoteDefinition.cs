using UnityEngine;

/// <summary>
/// ScriptableObject quản lý danh sách note hợp lệ của piano.
/// Tạo: Assets → chuột phải → Create → Piano → Note Definition
/// Kéo vào PianoKey và PianoInteractable để dùng chung 1 nguồn dữ liệu.
/// </summary>
[CreateAssetMenu(fileName = "PianoNoteDefinition", menuName = "Piano/Note Definition")]
public class PianoNoteDefinition : ScriptableObject
{
    [Tooltip("Danh sách tên note hợp lệ — thêm/bớt ở đây, PianoKey và PianoInteractable tự cập nhật")]
    public string[] notes = { "Do", "Re", "Mi", "Fa", "Sol", "La", "Si" };
}