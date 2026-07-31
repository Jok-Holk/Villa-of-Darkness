using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HideSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Transform _hidePosition;

    [Header("Door")]
    [SerializeField] private DoorController _door;
    [SerializeField] private float _doorWaitTime = 0.4f; // thời gian chờ cửa xoay xong

    [Header("Cinematic — lướt camera thay vì teleport thẳng")]
    [Tooltip("Thời gian lướt camera vào/ra (giây) — KHÔNG phải teleport tức thì nữa")]
    [SerializeField] private float _slideDuration = 0.6f;

    private bool _playerIsHiding = false;
    private Vector3 _playerReturnPosition;
    private Quaternion _playerReturnRotation;
    private int _hideFrame = -1;
    private bool _isBusy = false;

    private static int _lastInteractFrame = -1;
    private static HideSpot _currentActive;

    public UnityEvent OnHide   = new UnityEvent();
    public UnityEvent OnReveal = new UnityEvent();

    public bool IsPlayerHiding => _playerIsHiding;
    public static bool AnyPlayerHiding =>
        _currentActive != null && _currentActive._playerIsHiding;

    // THÊM -- GhostAI cần biết chính xác tủ nào đang có Player trốn bên trong (để tự mở cửa + kill bằng
    // code khi Player bật đèn pin, vì Collider Player đang bị tắt lúc trốn nên không va chạm vật lý được).
    public static HideSpot CurrentActive => _currentActive;

    // THÊM -- để GhostAI mở đúng cửa của tủ này khi bắt được Player.
    public DoorController Door => _door;

    private void Start()
    {
        if (_playerController == null)
            _playerController = PlayerController.Instance != null
                ? PlayerController.Instance
                : FindAnyObjectByType<PlayerController>();
    }

    private void Update()
    {
        if (!_playerIsHiding || _isBusy) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (Time.frameCount == _hideFrame) return;
        Interact();
    }

    public void Interact()
    {
        if (_isBusy) return;
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        if (!_playerIsHiding)
            StartCoroutine(EnterRoutine());
        else
            StartCoroutine(ExitRoutine());
    }

    // THÊM 2026-07-31 (cutscene "bị dí phải trốn ngay" -- ForcedHideCutscene.cs): Player KHÔNG tự bấm E,
    // cutscene ép trốn thẳng. Tái dùng NGUYÊN EnterRoutine() có sẵn (camera slide + mở/hé cửa) -- không viết
    // logic trốn riêng, tránh lệch hành vi so với lúc Player tự tương tác bình thường.
    public void ForceEnter()
    {
        if (_playerIsHiding || _isBusy) return;
        StartCoroutine(EnterRoutine());
    }

    private IEnumerator EnterRoutine()
    {
        _isBusy = true;
        _currentActive = this;
        _hideFrame = Time.frameCount;

        // Khoá cả move+look trong lúc lướt camera — tránh chuột người chơi giằng co với lerp xoay.
        if (_playerController != null) _playerController.SetInputEnabled(false);

        // 1. MỞ CỬA trước
        if (_door != null)
        {
            Debug.Log("[HideSpot] Gọi Door.Open()");
            _door.Open();
        }
        yield return new WaitForSeconds(_doorWaitTime);

        // 2. LƯỚT CAMERA VÀO (không teleport) — position + rotation cùng lerp, rotation lấy theo đúng
        // hướng của _hidePosition (thường xoay ngược 180° để nhìn ra khe cửa — set sẵn trong Editor).
        if (_playerController != null)
        {
            _playerReturnPosition = _playerController.transform.position;
            _playerReturnRotation = _playerController.transform.rotation;

            var cc = _playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            var col = _playerController.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Vector3 fromPos = _playerController.transform.position;
            Quaternion fromRot = _playerController.transform.rotation;
            Vector3 toPos = _hidePosition != null ? _hidePosition.position : transform.position;
            Quaternion toRot = _hidePosition != null ? _hidePosition.rotation : fromRot;

            float t = 0f;
            while (t < _slideDuration)
            {
                t += Time.deltaTime;
                float k = t / _slideDuration;
                _playerController.transform.SetPositionAndRotation(
                    Vector3.Lerp(fromPos, toPos, k),
                    Quaternion.Slerp(fromRot, toRot, k));
                yield return null;
            }
            _playerController.transform.SetPositionAndRotation(toPos, toRot);

            // Đã vào hẳn — mở lại camera look (nhìn quanh qua khe cửa được), vẫn khoá di chuyển.
            _playerController.SetLookEnabled(true);
        }

        // 3. HÉ CỬA LẠI (không đóng kín — đúng style trốn, chừa khe nhìn ra ngoài)
        if (_door != null)
        {
            Debug.Log("[HideSpot] Gọi Door.SetAjar()");
            _door.SetAjar();
        }
        yield return new WaitForSeconds(_doorWaitTime);

        _playerIsHiding = true;
        _isBusy = false;
        Debug.Log($"[HideSpot] VÀO TỦ — AnyPlayerHiding = {AnyPlayerHiding}");
        OnHide.Invoke();
    }

    private IEnumerator ExitRoutine()
    {
        _isBusy = true;

        // Khoá lại move+look trong lúc lướt camera ra.
        if (_playerController != null) _playerController.SetInputEnabled(false);

        // 1. HÉ CỬA MỞ HẲN RA
        if (_door != null) _door.Open();
        yield return new WaitForSeconds(_doorWaitTime);

        // 2. LƯỚT CAMERA RA NGOÀI (không teleport), rotation trả về đúng hướng lúc vào
        if (_playerController != null)
        {
            var cc = _playerController.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Vector3 fromPos = _playerController.transform.position;
            Quaternion fromRot = _playerController.transform.rotation;

            float t = 0f;
            while (t < _slideDuration)
            {
                t += Time.deltaTime;
                float k = t / _slideDuration;
                _playerController.transform.SetPositionAndRotation(
                    Vector3.Lerp(fromPos, _playerReturnPosition, k),
                    Quaternion.Slerp(fromRot, _playerReturnRotation, k));
                yield return null;
            }
            _playerController.transform.SetPositionAndRotation(_playerReturnPosition, _playerReturnRotation);

            if (cc != null) cc.enabled = true;

            var col = _playerController.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            _playerController.SetInputEnabled(true);
        }

        // 3. ĐÓNG CỬA HẲN LẠI
        if (_door != null) _door.Close();
        yield return new WaitForSeconds(_doorWaitTime);

        if (_currentActive == this) _currentActive = null;
        _playerIsHiding = false;
        _isBusy = false;
        Debug.Log($"[HideSpot] THOÁT TỦ — AnyPlayerHiding = {AnyPlayerHiding}");
        OnReveal.Invoke();
    }

    // THÊM -- Ghost gọi trực tiếp khi phát hiện Player trốn mà bật đèn pin: Collider Player đang bị tắt
    // (xem EnterRoutine) nên không thể va chạm vật lý để kill như bình thường. Hàm này mở cửa HẲN ra (không
    // phải hé cửa SetAjar -- cần lộ rõ Player cho cinematic jumpscare) và thoát trạng thái trốn ngay lập
    // tức. Không chạy lại ExitRoutine (không cần lướt camera êm ái nữa vì đây là bị bắt, không phải Player
    // chủ động thoát ra).
    public void ForceCaughtOpen()
    {
        if (_door != null) _door.Open();

        if (_currentActive == this) _currentActive = null;
        _playerIsHiding = false;
    }

    private void OnDestroy()
    {
        if (_currentActive == this) _currentActive = null;
    }
}