using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public UnityEvent OnMenuOpen = new UnityEvent();

    public void StartGame() { GameManager.Instance?.LoadChapter(1);
        SceneManager.LoadScene("TestMenu"); // đổi tên đúng scene của bạn
    }
    public void QuitGame()  { Application.Quit(); }
    public void Show() { gameObject.SetActive(true); OnMenuOpen.Invoke(); }
    public void Hide() { gameObject.SetActive(false); }
}
