using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

namespace Phase1.PhucThai
{
    // ════════════════════════════════════
    // GAMEDATA
    // ════════════════════════════════════
    public class GameDataTests
    {
        [SetUp] public void Before() => GameData.Reset();

        [Test]
        public void Reset_ClearsItems()
        {
            GameData.collectedItems.Add("music_box");
            GameData.Reset();
            Assert.AreEqual(0, GameData.collectedItems.Count);
        }

        [Test]
        public void Reset_ClearsAudioLogs()
        {
            GameData.audioLogsHeard = 7;
            GameData.Reset();
            Assert.AreEqual(0, GameData.audioLogsHeard);
        }

        [Test]
        public void Reset_SetsChapterToOne()
        {
            GameData.currentChapter = 4;
            GameData.Reset();
            Assert.AreEqual(1, GameData.currentChapter);
        }

        [Test]
        public void AddItem_PersistsInList()
        {
            GameData.collectedItems.Add("silver_mirror");
            Assert.IsTrue(GameData.collectedItems.Contains("silver_mirror"));
        }

        [Test]
        public void AddMultipleItems_AllPersist()
        {
            GameData.collectedItems.Add("music_box");
            GameData.collectedItems.Add("silver_mirror");
            GameData.collectedItems.Add("salt_jar");
            Assert.AreEqual(3, GameData.collectedItems.Count);
        }

        [Test]
        public void AudioLogsHeard_Increments()
        {
            GameData.audioLogsHeard++;
            GameData.audioLogsHeard++;
            Assert.AreEqual(2, GameData.audioLogsHeard);
        }
    }

    // ════════════════════════════════════
    // GAMEMANAGER
    // ════════════════════════════════════
    public class GameManagerTests
    {
        private GameObject _go;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go = new GameObject("GameManager");
            _go.AddComponent<GameManager>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Object.Destroy(_go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Instance_NotNull_AfterAwake()
        {
            yield return null;
            Assert.IsNotNull(GameManager.Instance);
        }

        [UnityTest]
        public IEnumerator Singleton_DestroysDuplicate()
        {
            var first = GameManager.Instance;
            var dup = new GameObject("GM_Dup");
            dup.AddComponent<GameManager>();
            yield return null;
            Assert.AreEqual(first, GameManager.Instance,
                "Instance phải là cái đầu tiên");
        }

        [UnityTest]
        public IEnumerator PlayerDead_DoesNotThrow()
        {
            yield return null;
            Assert.DoesNotThrow(() => GameManager.Instance.PlayerDead());
        }
    }

    // ════════════════════════════════════
    // IINTERACTABLE
    // ════════════════════════════════════
    public class IInteractableTests
    {
        [Test]
        public void Interface_CanBeImplemented()
        {
            IInteractable obj = new Dummy();
            Assert.DoesNotThrow(() => obj.Interact());
        }

        [Test]
        public void Interact_IsCalled()
        {
            var d = new Dummy();
            d.Interact();
            Assert.IsTrue(d.Called);
        }

        [Test]
        public void TwoInstances_IndependentState()
        {
            var a = new Dummy();
            var b = new Dummy();
            a.Interact();
            Assert.IsTrue(a.Called);
            Assert.IsFalse(b.Called);
        }

        class Dummy : IInteractable
        {
            public bool Called { get; private set; }
            public void Interact() => Called = true;
        }
    }

    // ════════════════════════════════════
    // PLAYERCONTROLLER
    // ════════════════════════════════════
    public class PlayerControllerTests
    {
        private GameObject _player;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _player = new GameObject("Player");
            _player.tag = "Player";
            _player.AddComponent<CharacterController>();
            var cam = new GameObject("Camera");
            cam.transform.SetParent(_player.transform);
            var pc = _player.AddComponent<PlayerController>();
            typeof(PlayerController)
                .GetField("_cameraTransform", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(pc, cam.transform);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_player); yield return null; }

