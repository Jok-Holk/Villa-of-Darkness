using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

namespace Phase1.NguyenHuuPhuc
{
    static class R
    {
        static BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
        public static float Float(object o, string n) => (float)(o.GetType().GetField(n, F)?.GetValue(o) ?? 0f);
        public static bool Bool(object o, string n) => (bool)(o.GetType().GetField(n, F)?.GetValue(o) ?? false);
        public static void Set(object o, string n, object v) => o.GetType().GetField(n, F)?.SetValue(o, v);
    }

    // ══════════════════════════════════════════════════════
    // TRIGGER ZONE
    // ══════════════════════════════════════════════════════
    public class TriggerZoneTests
    {
        private GameObject _go;
        private TriggerZone _tz;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("TriggerZone");
            var col = _go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            _tz = _go.AddComponent<TriggerZone>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator HasTriggerCollider()
        {
            yield return null;
            var col = _go.GetComponent<Collider>();
            Assert.IsNotNull(col);
            Assert.IsTrue(col.isTrigger);
        }

        [UnityTest]
        public IEnumerator HasOnTriggeredEvent()
        {
            yield return null;
            var f = typeof(TriggerZone).GetField("OnTriggered", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(f, "OnTriggered phải là public UnityEvent");
        }

        // ── Test mới ──

        [UnityTest]
        public IEnumerator CorrectTag_FiresEvent()
        {
            var tagField = typeof(TriggerZone).GetField("_targetTag", BindingFlags.NonPublic | BindingFlags.Instance);
            tagField?.SetValue(_tz, "Player");

            bool fired = false;
            _tz.OnTriggered.AddListener(() => fired = true);

            var player = new GameObject("Player");
            player.tag = "Player";
            var rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            player.AddComponent<BoxCollider>();
            player.transform.position = _go.transform.position;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsTrue(fired, "Object đúng tag phải kích hoạt OnTriggered");
            Object.Destroy(player);
        }

        [UnityTest]
        public IEnumerator WrongTag_DoesNotFireEvent()
        {
            var tagField = typeof(TriggerZone).GetField("_targetTag", BindingFlags.NonPublic | BindingFlags.Instance);
            tagField?.SetValue(_tz, "Player");

            bool fired = false;
            _tz.OnTriggered.AddListener(() => fired = true);

            var enemy = new GameObject("Enemy");
            enemy.tag = "Untagged";
            var rb = enemy.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            enemy.AddComponent<BoxCollider>();
            enemy.transform.position = _go.transform.position;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsFalse(fired, "Object sai tag không được kích hoạt OnTriggered");
            Object.Destroy(enemy);
        }

        [UnityTest]
        public IEnumerator Disabled_DoesNotFire()
        {
            _tz.enabled = false;
            bool fired = false;
            _tz.OnTriggered.AddListener(() => fired = true);

            var player = new GameObject("Player");
            player.tag = "Player";
            var rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            player.AddComponent<BoxCollider>();
            player.transform.position = _go.transform.position;

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsFalse(fired, "TriggerZone bị disabled không được fire");
            Object.Destroy(player);
        }

        [UnityTest]
        public IEnumerator TargetTag_IsConfigurable()
        {
            yield return null;
            var f = typeof(TriggerZone).GetField("_targetTag", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, "_targetTag phải tồn tại và configurable qua SerializeField");
        }
    }

    // ══════════════════════════════════════════════════════
    // SPAWN MANAGER
    // ══════════════════════════════════════════════════════
    public class SpawnManagerTests
    {
        private GameObject _go;
        private SpawnManager _sm;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("SpawnManager");
            _sm = _go.AddComponent<SpawnManager>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator SpawnAt_Null_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _sm.SpawnAt(null, Vector3.zero));
        }

        [UnityTest]
        public IEnumerator SpawnAt_ValidPrefab_ReturnsObject()
        {
            var prefab = new GameObject("Prefab");
            var spawned = _sm.SpawnAt(prefab, Vector3.zero);
            yield return null;
            Assert.IsNotNull(spawned);
            Object.Destroy(prefab);
            if (spawned != null) Object.Destroy(spawned);
        }

        // ── Test mới ──

        [UnityTest]
        public IEnumerator SpawnAt_CorrectPosition()
        {
            var prefab = new GameObject("Prefab");
            var pos = new Vector3(3f, 1f, 5f);
            var spawned = _sm.SpawnAt(prefab, pos);
            yield return null;
            Assert.AreEqual(pos, spawned.transform.position,
                "Object spawn phải xuất hiện đúng vị trí");
            Object.Destroy(prefab);
            if (spawned != null) Object.Destroy(spawned);
        }

        [UnityTest]
        public IEnumerator SpawnAt_MultipleTimes_CreatesDistinctObjects()
        {
            var prefab = new GameObject("Prefab");
            var a = _sm.SpawnAt(prefab, Vector3.zero);
            var b = _sm.SpawnAt(prefab, Vector3.one);
            yield return null;
            Assert.AreNotSame(a, b, "Mỗi lần SpawnAt phải tạo object mới");
            Object.Destroy(prefab);
            if (a != null) Object.Destroy(a);
            if (b != null) Object.Destroy(b);
        }

