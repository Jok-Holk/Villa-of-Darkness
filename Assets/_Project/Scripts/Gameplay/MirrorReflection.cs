using UnityEngine;

// Gương phản chiếu THẬT -- Player soi đèn vào gương thấy chính mình, KHÔNG phải hiệu ứng jumpscare (đó là
// MirrorJumpscareReaction, độc lập hoàn toàn với script này).
//
// SỬA 2026-07-26: Bản cũ nhóm làm áp thẳng material phản chiếu lên MẶT KÍNH CỦA MODEL GỐC (PH_OrnateMirror
// hay tương tự) -- mesh gốc là model trang trí phức tạp, UV mặt kính KHÔNG chuẩn 0-1, nên gán RenderTexture
// vào bị nén/méo ở viền. Giờ dùng 1 QUAD RIÊNG (KHÔNG đụng mesh gốc) đặt đè lên đúng vị trí mặt kính, UV
// chuẩn 0-1 -- không bao giờ bị nén/méo bất kể model gương thật là gì.
//
// ĐƠN GIẢN HOÁ 2026-07-27 (Jok yêu cầu): Bản trước dùng kỹ thuật gương phẳng "chính xác" (ma trận phản
// chiếu theo vị trí Player + Oblique Near-Plane Clipping) -- càng làm càng phát sinh bug khó chịu (nhìn
// xuyên tường, camera lạc sang phòng khác, Edit Mode với Play Mode lệch nhau...) do phải phân biệt quá
// nhiều trường hợp. Bỏ HẲN kỹ thuật đó. Giờ chỉ 1 Camera CỐ ĐỊNH, đặt sẵn 1 lần ngay trước mặt kính, quay ra
// phòng, KHÔNG di chuyển theo Player, không có Camera thứ 2, không cần phân biệt Edit/Play. Không phản chiếu
// đúng 100% góc nhìn thật của Player, nhưng đơn giản, ổn định, luôn cho ra đúng "căn phòng trước gương".
[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class MirrorReflection : MonoBehaviour
{
    [Header("Kích thước thật của mặt kính (m) -- ước lượng theo Scale của chính Quad này, chỉnh Transform Scale để khớp khung gương thật")]
    [SerializeField] private int _textureSize = 512;

    [Tooltip("Mặc định = tất cả layer. Player đã có sẵn model thân thật (object 'Breathing Idle', layer " +
             "'MirrorOnly') bị ẩn khỏi Camera chính qua Culling Mask -- Camera phản chiếu ở đây mặc định " +
             "bao gồm layer đó nên tự thấy được thân người, không cần chỉnh gì thêm.")]
    [SerializeField] private LayerMask _reflectLayers = ~0;

    [Header("Vị trí Camera cố định -- đặt ngay trước mặt kính, quay ra phòng")]
    [Tooltip("Khoảng cách lùi ra khỏi mặt kính -- đủ để không kẹt vào khung gương")]
    [SerializeField] private float _cameraOffset = 0.15f;
    [Tooltip("Góc nhìn (FOV) -- không cần khớp Player, chỉnh cho vừa khung cảnh muốn thấy trong gương")]
    [SerializeField] private float _fieldOfView = 60f;

    [Header("Hình dạng thật của mặt kính (vòm cong, không phải chữ nhật) -- bake sẵn từ mesh gốc bằng " +
             "VoD_BuildMirrorGlassMask.cs, KHÔNG chỉnh tay. Trắng = hiện phản chiếu, đen = cắt bỏ.")]
    [SerializeField] private Texture2D _glassMask;

    [Header("Thực tại khác trong gương -- CỐ Ý sáng độc lập với ánh sáng gameplay thật (Jok yêu cầu: dễ thấy " +
             "nhân vật + tạo cảm giác bất thường), KHÔNG cố mô phỏng đúng vật lý ánh sáng thật")]
    [Tooltip("Hệ số nhân sáng lên ảnh phản chiếu")]
    [SerializeField] private float _brightness = 1.3f;
    [Tooltip("Sàn sáng tối thiểu CỘNG THÊM -- khác phép nhân, cái này làm sáng được cả vùng nguồn ĐEN TUYỆT ĐỐI.")]
    [SerializeField] private Color _ambientLift = new Color(0.5f, 0.47f, 0.55f, 1f);

    [Header("Chớp nháy sáng/tối theo nhịp đếm gaze (Jok yêu cầu) -- báo hiệu trực quan trước khi jumpscare, " +
             "KHÔNG đổi thời điểm trigger thật (vẫn do GazeTrigger/GazeSettings quyết định)")]
    [Tooltip("Để trống thì tự tìm GazeTrigger ở object CHA (thường nằm trên GameObject gương gốc, không phải Quad này)")]
    [SerializeField] private GazeTrigger _gazeTrigger;
    [Tooltip("Độ sáng lúc pha 'tối' của chớp nháy (giây 2, 4, 6...)")]
    [SerializeField] private float _flickerDarkBrightness = 0.15f;
    [SerializeField] private Color _flickerDarkAmbientLift = new Color(0.02f, 0.02f, 0.03f, 1f);

    // Layer riêng cho CHÍNH Quad này -- 3 = " MirrorCamera" (có dấu cách đầu tên, layer cũ nhóm để sẵn từ hệ
    // thống gương cũ, không ai dùng). QUAN TRỌNG: KHÔNG được đổi sang layer bất kỳ (VD 31) -- Player Camera
    // chỉ có Culling Mask gồm layer 0-8, chọn ngoài phạm vi đó thì chính Player cũng sẽ không thấy được mặt
    // kính này nữa. Layer 3 vừa nằm trong phạm vi Player Camera thấy được, vừa an toàn để loại riêng khỏi
    // Camera phản chiếu (tránh gương tự thấy lại chính mình) mà không đụng layer Default.
    private const int MirrorSurfaceLayer = 3;

    private Camera _reflectionCamera;
    private RenderTexture _renderTexture;
    private MeshRenderer _surfaceRenderer;

    private void OnEnable()
    {
        _surfaceRenderer = GetComponent<MeshRenderer>();
        if (_gazeTrigger == null) _gazeTrigger = GetComponentInParent<GazeTrigger>();

        gameObject.layer = MirrorSurfaceLayer;

        // Dọn object "EditPreviewCamera" còn sót lại từ bản kỹ thuật cũ (đã bỏ hẳn) -- không dùng nữa, chỉ
        // còn là rác. Nếu đang chọn đúng object này trong Inspector lúc dọn có thể thấy 1 dòng lỗi console
        // vô hại (Unity tự đóng Inspector) -- không ảnh hưởng gì, bấm chọn lại object khác là hết.
        Transform oldPreview = transform.Find($"{name}_EditPreviewCamera");
        if (oldPreview != null) DestroyImmediate(oldPreview.gameObject);

        SetupReflectionCamera(); // tìm-hoặc-tạo + LUÔN áp lại toàn bộ setting -- không phụ thuộc cache field
        SetupSurfaceMaterial();
    }

    // Chỉnh Brightness/Ambient Lift/Top Corner Radius trong Inspector lúc Edit Mode -- áp ngay vào material đang dùng.
    private void OnValidate()
    {
        if (_surfaceRenderer == null) _surfaceRenderer = GetComponent<MeshRenderer>();
        if (_surfaceRenderer == null || _surfaceRenderer.sharedMaterial == null) return;

        var mat = _surfaceRenderer.sharedMaterial;
        if (mat.HasProperty("_Brightness"))  mat.SetFloat("_Brightness", _brightness);
        if (mat.HasProperty("_AmbientLift")) mat.SetColor("_AmbientLift", _ambientLift);
        if (_glassMask != null && mat.HasProperty("_GlassMask")) mat.SetTexture("_GlassMask", _glassMask);
    }

    // Tìm lại Camera con nếu đã có sẵn (KHÔNG destroy/tạo lại object -- Destroy khi object đang được chọn
    // trong Inspector gây lỗi SerializedObjectNotCreatableException), chỉ tạo mới nếu chưa từng có, rồi LUÔN
    // áp lại toàn bộ setting mỗi lần OnEnable() -- đảm bảo sửa code là áp dụng ngay, không cần đoán cache còn hạn hay không.
    private void SetupReflectionCamera()
    {
        string camName = $"{name}_ReflectionCamera";
        Transform camT = transform.Find(camName);
        GameObject camGO;
        bool isNewCamera = camT == null;
        if (!isNewCamera)
        {
            camGO = camT.gameObject;
        }
        else
        {
            camGO = new GameObject(camName);
            camGO.transform.SetParent(transform, false);
        }

        _reflectionCamera = camGO.GetComponent<Camera>();
        if (_reflectionCamera == null) _reflectionCamera = camGO.AddComponent<Camera>();

        // SỬA 2026-07-27: CHỈ đặt vị trí/hướng mặc định lúc TẠO MỚI lần đầu -- trước đây ép lại MỖI LẦN
        // OnEnable() nên Jok tự chỉnh tay Rotation trong Inspector bị trả về ngay lập tức, không sửa được.
        // Giờ nếu camera đã có sẵn thì giữ nguyên y hệt vị trí/hướng Jok tự chỉnh -- tự xoay thử bằng mắt
        // (nhờ [ExecuteAlways] thấy ngay không cần Play) rồi chốt hướng đúng, không cần đoán qua code nữa.
        if (isNewCamera)
        {
            camGO.transform.localPosition = new Vector3(0f, 0f, -_cameraOffset);
            camGO.transform.localRotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
        }

        int viewmodelLayer = LayerMask.NameToLayer("Viewmodel");
        int excludeMask = (1 << MirrorSurfaceLayer) | (viewmodelLayer >= 0 ? 1 << viewmodelLayer : 0);
        _reflectionCamera.cullingMask   = _reflectLayers & ~excludeMask;
        _reflectionCamera.fieldOfView   = _fieldOfView;
        _reflectionCamera.nearClipPlane = 0.05f;
        _reflectionCamera.farClipPlane  = 50f;
        _reflectionCamera.clearFlags      = CameraClearFlags.SolidColor;
        _reflectionCamera.backgroundColor = Color.black; // nền đen thật (không phải Skybox mặc định) -- tránh lộ màu skybox nhạt ở chỗ không có geometry

        float aspect = Mathf.Max(0.01f, transform.localScale.x) / Mathf.Max(0.01f, transform.localScale.y);
        _reflectionCamera.aspect = aspect;
        int texHeight = Mathf.RoundToInt(_textureSize / Mathf.Max(0.1f, aspect));

        if (_renderTexture == null || _renderTexture.width != _textureSize || _renderTexture.height != texHeight)
        {
            if (_renderTexture != null) _renderTexture.Release();
            _renderTexture = new RenderTexture(_textureSize, texHeight, 16) { name = $"{name}_ReflectionRT" };
        }
        _reflectionCamera.targetTexture = _renderTexture;
        _reflectionCamera.enabled = true; // để Unity tự render mỗi frame theo pipeline URP bình thường
    }

    private void SetupSurfaceMaterial()
    {
        if (_surfaceRenderer == null) return;

        // Dùng shader riêng "VoD/MirrorBrightReflection" -- xử lý NGAY TRÊN ẢNH ĐÃ CHỤP để hiện "thực tại
        // khác" sáng hơn thật (xem ghi chú trong file shader) thay vì cố mô phỏng đúng vật lý ánh sáng.
        var shader = Shader.Find("VoD/MirrorBrightReflection");
        if (shader == null)
        {
            Debug.LogWarning("[MirrorReflection] Không tìm thấy shader 'VoD/MirrorBrightReflection' -- dùng tạm Unlit thường.");
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Material mat = _surfaceRenderer.sharedMaterial;
        if (mat == null || mat.shader != shader)
        {
            mat = new Material(shader);
            _surfaceRenderer.sharedMaterial = mat;
        }

        // BUG THẬT: mat.mainTexture = ... là cách gán "ngầm định" (Unity tự đoán property nào là "texture
        // chính" dựa vào tag [MainTexture] trong ShaderLab) -- shader tự viết ở đây KHÔNG có tag đó, việc
        // gán ngầm định này có thể ÂM THẦM THẤT BẠI, khiến material không bao giờ thực sự nhận đúng
        // RenderTexture dù Camera vẫn render đúng -- đúng lý do "kéo/xoay Camera nhưng gương không đổi gì
        // cả". Gán THẲNG tên property bằng SetTexture() -- không mơ hồ, chắc chắn đúng property. Check cả
        // 2 tên (_MainTex cho shader tự viết, _BaseMap cho trường hợp fallback về URP Unlit chuẩn).
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", _renderTexture);
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", _renderTexture);
        if (mat.HasProperty("_Brightness"))  mat.SetFloat("_Brightness", _brightness);
        if (mat.HasProperty("_AmbientLift")) mat.SetColor("_AmbientLift", _ambientLift);
        // Mask hình vòm thật -- bake bởi VoD_BuildMirrorGlassMask.cs, tự gán vào field này, không chỉnh tay.
        if (_glassMask != null && mat.HasProperty("_GlassMask")) mat.SetTexture("_GlassMask", _glassMask);
    }

    // Chớp nháy sáng (giây 1, 3, 5...) / tối (giây 2, 4, 6...) theo ĐÚNG tiến trình gaze thật đang chạy trên
    // GazeTrigger -- không dùng đồng hồ riêng, tự đồng bộ + tự reset nếu player rời mắt khỏi gương giữa
    // chừng. Không đổi thời điểm trigger jumpscare thật -- chỉ là hiệu ứng hình ảnh phủ lên trên.
    private void Update()
    {
        if (_surfaceRenderer == null) return;
        var mat = _surfaceRenderer.sharedMaterial;
        if (mat == null || !mat.HasProperty("_Brightness")) return;

        bool isBrightPhase = true;
        if (Application.isPlaying && _gazeTrigger != null && _gazeTrigger.enabled && _gazeTrigger.GazeTimer > 0f)
        {
            int second = Mathf.FloorToInt(_gazeTrigger.GazeTimer);
            isBrightPhase = (second % 2 == 0);
        }

        mat.SetFloat("_Brightness", isBrightPhase ? _brightness : _flickerDarkBrightness);
        mat.SetColor("_AmbientLift", isBrightPhase ? _ambientLift : _flickerDarkAmbientLift);
    }

    private void OnDestroy()
    {
        if (_renderTexture == null) return;
        // [ExecuteAlways] -- OnDestroy() có thể chạy ở cả Edit Mode (VD lúc script recompile, domain
        // reload huỷ/tạo lại object) lẫn Play Mode. Destroy() chỉ hợp lệ lúc Play -- gọi ở Edit Mode ném
        // lỗi "Destroy may not be called from edit mode! Use DestroyImmediate instead."
        if (Application.isPlaying) Destroy(_renderTexture);
        else DestroyImmediate(_renderTexture);
    }
}
