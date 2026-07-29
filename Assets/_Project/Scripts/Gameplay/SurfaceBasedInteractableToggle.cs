using UnityEngine;

// Gắn lên Player -- tự bật/tắt Collider của 1 danh sách vật tương tác NGOÀI TRỜI (VD 2 khối đá chặn phía
// sau) tuỳ theo bề mặt Player đang đứng: đứng trên "Wood" (trong nhà) -> tắt hẳn, đứng trên bề mặt KHÁC
// Wood (VD "Grass" -- ngoài trời) -> bật lại. Dùng lại đúng cách raycast-xuống-lấy-PhysicsMaterial đã có
// trong FootstepSystem.cs (không phải trigger zone cố định -- vì "trong nhà" hay "ngoài trời" ở khu này
// không tách bạch rõ bằng 1 vùng không gian đơn giản, phải theo đúng chất liệu sàn thật đang đứng).
public class SurfaceBasedInteractableToggle : MonoBehaviour
{
    [Tooltip("Tên PhysicsMaterial coi là 'trong nhà' -- đứng trên bề mặt này thì TẮT tương tác. Bất kỳ bề mặt nào KHÁC tên này (VD Grass) đều coi là 'ngoài trời' -> BẬT lại.")]
    [SerializeField] private string _indoorMaterialName = "Wood";

    [Tooltip("Các vật tương tác NGOÀI TRỜI cần tắt/bật theo bề mặt -- kéo Transform gốc của từng object vào đây")]
    [SerializeField] private Transform[] _outdoorOnlyInteractables;

    [Tooltip("Khoảng cách giữa 2 lần check -- không cần mỗi frame, đứng yên vẫn tự cập nhật đúng")]
    [SerializeField] private float _checkInterval = 0.2f;

    [SerializeField] private float _rayLength = 1.2f;
    [SerializeField] private LayerMask _groundLayer = ~0;

    [Header("Ambient ngoài trời -- fade theo ĐÚNG bề mặt vừa detect ở trên (Wood = tắt hẳn, Grass/khác = fade in lại)")]
    [SerializeField] private AmbientZone _exteriorAmbient;
    [SerializeField] private float _outdoorAmbientVolume = 0.65f;
    [SerializeField] private float _indoorAmbientVolume = 0f;
    [SerializeField] private float _ambientFadeDuration = 2f;

    private float _timer;
    private bool? _lastIndoor; // null = chưa check lần nào

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < _checkInterval) return;
        _timer = 0f;

        bool isIndoor = DetectIndoor();
        if (_lastIndoor.HasValue && _lastIndoor.Value == isIndoor) return; // không đổi trạng thái, khỏi set lại Collider mỗi lần
        _lastIndoor = isIndoor;

        SetOutdoorInteractablesEnabled(!isIndoor);

        _exteriorAmbient?.FadeToVolume(isIndoor ? _indoorAmbientVolume : _outdoorAmbientVolume, _ambientFadeDuration);
    }

    private bool DetectIndoor()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.05f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, _rayLength, _groundLayer))
        {
            PhysicsMaterial mat = hit.collider.sharedMaterial;
            if (mat != null && mat.name.Equals(_indoorMaterialName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private void SetOutdoorInteractablesEnabled(bool enabled)
    {
        if (_outdoorOnlyInteractables == null) return;
        foreach (var t in _outdoorOnlyInteractables)
        {
            if (t == null) continue;

            // BUG THẬT (Jok phát hiện): RubbleBlocker_Dong_Back/_Tay_Back mỗi cái có tới 4 Collider (1 trên
            // object gốc + 3 collider con "Large Rock 1"/"Rock Group"/"FunctionalBlock" cho hình khối 3D
            // thật) -- trước đây chỉ tắt ĐÚNG 1 Collider trên object gốc (GetComponent không đệ quy). Raycast
            // tương tác (InteractionSystem.cs) vẫn trúng các Collider con còn bật, rồi tự tìm IInteractable
            // ngược lên object CHA -- vẫn tương tác được dù tưởng đã tắt. Giờ tắt HẾT collider con cháu luôn.
            foreach (var col in t.GetComponentsInChildren<Collider>(includeInactive: true))
                col.enabled = enabled;
        }
    }
}
