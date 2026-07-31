using UnityEngine;
using UnityEngine.Events;

// Quản lý trung tâm cảnh 3 (Chapter 1) -- theo kịch bản Jok nêu 2026-07-30:
// 1) Tương tác Hộp Âm Nhạc -- NGAY LẬP TỨC khoá tạm 2 cửa Thư Phòng (LockStudyDoorsImmediately, wire vào
//    MusicBoxInteractable.OnSequenceStarted).
// 2) Nghe băng xong -- cửa Thư Phòng->Hành Lang TỰ MỞ (không chỉ unlock như hành vi cũ), cửa Hành Lang->
//    Salon bị khoá + đóng lại (OnMusicBoxFinished, wire vào MusicBoxInteractable.OnTapeFinished).
// 3) ActivateScene3() -- tắt các object tương tác được không còn liên quan, force khoá cứng (kẹt, không mở
//    được dù có chìa) các cửa sân sau chỉ định, mở khoá đúng 1 cửa ngoại lệ (Salon->Hành Lang Phụ).
//
// TẤT CẢ field đều để trống/kéo tay trong Inspector -- không hardcode tên object nào (an toàn, không đoán
// mù cấu trúc scene thật). Đây là "khung" -- Jok tự kéo đúng DoorController/GameObject thật vào từng ô.
public class Chapter1Scene3Manager : MonoBehaviour
{
    public static Chapter1Scene3Manager Instance { get; private set; }
    public static bool IsActive { get; private set; }

    [Header("1) Ngay lúc bắt đầu tương tác Hộp Âm Nhạc -- khoá tạm 2 cửa Thư Phòng")]
    [Tooltip("2 cửa của Thư Phòng (VD Thư Phòng->Hành Lang, Thư Phòng->Salon...) -- đóng + khoá NGAY khi Player bắt đầu nghe.")]
    [SerializeField] private DoorController[] _studyDoorsToLockImmediately;

    [Header("2) Sau khi nghe băng xong")]
    [Tooltip("Cửa Thư Phòng -> Hành Lang -- TỰ MỞ (Open()), không chỉ unlock.")]
    [SerializeField] private DoorController _studyToHallwayDoor;
    [Tooltip("Cửa Hành Lang -> Salon -- đóng lại + khoá (không kẹt cứng, chỉ khoá bình thường).")]
    [SerializeField] private DoorController _hallwayToSalonDoor;

    [Header("3) Kích hoạt cảnh 3 (gọi sau bước 2, hoặc tự gọi thủ công để test)")]
    [Tooltip("Mấy object tương tác được không còn liên quan tới cảnh 3 -- tắt hẳn (SetActive(false)), BẤT KỂ trạng thái cảnh 2 để lại.")]
    [SerializeField] private GameObject[] _interactablesToDisable;

    [Tooltip("Chiều ngược lại của mảng trên -- object CHỈ xuất hiện/tương tác được TỪ cảnh 3 trở đi (VD tranh " +
             "ảnh/prop đổi khác đi khi vào cảnh 3) -- ép BẬT (SetActive(true)), BẤT KỂ trạng thái cảnh 2 để lại. " +
             "Nếu cần đổi cả HÌNH DẠNG (không chỉ bật/tắt) thì dùng ModelSwap.cs (đã có, xem Gameplay/ModelSwap.cs) " +
             "gắn trên chính object đó, rồi kéo object CHA của nó vào 1 trong 2 mảng này để ép chạy đúng lúc.")]
    [SerializeField] private GameObject[] _interactablesToEnable;

    [Tooltip("Cửa sân sau bị KẸT CỨNG (dù có chìa đúng cũng không mở) -- VD Phòng Ăn -> Hành Lang Sau.")]
    [SerializeField] private DoorController[] _backyardDoorsToJam;

    [Tooltip("Cửa NGOẠI LỆ vẫn mở được bình thường trong 3 cửa sân sau -- VD Salon -> Hành Lang Phụ. Đóng " +
             "lại lúc vào cảnh 3 nhưng KHÔNG kẹt cứng, Player tự mở khoá lại bằng chìa như thường.")]
    [SerializeField] private DoorController _backyardUnlockableDoor;

    [Tooltip("Cửa chính (đã khoá cứng từ cảnh 1) -- không cần đổi gì (itemId trống sẵn = không mở được), " +
             "chỉ liệt kê ở đây nếu Jok muốn ActivateScene3() tự double-check SetLocked(true) cho chắc.")]
    [SerializeField] private DoorController _mainDoorReconfirmLocked;

    [Header("Nhạc nền căng thẳng -- tự phát khi ActivateScene3(), tự tắt lúc WalkToWellCutscene ra tới giếng (StopBGM)")]
    [SerializeField] private AudioClip _chaseBgm;

    [Header("Sự kiện phụ -- wire thêm đổi patrol ghost / v.v.")]
    public UnityEvent OnScene3Activated;

    private void Awake()
    {
        Instance = this;
        IsActive = false;
    }

