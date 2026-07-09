using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace Phase1.LePhuTuanAnh
{
    /// <summary>
    /// Test này PHẢI chạy trong scene Chapter1.
    /// Mở scene Chapter1 trước khi chạy test.
    /// P4 dựng blockout xong thì tag/đặt tên object đúng theo quy ước
    /// rồi báo lead vào chạy test này để confirm.
    /// </summary>
    public class BlockoutTests
    {
        // ─── Object tồn tại ──────────────────────
        [UnityTest]
        public IEnumerator PlayerSpawn_Exists()
        {
            yield return null;
            var obj = GameObject.Find("PlayerSpawn");
            Assert.IsNotNull(obj,
                "Phải có GameObject tên 'PlayerSpawn' — điểm spawn player đầu chapter");
        }

        [UnityTest]
        public IEnumerator Piano_Exists()
        {
            yield return null;
            var obj = GameObject.Find("Piano");
            Assert.IsNotNull(obj,
                "Phải có GameObject tên 'Piano' trong phòng khách");
        }

        [UnityTest]
        public IEnumerator Well_Exists()
        {
            yield return null;
            var obj = GameObject.Find("Gieng") ?? GameObject.Find("Well");
            Assert.IsNotNull(obj,
                "Phải có GameObject tên 'Gieng' hoặc 'Well' ngoài sân");
        }

        [UnityTest]
        public IEnumerator HideSpot_Exists()
        {
            yield return null;
            var obj = GameObject.FindWithTag("HideSpot");
            Assert.IsNotNull(obj,
                "Phải có ít nhất 1 object tag 'HideSpot' (tủ trốn) trong nhà");
        }

        // ─── Waypoint cho AI ─────────────────────
        [UnityTest]
        public IEnumerator Waypoints_AtLeastFour()
        {
            yield return null;
            var waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
            Assert.GreaterOrEqual(waypoints.Length, 4,
                "Phải có ít nhất 4 waypoint tag 'Waypoint' cho AI patrol");
        }

        [UnityTest]
        public IEnumerator Waypoints_AreInsideBuilding()
        {
            yield return null;
            var waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
            foreach (var wp in waypoints)
            {
                // Waypoint không được ở quá xa gốc tọa độ — map nhỏ
                Assert.Less(wp.transform.position.magnitude, 100f,
                    $"Waypoint '{wp.name}' ở vị trí bất thường: {wp.transform.position}");
            }
        }

        // ─── NavMesh ─────────────────────────────
        [UnityTest]
        public IEnumerator NavMesh_IsBaked()
        {
            yield return null;
            // Thử sample một điểm gần gốc — nếu NavMesh chưa bake thì fail
            var result = new UnityEngine.AI.NavMeshHit();
            bool hit = UnityEngine.AI.NavMesh.SamplePosition(
                Vector3.zero, out result, 50f,
                UnityEngine.AI.NavMesh.AllAreas);
            Assert.IsTrue(hit,
                "NavMesh chưa được bake. Window → AI → Navigation → Bake.");
        }

        // ─── Collider và scale ────────────────────
        [UnityTest]
        public IEnumerator AllRoomObjects_HaveColliders()
        {
            yield return null;
            // Check các object quan trọng không bị thiếu collider
            string[] required = { "Piano", "Gieng", "Wall_Kitchen", "Floor" };
            foreach (var name in required)
            {
                var obj = GameObject.Find(name);
                if (obj == null) continue; // optional, chỉ check nếu tồn tại
                Assert.IsNotNull(obj.GetComponent<Collider>(),
                    $"'{name}' thiếu Collider — player sẽ xuyên qua");
            }
        }

        [UnityTest]
        public IEnumerator PlayerSpawn_IsAboveGround()
        {
            yield return null;
            var spawn = GameObject.Find("PlayerSpawn");
            if (spawn == null) yield break;
            Assert.Greater(spawn.transform.position.y, -1f,
                "PlayerSpawn không được dưới mặt đất");
        }

        // ─── Khoảng cách flow ─────────────────────
        [UnityTest]
        public IEnumerator PianoToExit_ReasonableDistance()
        {
            yield return null;
            var piano = GameObject.Find("Piano");
            var exit  = GameObject.Find("ExitDoor") ?? GameObject.Find("Door_Exit");
            if (piano == null || exit == null) yield break;

            float dist = Vector3.Distance(piano.transform.position, exit.transform.position);
            Assert.Less(dist, 30f,
                "Khoảng cách Piano → ExitDoor quá xa, player sẽ mất phương hướng");
            Assert.Greater(dist, 3f,
                "Piano và ExitDoor quá gần nhau");
        }
    }
}
