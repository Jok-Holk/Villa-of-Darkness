using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private bool _isPaused = false;
    [SerializeField] private GameObject _pauseMenuPanel;
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
        OnResume.Invoke();
    }

    public void Toggle()
    {
        if (_isPaused) Resume();
        else           Pause();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        ExamineItem activeExamine = FindActiveExamine();
        if (activeExamine != null && activeExamine.IsExamining)
        {
            // BUG FIX 1: Không gọi StopExamine() trực tiếp từ PauseMenu nữa.
            // ExamineItem.Update() đã xử lý Esc → E không phải Esc, nhưng nếu
            // ta muốn Esc cũng thoát examine, cần kiểm tra context:
            // - Nếu examine từ inventory → delegate về InventoryUI để close đúng
            // - Nếu examine độc lập → StopExamine bình thường
            //
            // Cách đơn giản nhất: tìm InventoryUI. Nếu IsExamining → đóng inventory
            // (Close() bên trong sẽ stop examine). Nếu không → StopExamine trực tiếp.
            InventoryUI invUI = Object.FindFirstObjectByType<InventoryUI>();
            if (invUI != null && invUI.IsExamining)
            {
                // Inventory đang giữ examine → đóng cả hai qua Close()
                invUI.Close();
            }
            else
            {
                // Examine độc lập từ scene → stop trực tiếp
                activeExamine.StopExamine();
            }
            return;
        }

        Toggle();
    }

    /// <summary>Tìm ExamineItem đang active trong scene.</summary>
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