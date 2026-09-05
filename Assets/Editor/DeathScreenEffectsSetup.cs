using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Chỉ THÊM các layer hiệu ứng khí quyển (nhiễu TV/scanline/vignette/bóng ma/rung sắc/chớp trắng)
/// vào DeathUI ĐÃ CÓ SẴN trong scene. KHÔNG bao giờ xoá hay chỉnh sửa Backdrop/BlackVeil/DateLine/
/// Title/ReportLine/MissingName/RetryButton/MenuButton - nếu các object này thiếu, tool sẽ báo lỗi
/// và dừng lại thay vì tự tạo lại (đó là việc của DeathScreenSceneBuilder, không phải tool này).
/// Chạy lại nhiều lần an toàn (idempotent): layer nào đã có thì bỏ qua, không tạo trùng.
/// </summary>
public static class DeathScreenEffectsSetup
{
    private static readonly string[] GameScenePaths =
    {
        "Assets/MainGame/Game.unity",
        "Assets/MainGame/GameP2.unity"
    };

    [MenuItem("Tools/MainGame/Add Death Screen Atmosphere Effects")]
    public static void AddEffectsToAllScenes()
    {
        foreach (var scenePath in GameScenePaths)
            AddEffectsToScene(scenePath);
    }

    private static void AddEffectsToScene(string scenePath)
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != scenePath)
            scene = EditorSceneManager.OpenScene(scenePath);

        var deathUiObject = FindSceneObject("DeathUI");
        if (deathUiObject == null)
        {
            Debug.LogWarning($"[{scenePath}] Không tìm thấy DeathUI - bỏ qua. Hãy tạo DeathUI trước (Tools/MainGame/Rebuild Death Screen UI) rồi chạy lại.");
            return;
        }

        var root = deathUiObject.GetComponent<RectTransform>();
        var titleSource = deathUiObject.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        if (titleSource == null)
        {
            Debug.LogError($"[{scenePath}] DeathUI thiếu 'Title' - không thể tạo bản ma headline. Không đụng gì thêm, dừng lại.");
            return;
        }

        var titleFont = titleSource.font;
        bool changed = false;

        changed |= EnsureRawImage(root, "GlitchNoise", new Color(1f, 1f, 1f, 0f), false, stretch: true);
        changed |= EnsureScanline(root);
        changed |= EnsureRawImage(root, "Vignette", Color.white, false, stretch: true);
        changed |= EnsureRawImage(root, "Apparition", new Color(1f, 1f, 1f, 0f), false, stretch: true);
        changed |= EnsureWhiteFlash(root);
        changed |= EnsureHeadlineGhost(root, "HeadlineGhostR", titleSource, titleFont, new Color(1f, 0.15f, 0.15f, 0f), 4f);
        changed |= EnsureHeadlineGhost(root, "HeadlineGhostC", titleSource, titleFont, new Color(0.15f, 0.85f, 1f, 0f), -4f);

        // Vignette/Apparition phải nằm DƯỚI nội dung báo giấy (chỉ làm tối nền), không được đè
        // lên chữ - luôn ép lại thứ tự này kể cả khi layer đã tồn tại từ lần chạy trước.
        PlaceRightAfter(root, "Vignette", "Backdrop");
        PlaceRightAfter(root, "Apparition", "Vignette");

        var effects = deathUiObject.GetComponent<DeathScreenEffects>();
        if (effects == null)
        {
            effects = Undo.AddComponent<DeathScreenEffects>(deathUiObject);
            changed = true;
        }

        if (!changed)
        {
            Debug.Log($"[{scenePath}] DeathUI đã có đủ hiệu ứng từ trước, không có gì để thêm.");
            return;
        }

        EditorUtility.SetDirty(deathUiObject);
        EditorUtility.SetDirty(effects);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[{scenePath}] Đã thêm hiệu ứng khí quyển vào DeathUI (không đụng nội dung báo giấy có sẵn).");
    }

    private static bool EnsureRawImage(RectTransform parent, string objectName, Color color, bool raycastTarget, bool stretch)
    {
        if (parent.Find(objectName) != null)
            return false;

        var rect = CreateUIObject(parent, objectName);
        if (stretch)
            Stretch(rect);

        var image = rect.gameObject.AddComponent<RawImage>();
        image.color = color;
        image.raycastTarget = raycastTarget;
        return true;
    }

    private static bool EnsureScanline(RectTransform parent)
    {
        if (parent.Find("ScanlineStrip") != null)
            return false;

        var rect = CreateUIObject(parent, "ScanlineStrip");
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = false;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(0f, 3f);
        rect.anchoredPosition = Vector2.zero;
        return true;
    }

    private static bool EnsureWhiteFlash(RectTransform parent)
    {
        if (parent.Find("WhiteFlash") != null)
            return false;

        var rect = CreateUIObject(parent, "WhiteFlash");
        Stretch(rect);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = false;
        return true;
    }

    private static bool EnsureHeadlineGhost(RectTransform parent, string objectName, TextMeshProUGUI source, TMP_FontAsset font, Color color, float xOffset)
    {
        if (parent.Find(objectName) != null)
            return false;

        var rect = CreateUIObject(parent, objectName);
        var sourceRect = source.rectTransform;
        rect.anchorMin = sourceRect.anchorMin;
        rect.anchorMax = sourceRect.anchorMax;
        rect.offsetMin = sourceRect.offsetMin;
        rect.offsetMax = sourceRect.offsetMax;
        rect.anchoredPosition = sourceRect.anchoredPosition + new Vector2(xOffset, 0f);

        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = source.text;
        label.fontSize = source.fontSize;
        label.enableAutoSizing = source.enableAutoSizing;
        label.fontSizeMin = source.fontSizeMin;
        label.fontSizeMax = source.fontSizeMax;
        label.alignment = source.alignment;
        label.fontStyle = source.fontStyle;
        label.characterSpacing = source.characterSpacing;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.color = color;
        if (font != null)
            label.font = font;
        return true;
    }

    private static RectTransform CreateUIObject(Transform parent, string objectName)
    {
        var obj = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer));
        Undo.RegisterCreatedObjectUndo(obj, "Add Death Screen Effect Layer");
        var rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.SetAsLastSibling();
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (var transform in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform != null && transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }

    private static void PlaceRightAfter(RectTransform root, string childName, string afterName)
    {
        var child = root.Find(childName);
        if (child == null)
            return;

        var after = root.Find(afterName);
        int index = after != null ? after.GetSiblingIndex() + 1 : 0;
        child.SetSiblingIndex(index);
    }
}
