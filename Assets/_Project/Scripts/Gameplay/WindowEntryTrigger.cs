using UnityEngine;
using System.Collections;

// Đặt vào world tại khu vực cửa sổ phòng ăn (theo COT_TRUYEN: "[INTERACT] Cửa sổ phòng ăn"). Người chơi
// bấm E -- nghe 1 câu thoại có voice thật, xong tự "trèo qua cửa sổ": fade đen NGAY TẠI VỊ TRÍ ĐANG ĐỨNG
// (không snap/teleport trước khi nói -- snap trước khi fade nhìn giật/xấu), teleport sang điểm B (trong
// Phòng Ăn, LUÔN xoay về đúng "Point B Y Rotation" bất kể Transform Point B đang xoay hướng gì), fade
// sáng lại, tự lưu checkpoint stage 2.
//
// Jok tự đặt GameObject này đúng vị trí cửa sổ + kéo Point B (trong Phòng Ăn) trong Inspector.
[RequireComponent(typeof(Collider))]
public class WindowEntryTrigger : MonoBehaviour, IInteractable, IInteractableLabel
{
    [Header("Thoại — 1 câu thoại có voice thật, chạy xong tự tiếp tục cảnh")]
    [SerializeField] private DialogueAsset dialogueAsset;
    [SerializeField] private string interactLabel = "Cửa sổ";

    [Header("Point B (trong Phòng Ăn) -- điểm teleport tới SAU khi fade đen. Không cần Point A -- giữ nguyên vị trí Player đang đứng lúc bấm E.")]
    [SerializeField] private Transform pointB;
    [Tooltip("Hướng nhìn Y CỐ ĐỊNH lúc teleport tới Point B -- KHÔNG dùng rotation của chính Transform Point B (dễ lệch tuỳ theo cách đặt object).")]
    [SerializeField] private float pointBYRotation = 90f;

    [Tooltip("Số stage checkpoint mà cảnh này lưu -- PHẢI khớp đúng số dùng trong CheckpointManager.Save() bên dưới")]
    [SerializeField] private int stage = 2;

    [Header("Fade đen giữa chừng — theo đúng kịch bản \"chớp tắt 0.5 giây kiểu điện ảnh\"")]
    [SerializeField] private float fadeOutDuration = 0.25f;
    [SerializeField] private float holdBlackDuration = 0.5f;
    [SerializeField] private float fadeInDuration = 0.25f;

    [Header("HUD ẩn suốt cả đoạn thoại + fade + teleport (VD: Stamina, pin đèn, InteractPrompt) -- giống cách IntroManager làm")]
    [SerializeField] private GameObject[] hudToHide;

    [Header("Vật tương tác NGOÀI TRỜI (cửa chính, đá chặn...) -- tự tắt Collider khi đã vào tới Phòng Ăn, " +
             "vì từ cửa sổ trong nhà raycast vẫn với đủ tầm ra ngoài xuyên kính, không hợp lý còn tương tác được nữa")]
    [SerializeField] private GameObject[] disableOnEntry;

    [SerializeField] private bool interactOnce = true;
    private bool _hasInteracted = false;

    private Collider _collider;

