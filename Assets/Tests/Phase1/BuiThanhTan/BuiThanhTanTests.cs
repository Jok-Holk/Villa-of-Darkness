using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

namespace Phase1.BuiThanhTan
{
    static class R
    {
        static BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
        public static float Float(object o, string n) => (float)(o.GetType().GetField(n,F)?.GetValue(o) ?? 0f);
        public static void  Set  (object o, string n, object v) => o.GetType().GetField(n,F)?.SetValue(o,v);
    }

    // ════════════════════════════════════
    // SANITY SYSTEM
    // ════════════════════════════════════
    public class SanitySystemTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("SanitySystem");
            _go.AddComponent<SanitySystem>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator StartsAtMax()
        {
            yield return null;
            Assert.AreEqual(1f, R.Float(_go.GetComponent<SanitySystem>(), "_sanity"), 0.01f,
                "_sanity phải bắt đầu ở 1.0");
        }

        [UnityTest]
        public IEnumerator Decrease_ReducesCorrectly()
        {
            var sys = _go.GetComponent<SanitySystem>();
            sys.DecreaseSanity(0.3f);
            yield return null;
            Assert.AreEqual(0.7f, R.Float(sys, "_sanity"), 0.01f);
        }

        [UnityTest]
        public IEnumerator ClampsAtZero()
        {
            var sys = _go.GetComponent<SanitySystem>();
            sys.DecreaseSanity(999f);
            yield return null;
            Assert.GreaterOrEqual(R.Float(sys, "_sanity"), 0f, "Sanity không được âm");
        }

        [UnityTest]
        public IEnumerator ClampsAtOne()
        {
            var sys = _go.GetComponent<SanitySystem>();
            R.Set(sys, "_sanity", 0.5f);
            sys.IncreaseSanity(999f);
            yield return null;
            Assert.LessOrEqual(R.Float(sys, "_sanity"), 1f);
        }

        [UnityTest]
        public IEnumerator GetLevel_High_WhenAbove75()
        {
            var sys = _go.GetComponent<SanitySystem>();
            R.Set(sys, "_sanity", 0.9f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.High, sys.GetLevel());
        }

        [UnityTest]
        public IEnumerator GetLevel_Medium_Between40And75()
        {
            var sys = _go.GetComponent<SanitySystem>();
            R.Set(sys, "_sanity", 0.5f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.Medium, sys.GetLevel());
        }

        [UnityTest]
        public IEnumerator GetLevel_Low_Between10And40()
        {
            var sys = _go.GetComponent<SanitySystem>();
            R.Set(sys, "_sanity", 0.2f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.Low, sys.GetLevel());
        }

        [UnityTest]
        public IEnumerator GetLevel_Critical_Below10()
        {
            var sys = _go.GetComponent<SanitySystem>();
            R.Set(sys, "_sanity", 0.05f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.Critical, sys.GetLevel());
        }
    }

    // ════════════════════════════════════
    // AUDIO MANAGER
    // ════════════════════════════════════
    public class AudioManagerTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("AudioManager");
            _go.AddComponent<AudioSource>();
            _go.AddComponent<AudioManager>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator Instance_NotNull()
        {
            yield return null;
            Assert.IsNotNull(AudioManager.Instance);
        }

        [UnityTest]
        public IEnumerator PlaySFX_NullClip_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => AudioManager.Instance.PlaySFX(null));
        }

        [UnityTest]
        public IEnumerator SetBGMVolume_OutOfRange_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => AudioManager.Instance.SetBGMVolume(2f));
            Assert.DoesNotThrow(() => AudioManager.Instance.SetBGMVolume(-1f));
        }

        [UnityTest]
        public IEnumerator SetBGMVolume_ClampsCorrectly()
        {
            var am = AudioManager.Instance;
            am.SetBGMVolume(2f);
            yield return null;
            float vol = R.Float(am, "_bgmVolume");
            Assert.LessOrEqual(vol, 1f, "Volume không được vượt quá 1");
            Assert.GreaterOrEqual(vol, 0f, "Volume không được âm");
        }
    }
}
