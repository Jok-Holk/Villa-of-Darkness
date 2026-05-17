using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayerDead()
    {
        // TODO: trigger death screen UI trước khi load
        Debug.Log("Player died");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void LoadChapter(int chapterNumber)
    {
        GameData.currentChapter = chapterNumber;
        // SceneManager.LoadScene("Chapter" + chapterNumber);
    }

    public void LoadMainMenu()
    {
        GameData.Reset();
        // SceneManager.LoadScene("MainMenu");
    }
    public void DebugMessage(string msg)
{
    Debug.Log(msg);
}
}