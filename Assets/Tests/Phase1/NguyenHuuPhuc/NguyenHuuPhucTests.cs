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
        public static float Float(object o, string n) => (float)(o.GetType().GetField(n,F)?.GetValue(o) ?? 0f);
        public static void  Set  (object o, string n, object v) => o.GetType().GetField(n,F)?.SetValue(o,v);
    }

    // ════════════════════════════════════
    // TRIGGER ZONE
    // ════════════════════════════════════
    public class TriggerZoneTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("TriggerZone");
            var col = _go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            _go.AddComponent<TriggerZone>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator HasTriggerCollider()
        {
            yield return null;
            var col = _go.GetComponent<Collider>();
            Assert.IsNotNull(col, "TriggerZone phải có Collider");
            Assert.IsTrue(col.isTrigger, "Collider phải là Trigger");
        }

        [UnityTest]
        public IEnumerator HasPublicUnityEvent()
        {
            yield return null;
            var f = typeof(TriggerZone).GetField("OnTriggered",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(f, "Phải có public field 'OnTriggered' kiểu UnityEvent");
        }

        [UnityTest]
        public IEnumerator TagFilter_DefaultIsPlayer()
        {
            yield return null;
            var tz = _go.GetComponent<TriggerZone>();
            var f = typeof(TriggerZone).GetField("_targetTag",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) yield break; // optional field
            string tag = (string)f.GetValue(tz);
            Assert.AreEqual("Player", tag,
                "_targetTag mặc định phải là 'Player'");
        }
    }

    // ════════════════════════════════════
    // SPAWN MANAGER
    // ════════════════════════════════════
    public class SpawnManagerTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("SpawnManager");
            _go.AddComponent<SpawnManager>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator SpawnAt_NullPrefab_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() =>
                _go.GetComponent<SpawnManager>().SpawnAt(null, Vector3.zero));
        }

        [UnityTest]
        public IEnumerator SpawnAt_ValidPrefab_ReturnsObject()
        {
            var sm = _go.GetComponent<SpawnManager>();
            var prefab = new GameObject("Prefab");
            yield return null;

            var spawned = sm.SpawnAt(prefab, new Vector3(1, 0, 2));
            yield return null;

            Assert.IsNotNull(spawned, "SpawnAt phải trả về object mới");
            Assert.AreEqual(new Vector3(1, 0, 2), spawned.transform.position);

            Object.Destroy(prefab);
            Object.Destroy(spawned);
        }

        [UnityTest]
        public IEnumerator SpawnAt_SetsCorrectPosition()
        {
            var sm = _go.GetComponent<SpawnManager>();
            var prefab = new GameObject("Prefab2");
            var pos = new Vector3(5f, 0f, 3f);
            yield return null;

            var spawned = sm.SpawnAt(prefab, pos);
            yield return null;

            Assert.AreEqual(pos, spawned.transform.position);
            Object.Destroy(prefab);
            Object.Destroy(spawned);
        }
    }

    // ════════════════════════════════════
    // DELAY EVENT
    // ════════════════════════════════════
    public class DelayEventTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("DelayEvent");
            _go.AddComponent<DelayEvent>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator HasPublicOnDelayComplete()
        {
            yield return null;
            var f = typeof(DelayEvent).GetField("OnDelayComplete",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(f, "Phải có field 'OnDelayComplete' kiểu UnityEvent");
        }

        [UnityTest]
        public IEnumerator DoesNotFireBeforeDelay()
        {
            var de = _go.GetComponent<DelayEvent>();
            bool fired = false;
            de.OnDelayComplete.AddListener(() => fired = true);
            R.Set(de, "_delaySeconds", 0.5f);
            de.StartDelay();

            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(fired, "Event không được fire trước khi hết delay");
        }

        [UnityTest]
        public IEnumerator FiresAfterDelay()
        {
            var de = _go.GetComponent<DelayEvent>();
            bool fired = false;
            de.OnDelayComplete.AddListener(() => fired = true);
            R.Set(de, "_delaySeconds", 0.2f);
            de.StartDelay();

            yield return new WaitForSeconds(0.4f);
            Assert.IsTrue(fired, "Event phải fire sau khi delay kết thúc");
        }
    }

    // ════════════════════════════════════
    // GAZE TRIGGER
    // ════════════════════════════════════
    public class GazeTriggerTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("GazeTrigger");
            _go.AddComponent<GazeTrigger>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator GazeTimer_StartsAtZero()
        {
            yield return null;
            Assert.AreEqual(0f, R.Float(_go.GetComponent<GazeTrigger>(), "_gazeTimer"), 0.01f);
        }

        [UnityTest]
        public IEnumerator GazeThreshold_DefaultIsThreeSecs()
        {
            yield return null;
            Assert.AreEqual(3f,
                R.Float(_go.GetComponent<GazeTrigger>(), "_gazeThreshold"), 0.01f,
                "Threshold mặc định phải là 3 giây theo thiết kế");
        }

        [UnityTest]
        public IEnumerator HasOnGazeComplete_Event()
        {
            yield return null;
            var f = typeof(GazeTrigger).GetField("OnGazeComplete",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(f, "Phải có field 'OnGazeComplete' kiểu UnityEvent");
        }
    }
}
