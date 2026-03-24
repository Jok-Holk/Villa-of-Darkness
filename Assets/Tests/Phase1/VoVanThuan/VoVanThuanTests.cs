using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

namespace Phase1.VoVanThuan
{
    static class R
    {
        static BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
        public static bool   Bool (object o, string n) => (bool  )(o.GetType().GetField(n,F)?.GetValue(o) ?? false);
        public static float  Float(object o, string n) => (float )(o.GetType().GetField(n,F)?.GetValue(o) ?? 0f);
        public static void   Set  (object o, string n, object v) => o.GetType().GetField(n,F)?.SetValue(o,v);
    }

    // ════════════════════════════════════
    // DOOR CONTROLLER
    // ════════════════════════════════════
    public class DoorControllerTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("Door");
            _go.AddComponent<BoxCollider>();
            _go.AddComponent<DoorController>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator ImplementsIInteractable()
        {
            yield return null;
            Assert.IsNotNull(_go.GetComponent<IInteractable>(),
                "DoorController phải implement IInteractable");
        }

        [UnityTest]
        public IEnumerator Interact_TogglesOpenState()
        {
            var door = _go.GetComponent<DoorController>();
            bool before = R.Bool(door, "_isOpen");
            door.Interact();
            yield return null;
            bool after = R.Bool(door, "_isOpen");
            Assert.AreNotEqual(before, after, "Interact() phải toggle _isOpen");
        }

