using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private bool _isPaused = false;
    [SerializeField] private GameObject _pauseMenuPanel;
    [SerializeField] private GameObject _settingsPanel; // NEW: gán panel Settings trong Inspector

    public UnityEvent OnPause  = new UnityEvent();
    public UnityEvent OnResume = new UnityEvent();

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;
        Time.timeScale = 0f;
        _pauseMenuPanel.SetActive(true);
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;

        // Ẩn HUD gameplay (crosshair + prompt tương tác, 2 vạch Thể lực/Đèn pin) -- trước đây KHÔNG có
        // dòng nào ẩn HUD lúc Pause/Settings cả, HUD đứng đè lên menu tạm dừng suốt (kể cả Settings vì
        // Settings chỉ lồng bên trong trạng thái đã Pause() sẵn, không cần xử lý riêng).
        InteractPromptUI.Instance?.SetDotVisible(false);
        HudMetersUI.Instance.SetVisible(false);

        OnPause.Invoke();
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;
        Time.timeScale = 1f;
        _pauseMenuPanel.SetActive(false);
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.Locked;

        InteractPromptUI.Instance?.SetDotVisible(true);
        HudMetersUI.Instance.SetVisible(true);

        OnResume.Invoke();
    }

    public void Toggle()
    {
        if (_isPaused) Resume();
        else           Pause();
    }

    // NEW: mở Settings — gọi từ nút Settings trong pause menu
    public void OpenSettings()
    {
        if (_settingsPanel == null) return;
        _pauseMenuPanel.SetActive(false);
        _settingsPanel.SetActive(true);
    }

    // NEW: đóng Settings, quay lại pause menu
    public void CloseSettings()
    {
        if (_settingsPanel == null) return;
        _settingsPanel.SetActive(false);
        _pauseMenuPanel.SetActive(true);
    }

    private bool IsSettingsOpen => _settingsPanel != null && _settingsPanel.activeSelf;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // NEW: nếu Settings đang mở → Esc chỉ đóng Settings, KHÔNG toggle pause menu
        if (IsSettingsOpen)
        {
            CloseSettings();
            return;
        }

        ExamineItem activeExamine = FindActiveExamine();
        if (activeExamine != null && activeExamine.IsExamining)
        {
            InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
            if (invUI != null && invUI.IsExamining)
            {
                invUI.Close();
            }
            else
            {
                activeExamine.StopExamine();
            }
            return;
        }

        Toggle();
    }

    private ExamineItem FindActiveExamine()
    {
        return Object.FindFirstObjectByType<ExamineItem>();
    }

    public void GoToMainMenu()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("MainMenu");
    }
}