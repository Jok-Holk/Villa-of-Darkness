using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private DeathScreenUI _deathScreenUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Tìm lại DeathScreenUI trong scene mới (kể cả đang inactive)
        _deathScreenUI = FindObjectOfType<DeathScreenUI>(true);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void PlayerDead(string characterName = "Minh Khoa", string characterYears = "1979 – 2000")
    {
        Debug.Log("Player died");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (_deathScreenUI != null)
            _deathScreenUI.Show(characterName, characterYears);
        else
            Debug.LogWarning("[GameManager] Không tìm thấy DeathScreenUI trong scene!");
    }

    public void PlayerRespawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadChapter(int chapterNumber)
    {
        GameData.currentChapter = chapterNumber;
        //SceneManager.LoadScene("Chapter" + chapterNumber);
    }

    public void LoadMainMenu()
    {
        GameData.Reset();
        //SceneManager.LoadScene("MainMenu");
    }

    public void DebugMessage(string msg)
    {
        Debug.Log(msg);
    }
}