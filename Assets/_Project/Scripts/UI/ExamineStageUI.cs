using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;

// Sân khấu soi vật phẩm (chìa khoá, mảnh giấy, sổ ghi nợ...) CÔ LẬP HẲN khỏi thế giới game -- vật được
// dời tới 1 toạ độ xa hoàn toàn giữa hư không (map villa chỉ nằm trong khoảng X:0-35, Z:-10-25 nên
// (2000,2000,2000) không bao giờ đụng geometry nào khác), quay bằng 1 Camera + đèn riêng LUÔN SÁNG ĐỀU,
// hiện kết quả qua RenderTexture lên RawImage FULL MÀN HÌNH trong 1 Canvas riêng (Screen Space Overlay).
//
// SỬA 2026-07-26 (2 bug thật Jok phát hiện):
// 1) Đèn sân khấu trước dùng LightType.Directional -- loại đèn này CHIẾU TOÀN CẢNH bất kể vị trí đặt xa
//    cỡ nào (hướng chiếu mới quan trọng, không phải vị trí), nên vô tình làm SÁNG LUÔN CẢ VILLA THẬT dù đặt
//    đèn ở toạ độ (2000,2000,2000). Đổi hẳn sang LightType.Point -- có falloff theo khoảng cách, ở xa
//    ~3464 đơn vị so với villa (quanh gốc toạ độ) thì cường độ tới villa gần như bằng 0.
// 2) RawImage trước chỉ chiếm 1 ô vuông nhỏ giữa màn hình (700x700), phần còn lại vẫn thấy xuyên qua thế
//    giới game thật đằng sau -- giờ full màn hình (anchor 0,0 -> 1,1), không còn thấy gì lọt qua.
// Đồng thời: tắt hẳn Post Processing (PSX/vignette toàn game) riêng cho camera này để không bị tối/nhiễu
// theo hiệu ứng chung, tự ẩn HUD (InteractPrompt/Flashlight/StaminaBreathBar) lúc soi, và hỗ trợ zoom
// (cuộn chuột) để nhìn sát chi tiết vật phẩm.
//
// SỬA 2026-07-28: [ExecuteAlways] -- theo đúng pattern FlashlightController đã có sẵn trong project. Sau
// khi đặt thành object thật trong scene (VoD_EmbedRuntimeUIInScene.cs), Jok bật GameObject con
// "ExamineStageCanvas" lên trong Hierarchy là xem NGAY layout tiêu đề + 4 góc hint (đã có sẵn chữ mặc định
// từ lúc BuildStage(), không cần gọi Show()) mà KHÔNG cần bấm Play. Phần Camera xoay vật 3D thật + zoom
// vẫn cần Play do phụ thuộc input/raycast thật.
[ExecuteAlways]
public class ExamineStageUI : MonoBehaviour
{
    public static ExamineStageUI Instance { get; private set; }

    private static readonly Vector3 StageWorldPosition = new Vector3(2000f, 2000f, 2000f);
    // "InteractPrompt" KHÔNG còn nằm trong danh sách này -- object đó có API riêng (InteractPromptUI.
    // SetDotVisible) quản lý CHẤM TRẮNG con bên trong nó độc lập. Trước đây SetHudVisible() ở đây tắt/bật
    // THẲNG cả GameObject cha "InteractPrompt" bằng SetActive -- 2 cơ chế đá nhau: nếu cha bị tắt mà đường
    // nào đó không gọi lại true (VD restoreHud=false lúc về Inventory), chấm trắng vĩnh viễn không hiện lại
    // được nữa dù SetDotVisible(true) gọi bao nhiêu lần (con không tự hiện khi cha đang tắt). Giờ luôn đi
    // qua đúng 1 API duy nhất (InteractPromptUI.SetDotVisible) thay vì SetActive trực tiếp GameObject cha.
    //
    // POLISH 2026-07-28: "Flashlight"/"StaminaBreathBar" (2 object HUD cũ) cũng KHÔNG còn trong danh sách
    // này -- đã thay bằng HudMetersUI (2 vạch mảnh tối giản), tự quản lý ẩn/hiện qua HudMetersUI.SetVisible()
    // ở dưới, cùng lý do tránh 2 cơ chế đá nhau như case InteractPrompt ở trên.
    private static readonly string[] HudObjectNames = new string[0];

