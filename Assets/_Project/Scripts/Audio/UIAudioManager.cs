using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Tự động gắn tiếng click vào MỌI Button trong scene (không cần wire tay từng cái).
/// Quét lại mỗi lần load scene mới vì Button khác nhau giữa các scene.
/// 2 clip chạy xen kẽ (round-robin) mỗi lần bấm để đỡ lặp một tiếng y hệt liên tục.
/// </summary>
public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }

    [SerializeField] private AudioClip[] clickClips;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.6f;

    private int _clipIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null); // DontDestroyOnLoad chỉ chạy được với root object -- object này có thể đang
        // nằm dưới divider "= MANAGERS" trong Hierarchy (chỉ để gọn lúc edit), tự tách ra root lúc runtime.
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += (scene, mode) => HookAllButtons();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start() => HookAllButtons();

    private void HookAllButtons()
    {
        var buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            btn.onClick.RemoveListener(PlayClick); // tránh gắn trùng nếu quét lại
            btn.onClick.AddListener(PlayClick);
        }
    }

    public void PlayClick()
    {
        if (clickClips == null || clickClips.Length == 0) return;
        var clip = clickClips[_clipIndex % clickClips.Length];
        _clipIndex++;
        AudioManager.Instance?.PlaySFX(clip, volume);
    }
}
