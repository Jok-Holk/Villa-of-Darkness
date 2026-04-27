using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

namespace Phase2.NguyenTruongVu
{
    public class AmbientZoneTests
    {
        private GameObject  _go;
        private AmbientZone _zone;
        private AudioSource _src;

        [UnitySetUp]
        public IEnumerator Before()
        {
            _go   = new GameObject("AmbientZone");
            _go.AddComponent<BoxCollider>().isTrigger = true;
            _src  = _go.AddComponent<AudioSource>();
            _zone = _go.AddComponent<AmbientZone>();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator After() { Object.Destroy(_go); yield return null; }

        private void Set(string name, object value) =>
            typeof(AmbientZone)
                .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(_zone, value);

        [UnityTest]
        public IEnumerator AttachesNoCrash()
        {
            yield return null;
            Assert.IsNotNull(_zone);
        }

        [UnityTest]
        public IEnumerator StartsInactive()
        {
            yield return null;
            Assert.IsFalse(_zone.IsActive, "AmbientZone phải inactive lúc start");
        }

        [UnityTest]
        public IEnumerator StartsWithZeroVolume()
        {
            yield return null;
            Assert.AreEqual(0f, _src.volume, 0.001f, "Volume phải = 0 lúc start");
        }

        [UnityTest]
        public IEnumerator FadeIn_SetsActiveTrue()
        {
            Set("_fadeDuration", 0.2f);
            Set("_targetVolume",  0.8f);
            _go.StartCoroutine(_zone.FadeIn());
            yield return new WaitForSeconds(0.4f);
            Assert.IsTrue(_zone.IsActive, "IsActive phải true sau FadeIn");
        }

        [UnityTest]
        public IEnumerator FadeIn_RaisesVolumeToTarget()
        {
            Set("_fadeDuration", 0.2f);
            Set("_targetVolume",  0.8f);
            _go.StartCoroutine(_zone.FadeIn());
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(0.8f, _src.volume, 0.05f, "Volume phải đạt _targetVolume sau FadeIn");
        }

        [UnityTest]
        public IEnumerator FadeOut_SetsActiveFalse()
        {
            Set("_fadeDuration", 0.1f);
            Set("_targetVolume",  0.8f);
            _go.StartCoroutine(_zone.FadeIn());
            yield return new WaitForSeconds(0.2f);
            _go.StartCoroutine(_zone.FadeOut());
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(_zone.IsActive, "IsActive phải false sau FadeOut");
        }

        [UnityTest]
        public IEnumerator FadeOut_DecreasesVolumeToZero()
        {
            _src.volume = 0.8f;
            Set("_fadeDuration", 0.2f);
            _go.StartCoroutine(_zone.FadeOut());
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(0f, _src.volume, 0.05f, "Volume phải về 0 sau FadeOut");
        }

        [UnityTest]
        public IEnumerator FadeOut_WhenNotActive_DoesNotThrow()
        {
            Set("_fadeDuration", 0.1f);
            bool threw = false;
            try { _go.StartCoroutine(_zone.FadeOut()); }
            catch { threw = true; }
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(threw, "FadeOut khi chưa active không được throw");
        }
    }
}