    private const float MinCameraDistance = 0.15f;
    private const float MaxCameraDistance = 1.5f;
    private const float DefaultCameraDistance = 0.7f;
    private const float ZoomSpeed = 0.35f;

    public Transform StagePivot { get; private set; }
    public Camera StageCamera { get; private set; }

    private Canvas _canvas;
    private float _currentCameraDistance = DefaultCameraDistance;
    private GameObject[] _hudObjects;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _cornerBL; // Đọc chữ [R] -- ẩn khi item không có mô tả đọc được
    private TextMeshProUGUI _cornerBR; // Thoát -- đổi chữ theo exitHint truyền vào Show()
    private string _pendingItemTitle;

    private GameObject _readOverlayGO;
    private TextMeshProUGUI _readOverlayText;
    private string _pendingReadableText;
    public bool IsReadOverlayVisible => _readOverlayGO != null && _readOverlayGO.activeSelf;

    public static ExamineStageUI GetOrCreate()
    {
        if (Instance != null) return Instance;

        // Ưu tiên tìm object đã đặt sẵn trong scene (qua VoD_EmbedRuntimeUIInScene.cs) trước khi tạo mới.
        Instance = FindFirstObjectByType<ExamineStageUI>(FindObjectsInactive.Include);
        if (Instance != null) return Instance;

        var go = new GameObject("ExamineStageUI");
        return go.AddComponent<ExamineStageUI>();
    }

    private bool _built;

