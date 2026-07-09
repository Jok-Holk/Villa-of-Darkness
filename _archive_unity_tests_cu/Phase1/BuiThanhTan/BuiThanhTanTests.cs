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
        public static float Float(object o, string n) => (float)(o.GetType().GetField(n, F)?.GetValue(o) ?? 0f);
        public static void  Set  (object o, string n, object v) => o.GetType().GetField(n, F)?.SetValue(o, v);
    }

    // ══════════════════════════════════════════════════════
    // SANITY SYSTEM
    // Stub đã có: _sanity=1f, DecreaseSanity clamp, IncreaseSanity clamp, GetLevel() đúng threshold
    // Test mới phải kiểm tra BEHAVIOUR mà stub chưa có:
    //   - Event OnSanityChanged phải được gọi khi sanity thay đổi
    //   - DecreaseSanity nhiều lần liên tiếp phải tích lũy đúng
    //   - IncreaseSanity sau khi giảm phải phục hồi đúng
    //   - Ngưỡng chính xác tại boundary (0.75, 0.40, 0.10)
    //   - GetLevel khi sanity = 0 phải là Critical
    //   - GetLevel khi sanity = 1 phải là High
    //   - Không được tự giảm sanity theo thời gian nếu không gọi Decrease (stub có thể Update)
    // ══════════════════════════════════════════════════════
    public class SanitySystemTests
    {
        private GameObject _go;
        private SanitySystem _sys;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("SanitySystem");
            _sys = _go.AddComponent<SanitySystem>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass: có sẵn ──────────────────────────
        [UnityTest]
        public IEnumerator StartsAtMax()
        {
            yield return null;
            Assert.AreEqual(1f, R.Float(_sys, "_sanity"), 0.001f);
        }

        [UnityTest]
        public IEnumerator Decrease_ReducesCorrectly()
        {
            _sys.DecreaseSanity(0.3f);
            yield return null;
            Assert.AreEqual(0.7f, R.Float(_sys, "_sanity"), 0.001f);
        }

        [UnityTest]
        public IEnumerator ClampsAtZero()
        {
            _sys.DecreaseSanity(999f);
            yield return null;
            Assert.GreaterOrEqual(R.Float(_sys, "_sanity"), 0f);
        }

        [UnityTest]
        public IEnumerator ClampsAtOne()
        {
            R.Set(_sys, "_sanity", 0.5f);
            _sys.IncreaseSanity(999f);
            yield return null;
            Assert.LessOrEqual(R.Float(_sys, "_sanity"), 1f);
        }

        [UnityTest]
        public IEnumerator GetLevel_High() { R.Set(_sys, "_sanity", 0.9f); yield return null; Assert.AreEqual(SanitySystem.SanityLevel.High, _sys.GetLevel()); }
        [UnityTest]
        public IEnumerator GetLevel_Medium() { R.Set(_sys, "_sanity", 0.5f); yield return null; Assert.AreEqual(SanitySystem.SanityLevel.Medium, _sys.GetLevel()); }
        [UnityTest]
        public IEnumerator GetLevel_Low() { R.Set(_sys, "_sanity", 0.2f); yield return null; Assert.AreEqual(SanitySystem.SanityLevel.Low, _sys.GetLevel()); }
        [UnityTest]
        public IEnumerator GetLevel_Critical() { R.Set(_sys, "_sanity", 0.05f); yield return null; Assert.AreEqual(SanitySystem.SanityLevel.Critical, _sys.GetLevel()); }

        // ── Test mới: Stub KHÔNG có, P3 phải implement ──

        // Giảm nhiều lần phải tích lũy
        [UnityTest]
        public IEnumerator Decrease_MultipleCallsAccumulate()
        {
            _sys.DecreaseSanity(0.2f);
            _sys.DecreaseSanity(0.2f);
            _sys.DecreaseSanity(0.2f);
            yield return null;
            Assert.AreEqual(0.4f, R.Float(_sys, "_sanity"), 0.001f,
                "Ba lần DecreaseSanity(0.2) phải tích lũy thành 0.4");
        }

        // IncreaseSanity sau khi giảm phải phục hồi đúng
        [UnityTest]
        public IEnumerator Increase_AfterDecrease_RestoresCorrectly()
        {
            _sys.DecreaseSanity(0.5f);
            _sys.IncreaseSanity(0.2f);
            yield return null;
            Assert.AreEqual(0.7f, R.Float(_sys, "_sanity"), 0.001f,
                "1.0 - 0.5 + 0.2 = 0.7");
        }

        // Boundary chính xác tại 0.75 — stub threshold > 0.75 nên 0.75 rơi vào Medium
        [UnityTest]
        public IEnumerator GetLevel_ExactBoundary_0_75_IsMedium()
        {
            R.Set(_sys, "_sanity", 0.75f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.Medium, _sys.GetLevel(),
                "sanity=0.75 phải là Medium (threshold: High khi > 0.75)");
        }

        // Boundary tại 0.40 — rơi vào Low
        [UnityTest]
        public IEnumerator GetLevel_ExactBoundary_0_40_IsLow()
        {
            R.Set(_sys, "_sanity", 0.40f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.Low, _sys.GetLevel(),
                "sanity=0.40 phải là Low (threshold: Medium khi > 0.40)");
        }

        // Boundary tại 0.10 — rơi vào Critical
        [UnityTest]
        public IEnumerator GetLevel_ExactBoundary_0_10_IsCritical()
        {
            R.Set(_sys, "_sanity", 0.10f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.Critical, _sys.GetLevel(),
                "sanity=0.10 phải là Critical (threshold: Low khi > 0.10)");
        }

        // GetLevel khi = 0 phải là Critical
        [UnityTest]
        public IEnumerator GetLevel_Zero_IsCritical()
        {
            R.Set(_sys, "_sanity", 0f);
            yield return null;
            Assert.AreEqual(SanitySystem.SanityLevel.Critical, _sys.GetLevel());
        }

        // OnSanityChanged event phải được invoke khi Decrease
        [UnityTest]
        public IEnumerator OnSanityChanged_FiredOnDecrease()
        {
            bool fired = false;
            _sys.OnSanityChanged.AddListener(() => fired = true);
            _sys.DecreaseSanity(0.1f);
            yield return null;
            Assert.IsTrue(fired, "OnSanityChanged phải được invoke khi DecreaseSanity()");
        }

        // OnSanityChanged event phải được invoke khi Increase
        [UnityTest]
        public IEnumerator OnSanityChanged_FiredOnIncrease()
        {
            bool fired = false;
            R.Set(_sys, "_sanity", 0.5f);
            _sys.OnSanityChanged.AddListener(() => fired = true);
            _sys.IncreaseSanity(0.1f);
            yield return null;
            Assert.IsTrue(fired, "OnSanityChanged phải được invoke khi IncreaseSanity()");
        }

        // Sanity không tự giảm theo thời gian (stub không có Update nhưng kiểm tra chắc)
        [UnityTest]
        public IEnumerator Sanity_DoesNotDecreaseByItself()
        {
            float initial = R.Float(_sys, "_sanity");
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(initial, R.Float(_sys, "_sanity"), 0.001f,
                "Sanity không được tự giảm khi không gọi DecreaseSanity()");
        }

        // Decrease(0) không thay đổi giá trị
        [UnityTest]
        public IEnumerator Decrease_Zero_NoChange()
        {
            float before = R.Float(_sys, "_sanity");
            _sys.DecreaseSanity(0f);
            yield return null;
            Assert.AreEqual(before, R.Float(_sys, "_sanity"), 0.001f);
        }
    }

    // ══════════════════════════════════════════════════════
    // AUDIO MANAGER
    // Stub đã có: singleton, PlaySFX null check, SetBGMVolume clamp
    // Test mới:
    //   - PlaySFX với clip thật phải thật sự phát (AudioSource.isPlaying)
    //   - SetBGMVolume phải sync với AudioSource.volume
    //   - StopBGM() phải dừng âm thanh
    //   - PlayBGM(clip) phải bắt đầu phát
    //   - Singleton bền qua DontDestroyOnLoad
    //   - SetSFXVolume nếu có
    // ══════════════════════════════════════════════════════
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

        // ── Stub pass: có sẵn ──────────────────────────
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
        public IEnumerator SetBGMVolume_ClampsToMax()
        {
            AudioManager.Instance.SetBGMVolume(2f);
            yield return null;
            var vol = (float)typeof(AudioManager)
                .GetField("_bgmVolume", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(AudioManager.Instance);
            Assert.LessOrEqual(vol, 1f);
        }

        [UnityTest]
        public IEnumerator SetBGMVolume_ClampsToMin()
        {
            AudioManager.Instance.SetBGMVolume(-1f);
            yield return null;
            var vol = (float)typeof(AudioManager)
                .GetField("_bgmVolume", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(AudioManager.Instance);
            Assert.GreaterOrEqual(vol, 0f);
        }

        // ── Test mới: Stub KHÔNG có, P3 phải implement ──

        // SetBGMVolume phải sync AudioSource.volume
        [UnityTest]
        public IEnumerator SetBGMVolume_SyncsAudioSourceVolume()
        {
            AudioManager.Instance.SetBGMVolume(0.6f);
            yield return null;
            var src = _go.GetComponent<AudioSource>();
            Assert.AreEqual(0.6f, src.volume, 0.001f,
                "SetBGMVolume(0.6) phải set AudioSource.volume = 0.6");
        }

        // PlayBGM(clip) phải bắt đầu phát — P3 phải implement method này
        [UnityTest]
        public IEnumerator PlayBGM_StartsPlaying()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            Assert.DoesNotThrow(() => AudioManager.Instance.PlayBGM(clip),
                "PlayBGM(clip) phải tồn tại và không crash");
            yield return null;
            var src = _go.GetComponent<AudioSource>();
            Assert.IsTrue(src.isPlaying || src.clip == clip,
                "Sau PlayBGM(), AudioSource phải đang phát hoặc clip phải được set");
            Object.Destroy(clip);
        }

        // StopBGM() phải dừng — P3 phải implement method này
        [UnityTest]
        public IEnumerator StopBGM_StopsPlaying()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            AudioManager.Instance.PlayBGM(clip);
            yield return null;
            Assert.DoesNotThrow(() => AudioManager.Instance.StopBGM(),
                "StopBGM() phải tồn tại");
            yield return null;
            var src = _go.GetComponent<AudioSource>();
            Assert.IsFalse(src.isPlaying, "Sau StopBGM(), AudioSource.isPlaying phải false");
            Object.Destroy(clip);
        }

        // Volume mặc định phải là 1f
        [UnityTest]
        public IEnumerator DefaultBGMVolume_IsOne()
        {
            var vol = (float)typeof(AudioManager)
                .GetField("_bgmVolume", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(AudioManager.Instance);
            yield return null;
            Assert.AreEqual(1f, vol, 0.001f, "_bgmVolume mặc định phải là 1.0");
        }

        // SetBGMVolume(0.5) rồi get lại phải đúng
        [UnityTest]
        public IEnumerator SetBGMVolume_PreciseValue()
        {
            AudioManager.Instance.SetBGMVolume(0.42f);
            yield return null;
            var vol = (float)typeof(AudioManager)
                .GetField("_bgmVolume", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(AudioManager.Instance);
            Assert.AreEqual(0.42f, vol, 0.001f);
        }

        // PlaySFX với clip thật không được crash và phát được
        [UnityTest]
        public IEnumerator PlaySFX_ValidClip_DoesNotThrow()
        {
            var clip = AudioClip.Create("sfx", 4410, 1, 44100, false);
            yield return null;
            Assert.DoesNotThrow(() => AudioManager.Instance.PlaySFX(clip),
                "PlaySFX(clip thật) không được crash");
            Object.Destroy(clip);
        }
    }
}
