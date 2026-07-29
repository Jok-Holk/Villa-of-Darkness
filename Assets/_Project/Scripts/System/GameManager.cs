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
        transform.SetParent(null); // DontDestroyOnLoad chỉ chạy được với root object -- object này có thể đang
        // nằm dưới divider "= MANAGERS" trong Hierarchy (chỉ để gọn lúc edit), tự tách ra root lúc runtime.
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

        // Retry (PlayerRespawn) load lại scene — nếu đã có checkpoint thì đưa Player về đúng vị trí đó
        // thay vì vị trí spawn mặc định của scene. Chưa có checkpoint nào thì bỏ qua, giữ hành vi cũ.
        if (_pendingCheckpointRestore && CheckpointManager.HasCheckpoint && PlayerController.Instance != null)
        {
            // revertInventory: true -- chết (Retry) mất hết đồ nhặt SAU checkpoint gần nhất, đúng yêu cầu
            // "đồ đạc sẽ reset nếu chết" (trước đây mặc định giữ nguyên toàn bộ, không revert).
            CheckpointManager.Restore(PlayerController.Instance.transform, revertInventory: true);
        }
        _pendingCheckpointRestore = false;
    }

    private bool _pendingCheckpointRestore = false;

    public void PlayerDead()
    {
        PlayerDead("Nguyễn Minh Khoa", "1979 – 2000");
    }

    public void PlayerDead(string characterName = "Nguyễn Minh Khoa", string characterYears = "1979 – 2000")
    {
        Debug.Log("Player died");
        Time.timeScale   = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Time.timeScale=0 chặn được di chuyển (Move dùng Time.deltaTime) nhưng KHÔNG chặn xoay camera
        // bằng chuột (Input.GetAxis không phụ thuộc deltaTime) — cần tắt input tay như DialogueUI đang làm.
        PlayerController.Instance?.SetInputEnabled(false);

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

        _pendingCheckpointRestore = true;
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
        CheckpointManager.Clear(); // về Main Menu = coi như bỏ ván đang chơi -- không thì "New Game" lần
        // sau vẫn thấy HasCheckpoint=true, IntroManager tưởng đã qua cổng nên bỏ qua cinematic sai.
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