    // THÊM 2026-07-31 (Jok hỏi "tuỳ vào checkpoint cảnh thực tế thì sao?"): CheckpointManager CHỈ khôi phục
    // đúng field "_isLocked" của DoorController (xem DoorController.Awake -- RegisterFlag), KHÔNG hề biết
    // gì về "_forceJammed" hay trạng thái SetActive() của _interactablesToDisable[]. Nếu Player chết GIỮA
    // cảnh 3 (sau khi ActivateScene3() đã kẹt cứng cửa sân sau) rồi Retry về checkpoint stage 3 -- scene
    // reload ra 1 Chapter1Scene3Manager HOÀN TOÀN MỚI, IsActive tự về false, và KHÔNG có gì tự gọi lại
    // ActivateScene3() nữa (chỉ OnMusicBoxFinished() gọi, mà băng đã nghe xong từ trước rồi, không phát lại)
    // -- kết quả: 2 cửa lẽ ra kẹt cứng vĩnh viễn chỉ còn khoá thường (mở lại được bằng chìa), phá vỡ thiết
    // kế "không còn đường lùi". Fix: nếu checkpoint đã ở stage 3+ (nghĩa là Player đang ở lại/quay lại đúng
    // đoạn này, không phải lần đầu tới), tự ép ActivateScene3() lại ngay khi scene vừa load xong -- hàm này
    // đã có sẵn guard IsActive nên gọi nhiều lần vẫn an toàn, idempotent.
    //
    // SỬA 2026-07-31: KHÔNG gọi thẳng trong Start() -- Unity KHÔNG đảm bảo thứ tự Start() giữa các object
    // khác nhau trong cùng 1 scene, nếu Start() của Chapter1Scene3Manager chạy TRƯỚC Start() của chính cửa
    // bị Close() ở đây thì DoorController._closedRot/_openRot chưa kịp tính (vẫn Quaternion.identity mặc
    // định) -- cửa sẽ đóng về góc rác. Trì hoãn đúng 1 frame (WaitForEndOfFrame) để chắc chắn MỌI Start()
    // trong scene (kể cả DoorController) đã chạy xong hết trước khi đụng vào door.Close().
    private void Start()
    {
        if (CheckpointManager.CurrentStage >= 3) StartCoroutine(ActivateScene3AfterAllStart());
    }

    private System.Collections.IEnumerator ActivateScene3AfterAllStart()
    {
        yield return new WaitForEndOfFrame();
        ActivateScene3();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>Wire vào MusicBoxInteractable.OnSequenceStarted -- khoá tạm 2 cửa Thư Phòng NGAY lúc bắt đầu nghe.</summary>
    public void LockStudyDoorsImmediately()
    {
        if (_studyDoorsToLockImmediately == null) return;
        foreach (var door in _studyDoorsToLockImmediately)
        {
            if (door == null) continue;
            door.Close();
            door.SetLocked(true);
        }
    }

    /// <summary>Wire vào MusicBoxInteractable.OnTapeFinished -- mở cửa Thư Phòng->Hành Lang, khoá Hành Lang->Salon, rồi vào cảnh 3.</summary>
    public void OnMusicBoxFinished()
    {
        if (_studyToHallwayDoor != null)
        {
            _studyToHallwayDoor.SetLocked(false);
            _studyToHallwayDoor.Open();
        }

        if (_hallwayToSalonDoor != null)
        {
            _hallwayToSalonDoor.Close();
            _hallwayToSalonDoor.SetLocked(true);
        }

        ActivateScene3();
    }

    /// <summary>Có thể gọi tay (VD context menu lúc test trong Editor) hoặc tự động từ OnMusicBoxFinished().</summary>
    [ContextMenu("Kích hoạt Cảnh 3 (test tay)")]
    public void ActivateScene3()
    {
        if (IsActive) return;
        IsActive = true;

        if (_interactablesToDisable != null)
            foreach (var go in _interactablesToDisable)
                if (go != null) go.SetActive(false);

        if (_interactablesToEnable != null)
            foreach (var go in _interactablesToEnable)
                if (go != null) go.SetActive(true);

        if (_backyardDoorsToJam != null)
        {
            foreach (var door in _backyardDoorsToJam)
            {
                if (door == null) continue;
                door.Close();
                door.SetLocked(true);
                door.SetForceJammed(true);
            }
        }

        if (_backyardUnlockableDoor != null)
        {
            _backyardUnlockableDoor.Close();
            _backyardUnlockableDoor.SetLocked(true);
            _backyardUnlockableDoor.SetForceJammed(false); // KHÔNG kẹt cứng -- mở lại bình thường bằng chìa
        }

        if (_mainDoorReconfirmLocked != null)
            _mainDoorReconfirmLocked.SetLocked(true);

        if (_chaseBgm != null) AudioManager.Instance?.PlayBGM(_chaseBgm);

        Debug.Log("[Chapter1Scene3Manager] Cảnh 3 đã kích hoạt.");
        OnScene3Activated?.Invoke();
    }
}
