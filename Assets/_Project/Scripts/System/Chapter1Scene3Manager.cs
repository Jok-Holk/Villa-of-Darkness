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
    [Tooltip("Mấy object tương tác được không còn liên quan tới cảnh 3 -- tắt hẳn (SetActive(false)).")]
    [SerializeField] private GameObject[] _interactablesToDisable;

    [Tooltip("Cửa sân sau bị KẸT CỨNG (dù có chìa đúng cũng không mở) -- VD Phòng Ăn -> Hành Lang Sau.")]
    [SerializeField] private DoorController[] _backyardDoorsToJam;

    [Tooltip("Cửa NGOẠI LỆ vẫn mở được bình thường trong 3 cửa sân sau -- VD Salon -> Hành Lang Phụ. Đóng " +
             "lại lúc vào cảnh 3 nhưng KHÔNG kẹt cứng, Player tự mở khoá lại bằng chìa như thường.")]
    [SerializeField] private DoorController _backyardUnlockableDoor;

    [Tooltip("Cửa chính (đã khoá cứng từ cảnh 1) -- không cần đổi gì (itemId trống sẵn = không mở được), " +
             "chỉ liệt kê ở đây nếu Jok muốn ActivateScene3() tự double-check SetLocked(true) cho chắc.")]
    [SerializeField] private DoorController _mainDoorReconfirmLocked;

    [Header("Sự kiện phụ -- wire thêm nhạc nền căng thẳng / đổi patrol ghost / v.v.")]
    public UnityEvent OnScene3Activated;

    private void Awake()
    {
        Instance = this;
        IsActive = false;
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

        Debug.Log("[Chapter1Scene3Manager] Cảnh 3 đã kích hoạt.");
        OnScene3Activated?.Invoke();
    }
}
