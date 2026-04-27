using UnityEngine;
using UnityEngine.Events;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventorySystem _inventorySystem;

    private bool _isOpen = false;
    public bool IsOpen => _isOpen;

    public UnityEvent OnOpen  = new UnityEvent();
    public UnityEvent OnClose = new UnityEvent();

    private void Update()
    {
        // TODO: if (Input.GetKeyDown(KeyCode.Tab)) Toggle();
    }

    public void Toggle()
    {
        // TODO: gọi Open() nếu đang đóng, gọi Close() nếu đang mở
        throw new System.NotImplementedException();
    }

    public void Open()
    {
        // TODO: _isOpen = true, bật canvas/panel, invoke OnOpen, gọi Refresh()
        throw new System.NotImplementedException();
    }

    public void Close()
    {
        // TODO: _isOpen = false, tắt canvas/panel, invoke OnClose
        throw new System.NotImplementedException();
    }

    public void Refresh()
    {
        // TODO: lấy _inventorySystem.GetAllItems()
        //       cập nhật từng slot UI, slot trống thì clear icon/text
        throw new System.NotImplementedException();
    }

    public void OnItemClicked(string itemId)
    {
        // TODO: phát AudioClip monologue qua AudioManager.Instance.PlaySFX(clip)
        //       hiện tên + mô tả item nếu có UI text
        throw new System.NotImplementedException();
    }
}