        [UnityTest]
        public IEnumerator InteractTwice_ReturnsToOriginalState()
        {
            var door = _go.GetComponent<DoorController>();
            bool original = R.Bool(door, "_isOpen");
            door.Interact();
            door.Interact();
            yield return null;
            Assert.AreEqual(original, R.Bool(door, "_isOpen"),
                "2 lần Interact() phải trở về state ban đầu");
        }
    }

    // ════════════════════════════════════
    // FLASHLIGHT CONTROLLER
    // ════════════════════════════════════
    public class FlashlightControllerTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("Flashlight");
            _go.AddComponent<Light>();
            _go.AddComponent<FlashlightController>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator StartsFullBattery()
        {
            yield return null;
            Assert.AreEqual(1f, R.Float(_go.GetComponent<FlashlightController>(), "_batteryLevel"), 0.01f);
        }

        [UnityTest]
        public IEnumerator BatteryDecreasesOverTime()
        {
            var fc = _go.GetComponent<FlashlightController>();
            float before = R.Float(fc, "_batteryLevel");
            yield return new WaitForSeconds(0.3f);
            Assert.Less(R.Float(fc, "_batteryLevel"), before,
                "Battery phải giảm khi đèn bật");
        }

        [UnityTest]
        public IEnumerator AddBattery_IncreasesLevel()
        {
            var fc = _go.GetComponent<FlashlightController>();
            R.Set(fc, "_batteryLevel", 0.2f);
            yield return null;
            fc.AddBattery(0.3f);
            Assert.AreEqual(0.5f, R.Float(fc, "_batteryLevel"), 0.01f);
        }

        [UnityTest]
        public IEnumerator AddBattery_ClampsAtOne()
        {
            var fc = _go.GetComponent<FlashlightController>();
            R.Set(fc, "_batteryLevel", 0.9f);
            fc.AddBattery(0.5f);
            yield return null;
            Assert.LessOrEqual(R.Float(fc, "_batteryLevel"), 1f);
        }
    }

    // ════════════════════════════════════
    // PIANO INTERACTABLE
    // ════════════════════════════════════
    public class PianoInteractableTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("Piano");
            _go.AddComponent<PianoInteractable>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator ImplementsIInteractable()
        {
            yield return null;
            Assert.IsNotNull(_go.GetComponent<IInteractable>());
        }

        [UnityTest]
        public IEnumerator WrongSequence_NotCompleted()
        {
            var piano = _go.GetComponent<PianoInteractable>();
            piano.PressNote("A");
            piano.PressNote("A");
            piano.PressNote("A");
            yield return null;
            Assert.IsFalse(R.Bool(piano, "_isCompleted"));
        }

        [UnityTest]
        public IEnumerator CorrectSequence_Completes()
        {
            var piano = _go.GetComponent<PianoInteractable>();
            var seq = new string[] { "D", "E", "G", "A", "F" };
            R.Set(piano, "_correctSequence", seq);
            yield return null;
            foreach (var note in seq) piano.PressNote(note);
            Assert.IsTrue(R.Bool(piano, "_isCompleted"),
                "Đúng sequence phải set _isCompleted = true");
        }

        [UnityTest]
        public IEnumerator PartialSequence_NotCompleted()
        {
            var piano = _go.GetComponent<PianoInteractable>();
            var seq = new string[] { "D", "E", "G", "A", "F" };
            R.Set(piano, "_correctSequence", seq);
            piano.PressNote("D");
            piano.PressNote("E");
            yield return null;
            Assert.IsFalse(R.Bool(piano, "_isCompleted"),
                "Sequence chưa đủ không được mark completed");
        }
    }

    // ════════════════════════════════════
    // HIDE SPOT
    // ════════════════════════════════════
    public class HideSpotTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("HideSpot");
            _go.tag = "HideSpot";
            _go.AddComponent<BoxCollider>();
            _go.AddComponent<HideSpot>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator ImplementsIInteractable()
        {
            yield return null;
            Assert.IsNotNull(_go.GetComponent<IInteractable>());
        }

        [UnityTest]
        public IEnumerator Interact_SetsHidingTrue()
        {
            var spot = _go.GetComponent<HideSpot>();
            spot.Interact();
            yield return null;
            Assert.IsTrue(R.Bool(spot, "_playerIsHiding"));
        }

        [UnityTest]
        public IEnumerator InteractTwice_ExitsHiding()
        {
            var spot = _go.GetComponent<HideSpot>();
            spot.Interact();
            spot.Interact();
            yield return null;
            Assert.IsFalse(R.Bool(spot, "_playerIsHiding"),
                "Interact() lần 2 phải thoát khỏi hiding");
        }
    }

    // ════════════════════════════════════
    // INVENTORY SYSTEM
    // ════════════════════════════════════
    public class InventorySystemTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            GameData.Reset();
            _go = new GameObject("Inventory");
            _go.AddComponent<InventorySystem>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator AddItem_AppearsInGameData()
        {
            var inv = _go.GetComponent<InventorySystem>();
            inv.AddItem("music_box");
            yield return null;
            Assert.IsTrue(GameData.collectedItems.Contains("music_box"));
        }

        [UnityTest]
        public IEnumerator AddSameItem_NoDuplicate()
        {
            var inv = _go.GetComponent<InventorySystem>();
            inv.AddItem("music_box");
            inv.AddItem("music_box");
            yield return null;
            int count = 0;
            foreach (var i in GameData.collectedItems)
                if (i == "music_box") count++;
            Assert.AreEqual(1, count, "Không được thêm item trùng lặp");
        }

        [UnityTest]
        public IEnumerator HasItem_True_AfterAdd()
        {
            var inv = _go.GetComponent<InventorySystem>();
            inv.AddItem("salt_jar");
            yield return null;
            Assert.IsTrue(inv.HasItem("salt_jar"));
        }

        [UnityTest]
        public IEnumerator HasItem_False_WhenNotAdded()
        {
            var inv = _go.GetComponent<InventorySystem>();
            yield return null;
            Assert.IsFalse(inv.HasItem("nonexistent"));
        }

        [UnityTest]
        public IEnumerator ThreeKeyItems_AllStoredCorrectly()
        {
            var inv = _go.GetComponent<InventorySystem>();
            inv.AddItem("music_box");
            inv.AddItem("silver_mirror");
            inv.AddItem("salt_jar");
            yield return null;
            Assert.IsTrue(inv.HasItem("music_box"));
            Assert.IsTrue(inv.HasItem("silver_mirror"));
            Assert.IsTrue(inv.HasItem("salt_jar"));
        }
    }
}
