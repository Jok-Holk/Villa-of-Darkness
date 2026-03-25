using UnityEngine;

namespace Assets._Project.Scripts.Audio
{
    // Giả sử IInteractable đã được định nghĩa sẵn trong Project
    public class AudioLogItem : MonoBehaviour, IInteractable 
    {
        [SerializeField] private AudioClip _logClip;
        [SerializeField] private string _logText;
        private bool _hasBeenHeard = false;

        public void Interact()
        {
            if (_hasBeenHeard) return;

            if (_logClip != null)
            {
                AudioManager.Instance.PlaySFX(_logClip);
                _hasBeenHeard = true;
                
                // Giả sử GameData là một static class quản lý dữ liệu toàn cục
                // GameData.audioLogsHeard++; 
                Debug.Log($"Phát Audio Log: {_logText}");
            }
        }
    }

    public interface IInteractable { void Interact(); }
}