using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PianoInteractable : MonoBehaviour, IInteractable
{
    [Header("Sequence đúng — điền tên note khớp với PianoKey._note")]
    [SerializeField] private string[] _correctSequence;

    [Header("Sound")]
    [SerializeField] private AudioClip _correctNoteClip;
    [SerializeField] private AudioClip _wrongNoteClip;
    [SerializeField] private AudioClip _sequenceCompleteClip;

    [Header("Ghost Spawn")]
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private GameObject   _ghostPrefab;
    [SerializeField] private Transform    _ghostSpawnPoint;

    private List<string> _inputSequence = new List<string>();
    private bool         _isCompleted   = false;

    // Chống gọi AddNote 2 lần trong cùng 1 frame
    private int _lastNoteFrame = -1;

    public UnityEvent OnSequenceComplete = new UnityEvent();

    // ─── IInteractable ─────────────────────────────────────────────────────────
    public void Interact()
    {
        if (_isCompleted)
        {
            Debug.Log("[Piano] Đã hoàn thành rồi.");
            return;
        }
        Debug.Log("[Piano] Hãy nhấn vào từng phím đàn theo thứ tự đúng.");
    }

    // ─── NHẬN NOTE TỪ PIANO KEY ────────────────────────────────────────────────
    public void AddNote(string note)
    {
        if (_isCompleted) return;
        if (string.IsNullOrEmpty(note)) return;
        if (_correctSequence == null || _correctSequence.Length == 0) return;

        // Chặn gọi 2 lần trong cùng 1 frame
        if (Time.frameCount == _lastNoteFrame) return;
        _lastNoteFrame = Time.frameCount;

        bool noteIsCorrect = _inputSequence.Count < _correctSequence.Length
                             && note == _correctSequence[_inputSequence.Count];

        if (noteIsCorrect)
        {
            _inputSequence.Add(note);
            PlaySFX(_correctNoteClip);
            Debug.Log($"[Piano] ✔ Đúng: {note} ({_inputSequence.Count}/{_correctSequence.Length})");

            if (_inputSequence.Count == _correctSequence.Length)
                CompleteSequence();
        }
        else
        {
            _inputSequence.Clear();
            PlaySFX(_wrongNoteClip);
            Debug.Log($"[Piano] ✘ Sai: {note} — reset, nhập lại từ đầu.");
        }
    }

    public void PressNote(string note) => AddNote(note);

    // ─── COMPLETE ──────────────────────────────────────────────────────────────
    private void CompleteSequence()
    {
        _isCompleted = true;

        PlaySFX(_sequenceCompleteClip);
        Debug.Log("[Piano] Piano done!");
        OnSequenceComplete.Invoke();

        if (_spawnManager != null && _ghostPrefab != null)
        {
            if (_ghostSpawnPoint != null)
                _spawnManager.SpawnAt(_ghostPrefab, _ghostSpawnPoint);
            else
                _spawnManager.SpawnAt(_ghostPrefab, transform.position + transform.forward * 2f);

            Debug.Log("[Piano] Ghost spawned!");
        }
    }

    // ─── HELPER ────────────────────────────────────────────────────────────────
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip);
    }
}