    // OnEnable() thay vì Awake() -- chạy được ở CẢ Edit Mode lẫn Play Mode nhờ [ExecuteAlways].
    private void OnEnable()
    {
        if (Instance != null && Instance != this && Application.isPlaying) { Destroy(gameObject); return; }
        Instance = this;
        // KHÔNG DontDestroyOnLoad -- giờ là object thật nằm trong scene (không phải tạo runtime nữa).
        if (!_built) { BuildStage(); _built = true; }

        // AN TOÀN: lúc Jok bật "ExamineStageCanvas" lên xem thử ở Edit Mode rồi quên tắt lại trước khi bấm
        // Play, trạng thái bật đó bị lưu vào scene và kẹt sang thật -- đè lên gameplay thật. Vào Play luôn
        // ép về false, chỉ Show() mới được bật lại, không phụ thuộc trạng thái Jok để lại lúc preview.
        if (Application.isPlaying && _canvas != null) _canvas.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    private void BuildStage()
    {
        // BUG THẬT (Jok phát hiện -- NullReferenceException tại PositionCamera() ngay lúc nhặt chìa khoá):
        // early-return CŨ ở đây chỉ kiểm tra object con "ExamineStageCanvas" đã tồn tại rồi BỎ QUA HẲN việc
        // gán StagePivot/StageCamera/_canvas/... cho INSTANCE HIỆN TẠI -- nếu Unity tạo ra 1 ExamineStageUI
        // component MỚI (domain reload/script recompile) trong khi các GameObject con (StageCamera,
        // StagePivot...) đã có sẵn từ trước trong scene, properties của instance mới này vẫn NULL dù object
        // thật đã tồn tại, gây crash ngay khi Show()->PositionCamera() đụng StageCamera.transform. Giờ LUÔN
        // fetch lại đầy đủ property/field từ children có sẵn thay vì chỉ return trơn.
        var existingCanvas = transform.Find("ExamineStageCanvas");
        if (existingCanvas != null)
        {
            var pivotT = transform.Find("StagePivot");
            if (pivotT != null) StagePivot = pivotT;

            var camT = transform.Find("StageCamera");
            if (camT != null) StageCamera = camT.GetComponent<Camera>();

            _canvas = existingCanvas.GetComponent<Canvas>();

            var titleT = existingCanvas.Find("ExamineTitleCard");
            if (titleT != null) _titleText = titleT.GetComponent<TextMeshProUGUI>();

            var cornerBLT = existingCanvas.Find("ExamineCorner_ĐỌC CHỮ");
            if (cornerBLT != null) _cornerBL = cornerBLT.GetComponent<TextMeshProUGUI>();

            var cornerBRT = existingCanvas.Find("ExamineCorner_THOÁT");
            if (cornerBRT != null) _cornerBR = cornerBRT.GetComponent<TextMeshProUGUI>();

            var overlayT = existingCanvas.Find("ExamineReadOverlay");
            if (overlayT != null)
            {
                _readOverlayGO = overlayT.gameObject;
                var overlayTextT = overlayT.Find("ReadOverlayText");
                if (overlayTextT != null) _readOverlayText = overlayTextT.GetComponent<TextMeshProUGUI>();
            }

            return;
        }

        var pivotGO = new GameObject("StagePivot");
        pivotGO.transform.SetParent(transform, false);
        pivotGO.transform.position = StageWorldPosition;
        StagePivot = pivotGO.transform;

        // Đèn chính + đèn fill nhẹ đối diện -- Point (KHÔNG phải Directional) để không lỡ chiếu sáng
        // ngược lại villa thật ở cách xa hàng nghìn đơn vị. Range giới hạn nhỏ, chỉ đủ phủ khu vực soi vật.
        var keyLightGO = new GameObject("StageKeyLight");
        keyLightGO.transform.SetParent(transform, false);
        keyLightGO.transform.position = StageWorldPosition + new Vector3(0.3f, 0.5f, -0.6f);
        keyLightGO.transform.LookAt(StageWorldPosition);
        var keyLight = keyLightGO.AddComponent<Light>();
        keyLight.type = LightType.Point;
        keyLight.range = 3f;
        keyLight.intensity = 3f;
        keyLight.color = new Color(1f, 0.98f, 0.92f);
        keyLight.shadows = LightShadows.Soft;

        var fillLightGO = new GameObject("StageFillLight");
        fillLightGO.transform.SetParent(transform, false);
        fillLightGO.transform.position = StageWorldPosition + new Vector3(-0.5f, 0.2f, 0.6f);
        fillLightGO.transform.LookAt(StageWorldPosition);
        var fillLight = fillLightGO.AddComponent<Light>();
        fillLight.type = LightType.Point;
        fillLight.range = 3f;
        fillLight.intensity = 1.2f;
        fillLight.color = new Color(0.85f, 0.9f, 1f);
        fillLight.shadows = LightShadows.None;

        // Camera riêng render ra RenderTexture -- tắt sẵn, chỉ bật lúc Show() để đỡ tốn hiệu năng lúc không dùng.
        var camGO = new GameObject("StageCamera");
        camGO.transform.SetParent(transform, false);
        StageCamera = camGO.AddComponent<Camera>();
        StageCamera.clearFlags = CameraClearFlags.SolidColor;
        // SỬA 2026-07-28 (Jok phát hiện): xám-xanh lạnh (0.22,0.22,0.25) không hợp tông ấm (nâu tối + vàng
        // gold) đã dùng xuyên suốt Inventory/HUD -- chữ trắng-vàng ấm đặt lên nền lạnh bị "lép tone", như 2
        // phong cách khác nhau ghép lại. Đổi sang nâu-xám ẤM cùng hệ, giữ độ sáng tương đương (vẫn thấy rõ
        // vật như yêu cầu gốc, chỉ đổi tông màu).
        StageCamera.backgroundColor = new Color(0.24f, 0.20f, 0.16f);
        StageCamera.fieldOfView = 35f;
        StageCamera.nearClipPlane = 0.05f;
        StageCamera.farClipPlane = 5f;
        StageCamera.enabled = false;
        PositionCamera();

        // Tắt hẳn Post Processing (hiệu ứng vignette/nhiễu hạt kiểu Volume toàn game) riêng cho camera này --
        // không thì Volume chung của URP vẫn áp lên, khiến sân khấu "luôn sáng" bị tối/nhiễu y hệt màn hình chính.
        var camData = camGO.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null) camData = camGO.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = false;

        // LƯU Ý: hiệu ứng pixelate/dither PSX KHÔNG phải Volume -- nó là 1 Full Screen Pass Renderer Feature
        // gắn thẳng vào URP Renderer Data dùng chung cho MỌI camera, renderPostProcessing=false ở trên KHÔNG
        // tắt được nó. Kết quả: camera này vẫn bị chia độ phân giải xuống thấp (Pixel Divisor) như màn chính,
        // nhìn "bị ép xuống 480p rồi phóng to" khi soi cận cảnh 1 vật nhỏ. Cách tắt triệt để (thêm 1 URP
        // Renderer Data riêng không có PSX rồi gán cho camera này qua SetRenderer()) cần Jok tạo asset đó
        // trong Unity trước vì đụng tới Graphics Settings dùng chung toàn project -- rủi ro nếu làm mù qua
        // code mà không xem trực tiếp. Tạm thời giảm nhẹ vấn đề bằng cách tăng hẳn độ phân giải RenderTexture
        // gốc (trước khi bị chia nhỏ) -- cùng 1 tỉ lệ chia nhưng chia từ nguồn lớn hơn nhiều thì khối pixel
        // hiện ra nhỏ hơn hẳn, đỡ vỡ hạt hơn rõ rệt dù effect PSX vẫn còn áp dụng.
        var renderTexture = new RenderTexture(2048, 2048, 16);
        StageCamera.targetTexture = renderTexture;

        var canvasGO = new GameObject("ExamineStageCanvas");
        canvasGO.transform.SetParent(transform, false);
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 500;
        // BUG THẬT: trước đây AddComponent<CanvasScaler>() rồi bỏ mặc định (Constant Pixel Size), không
        // đồng bộ với HudMetersUI/TutorialHintUI (đều dùng ScaleWithScreenSize + reference 1920x1080) --
        // đúng nguyên nhân chữ 4 góc bị phóng to bất thường Jok phát hiện qua ảnh Play thật.
        var examineScaler = canvasGO.AddComponent<CanvasScaler>();
        examineScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        examineScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGO.AddComponent<GraphicRaycaster>();

        // FULL MÀN HÌNH -- trước đây chỉ 1 ô 700x700 giữa màn hình, phần còn lại vẫn lộ thế giới game thật
        // đằng sau. Giờ phủ kín (anchor 0,0 -> 1,1), không còn lọt gì qua.
        var imgGO = new GameObject("ExamineRawImage");
        imgGO.transform.SetParent(canvasGO.transform, false);
        var rawImage = imgGO.AddComponent<RawImage>();
        rawImage.texture = renderTexture;
        var rt = rawImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // THÊM 2026-07-27: Vignette tối 4 góc -- trước đây nền phẳng lì trông "cũ/xấu" (Jok phản hồi "như đồ
        // cổ"). Vignette đơn giản, không cần asset ngoài, dựng ngay bằng code (gradient đen alpha tăng dần
        // ra rìa) -- cho cảm giác có chiều sâu/tiêu điểm vào giữa vật thay vì phẳng đều toàn khung.
        var vignetteGO = new GameObject("ExamineVignette");
        vignetteGO.transform.SetParent(canvasGO.transform, false);
        var vignetteImage = vignetteGO.AddComponent<Image>();
        vignetteImage.sprite = CreateVignetteSprite();
        vignetteImage.type = Image.Type.Simple;
        vignetteImage.raycastTarget = false;
        var vignetteRt = vignetteImage.rectTransform;
        vignetteRt.anchorMin = Vector2.zero;
        vignetteRt.anchorMax = Vector2.one;
        vignetteRt.offsetMin = Vector2.zero;
        vignetteRt.offsetMax = Vector2.zero;

        // POLISH 2026-07-28 (Jok yêu cầu tối giản hơn): bỏ hẳn 1 thanh chữ dài ngang dưới màn hình -- thay
        // bằng title card (tên vật) ở trên đầu + 4 cụm gợi ý nhỏ mỗi góc, đúng theo mockup đã duyệt. Đỡ rối
        // mắt hơn nhiều so với 1 dòng dài liệt kê hết mọi thao tác cùng lúc.
        // SỬA 2026-07-28: offset 24px đoán bừa -- giờ tính lại đúng từ mockup (khung 1180px, offset 24px)
        // scale lên CanvasScaler thật 1920px (hệ số 1.627) => 39px.
        const float cornerOffset = 39f;
        BuildTitleCard(canvasGO.transform);
        // SỬA 2026-07-28 (Jok yêu cầu): "kéo tự do" là ngôn ngữ mô tả kiểu dev, không phải cách người chơi
        // nói -- rút gọn còn đúng tên phím/nút bấm, đủ hiểu, tự nhiên hơn hẳn.
        BuildCorner(canvasGO.transform, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(cornerOffset, -cornerOffset),
            "XOAY", "Chuột trái", out _);
        BuildCorner(canvasGO.transform, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(-cornerOffset, -cornerOffset),
            "ZOOM", "Cuộn chuột", out _);
        // SỬA 2026-07-28 (Jok yêu cầu): "[R] bật lớp đọc" nói kiểu dev ("lớp" = layer), thừa cả ngoặc vuông
        // lặp với chữ R. Rút gọn còn mỗi "R" -- vừa tự nhiên hơn, vừa KHÔNG còn "hứa" đúng 1 hành động cụ
        // thể (bật) nên không sai ngữ cảnh nữa lúc R thực ra đang dùng để ĐÓNG (đã mở lớp đọc rồi).
        BuildCorner(canvasGO.transform, TextAnchor.LowerLeft, new Vector2(0f, 0f), new Vector2(cornerOffset, cornerOffset),
            "ĐỌC CHỮ", "R", out _cornerBL);
        BuildCorner(canvasGO.transform, TextAnchor.LowerRight, new Vector2(1f, 0f), new Vector2(-cornerOffset, cornerOffset),
            "THOÁT", "Chuột phải", out _cornerBR);

        // XOÁ 2026-07-28 (Jok yêu cầu): "ExamineDebugCalibrateHint" (gợi ý mũi tên xoay 90°/[L] log) + phím
        // tắt tương ứng trong ExamineItem.cs -- công cụ calibrate offset TẠM, đã bake xong hết offset cho
        // toàn bộ item nên bỏ hẳn, không còn cần nữa.

        BuildReadOverlay(canvasGO.transform);

        canvasGO.SetActive(false);
    }

