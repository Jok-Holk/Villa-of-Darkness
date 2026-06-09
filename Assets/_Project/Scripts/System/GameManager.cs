using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private DeathScreenUI _deathScreenUI;
    private ChapterTransition _chapterTransition;

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

        _deathScreenUI = FindObjectOfType<DeathScreenUI>(true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _deathScreenUI = FindObjectOfType<DeathScreenUI>(true);
        _chapterTransition = FindObjectOfType<ChapterTransition>(true);

        Debug.Log($"[GameManager] Scene loaded: {scene.name} | ChapterTransition: {(_chapterTransition != null ? "OK" : "NULL")}");

        // Khi vừa load vào TestMenu → tự động play transition
        if (scene.name == "TestMenu" && GameData.currentChapter > 0)
        {
            if (_chapterTransition != null)
            {
                _chapterTransition.PlayTransition(
                    "Chương 1 – Căn Nhà Của Ký Ức",
                    "Biệt Thự Gia Đình Đặng · 1965–2000"
                );
            }
            else
            {
                Debug.LogWarning("[GameManager] Không tìm thấy ChapterTransition trong TestMenu!");
            }
        }
    }

    public void PlayerDead(string characterName = "Minh Khoa", string characterYears = "1979 – 2000")
    {
        Debug.Log("Player died");
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_deathScreenUI != null)
            _deathScreenUI.Show(characterName, characterYears);
        else
            Debug.LogWarning("[GameManager] Không tìm thấy DeathScreenUI trong scene!");
    }

    public void PlayerRespawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadChapter(int chapterNumber)
    {
        GameData.currentChapter = chapterNumber;
        SceneManager.LoadScene("TestMenu"); // load scene → OnSceneLoaded sẽ tự play transition
    }

    public void LoadMainMenu()
    {
        GameData.Reset();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void DebugMessage(string msg)
    {
        Debug.Log(msg);
    }
}