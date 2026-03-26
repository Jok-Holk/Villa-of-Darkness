using UnityEngine;

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
            
            // Đã mở comment để counter hoạt động giúp trigger Ending
            GameData.audioLogsHeard++; 
            
            Debug.Log($"Phát Audio Log: {_logText}");
        }
    }
}