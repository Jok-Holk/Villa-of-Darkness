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
        public static bool  Bool (object o, string n) => (bool) (o.GetType().GetField(n, F)?.GetValue(o) ?? false);
        public static void  Set  (object o, string n, object v) => o.GetType().GetField(n, F)?.SetValue(o, v);
    }

    // ══════════════════════════════════════════════════════
    // TRIGGER ZONE
    // Stub: OnTriggerEnter với tag filter, OnTriggered event
    // Test mới: tag filter hoạt động đúng (sai tag không fire),
    //           fire đúng 1 lần khi object vào, OnTriggerExit nếu có,
    //           disable component thì không fire
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

        // Tag filter: object đúng tag mới fire
        [UnityTest]
        public IEnumerator CorrectTag_FiresEvent()
        {
            // Set target tag = "Player"
            var tagField = typeof(TriggerZone).GetField("_targetTag", BindingFlags.NonPublic | BindingFlags.Instance);
            tagField?.SetValue(_tz, "Player");

            bool fired = false;
            _tz.OnTriggered.AddListener(() => fired = true);

            var player = new GameObject("Player");
            player.tag = "Player";
            var rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            player.AddComponent<BoxCollider>();
            player.transform.position = _go.transform.position; // overlap

            yield return new WaitForFixedUpdate();
            yield return null;

            Assert.IsTrue(fired, "Object đúng tag phải kích hoạt OnTriggered");
            Object.Destroy(player);
        }

        // Tag filter: object sai tag không fire
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

        // Disabled TriggerZone không fire
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

        // _targetTag có thể config được
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
    // Stub: SpawnAt(prefab, pos) instantiate và return
    // Test mới: spawn nhiều lần không override nhau, position chính xác,
    //           rotation identity, parent nếu có, track spawned objects
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

        // Position chính xác
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

        // Spawn nhiều lần tạo nhiều object riêng biệt
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

        // SpawnAt null trả về null (không crash + return null)
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
    // Stub: StartDelay() → coroutine → fire sau _delaySeconds
    // Test mới: không fire trước khi hết delay, fire đúng 1 lần,
    //           CancelDelay nếu có, delay = 0 vẫn hoạt động
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

        // Fire đúng 1 lần
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

        // StartDelay nhiều lần chỉ fire 1 lần (hoặc reset timer)
        [UnityTest]
        public IEnumerator StartDelay_MultipleCall_BehavesConsistently()
        {
            int count = 0;
            _de.OnDelayComplete.AddListener(() => count++);
            R.Set(_de, "_delaySeconds", 0.15f);
            _de.StartDelay();
            _de.StartDelay(); // gọi lại
            yield return new WaitForSeconds(0.5f);
            // Chấp nhận 1 hoặc 2 lần, nhưng không được 0
            Assert.Greater(count, 0, "StartDelay() gọi nhiều lần phải fire ít nhất 1 lần");
        }

        // CancelDelay ngăn không cho fire
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
    // Stub: _gazeThreshold=3f, _gazeTimer=0f — chỉ có fields, không có logic
    // Test mới: timer tăng khi đang nhìn vào, reset khi nhìn đi,
    //           fire event khi đủ 3 giây, không fire nếu nhìn dưới 3 giây,
    //           IsGazing property
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

        // ── Test mới — stub chỉ có fields, không có logic, tất cả sẽ FAIL ──

        // StartGaze() làm timer tăng
        [UnityTest]
        public IEnumerator StartGaze_IncreasesTimer()
        {
            _gt.StartGaze();
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(R.Float(_gt, "_gazeTimer"), 0f,
                "StartGaze() phải làm _gazeTimer tăng theo thời gian");
        }

        // StopGaze() reset timer
        [UnityTest]
        public IEnumerator StopGaze_ResetsTimer()
        {
            _gt.StartGaze();
            yield return new WaitForSeconds(0.2f);
            _gt.StopGaze();
            yield return null;
            Assert.AreEqual(0f, R.Float(_gt, "_gazeTimer"), 0.001f,
                "StopGaze() phải reset _gazeTimer về 0");
        }

        // Fire event sau đủ 3 giây nhìn
        [UnityTest]
        public IEnumerator GazeComplete_FiresAfterThreshold()
        {
            R.Set(_gt, "_gazeThreshold", 0.3f); // rút ngắn để test nhanh
            bool fired = false;
            _gt.OnGazeComplete.AddListener(() => fired = true);
            _gt.StartGaze();
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(fired, "OnGazeComplete phải fire sau khi nhìn đủ thời gian");
        }

        // Không fire nếu nhìn chưa đủ thời gian
        [UnityTest]
        public IEnumerator Gaze_TooShort_DoesNotFire()
        {
            R.Set(_gt, "_gazeThreshold", 0.5f);
            bool fired = false;
            _gt.OnGazeComplete.AddListener(() => fired = true);
            _gt.StartGaze();
            yield return new WaitForSeconds(0.2f);
            _gt.StopGaze();
            yield return new WaitForSeconds(0.1f);
            Assert.IsFalse(fired, "Nhìn chưa đủ thời gian không được fire event");
        }

        // IsGazing property
        [UnityTest]
        public IEnumerator IsGazing_TrueAfterStartGaze()
        {
            _gt.StartGaze();
            yield return null;
            Assert.IsTrue(_gt.IsGazing, "IsGazing phải true sau StartGaze()");
        }

        [UnityTest]
        public IEnumerator IsGazing_FalseAfterStopGaze()
        {
            _gt.StartGaze();
            yield return null;
            _gt.StopGaze();
            yield return null;
            Assert.IsFalse(_gt.IsGazing, "IsGazing phải false sau StopGaze()");
        }
    }
}