    public string InteractLabel => interactLabel;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        // _hasInteracted chỉ là biến runtime thường, KHÔNG sống qua reload/Force Stage debug-jump -- nếu
        // không check lại, Retry sau khi đã qua cửa sổ (hoặc debug-jump thẳng stage 2) sẽ về false, đứng
        // gần cửa sổ bấm E lại là chạy lại nguyên cảnh + teleport lần nữa dù đã qua đoạn này rồi. Đồng bộ
        // lại theo đúng checkpoint PlayerPrefs thật đang có, y hệt cách IntroManager tự skip cinematic.
        if (CheckpointManager.CurrentStage >= stage)
        {
            _hasInteracted = true;
            if (_collider != null) _collider.enabled = false; // đã qua rồi -- tắt luôn, khỏi hiện "[E] Cửa sổ" vô nghĩa từ trong nhà nhìn ra
        }
    }

    public void Interact()
    {
        if (interactOnce && _hasInteracted) return;
        _hasInteracted = true;
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        // Ẩn HUD ngay từ đầu (giống IntroManager) -- DialogueUI.StartDialogue() cũng tự ẩn HUD lúc đang
        // thoại, nhưng CloseDialogue() lại tự HIỆN LẠI ngay khi thoại xong, trong khi vẫn còn đang fade +
        // teleport dở -- ẩn tay ở đây để đảm bảo HUD KHÔNG lộ ra giữa chừng đoạn fade, chỉ hiện lại ở cuối.
        SetHudVisible(false);

        if (dialogueAsset != null)
        {
            DialogueUI.Instance?.StartDialogue(dialogueAsset);
            while (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueOpen())
                yield return null;
        }

        // DialogueUI.CloseDialogue() tự trả lại input lúc đóng hộp thoại -- khoá lại ngay, không cho di
        // chuyển/nhìn lung tung trong lúc đang fade + teleport giữa chừng (giống cách IntroManager làm).
        PlayerController.Instance?.SetInputEnabled(false);

        bool faded = false;
        ScreenFader.Instance?.FadeOut(fadeOutDuration, () => faded = true);
        if (ScreenFader.Instance != null) while (!faded) yield return null;

        if (holdBlackDuration > 0f) yield return new WaitForSecondsRealtime(holdBlackDuration);

        // "Trèo qua cửa sổ" -- teleport thẳng sang Point B (trong Phòng Ăn), LUÔN xoay Y cố định.
        SnapPlayerTo(pointB, pointBYRotation);

        // Tắt hết vật tương tác ngoài trời NGAY LÚC NÀY (chuyển cảnh sống, không phải reload scene nên
        // CheckpointManager-check trong Start() của các script kia đã chạy TRƯỚC đó rồi, không tự bắt được) --
        // còn màn hình đang đen nên tắt lúc này không lộ gì.
        if (disableOnEntry != null)
        {
            foreach (var go in disableOnEntry)
            {
                if (go == null) continue;
                var col = go.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
        }

        bool fadedIn = false;
        ScreenFader.Instance?.FadeIn(fadeInDuration, () => fadedIn = true);
        if (ScreenFader.Instance != null) while (!fadedIn) yield return null;

        PlayerController.Instance?.SetInputEnabled(true);
        SetHudVisible(true);

        // Checkpoint "Phòng ăn" -- lưu NGAY khi vào tới nơi. Từ đây Retry/load lại scene sẽ bỏ qua cả
        // đoạn cổng, spawn thẳng đây.
        if (PlayerController.Instance != null)
            CheckpointManager.Save(stage, PlayerController.Instance.transform.position, PlayerController.Instance.transform.rotation);

        // Tắt luôn Collider của chính cửa sổ -- đã dùng xong, không cần hiện "[E] Cửa sổ" lại nữa dù đứng
        // gần từ phía nào (interactOnce đã chặn Interact() chạy lại, nhưng Collider vẫn bật thì UI prompt
        // vẫn hiện ra dù bấm E không có tác dụng gì -- tắt hẳn cho gọn).
        if (_collider != null) _collider.enabled = false;
    }

    private void SetHudVisible(bool visible)
    {
        if (hudToHide == null) return;
        foreach (var go in hudToHide)
            if (go != null) go.SetActive(visible);
    }

    private void SnapPlayerTo(Transform point, float? forcedYRotation = null)
    {
        if (point == null || PlayerController.Instance == null) return;

        Transform player = PlayerController.Instance.transform;
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = point.position;
        player.rotation = forcedYRotation.HasValue ? Quaternion.Euler(0f, forcedYRotation.Value, 0f) : point.rotation;
        if (cc != null) cc.enabled = true;
    }
}