    // SỬA 2026-07-28: font/offset tính lại đúng theo mockup (khung 1180px: kind=10px h3=16px top=24px)
    // scale lên 1920px thật (hệ số 1.627) => kind=16px h3=26px top=39px. Thêm luôn dòng "ĐANG XEM" (kind)
    // phía trên tên vật -- mockup có 2 dòng, code cũ chỉ có 1 dòng tên vật, thiếu mất eyebrow.
    private void BuildTitleCard(Transform canvasParent)
    {
        var titleGO = new GameObject("ExamineTitleCard");
        titleGO.transform.SetParent(canvasParent, false);
        var title = titleGO.AddComponent<TextMeshProUGUI>();
        // SỬA 2026-07-28 (Jok yêu cầu): x3 thay vì x2 (trước 26 gốc -> giờ 78), đổi font NotoSans SDF (chữ
        // mỏng quá với LiberationSans mặc định), tăng box chứa cho đủ chỗ chữ to hơn hẳn.
        title.enableAutoSizing = false; // đề phòng project có preset TMP mặc định bật Auto Size
        title.fontSize = 78; // x3 gốc (26 * 3)
        title.fontStyle = FontStyles.Bold;
        var notoFont = VoDFontUtil.FindNotoSansFont();
        if (notoFont != null) title.font = notoFont;
        title.alignment = TextAlignmentOptions.Top;
        title.color = new Color(0.92f, 0.89f, 0.84f);
        // SỬA 2026-07-28 (Jok phát hiện): trước đây KHÔNG set text mặc định gì cả -- TextMeshProUGUI mới tạo
        // rỗng trơn ("") cho tới lúc Show() gọi thật. Kết quả: bật preview qua VoD_UIPreviewWindow (không đi
        // qua Show()) thì tiêu đề "ĐANG XEM" hiện HOÀN TOÀN TRỐNG dù object/style đều đã đúng -- nhìn như
        // thiếu hẳn object. Đặt sẵn chữ mẫu placeholder ngay lúc dựng để preview LUÔN thấy được vị trí/font/
        // màu thật, Show() lúc Play vẫn ghi đè đúng tên vật thật như cũ.
        title.text = "<size=62%><color=#8a713f>ĐANG XEM</color></size>\nTên Vật Phẩm";
        var titleRt = title.rectTransform;
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot     = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(1100f, 190f); // cao hơn cho đủ chỗ chữ x3
        titleRt.anchoredPosition = new Vector2(0f, -45f);
        _titleText = title;
    }

