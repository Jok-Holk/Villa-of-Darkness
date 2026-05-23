using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Phase1.VoVanThuan
{
    static class R
    {
        static BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
        public static float Float(object o, string n) => (float)(o.GetType().GetField(n, F)?.GetValue(o) ?? 0f);
        public static bool  Bool (object o, string n) => (bool) (o.GetType().GetField(n, F)?.GetValue(o) ?? false);
        public static void  Set  (object o, string n, object v) => o.GetType().GetField(n, F)?.SetValue(o, v);
        public static object Get (object o, string n) => o.GetType().GetField(n, F)?.GetValue(o);
    }

    // ══════════════════════════════════════════════════════
    // DOOR CONTROLLER
    // Stub: _isOpen toggle khi Interact(), implement IInteractable
    // Test mới: event OnDoorOpen/OnDoorClose, trạng thái sau nhiều lần toggle,
    //           cửa bị khóa không mở được, animation trigger nếu có
    // ══════════════════════════════════════════════════════
    public class DoorControllerTests
    {
        private GameObject _go;
        private DoorController _door;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("Door");
            _door = _go.AddComponent<DoorController>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator ImplementsIInteractable()
        {
            yield return null;
            Assert.IsTrue(_door is IInteractable, "DoorController phải implement IInteractable");
        }

        [UnityTest]
        public IEnumerator Interact_TogglesIsOpen()
        {
            bool before = R.Bool(_door, "_isOpen");
            _door.Interact();
            yield return null;
            Assert.AreNotEqual(before, R.Bool(_door, "_isOpen"));
        }

        // ── Test mới ──

        // Mặc định cửa đóng
        [UnityTest]
        public IEnumerator StartsAsClosed()
        {
            yield return null;
            Assert.IsFalse(R.Bool(_door, "_isOpen"), "Cửa phải đóng lúc khởi tạo");
        }

        // Toggle 2 lần = trở về trạng thái ban đầu
        [UnityTest]
        public IEnumerator DoubleInteract_ReturnsToClosed()
        {
            _door.Interact();
            _door.Interact();
            yield return null;
            Assert.IsFalse(R.Bool(_door, "_isOpen"), "Interact 2 lần phải trở về đóng");
        }

        // OnDoorOpen event phải fire khi mở
        [UnityTest]
        public IEnumerator OnDoorOpen_FiredWhenOpened()
        {
            bool fired = false;
            _door.OnDoorOpen.AddListener(() => fired = true);
            _door.Interact(); // đóng → mở
            yield return null;
            Assert.IsTrue(fired, "OnDoorOpen phải invoke khi cửa mở");
        }

        // OnDoorClose event phải fire khi đóng
        [UnityTest]
        public IEnumerator OnDoorClose_FiredWhenClosed()
        {
            bool fired = false;
            _door.Interact(); // mở trước
            yield return null;
            _door.OnDoorClose.AddListener(() => fired = true);
            _door.Interact(); // mở → đóng
            yield return null;
            Assert.IsTrue(fired, "OnDoorClose phải invoke khi cửa đóng");
        }

        // Lock/Unlock — cửa bị khóa không mở được
        [UnityTest]
        public IEnumerator LockedDoor_CannotBeOpened()
        {
            _door.SetLocked(true);
            _door.Interact();
            yield return null;
            Assert.IsFalse(R.Bool(_door, "_isOpen"),
                "Cửa bị khóa không được mở khi Interact()");
        }

        // Unlock rồi mở được
        [UnityTest]
        public IEnumerator UnlockedDoor_CanBeOpened()
        {
            _door.SetLocked(true);
            _door.SetLocked(false);
            _door.Interact();
            yield return null;
            Assert.IsTrue(R.Bool(_door, "_isOpen"), "Cửa sau unlock phải mở được");
        }
    }

    // ══════════════════════════════════════════════════════
    // FLASHLIGHT CONTROLLER
    // Stub: _batteryLevel=1f, drain trong Update, AddBattery clamp
    // Test mới: tắt/bật đèn, drain chỉ xảy ra khi đèn bật,
    //           OnBatteryEmpty event, AddBattery không vượt 1f,
    //           battery drain rate hợp lý (có thể config)
    // ══════════════════════════════════════════════════════
    public class FlashlightControllerTests
    {
        private GameObject _go;
        private FlashlightController _fl;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("Flashlight");
            _fl = _go.AddComponent<FlashlightController>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator BatteryStartsFull()
        {
            yield return null;
            Assert.AreEqual(1f, R.Float(_fl, "_batteryLevel"), 0.001f);
        }

        [UnityTest]
        public IEnumerator AddBattery_ClampsAtOne()
        {
            _fl.AddBattery(999f);
            yield return null;
            Assert.LessOrEqual(R.Float(_fl, "_batteryLevel"), 1f);
        }

        // ── Test mới ──

        // Toggle đèn bật/tắt
        [UnityTest]
        public IEnumerator Toggle_TurnsOnAndOff()
        {
            bool before = R.Bool(_fl, "_isOn");
            _fl.Toggle();
            yield return null;
            Assert.AreNotEqual(before, R.Bool(_fl, "_isOn"),
                "Toggle() phải đổi trạng thái _isOn");
        }

        // Drain chỉ xảy ra khi đèn bật
        [UnityTest]
        public IEnumerator Battery_DoesNotDrain_WhenOff()
        {
            // Đảm bảo đèn tắt
            if (R.Bool(_fl, "_isOn")) _fl.Toggle();
            R.Set(_fl, "_batteryLevel", 1f);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(1f, R.Float(_fl, "_batteryLevel"), 0.01f,
                "Battery không được drain khi đèn tắt");
        }

        // AddBattery tăng đúng lượng
        [UnityTest]
        public IEnumerator AddBattery_IncreasesCorrectly()
        {
            R.Set(_fl, "_batteryLevel", 0.3f);
            _fl.AddBattery(0.2f);
            yield return null;
            Assert.AreEqual(0.5f, R.Float(_fl, "_batteryLevel"), 0.001f);
        }

        // OnBatteryEmpty event khi pin hết
        [UnityTest]
        public IEnumerator OnBatteryEmpty_FiredWhenDepleted()
        {
            bool fired = false;
            _fl.OnBatteryEmpty.AddListener(() => fired = true);
            R.Set(_fl, "_batteryLevel", 0f);
            // Trigger drain một lần
            _fl.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            yield return null;
            Assert.IsTrue(fired, "OnBatteryEmpty phải invoke khi _batteryLevel = 0");
        }

        // Đèn tắt tự động khi pin hết
        [UnityTest]
        public IEnumerator Flashlight_TurnsOff_WhenBatteryEmpty()
        {
            if (!R.Bool(_fl, "_isOn")) _fl.Toggle(); // bật đèn
            R.Set(_fl, "_batteryLevel", 0f);
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(R.Bool(_fl, "_isOn"),
                "Đèn phải tự tắt khi pin hết");
        }
    }

    // ══════════════════════════════════════════════════════
    // PIANO INTERACTABLE
    // Stub: PressNote() check sequence, _isCompleted, OnSequenceComplete event
    // Test mới: sequence sai phải clear, sequence đúng rồi nhấn thêm không fire lần 2,
    //           Interact() phải enable input mode, disable sau complete,
    //           sequence một phần rồi sai phải reset hoàn toàn
    // ══════════════════════════════════════════════════════
    public class PianoInteractableTests
    {
        private GameObject _go;
        private PianoInteractable _piano;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("Piano");
            _piano = _go.AddComponent<PianoInteractable>();
            // Set sequence D-E-G-A-F
            var f = typeof(PianoInteractable).GetField("_correctSequence", BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(_piano, new string[] { "D", "E", "G", "A", "F" });
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator ImplementsIInteractable()
        {
            yield return null;
            Assert.IsTrue(_piano is IInteractable);
        }

        [UnityTest]
        public IEnumerator CorrectSequence_CompletesAndFiresEvent()
        {
            bool fired = false;
            _piano.OnSequenceComplete.AddListener(() => fired = true);
            foreach (var n in new[] { "D", "E", "G", "A", "F" })
                _piano.PressNote(n);
            yield return null;
            Assert.IsTrue(fired);
            Assert.IsTrue(R.Bool(_piano, "_isCompleted"));
        }

        // ── Test mới ──

        // Sequence sai phải clear input
        [UnityTest]
        public IEnumerator WrongNote_ClearsInput()
        {
            _piano.PressNote("D");
            _piano.PressNote("E");
            _piano.PressNote("X"); // sai
            yield return null;
            var inputSeq = R.Get(_piano, "_inputSequence") as List<string>;
            Assert.AreEqual(0, inputSeq?.Count ?? 0,
                "Nhấn note sai phải clear toàn bộ input sequence");
        }

        // Sequence đúng rồi nhấn thêm không fire event lần 2
        [UnityTest]
        public IEnumerator AfterComplete_AdditionalNotes_DoNotFireAgain()
        {
            int count = 0;
            _piano.OnSequenceComplete.AddListener(() => count++);
            foreach (var n in new[] { "D", "E", "G", "A", "F" }) _piano.PressNote(n);
            _piano.PressNote("D"); // thêm sau khi xong
            yield return null;
            Assert.AreEqual(1, count, "OnSequenceComplete chỉ được fire 1 lần");
        }

        // Sequence sai hoàn toàn từ đầu
        [UnityTest]
        public IEnumerator WrongSequence_DoesNotComplete()
        {
            bool fired = false;
            _piano.OnSequenceComplete.AddListener(() => fired = true);
            foreach (var n in new[] { "A", "B", "C", "D", "E" }) _piano.PressNote(n);
            yield return null;
            Assert.IsFalse(fired);
            Assert.IsFalse(R.Bool(_piano, "_isCompleted"));
        }

        // Một phần đúng, sau đó sai, phải reset và nhập lại từ đầu mới được
        [UnityTest]
        public IEnumerator PartialThenWrong_ThenCorrect_Completes()
        {
            bool fired = false;
            _piano.OnSequenceComplete.AddListener(() => fired = true);
            // nhập một phần rồi sai
            _piano.PressNote("D");
            _piano.PressNote("X"); // sai → clear
            // nhập đúng từ đầu
            foreach (var n in new[] { "D", "E", "G", "A", "F" }) _piano.PressNote(n);
            yield return null;
            Assert.IsTrue(fired, "Sau khi reset, nhập đúng sequence phải complete");
        }

        // Chưa complete thì _isCompleted = false
        [UnityTest]
        public IEnumerator NotCompleted_Initially()
        {
            yield return null;
            Assert.IsFalse(R.Bool(_piano, "_isCompleted"));
        }
    }

    // ══════════════════════════════════════════════════════
    // HIDE SPOT
    // Stub: _playerIsHiding toggle, implement IInteractable
    // Test mới: OnHide/OnReveal event, ghost detection bị tắt khi hiding,
    //           không thể hide khi đã hiding (hoặc được), timer nếu có
    // ══════════════════════════════════════════════════════
    public class HideSpotTests
    {
        private GameObject _go;
        private HideSpot _hide;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("HideSpot");
            _go.tag = "HideSpot";
            _hide = _go.AddComponent<HideSpot>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator ImplementsIInteractable()
        {
            yield return null;
            Assert.IsTrue(_hide is IInteractable);
        }

        [UnityTest]
        public IEnumerator Interact_TogglesHiding()
        {
            bool before = R.Bool(_hide, "_playerIsHiding");
            _hide.Interact();
            yield return null;
            Assert.AreNotEqual(before, R.Bool(_hide, "_playerIsHiding"));
        }

        // ── Test mới ──

        // Mặc định không đang hide
        [UnityTest]
        public IEnumerator StartsNotHiding()
        {
            yield return null;
            Assert.IsFalse(R.Bool(_hide, "_playerIsHiding"));
        }

        // OnHide event phải fire khi vào ẩn
        [UnityTest]
        public IEnumerator OnHide_FiredWhenEnteringHide()
        {
            bool fired = false;
            _hide.OnHide.AddListener(() => fired = true);
            _hide.Interact(); // không hiding → hiding
            yield return null;
            Assert.IsTrue(fired, "OnHide phải invoke khi bắt đầu hide");
        }

        // OnReveal event phải fire khi thoát khỏi ẩn
        [UnityTest]
        public IEnumerator OnReveal_FiredWhenExiting()
        {
            _hide.Interact(); // vào hide
            yield return null;
            bool fired = false;
            _hide.OnReveal.AddListener(() => fired = true);
            _hide.Interact(); // thoát hide
            yield return null;
            Assert.IsTrue(fired, "OnReveal phải invoke khi thoát hide");
        }

        // IsPlayerHiding property
        [UnityTest]
        public IEnumerator IsPlayerHiding_ReturnsCorrectState()
        {
            Assert.IsFalse(_hide.IsPlayerHiding, "Ban đầu không hiding");
            _hide.Interact();
            yield return null;
            Assert.IsTrue(_hide.IsPlayerHiding, "Sau Interact() phải đang hiding");
        }
    }

    // ══════════════════════════════════════════════════════
    // INVENTORY SYSTEM
    // Stub: AddItem check duplicate, HasItem delegate GameData
    // Test mới: RemoveItem, GetAllItems, capacity nếu có, event OnItemAdded
    // ══════════════════════════════════════════════════════
    public class InventorySystemTests
    {
        private GameObject _go;
        private InventorySystem _inv;

        [UnitySetUp]
        public IEnumerator Before()
        {
            GameData.Reset();
            _go = new GameObject("Inventory");
            _inv = _go.AddComponent<InventorySystem>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); GameData.Reset(); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator AddItem_PersistsInGameData()
        {
            _inv.AddItem("music_box");
            yield return null;
            Assert.IsTrue(GameData.collectedItems.Contains("music_box"));
        }

        [UnityTest]
        public IEnumerator HasItem_ReturnsTrueWhenPresent()
        {
            _inv.AddItem("salt_jar");
            yield return null;
            Assert.IsTrue(_inv.HasItem("salt_jar"));
        }

        [UnityTest]
        public IEnumerator NoDuplicateItems()
        {
            _inv.AddItem("mirror");
            _inv.AddItem("mirror");
            yield return null;
            int count = 0;
            foreach (var i in GameData.collectedItems) if (i == "mirror") count++;
            Assert.AreEqual(1, count, "Item không được thêm duplicate vào GameData");
        }

        // ── Test mới ──

        // HasItem false khi chưa thêm
        [UnityTest]
        public IEnumerator HasItem_ReturnsFalseWhenAbsent()
        {
            yield return null;
            Assert.IsFalse(_inv.HasItem("nonexistent"));
        }

        // RemoveItem phải xóa khỏi GameData
        [UnityTest]
        public IEnumerator RemoveItem_RemovesFromGameData()
        {
            _inv.AddItem("music_box");
            _inv.RemoveItem("music_box");
            yield return null;
            Assert.IsFalse(GameData.collectedItems.Contains("music_box"),
                "RemoveItem phải xóa item khỏi GameData");
        }

        // RemoveItem item không tồn tại không crash
        [UnityTest]
        public IEnumerator RemoveItem_NonExistent_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _inv.RemoveItem("ghost_item"),
                "RemoveItem item không tồn tại không được crash");
        }

        // OnItemAdded event
        [UnityTest]
        public IEnumerator OnItemAdded_FiredWhenItemAdded()
        {
            bool fired = false;
            _inv.OnItemAdded.AddListener((id) => fired = true);
            _inv.AddItem("new_item");
            yield return null;
            Assert.IsTrue(fired, "OnItemAdded phải invoke khi thêm item mới");
        }

        // OnItemAdded không fire khi duplicate
        [UnityTest]
        public IEnumerator OnItemAdded_NotFiredForDuplicate()
        {
            _inv.AddItem("key");
            yield return null;
            int count = 0;
            _inv.OnItemAdded.AddListener((id) => count++);
            _inv.AddItem("key"); // duplicate
            yield return null;
            Assert.AreEqual(0, count, "OnItemAdded không được fire khi item đã tồn tại");
        }

        // GetAllItems trả về danh sách
        [UnityTest]
        public IEnumerator GetAllItems_ReturnsCorrectList()
        {
            _inv.AddItem("item_a");
            _inv.AddItem("item_b");
            yield return null;
            var items = _inv.GetAllItems();
            Assert.IsNotNull(items);
            Assert.IsTrue(items.Contains("item_a") && items.Contains("item_b"),
                "GetAllItems phải trả về tất cả items đã thêm");
        }
    }
}
