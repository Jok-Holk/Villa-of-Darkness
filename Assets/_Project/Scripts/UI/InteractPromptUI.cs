using UnityEngine;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [Tooltip("GameObject chứa tâm tròn trắng + chữ E — bật/tắt cả cụm cùng lúc.")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TextMeshProUGUI keyLabel;
    [SerializeField] private string defaultKeyText = "E";

    private void Awake()
    {
        Instance = this;
        if (keyLabel != null) keyLabel.text = defaultKeyText;
        Hide();
    }

    public void Show()
    {
        if (promptRoot != null) promptRoot.SetActive(true);
    }

    public void Hide()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }
}
