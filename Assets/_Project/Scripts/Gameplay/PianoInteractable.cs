using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class PianoInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string[] _correctSequence;
    [SerializeField] private bool _isCompleted = false;
    public UnityEvent OnSequenceComplete;
    private List<string> _inputSequence = new List<string>();
    public void Interact() { }
    public void PressNote(string note)
    {
        _inputSequence.Add(note);
        if (_inputSequence.Count == _correctSequence.Length)
        {
            bool correct = true;
            for (int i = 0; i < _correctSequence.Length; i++)
                if (_inputSequence[i] != _correctSequence[i]) { correct = false; break; }
            if (correct) { _isCompleted = true; OnSequenceComplete?.Invoke(); }
            else _inputSequence.Clear();
        }
    }
}
