using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public UnityEvent OnMenuOpen = new UnityEvent();
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    public void StartGame()
    {
        // Bắt đầu lượt chơi mới — xóa toàn bộ tiến trình cũ (item, chapter, audio log)
        GameData.Reset();

        // Xóa luôn save trên đĩa (PlayerPrefs) để ItemPersistence.Awake() không Load() lại item cũ
        ItemPersistence persistence = Object.FindFirstObjectByType<ItemPersistence>();
        if (persistence != null)
            persistence.DeleteSave();
        else
            PlayerPrefs.DeleteAll(); // fallback nếu không có ItemPersistence trong scene MainMenu

        GameManager.Instance?.LoadChapter(1);
        SceneManager.LoadScene("Chapter1");
    }
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
    public void QuitGame()  { Application.Quit(); }
    public void Show() { gameObject.SetActive(true); OnMenuOpen.Invoke(); }
    public void Hide() { gameObject.SetActive(false); }
}