    // 1 cụm góc = nhãn nhỏ màu vàng (loại thao tác) + dòng mô tả bên dưới -- dùng rich text 2 dòng trong
    // cùng 1 TMP thay vì 2 object riêng cho gọn. corner = pivot/anchor góc tương ứng.
    // SỬA 2026-07-28: font tính lại theo mockup (value=11px label=9.5px, tỉ lệ label/value=86%, không phải
    // 70% đoán bừa trước đây) scale lên 1920px => value=18px label=86% của 18 ≈ 16px.
    private void BuildCorner(Transform canvasParent, TextAnchor align, Vector2 anchor, Vector2 offset, string label, string value, out TextMeshProUGUI valueText)
    {
        var go = new GameObject($"ExamineCorner_{label}");
        go.transform.SetParent(canvasParent, false);
        var text = go.AddComponent<TextMeshProUGUI>();
        // SỬA 2026-07-28 (Jok yêu cầu): x3 thay vì x2 (18 gốc -> 54), chữ mỏng quá nên bật Bold luôn, đổi
        // font NotoSans SDF (đậm/rõ hơn LiberationSans mặc định), tăng box chứa + giãn lineSpacing tương
        // ứng cho 2 dòng khỏi đè lên nhau.
        text.enableAutoSizing = false; // đề phòng project có preset TMP mặc định bật Auto Size
        text.text = $"<size=86%><color=#8a713f>{label}</color></size>\n{value}";
        text.fontSize = 54; // x3 gốc (18 * 3)
        text.fontStyle = FontStyles.Bold;
        var notoFont = VoDFontUtil.FindNotoSansFont();
        if (notoFont != null) text.font = notoFont;
        text.lineSpacing = 20f;
        text.color = new Color(0.78f, 0.74f, 0.66f, 0.95f);

        bool right = anchor.x >= 0.5f;
        bool top   = anchor.y >= 0.5f;
        text.alignment = top
            ? (right ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft)
            : (right ? TextAlignmentOptions.BottomRight : TextAlignmentOptions.BottomLeft);

        var rt = text.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot     = anchor;
        rt.sizeDelta = new Vector2(750f, 210f); // to hơn cho đủ chỗ chữ x3
        rt.anchoredPosition = offset;

        valueText = text;
    }

