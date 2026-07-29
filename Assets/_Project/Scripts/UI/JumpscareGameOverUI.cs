using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Màn hình "Game Over" ĐƠN GIẢN -- dùng cho các cái chết KHÔNG phải kết Chương 1 (nhìn vào gương ma,
// bị Ma Vú Dài đuổi kịp bắt được). Khác với DeathScreenUI (bản tin báo mất tích, đầy đủ hiệu ứng) chỉ
// dành riêng cho kết Chapter 1 thật ở giếng nước (xem WellDeathSequence).
//
// Tự dựng toàn bộ UI qua code ngay lúc Trigger() -- không cần Jok build Canvas/Image/Text tay, theo đúng
// pattern WellDeathSequence đã dùng cho overlay/fade. Sau khi người chơi bấm E/chuột trái, tự gọi
// GameManager.PlayerRespawn() để reload lại checkpoint gần nhất (y hệt nút "Thử Lại" của DeathScreenUI).
public static class JumpscareGameOverUI
{
    private class Runner : MonoBehaviour { }
    private static Runner _runner;

    private const string GameOverLine = "BẠN ĐÃ CHẾT";
    private const string ContinueHint = "Nhấn [E] để tiếp tục";

    /// <param name="jumpscareImage">Ảnh jumpscare bật đột ngột giữa màn hình. Để trống vẫn chạy được (bỏ qua bước ảnh, chỉ còn tiếng hét + fade đen).</param>
    /// <param name="scream">Tiếng hét phát cùng lúc ảnh bật ra.</param>
    /// <param name="impactScale">Độ "to" của ẢNH jumpscare -- 1 = bình thường, 1.5 = to hơn (Ma Vú Dài bắt được, dồn dập hơn).</param>
    /// <param name="volumeMultiplier">Độ "to" của TIẾNG HÉT -- tách riêng khỏi impactScale (ảnh to không nhất thiết tiếng phải to theo và ngược lại). 2 = x2 volume (gương), có thể vượt 1 -- KHÔNG Clamp01 như trước (trước đây lỡ kẹp về tối đa 1.0 nên "impactScale 1.5" chưa từng thật sự to hơn tiếng mặc định).</param>
    public static void Trigger(Sprite jumpscareImage, AudioClip scream, float impactScale = 1f, float volumeMultiplier = 1f)
    {
        if (_runner == null)
        {
            var go = new GameObject("JumpscareGameOverUI_Runner");
            Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<Runner>();
        }
        _runner.StartCoroutine(RunSequence(jumpscareImage, scream, impactScale, volumeMultiplier));
    }

    private static IEnumerator RunSequence(Sprite jumpscareImage, AudioClip scream, float impactScale, float volumeMultiplier)
    {
        PlayerController.Instance?.SetInputEnabled(false);

        // Ẩn HUD gameplay -- không cần gọi lại true, sau khi bấm E/chuột trái sẽ PlayerRespawn() reload
        // scene, HudMetersUI tự huỷ + tự dựng lại sạch (không DontDestroyOnLoad), InteractPrompt cũng vậy.
        InteractPromptUI.Instance?.SetDotVisible(false);
        HudMetersUI.Instance.SetVisible(false);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[JumpscareGameOverUI] Không tìm thấy Canvas nào trong scene!");
            yield break;
        }

        // ─ 1. Ảnh jumpscare bật đột ngột (pop scale) + tiếng hét cùng lúc ─
        GameObject imgGO = null;
        if (jumpscareImage != null)
        {
            imgGO = new GameObject("JumpscareImage");
            imgGO.transform.SetParent(canvas.transform, false);
            var img = imgGO.AddComponent<Image>();
            img.sprite = jumpscareImage;
            img.preserveAspect = true;
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            float startScale = impactScale * 0.7f;
            rt.localScale = Vector3.one * startScale;

            float pop = 0.08f;
            float t = 0f;
            while (t < pop)
            {
                t += Time.unscaledDeltaTime;
                rt.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * impactScale, t / pop);
                yield return null;
            }
            rt.localScale = Vector3.one * impactScale;
        }

        if (scream != null)
            // KHÔNG Clamp01 -- volumeMultiplier > 1 phải thật sự to hơn AudioSource.volume mặc định (Unity
            // cho phép volume vượt 1, chỉ clamp nhẹ ở mức 3 để tránh vặn quá tay gây rè/clip tiếng).
            AudioManager.Instance?.PlaySFX(scream, Mathf.Clamp(volumeMultiplier, 0f, 3f));

        yield return new WaitForSecondsRealtime(0.5f);

        // ─ 2. Fade đen ─
        GameObject fadeGO = new GameObject("GameOverFade");
        fadeGO.transform.SetParent(canvas.transform, false);
        var fadeImg = fadeGO.AddComponent<Image>();
        fadeImg.color = Color.clear;
        var frt = fadeImg.rectTransform;
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;

        float fadeDuration = 1f;
        float ft = 0f;
        while (ft < fadeDuration)
        {
            ft += Time.unscaledDeltaTime;
            fadeImg.color = Color.Lerp(Color.clear, Color.black, ft / fadeDuration);
            yield return null;
        }
        fadeImg.color = Color.black;

        if (imgGO != null) Object.Destroy(imgGO);

        // ─ 3. "BẠN ĐÃ CHẾT" fade in ─
        var textGO = new GameObject("GameOverText");
        textGO.transform.SetParent(canvas.transform, false);
        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = GameOverLine;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 64;
        tmp.fontStyle = FontStyles.Bold;
        Color textTarget = new Color(0.75f, 0.05f, 0.05f, 1f);
        tmp.color = new Color(textTarget.r, textTarget.g, textTarget.b, 0f);
        var trt = tmp.rectTransform;
        trt.anchorMin = new Vector2(0.5f, 0.55f);
        trt.anchorMax = new Vector2(0.5f, 0.55f);
        trt.sizeDelta = new Vector2(900, 150);
        trt.anchoredPosition = Vector2.zero;

        var hintGO = new GameObject("GameOverHint");
        hintGO.transform.SetParent(canvas.transform, false);
        var hintTmp = hintGO.AddComponent<TextMeshProUGUI>();
        hintTmp.text = ContinueHint;
        hintTmp.alignment = TextAlignmentOptions.Center;
        hintTmp.fontSize = 22;
        hintTmp.color = new Color(0.8f, 0.8f, 0.8f, 0f);
        var hrt = hintTmp.rectTransform;
        hrt.anchorMin = new Vector2(0.5f, 0.4f);
        hrt.anchorMax = new Vector2(0.5f, 0.4f);
        hrt.sizeDelta = new Vector2(700, 60);
        hrt.anchoredPosition = Vector2.zero;

        float textFadeDuration = 1f;
        float tt = 0f;
        while (tt < textFadeDuration)
        {
            tt += Time.unscaledDeltaTime;
            float a = tt / textFadeDuration;
            tmp.color = new Color(textTarget.r, textTarget.g, textTarget.b, a);
            hintTmp.color = new Color(0.8f, 0.8f, 0.8f, a * 0.8f);
            yield return null;
        }

        // Đợi ít nhất 1 nhịp rồi mới nhận input, tránh trường hợp phím E vừa gỡ vải/nhìn gương lỡ tay bị tính luôn ở đây
        yield return new WaitForSecondsRealtime(0.5f);

        while (!Input.GetKeyDown(KeyCode.E) && !Input.GetMouseButtonDown(0))
            yield return null;

        Object.Destroy(fadeGO);
        Object.Destroy(textGO);
        Object.Destroy(hintGO);

        GameManager.Instance?.PlayerRespawn();
    }
}
