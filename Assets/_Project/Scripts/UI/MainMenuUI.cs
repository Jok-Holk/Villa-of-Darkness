using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public UnityEvent OnMenuOpen = new UnityEvent();
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    public void StartGame() { GameManager.Instance?.LoadChapter(1);
        SceneManager.LoadScene("TestMenu"); // đổi tên đúng scene của bạn
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
