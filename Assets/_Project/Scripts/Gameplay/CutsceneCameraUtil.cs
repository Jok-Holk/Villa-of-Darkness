using System.Collections;
using UnityEngine;

// Helper dùng chung cho các cutscene ép camera xoay nhìn về 1 điểm (Slerp) -- tách đúng yaw (xoay object
// gốc Player) + pitch (xoay local camera con) theo kiến trúc PlayerController hiện có (xem
// PlayerController.HandleMouseLook: root xoay yaw, _cameraTransform con xoay pitch riêng bằng
// localRotation X). Dùng chung cho StairsRevealCutscene, DoorRelockTwistCutscene.
public static class CutsceneCameraUtil
{
    public static IEnumerator LookAt(PlayerController player, Transform camTransform, Transform target, float duration)
    {
        if (player == null || camTransform == null || target == null) yield break;

        Transform root = player.transform;
        Quaternion fromYaw = root.rotation;
        Quaternion fromPitch = camTransform.localRotation;

        Vector3 dir = target.position - camTransform.position;
        if (dir.sqrMagnitude < 0.0001f) yield break;

        float targetYawDeg = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z)).eulerAngles.y;
        Quaternion toYaw = Quaternion.Euler(0f, targetYawDeg, 0f);

        float pitchDeg = Quaternion.LookRotation(dir).eulerAngles.x;
        if (pitchDeg > 180f) pitchDeg -= 360f; // giữ đúng khoảng -80..80 khớp Clamp trong PlayerController
        pitchDeg = Mathf.Clamp(pitchDeg, -80f, 80f);
        Quaternion toPitch = Quaternion.Euler(pitchDeg, 0f, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            root.rotation = Quaternion.Slerp(fromYaw, toYaw, k);
            camTransform.localRotation = Quaternion.Slerp(fromPitch, toPitch, k);
            yield return null;
        }
        root.rotation = toYaw;
        camTransform.localRotation = toPitch;

        // Đồng bộ lại _xRotation nội bộ PlayerController -- tránh camera giật về góc cũ ngay khung hình
        // đầu tiên mouse-look chạy lại (đúng pattern IntroManager đã dùng, xem PlayerController.SetPitch()).
        player.SetPitch(pitchDeg);
    }
}
