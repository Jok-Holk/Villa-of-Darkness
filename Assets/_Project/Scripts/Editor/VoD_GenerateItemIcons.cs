using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

// TỰ ĐỘNG CHỤP ICON cho từng ItemData bằng chính model 3D thật của nó -- không cần ai vẽ icon 2D tay.
// Nguồn model lấy theo thứ tự ưu tiên: handHeldPrefab (chìa khoá) -> object đăng ký trong
// InventoryUI._examineRegistry (giấy, sổ ghi nợ...) -> 1 vài fallback tên object biết trước (nhật ký).
// Camera tạm đặt xa hoàn toàn khỏi villa thật (3000,3000,3000), tự canh khung theo bounds thật của model,
// tự chọn hướng nhìn theo TRỤC MỎNG NHẤT (vật phẳng như giấy/trang sổ sẽ được nhìn gần thẳng mặt thay vì
// nhìn trúng cạnh mỏng dính) -- xong chụp ra PNG, gán làm Sprite thẳng vào ItemData.icon.
public static class VoD_GenerateItemIcons
{
    private const string OutputFolder = "Assets/_Project/Textures/ItemIcons";
    private const int IconSize = 256;

    [MenuItem("VoD/Villa/Fix - Tự Chụp Icon Cho Tất Cả Item")]
    public static void GenerateAll()
    {
        EnsureFolder();

        Dictionary<string, ExamineItem> examineMap = BuildExamineMap();

        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        int done = 0, skipped = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data == null) continue;

            GameObject source = ResolveIconSource(data, examineMap);
            if (source == null)
            {
                Debug.LogWarning($"[VoD][Icon] Bỏ qua '{data.itemId}' -- không tìm được model nguồn (không có handHeldPrefab, không có trong Examine Registry, không khớp fallback tên object nào biết trước).");
                skipped++;
                continue;
            }

            string outputPath = $"{OutputFolder}/icon_{data.itemId}.png";
            if (SnapshotIcon(source, outputPath)) done++;
            else skipped++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int assigned = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (data == null) continue;

            string iconPath = $"{OutputFolder}/icon_{data.itemId}.png";
            if (!File.Exists(iconPath)) continue;

            var importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType     = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled   = false;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            if (sprite != null)
            {
                var so = new SerializedObject(data);
                so.FindProperty("icon").objectReferenceValue = sprite;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(data);
                assigned++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[VoD][Icon] XONG -- {done} icon đã chụp, {assigned} đã gán vào ItemData, {skipped} bỏ qua (xem Warning ở trên).");
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Textures"))
            AssetDatabase.CreateFolder("Assets/_Project", "Textures");
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/_Project/Textures", "ItemIcons");
    }

    private static Dictionary<string, ExamineItem> BuildExamineMap()
    {
        var map = new Dictionary<string, ExamineItem>();
        var inventoryUI = Object.FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (inventoryUI == null) return map;

        var so = new SerializedObject(inventoryUI);
        var registryProp = so.FindProperty("_examineRegistry");
        if (registryProp == null) return map;

        for (int i = 0; i < registryProp.arraySize; i++)
        {
            var entry = registryProp.GetArrayElementAtIndex(i);
            string id = entry.FindPropertyRelative("itemId").stringValue;
            var examineRef = entry.FindPropertyRelative("examineItem").objectReferenceValue as ExamineItem;
            if (examineRef != null && !map.ContainsKey(id))
                map[id] = examineRef;
        }
        return map;
    }

    private static GameObject ResolveIconSource(ItemData data, Dictionary<string, ExamineItem> examineMap)
    {
        if (data.handHeldPrefab != null) return data.handHeldPrefab;

        if (examineMap.TryGetValue(data.itemId, out var examineItem) && examineItem != null)
            return examineItem.gameObject;

        // Tìm theo ĐÚNG quan hệ dữ liệu thật -- bất kỳ PickupItem nào trong scene có _itemData trỏ đúng
        // ItemData này -- thay vì đoán tên object (VD hộp âm nhạc nằm trong prefab cuộn băng, PickupItem bị
        // disable sẵn chờ nghe băng xong mới bật, tên object không liên quan gì tới "hop_am_nhac" cả).
        // Include Inactive vì nhiều PickupItem cố ý tắt sẵn (m_Enabled=0) chờ trigger khác kích hoạt.
        var pickupItems = Object.FindObjectsByType<PickupItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var pickup in pickupItems)
        {
            var so = new SerializedObject(pickup);
            var itemDataProp = so.FindProperty("_itemData");
            if (itemDataProp != null && itemDataProp.objectReferenceValue == data)
                return pickup.gameObject;
        }

