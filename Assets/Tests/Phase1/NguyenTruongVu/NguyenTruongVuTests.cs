using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

namespace Phase1.NguyenTruongVu
{
    static class R
    {
        static BindingFlags F = BindingFlags.NonPublic | BindingFlags.Instance;
        public static bool  Bool(object o, string n) => (bool )(o.GetType().GetField(n,F)?.GetValue(o) ?? false);
        public static void  Set (object o, string n, object v) => o.GetType().GetField(n,F)?.SetValue(o,v);
    }

    // ════════════════════════════════════
    // MAIN MENU UI
    // ════════════════════════════════════
    public class MainMenuUITests
    {
        private GameObject _gm, _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _gm = new GameObject("GameManager");
            _gm.AddComponent<GameManager>();
            _go = new GameObject("MainMenuUI");
            _go.AddComponent<MainMenuUI>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Object.Destroy(_gm);
            Object.Destroy(_go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_go.GetComponent<MainMenuUI>());
        }

        [UnityTest]
        public IEnumerator StartGame_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() =>
                _go.GetComponent<MainMenuUI>().StartGame());
        }

        [UnityTest]
        public IEnumerator QuitGame_DoesNotThrow()
        {
            yield return null;
            // QuitGame() trong editor không thật sự thoát, chỉ check không crash
            Assert.DoesNotThrow(() =>
                _go.GetComponent<MainMenuUI>().QuitGame());
        }
    }

    // ════════════════════════════════════
    // DEATH SCREEN UI
    // ════════════════════════════════════
    public class DeathScreenUITests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("DeathScreenUI");
            _go.AddComponent<DeathScreenUI>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_go.GetComponent<DeathScreenUI>());
        }

        [UnityTest]
        public IEnumerator Show_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() =>
                _go.GetComponent<DeathScreenUI>().Show("Minh Khoa", "1979 – 2000"));
        }

        [UnityTest]
        public IEnumerator Show_WithEmptyString_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() =>
                _go.GetComponent<DeathScreenUI>().Show("", ""));
        }

        [UnityTest]
        public IEnumerator Hide_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() =>
                _go.GetComponent<DeathScreenUI>().Hide());
        }
    }

    // ════════════════════════════════════
    // PAUSE MENU UI
    // ════════════════════════════════════
    public class PauseMenuUITests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("PauseMenuUI");
            _go.AddComponent<PauseMenuUI>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Time.timeScale = 1f;
            Object.Destroy(_go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_go.GetComponent<PauseMenuUI>());
        }

        [UnityTest]
        public IEnumerator Pause_SetsTimeScaleZero()
        {
            _go.GetComponent<PauseMenuUI>().Pause();
            yield return null;
            Assert.AreEqual(0f, Time.timeScale, 0.001f,
                "Pause() phải set Time.timeScale = 0");
        }

        [UnityTest]
        public IEnumerator Resume_SetsTimeScaleOne()
        {
            var pm = _go.GetComponent<PauseMenuUI>();
            pm.Pause();
            pm.Resume();
            yield return null;
            Assert.AreEqual(1f, Time.timeScale, 0.001f,
                "Resume() phải set Time.timeScale = 1");
        }

        [UnityTest]
        public IEnumerator IsPaused_TrueAfterPause()
        {
            var pm = _go.GetComponent<PauseMenuUI>();
            pm.Pause();
            yield return null;
            Assert.IsTrue(R.Bool(pm, "_isPaused"),
                "_isPaused phải là true sau Pause()");
        }

        [UnityTest]
        public IEnumerator IsPaused_FalseAfterResume()
        {
            var pm = _go.GetComponent<PauseMenuUI>();
            pm.Pause();
            pm.Resume();
            yield return null;
            Assert.IsFalse(R.Bool(pm, "_isPaused"),
                "_isPaused phải là false sau Resume()");
        }
    }

    // ════════════════════════════════════
    // CHAPTER TRANSITION
    // ════════════════════════════════════
    public class ChapterTransitionTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("ChapterTransition");
            _go.AddComponent<ChapterTransition>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_go.GetComponent<ChapterTransition>());
        }

        [UnityTest]
        public IEnumerator PlayTransition_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() =>
                _go.GetComponent<ChapterTransition>().PlayTransition("Chapter 1", "2000"));
        }

        [UnityTest]
        public IEnumerator IsPlaying_FalseInitially()
        {
            yield return null;
            Assert.IsFalse(R.Bool(_go.GetComponent<ChapterTransition>(), "_isPlaying"),
                "Transition không được tự chạy khi mới tạo");
        }

        [UnityTest]
        public IEnumerator IsPlaying_TrueAfterPlay()
        {
            var ct = _go.GetComponent<ChapterTransition>();
            ct.PlayTransition("Chapter 1", "2000");
            yield return null;
            Assert.IsTrue(R.Bool(ct, "_isPlaying"),
                "_isPlaying phải true khi đang chạy transition");
        }
    }
}
