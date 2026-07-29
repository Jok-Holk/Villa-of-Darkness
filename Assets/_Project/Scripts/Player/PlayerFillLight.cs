using UnityEngine;

// Ánh sáng phụ RẤT YẾU, tầm gần, LUÔN BẬT, gắn theo Camera Player -- KHÔNG phải đèn pin, không thay thế
// đèn pin. Chỉ đảm bảo khu vực NGAY SÁT người chơi không bao giờ đen thui tuyệt đối, kể cả khi đèn pin tắt.
//
// Lý do cần: Moonlight (Directional Light) hầu như không lọt được vào trong nhà (tường/mái chặn hết, đúng
// vật lý). Ambient của cả map lại tính theo skybox tối đều khắp nơi (không phân biệt trong/ngoài nhà), cộng
// thêm Fog dày -- kết quả trong nhà gần như = 0 ánh sáng nếu không có đèn pin, PSX post-process (dither/
// color depth thấp) càng làm vùng tối gần đen bị bệt mảng khó nhìn hơn.
//
// KHÔNG sửa bằng cách tăng ambient/moonlight toàn map -- sẽ phá luôn độ tối cần có ở xa (mất không khí
// horror). Giải pháp đúng phạm vi: 1 Point Light tầm ngắn (~3.5m), cường độ rất thấp, đi theo camera --
// chỉ ảnh hưởng khu vực sát người chơi, xa hơn vẫn tối như thiết kế.
public class PlayerFillLight : MonoBehaviour
{
    // SỬA 2026-07-27: 0.25 gần như vô hình -- scene này dùng thang cường độ Point/Spot RẤT LỚN (đèn pin
    // maxIntensity = 80!), 0.25 chỉ đủ tạo 1 đốm specular li ti trên bề mặt bóng (cửa sổ kính) chứ không đủ
    // khuếch tán sáng thấy được, đúng hiện tượng "thấy phản chiếu trong cửa sổ nhưng góc nhìn thường thì
    // chẳng sáng lên gì cả". Tăng lên đúng thang -- vẫn thấp hơn nhiều so với đèn pin (80) để không lấn át.
    [Tooltip("Thấp hơn NHIỀU so với đèn pin (80) nhưng đủ thấy khuếch tán thật, không chỉ lóe specular trên mặt bóng")]
    [SerializeField] private float _intensity = 4f;
    [SerializeField] private float _range = 3.5f;
    [Tooltip("Vàng ấm -- theo yêu cầu Jok, cảm giác ấm hơn hẳn tông lạnh trước đó")]
    [SerializeField] private Color _color = new Color(0.85f, 0.72f, 0.5f);

    private Light _light;

    private void Awake()
    {
        _light = GetComponent<Light>();
        if (_light == null) _light = gameObject.AddComponent<Light>();

        _light.type      = LightType.Point;
        _light.intensity = _intensity;
        _light.range     = _range;
        _light.color     = _color;
        _light.shadows   = LightShadows.None; // không cần đổ bóng -- tránh xung đột/tốn hiệu năng thêm với bóng đèn pin
        _light.renderMode = LightRenderMode.ForcePixel;
    }
}
