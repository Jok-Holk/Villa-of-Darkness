using UnityEngine;

// SỬA LỖI KIẾN TRÚC THẬT (Jok phát hiện qua ScreenFader): 1 nhóm hệ thống UI (ScreenFader, ExamineStageUI,
// HudMetersUI, TutorialHintUI, InteractPrompt, DiaryReaderPanel) dùng pattern "Instance set trong Awake(),
// tự ẩn/hiện bằng CanvasGroup/SetActive con bên trong" -- Unity KHÔNG chạy Awake() nếu chính GameObject đó
// bị tắt sẵn trong scene. Jok tắt object cha để dọn Hierarchy cho gọn lúc edit -> Awake() không bao giờ
// chạy -> Instance mãi mãi null -> mọi lời gọi kiểu "ScreenFader.Instance?.FadeOut()" ÂM THẦM không làm gì
// (dấu ?. nuốt lỗi, không có cảnh báo) -- đúng bug thật "vào phòng ăn không fade đen" Jok gặp phải.
//
// Script này chạy ĐẦU TIÊN (DefaultExecutionOrder rất âm, trước mọi script khác kể cả GameManager), ép các
// GameObject này BẬT LẠI ngay khi vào game thật -- BẤT KỂ Jok để chúng tắt sẵn trong scene để dọn giao
// diện lúc edit. Sau khi bật, LOGIC ẨN/HIỆN THẬT của từng script (CanvasGroup.alpha, SetActive con bên
// trong...) vẫn hoạt động y nguyên -- object CHA chỉ cần "tồn tại và chạy được", không đồng nghĩa "đang
// hiện lên màn hình".
[DefaultExecutionOrder(-1000)]
public class UIBootstrap : MonoBehaviour
{
    // Chỉ những object dùng pattern "luôn phải tồn tại + tự ẩn/hiện bên trong" mới cần ép bật ở đây.
    // KHÔNG thêm "InventoryPanel"/"DialoguePanel" -- 2 cái đó tự SetActive(true) đúng lúc cần từ chính code
    // mở ra (Open()/StartDialogue()), ép bật sẵn ở đây sẽ làm chúng LỘ RA ngay lúc vừa vào game, sai hẳn.
    private static readonly string[] AlwaysActiveRoots =
    {
        "ScreenFader_Canvas",
        "ExamineStageUI",
        "HudMetersUI",
        "TutorialHintUI",
        "InteractPrompt",
        "DiaryReaderPanel",
    };

    private void Awake()
    {
        int count = 0;
        foreach (string name in AlwaysActiveRoots)
        {
            GameObject go = FindByNameIncludingInactive(name);
            if (go == null)
            {
                Debug.LogWarning($"[UIBootstrap] Không tìm thấy '{name}' trong scene -- bỏ qua.");
                continue;
            }
            if (!go.activeSelf)
            {
                go.SetActive(true);
                count++;
            }

            // SỬA: KHÔNG ép bật đệ quy TẤT CẢ con ở đây -- Examine/HUD/TutorialHint/InteractPrompt/Diary
            // đều CỐ Ý giữ 1 lớp con luôn tắt sẵn (VD "ExamineStageCanvas"), chỉ hiện khi Show()/Open() thật
            // sự được gọi. Ép bật đại trà sẽ làm chúng LỘ RA NGAY lúc vừa vào game -- sai ngược hẳn. Con nào
            // thật sự "luôn phải tồn tại, chỉ ẩn qua alpha" (như BlackImage của ScreenFader) thì để CHÍNH
            // script sở hữu nó (VD ScreenFader.Awake()) tự đảm bảo, không đoán mù ở đây.
        }
        if (count > 0) Debug.Log($"[UIBootstrap] Đã tự bật lại {count} object UI Jok tắt sẵn để dọn Hierarchy lúc edit.");
    }

    private static GameObject FindByNameIncludingInactive(string name)
    {
        var all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in all)
        {
            if (t.hideFlags != HideFlags.None) continue;
            if (t.name != name) continue;
            if (!t.gameObject.scene.IsValid()) continue;
            return t.gameObject;
        }
        return null;
    }
}
