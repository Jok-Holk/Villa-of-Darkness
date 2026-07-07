using UnityEngine;
using UnityEngine.Events;

public class HideSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _hidePosition;

    private bool _playerIsHiding = false;
    private Vector3 _playerReturnPosition;
    private int _hideFrame = -1;

    // Biến static lưu lại frame vừa tương tác để chống lỗi double trigger (bị hút ngược vào lại)
    private static int _lastInteractFrame = -1;

    public UnityEvent OnHide   = new UnityEvent();
    public UnityEvent OnReveal = new UnityEvent();

    public bool IsPlayerHiding => _playerIsHiding;

    private static HideSpot _currentActive;

    public static bool AnyPlayerHiding =>
        _currentActive != null && _currentActive._playerIsHiding;

    private void Start()
    {
        // Tự động tìm Player trên Scene nếu bạn quên kéo vào Inspector
        if (_playerController == null)
        {
            _playerController = FindAnyObjectByType<PlayerController>();
        }
    }

    private void Update()
    {
        if (!_playerIsHiding) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (Time.frameCount == _hideFrame) return;
        Interact();
    }

    public void Interact()
    {
        // NẾU TRONG CÙNG 1 FRAME MÀ GỌI LẠI HÀM NÀY (Do InteractionSystem quét trúng) -> BỎ QUA KHÔNG XỬ LÝ
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        _playerIsHiding = !_playerIsHiding;

        if (_playerIsHiding)
        {
            _currentActive = this;
            _hideFrame     = Time.frameCount;

            if (_playerController != null)
            {
                _playerReturnPosition = _playerController.transform.position;

                // 1. TẮT CharacterController và Collider TRƯỚC KHI DỊCH CHUYỂN
                var cc = _playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                var col = _playerController.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                // 2. DỊCH CHUYỂN VÀO CHỖ TRỐN (SAU KHI ĐÃ TẮT VẬT LÝ)
                Vector3 hidePos = _hidePosition != null
                    ? _hidePosition.position
                    : transform.position;
                _playerController.transform.position = hidePos;
                
                // FIX: Chỉ khóa di chuyển WASD, không khóa xoay chuột (Camera vẫn quay được tự do khi trốn)
                _playerController.SetMovementEnabled(false); 
            }

            Debug.Log($"[HideSpot] VÀO GIƯỜNG — AnyPlayerHiding = {AnyPlayerHiding}");
            OnHide.Invoke();
        }
        else
        {
            if (_currentActive == this) _currentActive = null;

            if (_playerController != null)
            {
                // 1. ĐẢM BẢO CharacterController ĐANG TẮT ĐỂ CÓ THỂ DỊCH CHUYỂN
                var cc = _playerController.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                // 2. DỊCH CHUYỂN VỀ VỊ TRÍ CŨ bên ngoài giường
                _playerController.transform.position = _playerReturnPosition;

                // 3. BẬT LẠI CharacterController VÀ COLLIDER SAU KHI ĐÃ ĐẾN NƠI AN TOÀN
                if (cc != null) cc.enabled = true;
                
                var col = _playerController.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                // Bật lại toàn bộ hệ thống Input điều khiển như cũ
                _playerController.SetInputEnabled(true);
            }

            Debug.Log($"[HideSpot] THOÁT GIƯỜNG — AnyPlayerHiding = {AnyPlayerHiding}");
            OnReveal.Invoke();
        }
    }

    private void OnDestroy()
    {
        if (_currentActive == this) _currentActive = null;
    }
}