        [UnityTest]
        public IEnumerator PlayerController_AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_player.GetComponent<PlayerController>());
        }

        [UnityTest]
        public IEnumerator RequiresCharacterController()
        {
            yield return null;
            Assert.IsNotNull(_player.GetComponent<CharacterController>());
        }

        [UnityTest]
        public IEnumerator SetInputEnabled_False_DisablesComponent()
        {
            var pc = _player.GetComponent<PlayerController>();
            pc.SetInputEnabled(false);
            yield return null;
            Assert.IsFalse(pc.enabled);
        }

        [UnityTest]
        public IEnumerator SetInputEnabled_True_EnablesComponent()
        {
            var pc = _player.GetComponent<PlayerController>();
            pc.SetInputEnabled(false);
            pc.SetInputEnabled(true);
            yield return null;
            Assert.IsTrue(pc.enabled);
        }
    }

    // ════════════════════════════════════
    // INTERACTION SYSTEM
    // ════════════════════════════════════
    public class InteractionSystemTests
    {
        private GameObject _cam;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _cam = new GameObject("Camera");
            _cam.AddComponent<Camera>();
            var sys = _cam.AddComponent<InteractionSystem>();
            int maskValue = LayerMask.GetMask("Interactable");
            LayerMask mask = (LayerMask)maskValue; 
            typeof(InteractionSystem)
                .GetField("_interactLayer", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(sys, mask);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_cam); yield return null; }

        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_cam.GetComponent<InteractionSystem>());
        }

        [UnityTest]
        public IEnumerator RaycastHitsObject_CallsInteract()
        {
            var sys = _cam.GetComponent<InteractionSystem>();
            var target = new GameObject("Target");
            target.layer = LayerMask.NameToLayer("Interactable");
            target.transform.position = _cam.transform.position + Vector3.forward * 1.5f;
            target.AddComponent<BoxCollider>();
            var dummy = target.AddComponent<DummyMono>();
            yield return null;

            typeof(InteractionSystem)
                .GetMethod("TryInteract", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(sys, null);
            yield return null;

            Assert.IsTrue(dummy.Called, "Raycast phải gọi Interact() trên object trong tầm");
            Object.Destroy(target);
        }

        [UnityTest]
        public IEnumerator OutOfRange_DoesNotCallInteract()
        {
            var sys = _cam.GetComponent<InteractionSystem>();
            var target = new GameObject("FarTarget");
            target.layer = LayerMask.NameToLayer("Interactable");
            target.transform.position = _cam.transform.position + Vector3.forward * 50f;
            target.AddComponent<BoxCollider>();
            var dummy = target.AddComponent<DummyMono>();
            yield return null;

            typeof(InteractionSystem)
                .GetMethod("TryInteract", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(sys, null);
            yield return null;

            Assert.IsFalse(dummy.Called, "Ngoài tầm không được gọi Interact()");
            Object.Destroy(target);
        }

        class DummyMono : MonoBehaviour, IInteractable
        {
            public bool Called { get; private set; }
            public void Interact() => Called = true;
        }
    }

    // ════════════════════════════════════
    // GHOST AI
    // ════════════════════════════════════
    public class GhostAITests
    {
        private GameObject _gm, _ghost, _player;
        static readonly BindingFlags BF =
            BindingFlags.NonPublic | BindingFlags.Instance;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _gm = new GameObject("GameManager");
            _gm.AddComponent<GameManager>();

            _ghost = new GameObject("Ghost");
            _ghost.tag = "Ghost";
            _ghost.AddComponent<UnityEngine.AI.NavMeshAgent>();
            _ghost.AddComponent<GhostAI>();

            _player = new GameObject("Player");
            _player.tag = "Player";
            _player.AddComponent<CharacterController>();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After()
        {
            Object.Destroy(_gm);
            Object.Destroy(_ghost);
            Object.Destroy(_player);
            yield return null;
        }

        GhostAI.State GetState() =>
            (GhostAI.State)typeof(GhostAI)
                .GetField("_currentState", BF)
                .GetValue(_ghost.GetComponent<GhostAI>());

        void SetField(string name, object val) =>
            typeof(GhostAI).GetField(name, BF)
                ?.SetValue(_ghost.GetComponent<GhostAI>(), val);

        bool CallBool(string name) =>
            (bool)(typeof(GhostAI).GetMethod(name, BF)
                ?.Invoke(_ghost.GetComponent<GhostAI>(), null) ?? false);

        void CallVoid(string name) =>
            typeof(GhostAI).GetMethod(name, BF)
                ?.Invoke(_ghost.GetComponent<GhostAI>(), null);

        void CallVoidArg(string name, object arg) =>
            typeof(GhostAI).GetMethod(name, BF)
                ?.Invoke(_ghost.GetComponent<GhostAI>(), new[] { arg });

        [UnityTest]
        public IEnumerator StartsInPatrol()
        {
            yield return null;
            Assert.AreEqual(GhostAI.State.Patrol, GetState());
        }

        [UnityTest]
        public IEnumerator EnterChase_SetsChaseState()
        {
            yield return null;
            CallVoid("EnterChase");
            Assert.AreEqual(GhostAI.State.Chase, GetState());
        }

        [UnityTest]
        public IEnumerator EnterInvestigate_SetsInvestigateState()
        {
            yield return null;
            CallVoidArg("EnterInvestigate", Vector3.zero);
            Assert.AreEqual(GhostAI.State.Investigate, GetState());
        }

        [UnityTest]
        public IEnumerator EnterPatrol_FromChase_ReturnsPatrol()
        {
            yield return null;
            CallVoid("EnterChase");
            CallVoid("EnterPatrol");
            Assert.AreEqual(GhostAI.State.Patrol, GetState());
        }

        [UnityTest]
        public IEnumerator DetectPlayer_InFront_ReturnsTrue()
        {
            var ai = _ghost.GetComponent<GhostAI>();
            _player.transform.position =
                _ghost.transform.position + _ghost.transform.forward * 5f;
            SetField("_player", _player.transform);
            yield return null;
            Assert.IsTrue(CallBool("CanDetectPlayer"));
        }

        [UnityTest]
        public IEnumerator DetectPlayer_TooFar_ReturnsFalse()
        {
            SetField("_player", _player.transform);
            _player.transform.position =
                _ghost.transform.position + Vector3.forward * 99f;
            yield return null;
            Assert.IsFalse(CallBool("CanDetectPlayer"));
        }

        [UnityTest]
        public IEnumerator HearPlayer_InRange_ReturnsTrue()
        {
            SetField("_player", _player.transform);
            _player.transform.position =
                _ghost.transform.position + Vector3.right * 4f;
            yield return null;
            Assert.IsTrue(CallBool("CanHearPlayer"));
        }

        [UnityTest]
        public IEnumerator PlayerDead_ExistsAndDoesNotThrow()
        {
            yield return null;
            Assert.IsNotNull(GameManager.Instance);
            Assert.DoesNotThrow(() => GameManager.Instance.PlayerDead());
        }
    }
}
