using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Hệ thống gợi ý phím tắt cho người chơi mới -- Jok phát hiện: game hiện KHÔNG có cách nào dạy người chơi
// biết F bật/tắt đèn pin, T lắc hồi pin, hay Tab mở túi đồ. Tự dựng UI qua code (theo đúng pattern
// JumpscareGameOverUI/DeathScreenUI), không cần Jok kéo tay Canvas/Text.
//
// 2 kiểu:
//   - ShowOnce(hintId, ...): CHỈ hiện đúng 1 LẦN DUY NHẤT trong suốt đời game (lưu PlayerPrefs
//     "VoD_Hint_<id>") -- dùng cho hướng dẫn tổng quát 1 lần là đủ nhớ (VD Tab mở túi đồ lúc nhặt vật
//     phẩm đầu tiên, F bật/tắt đèn lúc vừa có quyền điều khiển).
//   - ShowRepeating(...): LUÔN hiện mỗi lần được gọi, KHÔNG nhớ đã xem hay chưa -- dùng cho gợi ý tình
//     huống lặp lại nhiều lần trong game (VD T lắc pin mỗi lần pin xuống mức yếu, F tắt đèn lúc trốn).
// Cả 2 đều NHẤP NHÁY vài lần đầu (Jok yêu cầu) để thu hút chú ý, giữ sáng ổn định rồi tự mờ dần tắt.
//
// SỬA 2026-07-28: [ExecuteAlways] -- theo đúng pattern FlashlightController đã có sẵn trong project, để
// Jok bật/tắt GameObject này trong Hierarchy xem layout tĩnh (vị trí, màu, chữ mẫu) mà KHÔNG cần bấm Play.
// Animation nhấp nháy/tự tắt vẫn chỉ chạy lúc Play thật (coroutine không tick ở Edit Mode).
[ExecuteAlways]
public class TutorialHintUI : MonoBehaviour
{
    // Ưu tiên tìm object đã đặt sẵn trong scene (qua VoD_EmbedRuntimeUIInScene.cs) -- chỉ tạo mới bằng
    // code nếu chưa từng đặt (fallback).
    private static TutorialHintUI _instance;
    public static TutorialHintUI Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<TutorialHintUI>(FindObjectsInactive.Include);
            if (_instance == null)
            {
                var go = new GameObject("TutorialHintUI");
                _instance = go.AddComponent<TutorialHintUI>();
            }
            return _instance;
        }
    }

    private const string SavePrefix = "VoD_Hint_";
    private const float DefaultDuration = 5f;
    private const float BlinkInterval = 0.35f;
    private const int   BlinkCount = 3;

    private GameObject _root;
    private TextMeshProUGUI _keyText;
    private TextMeshProUGUI _labelText;
    private LayoutElement _keyBadgeLE;
    private CanvasGroup _canvasGroup;
    private Coroutine _activeRoutine;

    private bool _built;

    // OnEnable() thay vì Awake() -- chạy được ở CẢ Edit Mode lẫn Play Mode nhờ [ExecuteAlways].
    private void OnEnable()
    {
        if (_instance != null && _instance != this && Application.isPlaying) { Destroy(gameObject); return; }
        _instance = this;
        // KHÔNG DontDestroyOnLoad -- giờ là object thật nằm trong scene (không phải tạo runtime nữa), để
        // nó tự huỷ/tự dựng lại theo scene, tránh kẹt lại/nhân đôi lúc đổi qua MainMenu rồi quay lại.
        if (!_built) { BuildUI(); _built = true; }

        // AN TOÀN: phòng Jok bật thử "TutorialHintUI_Canvas"/"HintRoot" ở Edit Mode xem layout rồi quên
        // tắt lại trước khi bấm Play -- ép về ẩn (alpha 0) mỗi lần vào Play thật, chỉ ShowOnce/ShowRepeating
        // mới được bật lại, không phụ thuộc trạng thái Jok để lại lúc preview.
        if (Application.isPlaying && _root != null) _root.SetActive(false);
    }

    private void OnDisable()
    {
        if (_instance == this) _instance = null;
    }

    private void BuildUI()
    {
        if (transform.Find("TutorialHintUI_Canvas") != null) return; // đã dựng rồi, không dựng chồng

        var canvasGO = new GameObject("TutorialHintUI_Canvas");
        canvasGO.transform.SetParent(transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500; // trên HUD thường, dưới màn hình chết/jumpscare (1000+)
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        _root = new GameObject("HintRoot", typeof(RectTransform));
        _root.transform.SetParent(canvasGO.transform, false);
        var rootRt = _root.GetComponent<RectTransform>();
        rootRt.anchorMin = new Vector2(0.5f, 0f);
        rootRt.anchorMax = new Vector2(0.5f, 0f);
        rootRt.pivot     = new Vector2(0.5f, 0f);
        // SỬA (Jok yêu cầu -- "kéo y xuống dưới 1 ít nữa"): 200 -> 150, thấp hơn 1 chút nữa.
        rootRt.anchoredPosition = new Vector2(0f, 150f);

        _canvasGroup = _root.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable   = false;

        // SỬA 2026-07-28 (Jok phát hiện): Image không gán sprite = Unity tự vẽ hình CHỮ NHẬT/VUÔNG TRƠN,
        // không bo góc gì cả -- nhìn như "chữ nổi trên 1 hộp xám", không ra dáng pill/badge như mockup. Tự
        // tạo sprite bo góc bằng code (không cần asset ngoài), dùng Image.Type.Sliced để bo góc CỐ ĐỊNH bất
        // kể pill co giãn rộng/hẹp theo độ dài chữ (ContentSizeFitter).
        // SỬA (Jok yêu cầu -- "nó bị nhỏ, nâng size lên gấp đôi nữa"): nhân đôi toàn bộ (font/badge/padding/
        // spacing/sprite nguồn) giống hệt đợt x2 đã áp cho InteractPromptUI.
        var bg = _root.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.035f, 0.03f, 0.85f);
        bg.sprite = VoDUISpriteUtil.CreateRoundedSprite(128, 128, 48);
        bg.type = Image.Type.Sliced;

        var hlayout = _root.AddComponent<HorizontalLayoutGroup>();
        hlayout.childAlignment = TextAnchor.MiddleCenter;
        hlayout.spacing = 20;
        hlayout.padding = new RectOffset(16, 40, 12, 12);
        hlayout.childForceExpandWidth  = false;
        hlayout.childForceExpandHeight = false;
        // SỬA cùng lỗi phát hiện ở InteractPromptUI.cs (childControlWidth/Height) -- KeyBadge có thể bị giữ
        // sizeDelta mặc định 100x100 thay vì co về đúng preferredWidth/Height=40 nếu thiếu 2 dòng này.
        hlayout.childControlWidth = true;
        hlayout.childControlHeight = true;

        var fitter = _root.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // Badge phím -- hình tròn vàng, khớp tông màu chủ đạo đã dùng ở Inventory/HUD.
        var keyBadgeGO = new GameObject("KeyBadge", typeof(RectTransform));
        keyBadgeGO.transform.SetParent(_root.transform, false);
        var keyBadgeImg = keyBadgeGO.AddComponent<Image>();
        keyBadgeImg.color = new Color(0.79f, 0.635f, 0.36f, 1f);
        keyBadgeImg.sprite = VoDUISpriteUtil.CreateRoundedSprite(128, 128, 64); // 2x nguồn, radius vẫn = nửa cạnh = tròn khi vuông
        keyBadgeImg.type = Image.Type.Sliced;

        // SỬA (Jok phát hiện qua InteractPromptUI, "height hơi lạ"/bầu dục méo -- ban đầu định lồng
        // HorizontalLayoutGroup+ContentSizeFitter ngay trong KeyBadge để tự co giãn, nhưng _root (cha) CŨNG
        // có childControlWidth/Height=true -- 2 hệ layout tranh nhau set cùng 1 RectTransform trong cùng 1
        // lượt rebuild, ra kích thước méo tuỳ hệ nào chạy sau. Bỏ hẳn lớp lồng -- chỉ dùng 1 LayoutElement,
        // width tính lại MỖI LẦN Display() đổi chữ (xem bên dưới) bằng GetPreferredValues() của chính TMP.
        var keyBadgeLE = keyBadgeGO.AddComponent<LayoutElement>();
        keyBadgeLE.preferredHeight = 80f;
        _keyBadgeLE = keyBadgeLE;

        var keyTextGO = new GameObject("KeyText", typeof(RectTransform));
        keyTextGO.transform.SetParent(keyBadgeGO.transform, false);
        _keyText = keyTextGO.AddComponent<TextMeshProUGUI>();
        _keyText.enableAutoSizing = false; // đề phòng project có preset TMP mặc định bật Auto Size
        _keyText.alignment = TextAlignmentOptions.Center;
        _keyText.fontSize  = 40;
        _keyText.fontStyle = FontStyles.Bold;
        var notoFont = VoDFontUtil.FindNotoSansFont();
        if (notoFont != null) _keyText.font = notoFont;
        _keyText.color     = new Color(0.1f, 0.08f, 0.04f, 1f);
        var keyTextRt = keyTextGO.GetComponent<RectTransform>();
        keyTextRt.anchorMin = Vector2.zero; keyTextRt.anchorMax = Vector2.one;
        keyTextRt.offsetMin = Vector2.zero; keyTextRt.offsetMax = Vector2.zero;
        UpdateKeyBadgeWidth(); // đặt sẵn cho placeholder/preview -- Display() sẽ tính lại đúng theo chữ thật

        var labelGO = new GameObject("Label", typeof(RectTransform));
        labelGO.transform.SetParent(_root.transform, false);
        _labelText = labelGO.AddComponent<TextMeshProUGUI>();
        _labelText.enableAutoSizing = false; // đề phòng project có preset TMP mặc định bật Auto Size
        _labelText.alignment = TextAlignmentOptions.MidlineLeft;
        _labelText.fontSize  = 40;
        if (notoFont != null) _labelText.font = notoFont;
        _labelText.color     = new Color(0.92f, 0.89f, 0.84f, 1f);
        var labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredHeight = 80;
        labelLE.minWidth = 240;

        _root.SetActive(false);
    }

    /// <summary>Chỉ hiện đúng 1 lần duy nhất trong suốt đời game (lưu PlayerPrefs, không reset theo
    /// checkpoint/Retry) -- dùng cho hướng dẫn tổng quát (VD "Tab mở túi đồ" lúc nhặt vật phẩm đầu tiên).</summary>
    public void ShowOnce(string hintId, string key, string label, float duration = DefaultDuration)
    {
        if (PlayerPrefs.GetInt(SavePrefix + hintId, 0) == 1) return;
        PlayerPrefs.SetInt(SavePrefix + hintId, 1);
        PlayerPrefs.Save();
        Display(key, label, duration);
    }

    /// <summary>Luôn hiện, KHÔNG nhớ đã xem -- dùng cho gợi ý tình huống lặp lại (VD "T lắc pin" mỗi lần
    /// pin xuống mức yếu, "F tắt đèn" mỗi lần vào chỗ trốn). Gọi lại nhiều lần trong 1 ván là CHỦ Ý.</summary>
    public void ShowRepeating(string key, string label, float duration = DefaultDuration)
    {
        Display(key, label, duration);
    }

    private void Display(string key, string label, float duration)
    {
        if (_root == null) return;
        _keyText.text   = key;
        _labelText.text = label;
        UpdateKeyBadgeWidth(); // "Tab" (3 ký tự) cần rộng hơn hẳn "F"/"T" (1 ký tự) -- tính lại mỗi lần đổi chữ
        _root.SetActive(true);

        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(BlinkThenHide(duration));
    }

    private IEnumerator BlinkThenHide(float duration)
    {
        // Nhấp nháy vài lần đầu để thu hút mắt (Jok yêu cầu), sau đó giữ sáng ổn định -- nhấp nháy suốt cả
        // duration sẽ gây khó chịu/khó đọc chữ, chỉ cần chớp đủ để bắt mắt lúc mới hiện ra.
        for (int i = 0; i < BlinkCount; i++)
        {
            yield return Fade(0f, 1f, BlinkInterval);
            yield return Fade(1f, 0.15f, BlinkInterval);
        }
        yield return Fade(_canvasGroup.alpha, 1f, BlinkInterval);

        float elapsed = BlinkCount * BlinkInterval * 2f + BlinkInterval;
        float hold = duration - elapsed;
        if (hold > 0f) yield return new WaitForSecondsRealtime(hold);

        yield return Fade(_canvasGroup.alpha, 0f, 0.3f);
        _root.SetActive(false);
        _activeRoutine = null;
    }

    // Badge tròn cho phím 1 ký tự ("F", "T"), tự giãn thành pill bo tròn cho phím dài hơn ("Tab") -- tính 1
    // LẦN mỗi khi đổi chữ (không lồng layout group con trong con để tránh xung đột, xem ghi chú ở BuildUI()).
    private void UpdateKeyBadgeWidth()
    {
        if (_keyBadgeLE == null || _keyText == null) return;
        float textWidth = _keyText.GetPreferredValues(_keyText.text).x;
        _keyBadgeLE.preferredWidth = Mathf.Max(80f, textWidth + 48f);
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }

}
