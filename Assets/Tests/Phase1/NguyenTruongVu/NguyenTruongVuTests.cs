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
        public static bool  Bool(object o, string n) => (bool)(o.GetType().GetField(n, F)?.GetValue(o) ?? false);
        public static void  Set (object o, string n, object v) => o.GetType().GetField(n, F)?.SetValue(o, v);
    }

    // ══════════════════════════════════════════════════════
    // MAIN MENU UI
    // Stub: StartGame() gọi LoadChapter(1), QuitGame() gọi Application.Quit
    // Test mới: StartGame không crash khi không có GameManager (graceful),
    //           button references tồn tại, OnMenuOpen event nếu có
    // ══════════════════════════════════════════════════════
    public class MainMenuUITests
    {
        private GameObject _gm, _go;
        private MainMenuUI _menu;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _gm = new GameObject("GameManager");
            _gm.AddComponent<GameManager>();
            _go = new GameObject("MainMenuUI");
            _menu = _go.AddComponent<MainMenuUI>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After()
        {
            Object.Destroy(_gm);
            Object.Destroy(_go);
            yield return null;
        }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_menu);
        }

        [UnityTest]
        public IEnumerator StartGame_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _menu.StartGame());
        }

        [UnityTest]
        public IEnumerator QuitGame_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _menu.QuitGame());
        }

        // ── Test mới ──

        // StartGame phải gọi LoadChapter(1)
        [UnityTest]
        public IEnumerator StartGame_LoadsChapterOne()
        {
            GameData.Reset();
            _menu.StartGame();
            yield return null;
            Assert.AreEqual(1, GameData.currentChapter,
                "StartGame() phải gọi LoadChapter(1) → currentChapter = 1");
        }

        // Không crash khi không có GameManager
        [UnityTest]
        public IEnumerator StartGame_GracefulWithoutGameManager()
        {
            Object.Destroy(_gm);
            yield return null;
            Assert.DoesNotThrow(() => _menu.StartGame(),
                "StartGame() không được crash khi GameManager.Instance = null");
        }

        // Hide() ẩn menu (nếu có canvas group hoặc SetActive)
        [UnityTest]
        public IEnumerator Hide_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _menu.Hide(),
                "Hide() phải tồn tại và không crash");
        }

        // Show() hiện menu
        [UnityTest]
        public IEnumerator Show_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _menu.Show(),
                "Show() phải tồn tại và không crash");
        }
    }

    // ══════════════════════════════════════════════════════
    // DEATH SCREEN UI
    // Stub: Show(name, years) set _isVisible=true, Hide() set false
    // Test mới: text được set đúng, screen ẩn lúc đầu, RetryLevel button hoạt động
    // ══════════════════════════════════════════════════════
    public class DeathScreenUITests
    {
        private GameObject _go;
        private DeathScreenUI _ds;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("DeathScreenUI");
            _ds = _go.AddComponent<DeathScreenUI>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator Show_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ds.Show("Minh Khoa", "1979 – 2000"));
        }

        [UnityTest]
        public IEnumerator Hide_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ds.Hide());
        }

        // ── Test mới ──

        // Mặc định ẩn
        [UnityTest]
        public IEnumerator StartsHidden()
        {
            yield return null;
            Assert.IsFalse(R.Bool(_ds, "_isVisible"),
                "DeathScreen phải ẩn (_isVisible=false) lúc khởi tạo");
        }

        // Show làm visible
        [UnityTest]
        public IEnumerator Show_SetsVisibleTrue()
        {
            _ds.Show("Minh Khoa", "1979 – 2000");
            yield return null;
            Assert.IsTrue(R.Bool(_ds, "_isVisible"),
                "Show() phải set _isVisible = true");
        }

        // Hide sau Show làm ẩn lại
        [UnityTest]
        public IEnumerator Hide_AfterShow_SetsVisibleFalse()
        {
            _ds.Show("Minh Khoa", "1979 – 2000");
            _ds.Hide();
            yield return null;
            Assert.IsFalse(R.Bool(_ds, "_isVisible"),
                "Hide() sau Show() phải set _isVisible = false");
        }

        // CharacterName được lưu
        [UnityTest]
        public IEnumerator Show_StoresCharacterName()
        {
            _ds.Show("Bích Ngọc", "1950 – 1970");
            yield return null;
            var nameField = typeof(DeathScreenUI).GetField("_characterName",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var stored = nameField?.GetValue(_ds) as string;
            Assert.AreEqual("Bích Ngọc", stored,
                "Show() phải lưu tên nhân vật vào _characterName");
        }

        // Show với empty string không crash
        [UnityTest]
        public IEnumerator Show_EmptyStrings_DoesNotCrash()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ds.Show("", ""));
        }

        // OnRetry event khi nhấn retry
        [UnityTest]
        public IEnumerator Retry_FiresOnRetryEvent()
        {
            bool fired = false;
            _ds.OnRetry.AddListener(() => fired = true);
            _ds.Show("Test", "2000");
            _ds.Retry();
            yield return null;
            Assert.IsTrue(fired, "Retry() phải invoke OnRetry event");
        }
    }

    // ══════════════════════════════════════════════════════
    // PAUSE MENU UI
    // Stub: Pause() _isPaused=true + timeScale=0, Resume() ngược lại
    // Test mới: Toggle hoạt động, không pause khi đã pause,
    //           timeScale reset khi destroy, OnPause/OnResume event
    // ══════════════════════════════════════════════════════
    public class PauseMenuUITests
    {
        private GameObject _go;
        private PauseMenuUI _pm;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("PauseMenuUI");
            _pm = _go.AddComponent<PauseMenuUI>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After()
        {
            Time.timeScale = 1f;
            Object.Destroy(_go);
            yield return null;
        }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator Pause_SetsTimeScaleZero()
        {
            _pm.Pause();
            yield return null;
            Assert.AreEqual(0f, Time.timeScale, 0.001f);
        }

        [UnityTest]
        public IEnumerator Resume_SetsTimeScaleOne()
        {
            _pm.Pause();
            _pm.Resume();
            yield return null;
            Assert.AreEqual(1f, Time.timeScale, 0.001f);
        }

        // ── Test mới ──

        // Mặc định không pause
        [UnityTest]
        public IEnumerator StartsNotPaused()
        {
            yield return null;
            Assert.IsFalse(R.Bool(_pm, "_isPaused"));
        }

        // _isPaused true sau Pause
        [UnityTest]
        public IEnumerator Pause_SetsIsPausedTrue()
        {
            _pm.Pause();
            yield return null;
            Assert.IsTrue(R.Bool(_pm, "_isPaused"));
        }

        // _isPaused false sau Resume
        [UnityTest]
        public IEnumerator Resume_SetsIsPausedFalse()
        {
            _pm.Pause();
            _pm.Resume();
            yield return null;
            Assert.IsFalse(R.Bool(_pm, "_isPaused"));
        }

        // Toggle: chưa pause → pause
        [UnityTest]
        public IEnumerator Toggle_WhenNotPaused_Pauses()
        {
            _pm.Toggle();
            yield return null;
            Assert.IsTrue(R.Bool(_pm, "_isPaused"), "Toggle khi chưa pause phải pause");
        }

        // Toggle: đang pause → resume
        [UnityTest]
        public IEnumerator Toggle_WhenPaused_Resumes()
        {
            _pm.Pause();
            _pm.Toggle();
            yield return null;
            Assert.IsFalse(R.Bool(_pm, "_isPaused"), "Toggle khi đang pause phải resume");
        }

        // Pause khi đã pause không thay đổi gì
        [UnityTest]
        public IEnumerator Pause_WhenAlreadyPaused_NoChange()
        {
            _pm.Pause();
            _pm.Pause(); // gọi lại
            yield return null;
            Assert.AreEqual(0f, Time.timeScale, 0.001f);
            Assert.IsTrue(R.Bool(_pm, "_isPaused"));
        }

        // OnPause event
        [UnityTest]
        public IEnumerator OnPause_FiredWhenPaused()
        {
            bool fired = false;
            _pm.OnPause.AddListener(() => fired = true);
            _pm.Pause();
            yield return null;
            Assert.IsTrue(fired, "OnPause phải invoke khi Pause()");
        }

        // OnResume event
        [UnityTest]
        public IEnumerator OnResume_FiredWhenResumed()
        {
            _pm.Pause();
            bool fired = false;
            _pm.OnResume.AddListener(() => fired = true);
            _pm.Resume();
            yield return null;
            Assert.IsTrue(fired, "OnResume phải invoke khi Resume()");
        }
    }

    // ══════════════════════════════════════════════════════
    // CHAPTER TRANSITION
    // Stub: PlayTransition() set _isPlaying=true, coroutine reset sau 2f
    // Test mới: _isPlaying false sau khi xong, OnTransitionComplete event,
    //           không play khi đang play, text data được set đúng
    // ══════════════════════════════════════════════════════
    public class ChapterTransitionTests
    {
        private GameObject _go;
        private ChapterTransition _ct;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("ChapterTransition");
            _ct = _go.AddComponent<ChapterTransition>();
            yield return null;
        }
        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        // ── Stub pass ──
        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_ct);
        }

        [UnityTest]
        public IEnumerator PlayTransition_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => _ct.PlayTransition("Chapter 1", "2000"));
        }

        [UnityTest]
        public IEnumerator IsPlaying_FalseInitially()
        {
            yield return null;
            Assert.IsFalse(R.Bool(_ct, "_isPlaying"));
        }

        [UnityTest]
        public IEnumerator IsPlaying_TrueAfterPlay()
        {
            _ct.PlayTransition("Chapter 1", "2000");
            yield return null;
            Assert.IsTrue(R.Bool(_ct, "_isPlaying"));
        }

        // ── Test mới ──

        // _isPlaying false sau khi transition xong (stub dùng 2s, rút ngắn)
        [UnityTest]
        public IEnumerator IsPlaying_FalseAfterComplete()
        {
            // Dùng reflection để rút ngắn duration nếu có
            var dur = typeof(ChapterTransition).GetField("_duration",
                BindingFlags.NonPublic | BindingFlags.Instance);
            dur?.SetValue(_ct, 0.2f);

            _ct.PlayTransition("Chapter 1", "2000");
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(R.Bool(_ct, "_isPlaying"),
                "_isPlaying phải false sau khi transition kết thúc");
        }

        // OnTransitionComplete event
        [UnityTest]
        public IEnumerator OnTransitionComplete_FiredAfterFinish()
        {
            var dur = typeof(ChapterTransition).GetField("_duration",
                BindingFlags.NonPublic | BindingFlags.Instance);
            dur?.SetValue(_ct, 0.2f);

            bool fired = false;
            _ct.OnTransitionComplete.AddListener(() => fired = true);
            _ct.PlayTransition("Chapter 1", "2000");
            yield return new WaitForSeconds(0.5f);
            Assert.IsTrue(fired, "OnTransitionComplete phải fire sau khi transition xong");
        }

        // Không play khi đang play (graceful ignore)
        [UnityTest]
        public IEnumerator PlayTransition_WhilePlaying_DoesNotCrash()
        {
            _ct.PlayTransition("Chapter 1", "2000");
            yield return null;
            Assert.DoesNotThrow(() => _ct.PlayTransition("Chapter 2", "1970"),
                "Gọi PlayTransition khi đang play không được crash");
        }

        // Chapter name được lưu
        [UnityTest]
        public IEnumerator PlayTransition_StoresChapterName()
        {
            _ct.PlayTransition("Chapter 3", "1990");
            yield return null;
            var nameField = typeof(ChapterTransition).GetField("_chapterName",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var stored = nameField?.GetValue(_ct) as string;
            Assert.AreEqual("Chapter 3", stored,
                "PlayTransition phải lưu chapter name vào _chapterName");
        }
    }
}
