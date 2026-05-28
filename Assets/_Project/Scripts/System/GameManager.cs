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
            return; // duplicate → không làm gì thêm, đặc biệt KHÔNG đăng ký sceneLoaded
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Chỉ đăng ký 1 lần duy nhất trên instance hợp lệ
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Tìm ngay lần đầu (scene hiện tại lúc game start)
        _deathScreenUI = FindObjectOfType<DeathScreenUI>(true);
    }

    private void OnDestroy()
    {
        // Chỉ unsubscribe nếu đây là instance hợp lệ
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _deathScreenUI = FindObjectOfType<DeathScreenUI>(true);
        Debug.Log($"[GameManager] Scene loaded: {scene.name} | DeathScreenUI: {(_deathScreenUI != null ? "OK" : "NULL")}");
    }

    public void PlayerDead(string characterName = "Minh Khoa", string characterYears = "1979 – 2000")
    {
        Debug.Log("Player died");
        Time.timeScale   = 0f;  // tạm dừng game

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
        Time.timeScale   = 1f;   // chạy lại game
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