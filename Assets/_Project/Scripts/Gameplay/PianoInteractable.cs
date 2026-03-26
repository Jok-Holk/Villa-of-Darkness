using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Phase1.VoVanThuan
{
    public class PianoInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string[] _correctSequence;
        private List<string> _inputSequence = new List<string>();
        private bool _isCompleted = false;

        public UnityEvent OnSequenceComplete = new UnityEvent();

        public void Interact() { /* Logic hiển thị UI piano nếu cần */ }

        public void PressNote(string note)
        {
            if (_isCompleted) return;

            // Kiểm tra nốt hiện tại có đúng vị trí trong chuỗi không
            if (_correctSequence != null && 
                _inputSequence.Count < _correctSequence.Length && 
                note == _correctSequence[_inputSequence.Count])
            {
                _inputSequence.Add(note);
                if (_inputSequence.Count == _correctSequence.Length)
                {
                    _isCompleted = true;
                    OnSequenceComplete.Invoke();
                }
            }
            else
            {
                _inputSequence.Clear(); // Sai một nốt là reset cả chuỗi
            }
        }
    }
}