using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame() { GameManager.Instance?.LoadChapter(1); }
    public void QuitGame()  { Application.Quit(); }
}