        return null;
    }

    private static bool SnapshotIcon(GameObject source, string outputPath)
    {
        GameObject temp = Object.Instantiate(source);
        temp.transform.position = new Vector3(3000f, 3000f, 3000f);
        temp.transform.rotation = Quaternion.identity;

        var renderers = temp.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Object.DestroyImmediate(temp);
            Debug.LogWarning($"[VoD][Icon] '{source.name}' không có Renderer nào trong toàn bộ children -- không chụp được.");
            return false;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);

        Vector3 localExtents  = LocalExtentsOfRenderers(temp.transform, renderers);
        Vector3 thinLocalAxis = ThinnestAxis(localExtents);
        Vector3 thinWorldAxis = temp.transform.TransformDirection(thinLocalAxis).normalized;
        if (thinWorldAxis == Vector3.zero) thinWorldAxis = Vector3.forward;

        // Nghiêng nhẹ lên-phải cho tự nhiên, không nhìn phẳng lì như ảnh scan thẳng đứng.
        Vector3 viewDir = (thinWorldAxis * 0.88f + Vector3.up * 0.35f + Vector3.right * 0.2f).normalized;

        var camGO = new GameObject("VoD_IconCam_TEMP");
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.09f, 0.09f, 0.1f, 1f);
        cam.fieldOfView      = 24f;
        cam.nearClipPlane    = 0.01f;

        float radius      = Mathf.Max(bounds.extents.magnitude, 0.02f);
        float fitDistance = radius / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        cam.farClipPlane  = fitDistance * 3f + 1f;

        camGO.transform.position = bounds.center + viewDir * fitDistance * 1.25f;
        camGO.transform.LookAt(bounds.center);

        var keyLightGO = new GameObject("VoD_IconKeyLight_TEMP");
        var keyLight   = keyLightGO.AddComponent<Light>();
        keyLight.type       = LightType.Directional;
        keyLightGO.transform.position = camGO.transform.position;
        keyLightGO.transform.rotation = Quaternion.LookRotation((bounds.center - camGO.transform.position).normalized);
        keyLight.intensity  = 1.3f;
        keyLight.shadows    = LightShadows.None;

        var fillLightGO = new GameObject("VoD_IconFillLight_TEMP");
        var fillLight   = fillLightGO.AddComponent<Light>();
        fillLight.type       = LightType.Directional;
        fillLightGO.transform.position = camGO.transform.position + Vector3.left;
        fillLightGO.transform.rotation = Quaternion.LookRotation((bounds.center - fillLightGO.transform.position).normalized);
        fillLight.intensity  = 0.5f;
        fillLight.shadows    = LightShadows.None;

        var rt = new RenderTexture(IconSize, IconSize, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        File.WriteAllBytes(outputPath, tex.EncodeToPNG());

        cam.targetTexture = null;
        rt.Release();
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(camGO);
        Object.DestroyImmediate(keyLightGO);
        Object.DestroyImmediate(fillLightGO);
        Object.DestroyImmediate(temp);

        return true;
    }

    // Xấp xỉ bounds LOCAL (theo trục riêng của object) bằng cách chuyển 8 góc bounds WORLD của từng renderer
    // về local space của root -- cho biết trục nào "mỏng nhất" (VD mặt phẳng tờ giấy) để canh góc nhìn đẹp.
    private static Vector3 LocalExtentsOfRenderers(Transform root, Renderer[] renderers)
    {
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool first = true;
        foreach (var r in renderers)
        {
            Bounds wb = r.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 sign = new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1);
                Vector3 worldCorner = wb.center + Vector3.Scale(wb.extents, sign);
                Vector3 localCorner = root.InverseTransformPoint(worldCorner);

                if (first) { localBounds = new Bounds(localCorner, Vector3.zero); first = false; }
                else localBounds.Encapsulate(localCorner);
            }
        }
        return localBounds.extents;
    }

    private static Vector3 ThinnestAxis(Vector3 extents)
    {
        if (extents.x <= extents.y && extents.x <= extents.z) return Vector3.right;
        if (extents.y <= extents.x && extents.y <= extents.z) return Vector3.up;
        return Vector3.forward;
    }

}
