using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PianoInteractable : MonoBehaviour, IInteractable
{
    [Header("Note Definition — kéo ScriptableObject vào để dùng dropdown")]
    [SerializeField] private PianoNoteDefinition _noteDefinition;

    [Header("Sequence đúng — mỗi element chọn từ dropdown")]
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

    public UnityEvent OnSequenceComplete = new UnityEvent();

    public void Interact()
    {
        if (_isCompleted) { Debug.Log("[Piano] Đã hoàn thành rồi."); return; }
        Debug.Log("[Piano] Hãy nhấn vào từng phím đàn theo thứ tự đúng.");
    }

    public void AddNote(string note)
    {
        if (_isCompleted) return;
        if (string.IsNullOrEmpty(note)) return;
        if (_correctSequence == null || _correctSequence.Length == 0) return;

        // Frame guard đã được xử lý tại PianoKey.Interact() — không cần guard ở đây
        // để tránh double-block khi reset sau note sai.

        bool noteIsCorrect = _inputSequence.Count < _correctSequence.Length
                             && note == _correctSequence[_inputSequence.Count];

        if (noteIsCorrect)
        {
            _inputSequence.Add(note);
            PlaySFX(_correctNoteClip);

            string progress = string.Join(" → ", _inputSequence);
            Debug.Log($"[Piano] ✔ {note} | {progress} ({_inputSequence.Count}/{_correctSequence.Length})");

            if (_inputSequence.Count == _correctSequence.Length)
                CompleteSequence();
        }
        else
        {
            PlaySFX(_wrongNoteClip);
            string nextExpected = _correctSequence[0];
            Debug.Log($"[Piano] ✘ Sai: [{note}] — reset! Bắt đầu lại từ [{nextExpected}]");

            _inputSequence.Clear();
        }
    }

    public void PressNote(string note) => AddNote(note);

    private void CompleteSequence()
    {
        _isCompleted = true;
        PlaySFX(_sequenceCompleteClip);    
        Debug.Log("[Piano] ✔✔✔ PIANO DONE!");
        OnSequenceComplete.Invoke();

        if (_spawnManager != null && _ghostPrefab != null)
        {
            if (_ghostSpawnPoint != null)
                _spawnManager.SpawnAtTransform(_ghostPrefab, _ghostSpawnPoint); // dùng đúng overload Transform
            else
                _spawnManager.SpawnAt(_ghostPrefab, transform.position + transform.forward * 2f);
            Debug.Log("[Piano] Ghost spawned!");
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(clip);
    }
}