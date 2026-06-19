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

        // ĐÃ SỬA DÒNG 25: Dùng FindFirstObjectByType kết hợp FindObjectsInactive.Include để tìm Object đang ẩn chuẩn URP mới
        _deathScreenUI = FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
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
        // ĐÃ SỬA DÒNG 40: Đồng bộ cách tìm kiếm mới khi load scene
        _deathScreenUI = FindFirstObjectByType<DeathScreenUI>(FindObjectsInactive.Include);
        Debug.Log($"[GameManager] Scene loaded: {scene.name} | DeathScreenUI: {(_deathScreenUI != null ? "OK" : "NULL")}");
    }

<<<<<<< HEAD
=======
    public void PlayerDead()
    {
        PlayerDead("Minh Khoa", "1979 – 2000");
    }

>>>>>>> c004a115986b3015959c8e75b2857ea8b7879cc2
    public void PlayerDead(string characterName = "Minh Khoa", string characterYears = "1979 – 2000")
    {
        Debug.Log("Player died");
        Time.timeScale   = 0f;  

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
        Time.timeScale   = 1f;   
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadChapter(int chapterNumber)
    {
        GameData.currentChapter = chapterNumber;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Chapter" + chapterNumber);
    }

    public void LoadMainMenu()
    {
        GameData.Reset();
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void DebugMessage(string msg)
    {
        Debug.Log(msg);
    }
}