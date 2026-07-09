using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Phase2.VoVanThuan
{
    // ══════════════════════════════════════════════════════════════════════════════
    // INVENTORY UI TESTS
    // ══════════════════════════════════════════════════════════════════════════════
    public class InventoryUITests
    {
        private GameObject      _goSys, _goUI, _goUIFull;
        private InventorySystem _inv;
        private InventoryUI     _ui;      // bare (không có Grid — test Open/Close/Toggle)
        private InventoryUI     _uiFull;  // đầy đủ Grid+Slots — test Refresh slot

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void SetPrivateUI(InventoryUI target, string field, object value) =>
            typeof(InventoryUI)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(target, value);

        private static TMP_Text GetSlotLabel(InventoryUI ui, int i) =>
            ((TMP_Text[])typeof(InventoryUI)
                .GetField("_slotLabels", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(ui))?[i];

        private static Image GetSlotIcon(InventoryUI ui, int i) =>
            ((Image[])typeof(InventoryUI)
                .GetField("_slotIcons", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(ui))?[i];

        /// <summary>Tạo InventoryPanel đầy đủ: Panel → Grid → 8 Slot (Icon + Label).</summary>
        private InventoryUI BuildWithGrid(InventorySystem inv)
        {
            var panelGO = new GameObject("InventoryPanel_Full");
            var ui      = panelGO.AddComponent<InventoryUI>();
            SetPrivateUI(ui, "_inventorySystem", inv);

            var gridGO = new GameObject("Grid");
            gridGO.transform.SetParent(panelGO.transform, false);

            for (int i = 0; i < 8; i++)
            {
                var slotGO = new GameObject($"Slot_{i}");
                slotGO.transform.SetParent(gridGO.transform, false);
                slotGO.AddComponent<Image>();
                slotGO.AddComponent<Button>();

                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(slotGO.transform, false);
                iconGO.AddComponent<Image>().color = new Color(0.267f, 0.267f, 0.267f);

                var labelGO = new GameObject("Label");
                labelGO.transform.SetParent(slotGO.transform, false);
                labelGO.AddComponent<TextMeshProUGUI>();
            }
            return ui;
        }

        // ── SetUp / TearDown ─────────────────────────────────────────────────────

        [UnitySetUp]
        public IEnumerator Before()
        {
            GameData.Reset();

            _goSys = new GameObject("InventorySystem");
            _inv   = _goSys.AddComponent<InventorySystem>();

            _goUI = new GameObject("InventoryUI_Bare");
            _ui   = _goUI.AddComponent<InventoryUI>();
            SetPrivateUI(_ui, "_inventorySystem", _inv);

            _uiFull   = BuildWithGrid(_inv);
            _goUIFull = _uiFull.gameObject;

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Object.Destroy(_goSys);
            Object.Destroy(_goUI);
            Object.Destroy(_goUIFull);
            GameData.Reset();
            yield return null;
        }

        // ── Trạng thái khởi đầu ──────────────────────────────────────────────────

        [UnityTest] public IEnumerator StartsHidden()
        {
            yield return null;
            Assert.IsFalse(_ui.IsOpen, "InventoryUI phải đóng lúc start");
        }

        [UnityTest] public IEnumerator StartsHidden_WithSlots()
        {
            yield return null;
            Assert.IsFalse(_uiFull.IsOpen, "InventoryUI full grid cũng phải đóng lúc start");
        }

        // ── Open ─────────────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator Open_SetsIsOpenTrue()
        {
            _ui.Open();
            yield return null;
            Assert.IsTrue(_ui.IsOpen);
        }

        [UnityTest] public IEnumerator Open_ActivatesGameObject()
        {
            _ui.Open();
            yield return null;
            Assert.IsTrue(_ui.gameObject.activeSelf);
        }

        [UnityTest] public IEnumerator Open_FiredOnOpenEvent()
        {
            bool fired = false;
            _ui.OnOpen.AddListener(() => fired = true);
            _ui.Open();
            yield return null;
            Assert.IsTrue(fired, "OnOpen phải invoke khi Open()");
        }

        // ── Close ────────────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator Close_SetsIsOpenFalse()
        {
            _ui.Open(); _ui.Close();
            yield return null;
            Assert.IsFalse(_ui.IsOpen);
        }

        [UnityTest] public IEnumerator Close_DeactivatesGameObject()
        {
            _ui.Open(); _ui.Close();
            yield return null;
            Assert.IsFalse(_ui.gameObject.activeSelf);
        }

        [UnityTest] public IEnumerator Close_FiredOnCloseEvent()
        {
            _ui.Open();
            bool fired = false;
            _ui.OnClose.AddListener(() => fired = true);
            _ui.Close();
            yield return null;
            Assert.IsTrue(fired, "OnClose phải invoke khi Close()");
        }

        // ── Toggle ───────────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator Toggle_WhenClosed_Opens()
        {
            _ui.Toggle();
            yield return null;
            Assert.IsTrue(_ui.IsOpen, "Toggle khi đóng phải mở");
        }

        [UnityTest] public IEnumerator Toggle_WhenOpen_Closes()
        {
            _ui.Open(); _ui.Toggle();
            yield return null;
            Assert.IsFalse(_ui.IsOpen, "Toggle khi mở phải đóng");
        }

        [UnityTest] public IEnumerator Toggle_ThreeTimes_BackToOpen()
        {
            _ui.Toggle(); _ui.Toggle(); _ui.Toggle();
            yield return null;
            Assert.IsTrue(_ui.IsOpen, "3 lần Toggle: closed→open→closed→open");
        }

        // ── Refresh (không slots) ─────────────────────────────────────────────────

        [UnityTest] public IEnumerator Refresh_EmptyInventory_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ui.Refresh());
        }

        [UnityTest] public IEnumerator Refresh_WithItems_DoesNotCrash()
        {
            _inv.AddItem("music_box"); _inv.AddItem("mirror");
            yield return null;
            Assert.DoesNotThrow(() => _ui.Refresh());
        }

        // ── Refresh slot (có Grid) ────────────────────────────────────────────────

        [UnityTest] public IEnumerator Refresh_Grid_Has8Slots()
        {
            yield return null;
            var grid = _uiFull.transform.Find("Grid");
            Assert.IsNotNull(grid);
            Assert.AreEqual(8, grid.childCount, "Grid phải có đúng 8 slot");
        }

        [UnityTest] public IEnumerator Refresh_SlotLabel_ShowsItemId()
        {
            _inv.AddItem("music_box");
            _uiFull.Open(); // gọi Refresh() bên trong
            yield return null;
            Assert.AreEqual("music_box", GetSlotLabel(_uiFull, 0)?.text);
        }

        [UnityTest] public IEnumerator Refresh_SlotLabel_EmptyWhenNoItem()
        {
            _inv.AddItem("music_box");
            _uiFull.Open();
            yield return null;
            Assert.AreEqual(string.Empty, GetSlotLabel(_uiFull, 1)?.text,
                "Slot không có item phải label rỗng");
        }

        [UnityTest] public IEnumerator Refresh_SlotIcon_WhiteWhenFilled()
        {
            _inv.AddItem("mirror");
            _uiFull.Open();
            yield return null;
            Assert.AreEqual(Color.white, GetSlotIcon(_uiFull, 0)?.color);
        }

        [UnityTest] public IEnumerator Refresh_SlotIcon_GrayWhenEmpty()
        {
            _uiFull.Open();
            yield return null;
            Assert.AreNotEqual(Color.white, GetSlotIcon(_uiFull, 0)?.color,
                "Slot rỗng phải màu xám");
        }

        [UnityTest] public IEnumerator Refresh_AfterAddItem_UpdatesLabel()
        {
            _uiFull.Open();
            _inv.AddItem("key");
            _uiFull.Refresh();
            yield return null;
            Assert.AreEqual("key", GetSlotLabel(_uiFull, 0)?.text);
        }

        // ── OnItemClicked ─────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator OnItemClicked_ValidItem_DoesNotCrash()
        {
            _inv.AddItem("music_box");
            yield return null;
            Assert.DoesNotThrow(() => _ui.OnItemClicked("music_box"));
        }

        [UnityTest] public IEnumerator OnItemClicked_InvalidItem_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ui.OnItemClicked("nonexistent"));
        }

        [UnityTest] public IEnumerator OnItemClicked_EmptyString_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ui.OnItemClicked(string.Empty));
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // PIANO INTERACTABLE TESTS
    // ══════════════════════════════════════════════════════════════════════════════
    public class PianoInteractableTests
    {
        private GameObject        _pianoGO;
        private PianoInteractable _piano;

        private static readonly string[] Seq = { "D", "E", "G", "A", "F" };

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetP(string field, object value) =>
            typeof(PianoInteractable)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_piano, value);

        private T GetP<T>(string field) =>
            (T)(typeof(PianoInteractable)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(_piano) ?? default(T));

        // Synchronous — chỉ dùng cho Assert.DoesNotThrow (1 note/frame không đủ complete)
        private void PressAll() { foreach (var n in Seq) _piano.PressNote(n); }

        // *** FIX: Coroutine — mỗi note một frame, vượt qua guard Time.frameCount ***
        // PianoInteractable.AddNote() chặn 2 lần gọi trong cùng 1 frame bằng:
        //   if (Time.frameCount == _lastNoteFrame) return;
        // Vì vậy các test nhấn nhiều note PHẢI yield return null giữa mỗi lần gọi.
        private IEnumerator PressAllCoro()
        {
            foreach (var n in Seq)
            {
                _piano.PressNote(n);
                yield return null; // advance 1 frame → _lastNoteFrame thay đổi
            }
        }

        // ── SetUp / TearDown ─────────────────────────────────────────────────────

        [UnitySetUp]
        public IEnumerator Before()
        {
            _pianoGO = new GameObject("Piano");
            _piano   = _pianoGO.AddComponent<PianoInteractable>();
            SetP("_correctSequence", Seq);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Object.Destroy(_pianoGO);
            yield return null;
        }

        // ── Trạng thái đầu ───────────────────────────────────────────────────────

        [UnityTest] public IEnumerator InitialState_NotCompleted()
        {
            yield return null;
            Assert.IsFalse(GetP<bool>("_isCompleted"));
        }

        [UnityTest] public IEnumerator InitialState_InputEmpty()
        {
            yield return null;
            Assert.AreEqual(0, GetP<List<string>>("_inputSequence").Count);
        }

        // ── Note đúng ────────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator PressNote_FirstCorrect_AdvancesSequence()
        {
            _piano.PressNote("D");
            yield return null;
            Assert.AreEqual(1, GetP<List<string>>("_inputSequence").Count);
        }

        // FIX: yield return null giữa từng note để mỗi note đi qua guard Time.frameCount
        [UnityTest] public IEnumerator PressNote_MultipleCorrect_Accumulates()
        {
            _piano.PressNote("D"); yield return null;
            _piano.PressNote("E"); yield return null;
            _piano.PressNote("G");
            yield return null;
            Assert.AreEqual(3, GetP<List<string>>("_inputSequence").Count);
        }

        // ── Note sai ─────────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator PressNote_WrongFirst_Resets()
        {
            _piano.PressNote("A"); // sai
            yield return null;
            Assert.AreEqual(0, GetP<List<string>>("_inputSequence").Count);
        }

        // FIX: yield return null giữa từng note
        [UnityTest] public IEnumerator PressNote_WrongMidway_Resets()
        {
            _piano.PressNote("D"); yield return null;
            _piano.PressNote("E"); yield return null;
            _piano.PressNote("A"); // sai (đúng phải là G) → reset
            yield return null;
            Assert.AreEqual(0, GetP<List<string>>("_inputSequence").Count);
        }

        // FIX: yield return null sau note sai để note đúng tiếp theo qua được guard
        [UnityTest] public IEnumerator PressNote_AfterReset_CanRestart()
        {
            _piano.PressNote("X"); // sai → reset
            yield return null;     // advance frame
            _piano.PressNote("D"); // đúng lại, frame mới → qua guard
            yield return null;
            Assert.AreEqual(1, GetP<List<string>>("_inputSequence").Count);
        }

        // ── Complete ─────────────────────────────────────────────────────────────

        // FIX: dùng PressAllCoro thay cho PressAll()
        [UnityTest] public IEnumerator Complete_SetsIsCompleted()
        {
            yield return PressAllCoro();
            Assert.IsTrue(GetP<bool>("_isCompleted"));
        }

        // FIX: dùng PressAllCoro
        [UnityTest] public IEnumerator Complete_FiresOnSequenceComplete()
        {
            bool fired = false;
            _piano.OnSequenceComplete.AddListener(() => fired = true);
            yield return PressAllCoro();
            Assert.IsTrue(fired, "OnSequenceComplete phải invoke khi nhập đúng toàn bộ");
        }

        // FIX: dùng PressAllCoro
        [UnityTest] public IEnumerator Complete_OnSequenceComplete_FiredExactlyOnce()
        {
            int count = 0;
            _piano.OnSequenceComplete.AddListener(() => count++);
            yield return PressAllCoro();
            Assert.AreEqual(1, count);
        }

        // ── Idempotent sau complete ───────────────────────────────────────────────

        // FIX: dùng PressAllCoro cho cả lần 1 và lần 2
        [UnityTest] public IEnumerator AfterComplete_IgnoresInput()
        {
            int count = 0;
            _piano.OnSequenceComplete.AddListener(() => count++);
            yield return PressAllCoro(); // lần 1 — complete
            yield return PressAllCoro(); // lần 2 — phải bị bỏ qua vì _isCompleted = true
            Assert.AreEqual(1, count, "OnSequenceComplete chỉ invoke đúng 1 lần");
        }

        // FIX: dùng PressAllCoro
        [UnityTest] public IEnumerator AfterComplete_StaysCompleted()
        {
            yield return PressAllCoro();
            _piano.PressNote("X"); // gọi sau complete — phải bị bỏ qua
            yield return null;
            Assert.IsTrue(GetP<bool>("_isCompleted"));
        }

        // ── Ghost Spawn ───────────────────────────────────────────────────────────

        // Giữ nguyên: PressAll() synchronous đủ để kiểm tra không crash
        // (chỉ note đầu "D" qua, không complete — nhưng mục đích chỉ là DoesNotThrow)
        [UnityTest] public IEnumerator Complete_WithoutSpawnManager_DoesNotCrash()
        {
            SetP("_spawnManager", null);
            yield return null;
            Assert.DoesNotThrow(PressAll);
        }

        // ── Edge cases ────────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator PressNote_EmptyString_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _piano.PressNote(""));
        }

        [UnityTest] public IEnumerator PressNote_Null_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _piano.PressNote(null));
        }

        [UnityTest] public IEnumerator PressNote_EmptyCorrectSequence_DoesNotCrash()
        {
            SetP("_correctSequence", new string[0]);
            yield return null;
            Assert.DoesNotThrow(() => _piano.PressNote("D"));
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // HIDESPOT + GHOSTAI TESTS
    // ══════════════════════════════════════════════════════════════════════════════
    public class HideSpotTests
    {
        private GameObject _cabinetGO;
        private HideSpot   _hideSpot;

        private GameObject _ghostGO;
        private GhostAI    _ghost;

        private GameObject _playerGO;

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void SetG(GhostAI g, string field, object v) =>
            typeof(GhostAI)
                .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(g, v);

        private static bool CanDetect(GhostAI g) =>
            (bool)(typeof(GhostAI)
                .GetMethod("CanDetectPlayer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(g, null) ?? false);

        private static bool CanHear(GhostAI g) =>
            (bool)(typeof(GhostAI)
                .GetMethod("CanHearPlayer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(g, null) ?? false);

        // ── SetUp / TearDown ─────────────────────────────────────────────────────

        [UnitySetUp]
        public IEnumerator Before()
        {
            _cabinetGO = new GameObject("Cabinet");
            _hideSpot  = _cabinetGO.AddComponent<HideSpot>();

            _playerGO           = new GameObject("Player");
            _playerGO.tag       = "Player";
            _playerGO.transform.position = Vector3.zero;

            _ghostGO  = new GameObject("Ghost");
            _ghost    = _ghostGO.AddComponent<GhostAI>();
            _ghostGO.transform.position = new Vector3(2f, 0f, 0f); // trong tầm nhìn

            // Inject fields để test không cần NavMesh / scene thật
            SetG(_ghost, "_player",        _playerGO.transform);
            SetG(_ghost, "_sightRadius",   10f);
            SetG(_ghost, "_hearingRadius", 10f);
            SetG(_ghost, "_sightAngle",    180f); // nhìn 360° đảm bảo detect

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            if (_hideSpot != null && _hideSpot.IsPlayerHiding)
                _hideSpot.Interact(); // dọn static AnyPlayerHiding

            Object.Destroy(_cabinetGO);
            Object.Destroy(_ghostGO);
            Object.Destroy(_playerGO);
            yield return null;
        }

        // ── HideSpot state ────────────────────────────────────────────────────────

        [UnityTest] public IEnumerator HideSpot_StartsNotHiding()
        {
            yield return null;
            Assert.IsFalse(_hideSpot.IsPlayerHiding);
        }

        [UnityTest] public IEnumerator HideSpot_Interact_SetsHiding()
        {
            _hideSpot.Interact();
            yield return null;
            Assert.IsTrue(_hideSpot.IsPlayerHiding);
        }

        [UnityTest] public IEnumerator HideSpot_InteractTwice_StopsHiding()
        {
            _hideSpot.Interact(); _hideSpot.Interact();
            yield return null;
            Assert.IsFalse(_hideSpot.IsPlayerHiding);
        }

        [UnityTest] public IEnumerator HideSpot_OnHide_Fired()
        {
            bool fired = false;
            _hideSpot.OnHide.AddListener(() => fired = true);
            _hideSpot.Interact();
            yield return null;
            Assert.IsTrue(fired);
        }

        [UnityTest] public IEnumerator HideSpot_OnReveal_Fired()
        {
            _hideSpot.Interact();
            bool fired = false;
            _hideSpot.OnReveal.AddListener(() => fired = true);
            _hideSpot.Interact();
            yield return null;
            Assert.IsTrue(fired);
        }

        // ── AnyPlayerHiding static ────────────────────────────────────────────────

        [UnityTest] public IEnumerator AnyPlayerHiding_DefaultFalse()
        {
            yield return null;
            Assert.IsFalse(HideSpot.AnyPlayerHiding);
        }

        [UnityTest] public IEnumerator AnyPlayerHiding_TrueWhenHiding()
        {
            _hideSpot.Interact();
            yield return null;
            Assert.IsTrue(HideSpot.AnyPlayerHiding);
        }

        [UnityTest] public IEnumerator AnyPlayerHiding_FalseAfterReveal()
        {
            _hideSpot.Interact(); _hideSpot.Interact();
            yield return null;
            Assert.IsFalse(HideSpot.AnyPlayerHiding);
        }

        [UnityTest] public IEnumerator AnyPlayerHiding_FalseAfterDestroy()
        {
            _hideSpot.Interact();
            Object.DestroyImmediate(_cabinetGO);
            yield return null;
            Assert.IsFalse(HideSpot.AnyPlayerHiding, "Destroy HideSpot → static phải reset");
            _cabinetGO = null; _hideSpot = null; // tránh TearDown gọi lại
        }

        // ── GhostAI — không detect khi ẩn ────────────────────────────────────────

        [UnityTest] public IEnumerator GhostAI_CanDetect_TrueWhenNotHiding()
        {
            yield return null;
            Assert.IsTrue(CanDetect(_ghost), "Ghost phải detect player khi không ẩn");
        }

        [UnityTest] public IEnumerator GhostAI_CanDetect_FalseWhenHiding()
        {
            _hideSpot.Interact();
            yield return null;
            Assert.IsFalse(CanDetect(_ghost), "Ghost KHÔNG detect player khi đang ẩn");
        }

        [UnityTest] public IEnumerator GhostAI_CanHear_TrueWhenNotHiding()
        {
            yield return null;
            Assert.IsTrue(CanHear(_ghost), "Ghost phải nghe thấy player khi không ẩn");
        }

        [UnityTest] public IEnumerator GhostAI_CanHear_FalseWhenHiding()
        {
            _hideSpot.Interact();
            yield return null;
            Assert.IsFalse(CanHear(_ghost), "Ghost KHÔNG nghe thấy player khi đang ẩn");
        }

        [UnityTest] public IEnumerator GhostAI_CanDetect_TrueAgainAfterReveal()
        {
            _hideSpot.Interact(); _hideSpot.Interact();
            yield return null;
            Assert.IsTrue(CanDetect(_ghost), "Sau khi ra tủ ghost detect lại bình thường");
        }

        [UnityTest] public IEnumerator GhostAI_CanHear_TrueAgainAfterReveal()
        {
            _hideSpot.Interact(); _hideSpot.Interact();
            yield return null;
            Assert.IsTrue(CanHear(_ghost), "Sau khi ra tủ ghost nghe lại bình thường");
        }

        [UnityTest] public IEnumerator GhostAI_CanDetect_FalseWhenPlayerNull()
        {
            SetG(_ghost, "_player", null);
            yield return null;
            Assert.IsFalse(CanDetect(_ghost));
        }

        [UnityTest] public IEnumerator GhostAI_CanDetect_FalseWhenTooFar()
        {
            _ghostGO.transform.position = new Vector3(20f, 0f, 0f);
            yield return null;
            Assert.IsFalse(CanDetect(_ghost), "Ngoài tầm nhìn → không detect");
        }
    }
}