        [UnityTest]
        public IEnumerator SpawnAt_NullPrefab_ReturnsNull()
        {
            var result = _sm.SpawnAt(null, Vector3.zero);
            yield return null;
            Assert.IsNull(result, "SpawnAt(null) phải return null");
        }
    }

    // ══════════════════════════════════════════════════════
    // DELAY EVENT
    // ══════════════════════════════════════════════════════
    public class DelayEventTests
    {
        private GameObject _go;
        private DelayEvent _de;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("DelayEvent");
            _de = _go.AddComponent<DelayEvent>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator FiresAfterDelay()
        {
            bool fired = false;
            _de.OnDelayComplete.AddListener(() => fired = true);
            R.Set(_de, "_delaySeconds", 0.2f);
            _de.StartDelay();
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(fired, "Chưa hết delay chưa được fire");
            yield return new WaitForSeconds(0.2f);
            Assert.IsTrue(fired, "Phải fire sau khi hết delay");
        }

        // ── Test mới ──

        [UnityTest]
        public IEnumerator FiresExactlyOnce()
        {
            int count = 0;
            _de.OnDelayComplete.AddListener(() => count++);
            R.Set(_de, "_delaySeconds", 0.1f);
            _de.StartDelay();
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(1, count, "OnDelayComplete phải fire đúng 1 lần");
        }

        [UnityTest]
        public IEnumerator StartDelay_MultipleCall_BehavesConsistently()
        {
            int count = 0;
            _de.OnDelayComplete.AddListener(() => count++);
            R.Set(_de, "_delaySeconds", 0.15f);
            _de.StartDelay();
            _de.StartDelay();
            yield return new WaitForSeconds(0.5f);
            Assert.Greater(count, 0, "StartDelay() gọi nhiều lần phải fire ít nhất 1 lần");
        }

        [UnityTest]
        public IEnumerator CancelDelay_PreventsFireing()
        {
            bool fired = false;
            _de.OnDelayComplete.AddListener(() => fired = true);
            R.Set(_de, "_delaySeconds", 0.3f);
            _de.StartDelay();
            yield return new WaitForSeconds(0.1f);
            _de.CancelDelay();
            yield return new WaitForSeconds(0.4f);
            Assert.IsFalse(fired, "CancelDelay() phải ngăn OnDelayComplete fire");
        }
    }

    // ══════════════════════════════════════════════════════
    // GAZE TRIGGER
    // ══════════════════════════════════════════════════════
    public class GazeTriggerTests
    {
        private GameObject _go;
        private GazeTrigger _gt;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("GazeTrigger");
            _gt = _go.AddComponent<GazeTrigger>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator GazeTimerStartsAtZero()
        {
            yield return null;
            Assert.AreEqual(0f, R.Float(_gt, "_gazeTimer"), 0.001f);
        }

        [UnityTest]
        public IEnumerator DefaultThresholdIsThreeSeconds()
        {
            yield return null;
            Assert.AreEqual(3f, R.Float(_gt, "_gazeThreshold"), 0.001f);
        }

        [UnityTest]
        public IEnumerator HasOnGazeComplete_Event()
        {
            yield return null;
            var f = typeof(GazeTrigger).GetField("OnGazeComplete",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(f, "Phải có field 'OnGazeComplete' kiểu UnityEvent");
        }

        // ── Test mới ──

        // Timer set trực tiếp đủ threshold → OnGazeComplete fire
        [UnityTest]
        public IEnumerator GazeComplete_FiresWhenTimerReachesThreshold()
        {
            float threshold = 3f;
            R.Set(_gt, "_gazeThreshold", threshold);
            bool fired = false;
            _gt.OnGazeComplete.AddListener(() => fired = true);

            // Simulate timer đã đến ngưỡng bằng cách set trực tiếp rồi gọi Update qua SendMessage
            R.Set(_gt, "_gazeTimer", threshold);
            _gt.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.IsTrue(fired, "OnGazeComplete phải fire khi _gazeTimer >= _gazeThreshold");
        }

        // Timer chưa đủ threshold → không fire
        [UnityTest]
        public IEnumerator GazeComplete_DoesNotFire_WhenTimerBelowThreshold()
        {
            R.Set(_gt, "_gazeThreshold", 3f);
            bool fired = false;
            _gt.OnGazeComplete.AddListener(() => fired = true);

            R.Set(_gt, "_gazeTimer", 1f); // chưa đủ
            _gt.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.IsFalse(fired, "OnGazeComplete không được fire khi timer chưa đủ");
        }

        // Timer reset về 0 sau khi fire
        [UnityTest]
        public IEnumerator GazeTimer_ResetsAfterComplete()
        {
            float threshold = 0.3f;
            R.Set(_gt, "_gazeThreshold", threshold);
            _gt.OnGazeComplete.AddListener(() => { }); // listener rỗng

            R.Set(_gt, "_gazeTimer", threshold);
            _gt.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.AreEqual(0f, R.Float(_gt, "_gazeTimer"), 0.001f,
                "_gazeTimer phải reset về 0 sau khi OnGazeComplete fire");
        }

        // OnGazeWarning fire ở giây thứ 1 (trước threshold)
        [UnityTest]
        public IEnumerator GazeWarning_FiresBetweenOneAndThreshold()
        {
            R.Set(_gt, "_gazeThreshold", 3f);
            bool warned = false;
            _gt.OnGazeWarning.AddListener(() => warned = true);

            R.Set(_gt, "_gazeTimer", 1.5f); // >= 1f và < 3f
            _gt.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            yield return null;

            Assert.IsTrue(warned, "OnGazeWarning phải fire khi timer >= 1f và < threshold");
        }
    }
}