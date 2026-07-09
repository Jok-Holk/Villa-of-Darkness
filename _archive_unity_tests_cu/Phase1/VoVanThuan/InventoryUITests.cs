using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

namespace Phase1.VoVanThuan
{
    public class InventoryUITests
    {
        private GameObject    _goSys, _goUI;
        private InventorySystem _inv;
        private InventoryUI     _ui;

        [UnitySetUp]
        public IEnumerator Before()
        {
            GameData.Reset();
            _goSys = new GameObject("InventorySystem");
            _inv   = _goSys.AddComponent<InventorySystem>();

            _goUI = new GameObject("InventoryUI");
            _ui   = _goUI.AddComponent<InventoryUI>();

            var f = typeof(InventoryUI).GetField("_inventorySystem",
                        BindingFlags.NonPublic | BindingFlags.Instance);
            f?.SetValue(_ui, _inv);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Object.Destroy(_goSys);
            Object.Destroy(_goUI);
            GameData.Reset();
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartsHidden()
        {
            yield return null;
            Assert.IsFalse(_ui.IsOpen, "Inventory UI phải đóng lúc start");
        }

        [UnityTest]
        public IEnumerator Open_SetsIsOpenTrue()
        {
            _ui.Open();
            yield return null;
            Assert.IsTrue(_ui.IsOpen);
        }

        [UnityTest]
        public IEnumerator Close_SetsIsOpenFalse()
        {
            _ui.Open();
            _ui.Close();
            yield return null;
            Assert.IsFalse(_ui.IsOpen);
        }

        [UnityTest]
        public IEnumerator Toggle_WhenClosed_Opens()
        {
            _ui.Toggle();
            yield return null;
            Assert.IsTrue(_ui.IsOpen, "Toggle khi đóng phải mở");
        }

        [UnityTest]
        public IEnumerator Toggle_WhenOpen_Closes()
        {
            _ui.Open();
            _ui.Toggle();
            yield return null;
            Assert.IsFalse(_ui.IsOpen, "Toggle khi mở phải đóng");
        }

        [UnityTest]
        public IEnumerator OnOpen_FiredWhenOpened()
        {
            bool fired = false;
            _ui.OnOpen.AddListener(() => fired = true);
            _ui.Open();
            yield return null;
            Assert.IsTrue(fired, "OnOpen phải invoke khi mở");
        }

        [UnityTest]
        public IEnumerator OnClose_FiredWhenClosed()
        {
            _ui.Open();
            bool fired = false;
            _ui.OnClose.AddListener(() => fired = true);
            _ui.Close();
            yield return null;
            Assert.IsTrue(fired, "OnClose phải invoke khi đóng");
        }

        [UnityTest]
        public IEnumerator Refresh_EmptyInventory_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ui.Refresh());
        }

        [UnityTest]
        public IEnumerator Refresh_WithItems_DoesNotCrash()
        {
            _inv.AddItem("music_box");
            _inv.AddItem("mirror");
            yield return null;
            Assert.DoesNotThrow(() => _ui.Refresh());
        }

        [UnityTest]
        public IEnumerator OnItemClicked_ValidItem_DoesNotCrash()
        {
            _inv.AddItem("music_box");
            yield return null;
            Assert.DoesNotThrow(() => _ui.OnItemClicked("music_box"));
        }

        [UnityTest]
        public IEnumerator OnItemClicked_InvalidItem_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ui.OnItemClicked("nonexistent"));
        }
    }
}
