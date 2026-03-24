using UnityEngine;

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private bool _isVisible = false;
    public void Show(string name, string years) { _isVisible = true; }
    public void Hide() { _isVisible = false; }
}