    // THÊM 2026-07-28: Đặt tên vật hiện ở title card đầu màn Examine -- gọi TRƯỚC Show() (giống pattern
    // SetReadableText). Để trống thì title card tự ẩn (VD examine trực tiếp ngoài world không qua Inventory).
    public void SetItemTitle(string title)
    {
        _pendingItemTitle = title;
    }

    // THÊM 2026-07-27: Vật nhỏ (chữ viết tay trên sổ ghi nợ...) rất khó đọc trực tiếp trên model 3D dù đã
    // xoay/zoom hết cỡ -- lớp phủ này hiện lại đúng nội dung ItemData.description (đã có sẵn, không cần viết
    // thêm) dưới dạng chữ máy rõ ràng, bấm [R] để bật/tắt qua lại. CHỈ áp dụng cho item xem từ Inventory có
    // mô tả (do chỉ nơi đó biết ItemData) -- nhật ký CỐ Ý không qua đường này vì đã có UI đọc lật trang riêng
    // (_diaryReader), không cần lớp phủ chữ chồng thêm lên trên nữa.
    // Vignette đơn giản dựng bằng code (không cần asset ngoài) -- gradient đen, alpha tăng dần từ giữa ra
    // rìa. Chỉ cần dựng 1 lần, dùng lại cho mọi lần mở Examine -- không phải texture nặng, 128x128 là đủ
    // mượt vì chỉ hiện full màn hình co giãn (không cần chi tiết cao).
    private static Sprite CreateVignetteSprite()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = center.magnitude;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                // Trong tâm trong suốt hoàn toàn, từ ~55% bán kính trở ra mới bắt đầu tối dần, tối đa ở rìa.
                float alpha = Mathf.Clamp01((dist - 0.35f) / 0.65f);
                alpha = alpha * alpha; // cong nhẹ cho mượt, không tối gắt đột ngột
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha * 0.75f));
            }
        }
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void BuildReadOverlay(Transform canvasParent)
    {
        var overlayGO = new GameObject("ExamineReadOverlay");
        overlayGO.transform.SetParent(canvasParent, false);

        var bg = overlayGO.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.06f, 0.92f);
        var bgRt = overlayGO.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0.5f, 0.5f);
        bgRt.anchorMax = new Vector2(0.5f, 0.5f);
        bgRt.sizeDelta = new Vector2(1000f, 640f);
        bgRt.anchoredPosition = Vector2.zero;

        var textGO = new GameObject("ReadOverlayText");
        textGO.transform.SetParent(overlayGO.transform, false);
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.enableAutoSizing = false;
        text.fontSize = 30;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.color = new Color(0.92f, 0.9f, 0.85f);
        var textRt = text.rectTransform;
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(40f, 40f);
        textRt.offsetMax = new Vector2(-40f, -40f);

        var closeHintGO = new GameObject("ReadOverlayCloseHint");
        closeHintGO.transform.SetParent(overlayGO.transform, false);
        var closeHint = closeHintGO.AddComponent<TextMeshProUGUI>();
        closeHint.enableAutoSizing = false;
        closeHint.text = "[R] Đóng";
        closeHint.fontSize = 22;
        closeHint.alignment = TextAlignmentOptions.BottomRight;
        closeHint.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        var closeRt = closeHint.rectTransform;
        closeRt.anchorMin = new Vector2(1f, 0f);
        closeRt.anchorMax = new Vector2(1f, 0f);
        closeRt.pivot     = new Vector2(1f, 0f);
        closeRt.sizeDelta = new Vector2(200f, 40f);
        closeRt.anchoredPosition = new Vector2(-16f, 12f);

        _readOverlayGO   = overlayGO;
        _readOverlayText = text;
        overlayGO.SetActive(false);
    }

    /// <summary>Gọi TRƯỚC khi Show() nếu muốn item này có sẵn nội dung để bấm [R] đọc -- truyền null/rỗng
    /// nếu item không có mô tả đáng đọc (hint [R] sẽ không hiện ra).</summary>
    public void SetReadableText(string text)
    {
        _pendingReadableText = text;
    }

    private void ToggleReadOverlay()
    {
        if (_readOverlayGO == null || string.IsNullOrEmpty(_pendingReadableText)) return;

        bool willShow = !_readOverlayGO.activeSelf;
        if (willShow && _readOverlayText != null) _readOverlayText.text = _pendingReadableText;
        _readOverlayGO.SetActive(willShow);
    }

    private void PositionCamera()
    {
        StageCamera.transform.position = StageWorldPosition + new Vector3(0f, 0f, -_currentCameraDistance);
        StageCamera.transform.LookAt(StageWorldPosition);
    }

    private float ComputeFitDistance()
    {
        if (StagePivot == null) return DefaultCameraDistance;

        var renderers = StagePivot.GetComponentsInChildren<Renderer>(includeInactive: false);
        if (renderers.Length == 0) return DefaultCameraDistance;

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);

        float radius = Mathf.Max(bounds.extents.magnitude, 0.02f);
        float fitDistance = radius / Mathf.Sin(StageCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Chừa thêm 40% khoảng trống quanh vật -- vừa khít sát viền quá thì xoay dễ bị cắt góc.
        return Mathf.Clamp(fitDistance * 1.4f, MinCameraDistance, MaxCameraDistance);
    }

    private void Update()
    {
        // Edit Mode chỉ để xem layout tĩnh (title/4 góc) -- zoom/đọc chữ [R] cần input thật, chỉ chạy lúc Play.
        if (!Application.isPlaying) return;
        if (_canvas == null || !_canvas.gameObject.activeSelf) return;

        // Cuộn chuột để zoom lại gần/xa vật phẩm -- trước đây không zoom được, chỉ xoay.
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            _currentCameraDistance = Mathf.Clamp(_currentCameraDistance - scroll * ZoomSpeed, MinCameraDistance, MaxCameraDistance);
            PositionCamera();
        }

        if (Input.GetKeyDown(KeyCode.R))
            ToggleReadOverlay();
    }

    /// <param name="pauseGame">true = tự Time.timeScale=0 lúc Show, =1 lúc Hide (soi đồ trực tiếp ngoài
    /// world). Truyền false khi soi đồ TỪ TRONG Inventory -- Inventory đã tự pause sẵn (Time.timeScale=0),
    /// Hide() ở đây KHÔNG được tự ý resume về 1 vì Inventory vẫn còn mở, để InventoryUI.Close() tự lo resume.</param>
    public void Show(bool pauseGame = true, string exitHint = "Chuột phải")
    {
        // THÊM 2026-07-27: Trước dùng CỐ ĐỊNH 1 khoảng cách camera cho MỌI item -- chìa khoá nhỏ hiện ra bé
        // tí giữa màn hình, phải tự zoom tay mỗi lần. Giờ tự đo bounds thật của vật (đã được reparent vào
        // StagePivot trước khi Show() được gọi) và canh khoảng cách vừa khung ngay từ đầu.
        _currentCameraDistance = ComputeFitDistance();
        PositionCamera();

        StageCamera.enabled = true;
        _canvas.gameObject.SetActive(true);
        if (_readOverlayGO != null) _readOverlayGO.SetActive(false);

        bool hasReadableText = !string.IsNullOrEmpty(_pendingReadableText);

        if (_titleText != null)
        {
            bool hasTitle = !string.IsNullOrEmpty(_pendingItemTitle);
            _titleText.gameObject.SetActive(hasTitle);
            // 2 dòng: "ĐANG XEM" (eyebrow nhỏ màu vàng, đúng mockup) + tên vật to bên dưới.
            if (hasTitle) _titleText.text = $"<size=62%><color=#8a713f>ĐANG XEM</color></size>\n{_pendingItemTitle}";
        }

        if (_cornerBL != null) _cornerBL.gameObject.SetActive(hasReadableText);

        if (_cornerBR != null)
            _cornerBR.text = $"<size=70%><color=#8a713f>THOÁT</color></size>\n{exitHint}";

        SetHudVisible(false);

        if (pauseGame) Time.timeScale = 0f;
    }

    /// <param name="restoreHud">false khi thoát Examine để quay VỀ Inventory (Inventory vẫn đang mở, tự lo
    /// ẩn HUD của chính nó qua _hudToHideDuringInventory) -- bật lại HUD ở đây trong trường hợp đó làm lộ
    /// Stamina/Flashlight NGAY TRONG lúc Inventory còn mở. Chỉ true khi thoát hẳn về gameplay thế giới thật.</param>
    public void Hide(bool pauseGame = true, bool restoreHud = true)
    {
        StageCamera.enabled = false;
        _canvas.gameObject.SetActive(false);
        if (_readOverlayGO != null) _readOverlayGO.SetActive(false);
        _pendingReadableText = null;
        _pendingItemTitle    = null;

        if (restoreHud) SetHudVisible(true);

        if (pauseGame) Time.timeScale = 1f;
    }

    private void SetHudVisible(bool visible)
    {
        // SỬA 2026-07-27: GameObject.Find() CHỈ tìm được object đang active -- nếu đúng lúc cache lần đầu mà
        // 1 trong 3 object này đang tắt sẵn (VD icon đèn pin chỉ bật khi đang cầm đèn), Find trả về null và
        // bị cache null MÃI MÃI cho cả phiên chơi, từ đó object đó không bao giờ ẩn được nữa dù gọi lại nhiều
        // lần -- đúng triệu chứng "HUD gameplay vẫn còn khi đang Examine" dù code gọi SetActive(false) đều đặn.
        // Giờ chỉ cache slot nào đã tìm được (không null), slot nào còn null thì thử tìm lại mỗi lần gọi.
        if (_hudObjects == null) _hudObjects = new GameObject[HudObjectNames.Length];

        for (int i = 0; i < HudObjectNames.Length; i++)
        {
            if (_hudObjects[i] == null)
                _hudObjects[i] = FindByNameIncludingInactive(HudObjectNames[i]);

            if (_hudObjects[i] != null)
                _hudObjects[i].SetActive(visible);
        }

        InteractPromptUI.Instance?.SetDotVisible(visible);
        HudMetersUI.Instance.SetVisible(visible);
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
