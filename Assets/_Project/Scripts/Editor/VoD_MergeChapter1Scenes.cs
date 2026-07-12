using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Text;
using System.IO;
using System.Collections.Generic;
using TMPro;

// Các tool VoD/Temp/* chạy tay trong Unity Editor (không qua MCP). Đã dọn bớt các tool 1 lần đã hoàn
// thành nhiệm vụ (import/fix xong rồi) — xem lịch sử git nếu cần xem lại. Chỉ giữ tool debug/utility còn
// đang cần dùng lặp lại.
public static class VoD_MergeChapter1Scenes
{
    // GameObject.Find() BỎ QUA object đang inactive — dùng search đệ quy có include inactive.
    private static GameObject FindByNameIncludingInactive(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var found = FindDeep(root.transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform child in t)
        {
            var found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    // Test nhanh hiệu ứng post-process/shake đổi theo nấc mà KHÔNG cần đứng gần ma — bấm lúc đang Play,
    // mỗi lần bấm trừ 20% để đi qua từng nấc trong SanityData (100→80→60→40→20→0), xem chuyển tiếp
    // grain/vignette/chromatic/rung camera trông thế nào ở mỗi mốc.
    [MenuItem("VoD/Temp/Debug - Drain Sanity 20%")]
    public static void DebugDrainSanity()
    {
        if (!Application.isPlaying) { Debug.LogError("[VoD] Chỉ dùng được lúc đang Play Mode."); return; }
        if (SanitySystem.Instance == null) { Debug.LogError("[VoD] Không tìm thấy SanitySystem.Instance — chưa vào Play hoặc scene thiếu SanitySystem."); return; }

        SanitySystem.Instance.DecreaseSanity(0.2f);
        Debug.Log($"[VoD] Sanity: {SanitySystem.Instance.GetSanity() * 100f:F0}% — nấc \"{SanitySystem.Instance.GetCurrentLevelName()}\".");
    }

    [MenuItem("VoD/Temp/Debug - Restore Sanity To Full")]
    public static void DebugRestoreSanity()
    {
        if (!Application.isPlaying) { Debug.LogError("[VoD] Chỉ dùng được lúc đang Play Mode."); return; }
        if (SanitySystem.Instance == null) { Debug.LogError("[VoD] Không tìm thấy SanitySystem.Instance."); return; }

        SanitySystem.Instance.IncreaseSanity(1f);
        Debug.Log($"[VoD] Sanity: {SanitySystem.Instance.GetSanity() * 100f:F0}%.");
    }

    // Quét toàn bộ RenderSettings, mọi Light, mọi Camera (culling mask giải mã tên layer), mọi Volume +
    // toàn bộ field trong Profile, URP Pipeline Asset đang active thật sự (Graphics vs Quality có thể
    // lệch nhau), FogManager, FlashlightController. Xuất ra file text ở project root, đọc thẳng bằng Read
    // tool thay vì gọi MCP lặp lại nhiều lần — dùng lại được bất cứ khi nào nghi ngờ lighting/pipeline.
    [MenuItem("VoD/Temp/SCAN TOÀN BỘ Lighting + Settings (export 1 file)")]
    public static void ScanFullLightingReport()
    {
        var sb = new StringBuilder();
        Scene activeScene = SceneManager.GetActiveScene();
        sb.AppendLine("=== VoD FULL LIGHTING SCAN ===");
        sb.AppendLine($"Scene: {activeScene.name} ({activeScene.path})");
        sb.AppendLine($"Time: {System.DateTime.Now}");
        sb.AppendLine();

        // ---------- RenderSettings ----------
        sb.AppendLine("--- RenderSettings ---");
        sb.AppendLine($"fog = {RenderSettings.fog}");
        sb.AppendLine($"fogMode = {RenderSettings.fogMode}");
        sb.AppendLine($"fogColor = {RenderSettings.fogColor}");
        sb.AppendLine($"fogDensity = {RenderSettings.fogDensity}");
        sb.AppendLine($"fogStartDistance = {RenderSettings.fogStartDistance}");
        sb.AppendLine($"fogEndDistance = {RenderSettings.fogEndDistance}");
        sb.AppendLine($"ambientMode = {RenderSettings.ambientMode}");
        sb.AppendLine($"ambientIntensity = {RenderSettings.ambientIntensity}");
        sb.AppendLine($"ambientLight = {RenderSettings.ambientLight}");
        sb.AppendLine($"ambientSkyColor = {RenderSettings.ambientSkyColor}");
        sb.AppendLine($"ambientEquatorColor = {RenderSettings.ambientEquatorColor}");
        sb.AppendLine($"ambientGroundColor = {RenderSettings.ambientGroundColor}");
        sb.AppendLine($"reflectionIntensity = {RenderSettings.reflectionIntensity}");
        sb.AppendLine($"defaultReflectionMode = {RenderSettings.defaultReflectionMode}");
        sb.AppendLine($"skybox = {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "null")}");
        sb.AppendLine($"sun (RenderSettings.sun) = {(RenderSettings.sun != null ? RenderSettings.sun.name : "null")}");
        sb.AppendLine();

        // ---------- QualitySettings / Pipeline Asset đang active thật sự ----------
        sb.AppendLine("--- Quality / Render Pipeline Asset đang active ---");
        sb.AppendLine($"QualitySettings.names[QualitySettings.GetQualityLevel()] = {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        var qualityRPA = QualitySettings.renderPipeline;
        var graphicsRPA = GraphicsSettings.defaultRenderPipeline;
        var currentRPA = GraphicsSettings.currentRenderPipeline;
        sb.AppendLine($"QualitySettings.renderPipeline (override theo Quality tier) = {(qualityRPA != null ? qualityRPA.name : "null (dùng Graphics default)")}");
        sb.AppendLine($"GraphicsSettings.defaultRenderPipeline = {(graphicsRPA != null ? graphicsRPA.name : "null")}");
        sb.AppendLine($"GraphicsSettings.currentRenderPipeline (cái ĐANG thực sự dùng để render) = {(currentRPA != null ? currentRPA.name : "null")}");

        var urpAsset = currentRPA as UniversalRenderPipelineAsset;
        if (urpAsset == null) urpAsset = (qualityRPA as UniversalRenderPipelineAsset) ?? (graphicsRPA as UniversalRenderPipelineAsset);
        if (urpAsset != null)
        {
            sb.AppendLine($"  => Dump chi tiết asset: {urpAsset.name}");
            var urpSO = new SerializedObject(urpAsset);
            var lightsMode = urpSO.FindProperty("m_AdditionalLightsRenderingMode");
            var lightsShadow = urpSO.FindProperty("m_AdditionalLightShadowsSupported");
            sb.AppendLine($"  additionalLightsRenderingMode = {(lightsMode != null ? lightsMode.enumValueIndex + " (" + (lightsMode.enumValueIndex < lightsMode.enumDisplayNames.Length ? lightsMode.enumDisplayNames[lightsMode.enumValueIndex] : "?") + ")" : "?")}  ⚠️ enum này KHÔNG theo thứ tự trực quan 0/1/2 — luôn verify qua UI Project Settings > Quality thật, đừng tin số suông (đã dính bug thật vì việc này).");
            sb.AppendLine($"  additionalLightShadowsSupported = {(lightsShadow != null ? lightsShadow.boolValue.ToString() : "?")}");

            var rendererList = urpSO.FindProperty("m_RendererDataList");
            var defaultIdx = urpSO.FindProperty("m_DefaultRendererIndex");
            sb.AppendLine($"  m_DefaultRendererIndex = {(defaultIdx != null ? defaultIdx.intValue.ToString() : "?")}");
            if (rendererList != null)
            {
                for (int i = 0; i < rendererList.arraySize; i++)
                {
                    var rd = rendererList.GetArrayElementAtIndex(i).objectReferenceValue as ScriptableRendererData;
                    sb.AppendLine($"  Renderer[{i}] = {(rd != null ? rd.name : "null")}{(defaultIdx != null && defaultIdx.intValue == i ? "  <== DEFAULT" : "")}");
                    if (rd != null)
                    {
                        foreach (var feature in rd.rendererFeatures)
                        {
                            if (feature == null) { sb.AppendLine("      [feature bị null/missing script!]"); continue; }
                            sb.AppendLine($"      Feature: {feature.GetType().Name} (\"{feature.name}\") active={feature.isActive}");
                        }
                    }
                }
            }
        }
        else
        {
            sb.AppendLine("  !! Không tìm thấy UniversalRenderPipelineAsset nào đang active — có thể đang chạy Built-in RP!");
        }
        sb.AppendLine();

        // ---------- Toàn bộ Light trong scene ----------
        var allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"--- Lights trong scene ({allLights.Length}) ---");
        foreach (var l in allLights)
        {
            string path = GetHierarchyPath(l.transform);
            int layerIdx = l.gameObject.layer;
            sb.AppendLine($"[{path}]");
            sb.AppendLine($"  type={l.type} enabled={l.enabled} activeSelf={l.gameObject.activeSelf} activeInHierarchy={l.gameObject.activeInHierarchy}");
            sb.AppendLine($"  layer={layerIdx} ({LayerMask.LayerToName(layerIdx)})  cullingMask(light riêng)={l.cullingMask}");
            sb.AppendLine($"  intensity={l.intensity} range={l.range} spotAngle={l.spotAngle} innerSpotAngle={l.innerSpotAngle} color={l.color}");
            sb.AppendLine($"  shadows={l.shadows} shadowStrength={l.shadowStrength}");
            sb.AppendLine($"  position(world)={l.transform.position} rotation(euler)={l.transform.rotation.eulerAngles}");
        }
        sb.AppendLine();

        // ---------- Toàn bộ Camera trong scene ----------
        var allCameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"--- Cameras trong scene ({allCameras.Length}) ---");
        foreach (var cam in allCameras)
        {
            string path = GetHierarchyPath(cam.transform);
            sb.AppendLine($"[{path}]");
            sb.AppendLine($"  enabled={cam.enabled} activeInHierarchy={cam.gameObject.activeInHierarchy} depth={cam.depth} clearFlags={cam.clearFlags}");
            sb.AppendLine($"  cullingMask=0b{System.Convert.ToString(cam.cullingMask, 2)} ({cam.cullingMask}) -> layers included: {DecodeLayerMask(cam.cullingMask)}");
            sb.AppendLine($"  fieldOfView={cam.fieldOfView} nearClip={cam.nearClipPlane} farClip={cam.farClipPlane}");
            var camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null)
            {
                sb.AppendLine($"  URP renderType={camData.renderType}");
                if (camData.renderType == CameraRenderType.Base && camData.cameraStack != null && camData.cameraStack.Count > 0)
                {
                    sb.AppendLine($"  cameraStack ({camData.cameraStack.Count}): {string.Join(", ", camData.cameraStack.ConvertAll(c => c != null ? c.name : "null"))}");
                }
            }
        }
        sb.AppendLine();

        // ---------- Cross-check: Light nào bị loại khỏi cullingMask của Camera Base nào ----------
        sb.AppendLine("--- Cross-check: Light có bị Camera cullingMask loại không ---");
        foreach (var cam in allCameras)
        {
            var camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null && camData.renderType != CameraRenderType.Base) continue; // chỉ xét Base, Overlay không tự quyết định light
            foreach (var l in allLights)
            {
                bool included = (cam.cullingMask & (1 << l.gameObject.layer)) != 0;
                if (!included)
                    sb.AppendLine($"  !! Camera \"{GetHierarchyPath(cam.transform)}\" KHÔNG bao gồm layer của Light \"{GetHierarchyPath(l.transform)}\" (layer {l.gameObject.layer}/{LayerMask.LayerToName(l.gameObject.layer)}) => light này KHÔNG chiếu sáng được gì camera này render.");
            }
        }
        sb.AppendLine();

        // ---------- Toàn bộ Volume + Profile ----------
        var allVolumes = Object.FindObjectsByType<Volume>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        sb.AppendLine($"--- Volumes trong scene ({allVolumes.Length}) ---");
        foreach (var v in allVolumes)
        {
            string path = GetHierarchyPath(v.transform);
            sb.AppendLine($"[{path}]");
            sb.AppendLine($"  enabled={v.enabled} activeInHierarchy={v.gameObject.activeInHierarchy} isGlobal={v.isGlobal} weight={v.weight} priority={v.priority} blendDistance={v.blendDistance}");
            var profile = v.sharedProfile != null ? v.sharedProfile : v.profile;
            if (profile == null) { sb.AppendLine("  !! profile = null"); continue; }
            string profilePath = AssetDatabase.GetAssetPath(profile);
            sb.AppendLine($"  profile = {profile.name}  ({profilePath})");
            foreach (var comp in profile.components)
            {
                if (comp == null) { sb.AppendLine("    [component bị null/missing script!]"); continue; }
                sb.AppendLine($"    * {comp.GetType().Name}  active={comp.active}");
                var compSO = new SerializedObject(comp);
                DumpSerializedObjectFlat(sb, compSO, "        ");
            }
        }
        sb.AppendLine();

        // ---------- FogManager ----------
        var fogMgr = Object.FindFirstObjectByType<FogManager>(FindObjectsInactive.Include);
        sb.AppendLine("--- FogManager ---");
        if (fogMgr == null) sb.AppendLine("  !! Không tìm thấy FogManager trong scene.");
        else
        {
            sb.AppendLine($"  [{GetHierarchyPath(fogMgr.transform)}] enabled={fogMgr.enabled} activeInHierarchy={fogMgr.gameObject.activeInHierarchy}");
            DumpSerializedObjectFlat(sb, new SerializedObject(fogMgr), "    ");
        }
        sb.AppendLine();

        // ---------- FlashlightController ----------
        var flashlight = Object.FindFirstObjectByType<FlashlightController>(FindObjectsInactive.Include);
        sb.AppendLine("--- FlashlightController ---");
        if (flashlight == null) sb.AppendLine("  !! Không tìm thấy FlashlightController trong scene.");
        else
        {
            sb.AppendLine($"  [{GetHierarchyPath(flashlight.transform)}] enabled={flashlight.enabled} activeInHierarchy={flashlight.gameObject.activeInHierarchy}");
            DumpSerializedObjectFlat(sb, new SerializedObject(flashlight), "    ");
        }
        sb.AppendLine();

        string outPath = Path.Combine(Application.dataPath, "..", "VoD_LightingReport.txt");
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log($"[VoD] Đã xuất scan report ra: {outPath}");
    }

    private static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    private static string DecodeLayerMask(int mask)
    {
        var names = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            string n = LayerMask.LayerToName(i);
            names.Add(string.IsNullOrEmpty(n) ? $"#{i}" : n);
        }
        return string.Join(", ", names);
    }

    // Duyệt phẳng toàn bộ SerializedProperty (kể cả field lồng bên trong struct như VolumeParameter<T>.m_Value)
    // để không cần biết trước tên field cụ thể của từng loại component/script — dùng chung cho mọi thứ.
    private static void DumpSerializedObjectFlat(StringBuilder sb, SerializedObject so, string indent)
    {
        var prop = so.GetIterator();
        bool enter = true;
        while (prop.NextVisible(enter))
        {
            enter = true;
            string n = prop.name;
            if (n == "m_Script" || n == "m_ObjectHideFlags" || n == "m_CorrespondingSourceObject" ||
                n == "m_PrefabInstance" || n == "m_PrefabAsset" || n == "m_GameObject" ||
                n == "m_EditorHideFlags" || n == "m_EditorClassIdentifier") continue;
            if (prop.propertyType == SerializedPropertyType.Generic) continue; // struct/array container, giá trị thật nằm ở children
            sb.AppendLine($"{indent}{prop.propertyPath} = {SerializedPropertyValueToString(prop)}");
        }
    }

    private static string SerializedPropertyValueToString(SerializedProperty p)
    {
        switch (p.propertyType)
        {
            case SerializedPropertyType.Integer: return p.intValue.ToString();
            case SerializedPropertyType.Boolean: return p.boolValue.ToString();
            case SerializedPropertyType.Float: return p.floatValue.ToString("F4");
            case SerializedPropertyType.String: return p.stringValue;
            case SerializedPropertyType.Color: return p.colorValue.ToString();
            case SerializedPropertyType.ObjectReference: return p.objectReferenceValue != null ? p.objectReferenceValue.name : "null";
            case SerializedPropertyType.Enum: return (p.enumValueIndex >= 0 && p.enumValueIndex < p.enumDisplayNames.Length) ? p.enumDisplayNames[p.enumValueIndex] : p.enumValueIndex.ToString();
            case SerializedPropertyType.Vector2: return p.vector2Value.ToString();
            case SerializedPropertyType.Vector3: return p.vector3Value.ToString();
            case SerializedPropertyType.Vector4: return p.vector4Value.ToString();
            case SerializedPropertyType.LayerMask: return p.intValue.ToString();
            default: return $"({p.propertyType})";
        }
    }

    // glTFast (importer cho .glb, dùng cho toàn bộ đồ nội thất Kenney) không có tuỳ chọn "Smooth Normals"
    // như FBX importer — kèm việc Kenney export vertex tách rời ở từng cạnh (cố ý để hỗ trợ flat shading),
    // nên đèn pin chiếu vào là thấy rõ từng mặt phẳng thay vì loang mượt. Xử lý trực tiếp trên mesh: gộp
    // vertex trùng vị trí (dù đang tách rời trong buffer), blend normal GỐC (không tự tính lại qua cross
    // product — dễ sai hướng do winding order không đồng nhất), tạo mesh mới rồi gán vào Object đang chọn.
    [MenuItem("VoD/Temp/Smooth Normals cho Object đang chọn")]
    public static void SmoothNormalsForSelected()
    {
        var go = Selection.activeGameObject;
        if (go == null) { Debug.LogError("[VoD] Chưa chọn GameObject nào trong Hierarchy."); return; }

        // Xử lý luôn cả object con — nhiều prop ghép từ nhiều mesh riêng (khung giường/nệm/gối...).
        var meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
        if (meshFilters.Length == 0) { Debug.LogError("[VoD] Object đang chọn (và con) không có MeshFilter/mesh nào."); return; }

        int done = 0;
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;
            SmoothNormalsOnMeshFilter(mf);
            done++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[VoD] Đã làm mượt normal cho {done}/{meshFilters.Length} mesh trong \"{go.name}\" (và con). Nhớ Save Scene rồi Play thử.");
    }

    // Áp dụng hàng loạt cho toàn bộ 2 group Furniture + Props luôn, khỏi phải chọn tay từng object — dùng
    // lại được mỗi khi thêm đồ nội thất mới vào scene.
    [MenuItem("VoD/Temp/Smooth Normals cho TOÀN BỘ Furniture + Props")]
    public static void SmoothNormalsForFurnitureAndProps()
    {
        string[] groupNames = { "── FURNITURE (HideSpot/Decor) ──", "── PROPS ──" };
        int totalMeshes = 0;

        foreach (var groupName in groupNames)
        {
            var group = FindByNameIncludingInactive(groupName);
            if (group == null) { Debug.LogWarning($"[VoD] Không tìm thấy group \"{groupName}\" — bỏ qua."); continue; }

            var meshFilters = group.GetComponentsInChildren<MeshFilter>(true);
            int done = 0;
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                SmoothNormalsOnMeshFilter(mf);
                done++;
            }
            totalMeshes += done;
            Debug.Log($"[VoD] Group \"{groupName}\": làm mượt {done}/{meshFilters.Length} mesh.");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[VoD] XONG — tổng cộng {totalMeshes} mesh đã được làm mượt normal trong Furniture + Props. Có thể mất vài giây nếu nhiều mesh. Nhớ Save Scene rồi Play thử.");
    }

    // "Đèn xuyên tường/tủ" — nghi phạm số 1 là MeshRenderer của vật cản đang tắt Cast Shadows (glTFast/
    // import mặc định đôi khi để Off), nên đèn Realtime chiếu thẳng qua như không có gì cản dù model vẫn
    // hiện bình thường. Quét + bật Cast Shadows = On hàng loạt cho Furniture + Props — dùng lại được mỗi
    // khi thêm đồ mới vào scene.
    [MenuItem("VoD/Temp/Scan + Bật Cast Shadows cho TOÀN BỘ Furniture + Props")]
    public static void FixCastShadowsForFurnitureAndProps()
    {
        string[] groupNames = { "── FURNITURE (HideSpot/Decor) ──", "── PROPS ──" };
        int totalOff = 0, totalChecked = 0;
        var sb = new StringBuilder();
        sb.AppendLine("=== Scan Cast Shadows — Furniture + Props ===");

        foreach (var groupName in groupNames)
        {
            var group = FindByNameIncludingInactive(groupName);
            if (group == null) { Debug.LogWarning($"[VoD] Không tìm thấy group \"{groupName}\" — bỏ qua."); continue; }

            var renderers = group.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var mr in renderers)
            {
                totalChecked++;
                if (mr.shadowCastingMode == ShadowCastingMode.Off)
                {
                    sb.AppendLine($"  TẮT shadow -> BẬT lại: [{GetHierarchyPath(mr.transform)}]");
                    Undo.RecordObject(mr, "Fix Cast Shadows");
                    mr.shadowCastingMode = ShadowCastingMode.On;
                    totalOff++;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        sb.AppendLine($"XONG — đã bật lại Cast Shadows cho {totalOff}/{totalChecked} MeshRenderer đang tắt. Nhớ Save Scene rồi Play thử.");
        Debug.Log(sb.ToString());
    }

    private static void SmoothNormalsOnMeshFilter(MeshFilter mf)
    {
        var srcMesh = mf.sharedMesh;
        var verts = srcMesh.vertices;
        var origNormals = srcMesh.normals;

        Vector3Int RoundKey(Vector3 v) => new Vector3Int(
            Mathf.RoundToInt(v.x * 10000f),
            Mathf.RoundToInt(v.y * 10000f),
            Mathf.RoundToInt(v.z * 10000f));

        var posGroups = new Dictionary<Vector3Int, List<int>>();
        for (int i = 0; i < verts.Length; i++)
        {
            var key = RoundKey(verts[i]);
            if (!posGroups.TryGetValue(key, out var list)) { list = new List<int>(); posGroups[key] = list; }
            list.Add(i);
        }

        // Ngưỡng góc mượt chuẩn (~60°, giống Blender/Maya mặc định): mỗi vertex chỉ gộp normal của các
        // vertex khác CHUNG VỊ TRÍ có góc lệch DƯỚI ngưỡng so với chính normal gốc của nó — cạnh sắc
        // (góc > ngưỡng, ví dụ góc bàn) giữ nguyên facet, chỉ mặt cong nhẹ mới mượt vào nhau.
        const float smoothingAngleDeg = 60f;
        float cosThreshold = Mathf.Cos(smoothingAngleDeg * Mathf.Deg2Rad);

        var smoothNormals = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 myNormal = origNormals[i];
            Vector3 sum = Vector3.zero;
            foreach (int vi in posGroups[RoundKey(verts[i])])
                if (Vector3.Dot(origNormals[vi], myNormal) >= cosThreshold)
                    sum += origNormals[vi];

            smoothNormals[i] = sum.sqrMagnitude > 0.0001f ? sum.normalized : myNormal;
        }

        var newMesh = Object.Instantiate(srcMesh);
        newMesh.name = srcMesh.name + "_Smooth";
        newMesh.normals = smoothNormals;

        string dir = "Assets/_Project/Models/VFX";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Models")) AssetDatabase.CreateFolder("Assets/_Project", "Models");
            AssetDatabase.CreateFolder("Assets/_Project/Models", "VFX");
        }
        string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{newMesh.name}.asset");
        AssetDatabase.CreateAsset(newMesh, path);

        Undo.RecordObject(mf, "Smooth Normals");
        mf.sharedMesh = newMesh;
    }

    private static void SetIfEmpty(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop == null) { Debug.LogWarning($"[VoD] Không tìm thấy field \"{propName}\" trên IntroManager."); return; }
        if (prop.objectReferenceValue == null) prop.objectReferenceValue = value;
    }

    // ConfirmArrow hiện chỉ là icon nhỏ (mũi tên/ký tự "v") — chỉ hiện SAU khi typewriter gõ xong (đã
    // đúng logic sẵn trong SubtitleDialogueView/PopupDialogueView.FinishLine → AdvanceOrSkip). Đổi text
    // rõ ràng + nới rộng box để không bị wrap/crop, giữ nguyên anchor/pivot hiện có (chỉ đổi sizeDelta.x).
    //
    // ĐÃ THỬ 3 FONT, CẢ 3 CRASH — NHƯNG toàn bộ test trước giờ đều làm trong EDIT MODE (chưa từng bấm
    // Play thật, chỉ bật tay GameObject lên xem). Rất có thể đây là hạn chế biết trước của TMP: Dynamic
    // atlas cần proper init qua Awake/OnEnable lúc Play mới chạy đúng, rebuild Canvas ngoài Play Mode qua
    // script dễ hit lỗi này dù font hoàn toàn ổn (JustMeAgainDownHere/AmaticSC-Bold vẫn hiện chữ bình
    // thường khi CHƠI GAME THẬT trước giờ — đó là lý do Jok thấy "trước đó dùng được"). Quay lại thử
    // JustMeAgainDownHere SDF + dấu tiếng Việt — lần này PHẢI bấm Play thật để test, không chỉ xem Edit
    // Mode. Xem [[project_tmp_font_vietnamese_glyph_bug]].
    [MenuItem("VoD/Temp/Đổi Confirm Arrow thành hint \"Nhấn Space\"")]
    public static void SetupSpaceContinueHint()
    {
        var subtitleViewGO = FindByNameIncludingInactive("SubtitleView");
        var popupViewGO    = FindByNameIncludingInactive("PopupView");
        if (subtitleViewGO == null) { Debug.LogError("[VoD] Không tìm thấy \"SubtitleView\" trong scene."); return; }
        if (popupViewGO == null) { Debug.LogError("[VoD] Không tìm thấy \"PopupView\" trong scene."); return; }

        const string hintText = "Nhấn [ SPACE ] để tiếp tục";

        string fontPath = "Assets/_Project/Fonts/JustMeAgainDownHere SDF.asset";
        var vietnameseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (vietnameseFont == null) Debug.LogWarning($"[VoD] Không tìm thấy font ở {fontPath} — giữ nguyên font cũ, chỉ đổi text.");

        var subtitleArrow = FindDeep(subtitleViewGO.transform, "ConfirmArrow");
        ApplySpaceHint(subtitleArrow, hintText, 340f, vietnameseFont);

        var popupArrow = FindDeep(popupViewGO.transform, "ConfirmArrow");
        ApplySpaceHint(popupArrow, hintText, 340f, vietnameseFont);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[VoD] Đã đổi ConfirmArrow sang \"Nhấn [ SPACE ] để tiếp tục\" (font JustMeAgainDownHere SDF). QUAN TRỌNG: Save Scene rồi BẤM PLAY THẬT để test (không chỉ xem Scene/Game view ở Edit Mode) — Console có thể vẫn báo lỗi 1 lần lúc Edit Mode rebuild, nhưng cái cần biết là lúc Play có bị crash/lỗi không.");
    }

    private static void ApplySpaceHint(Transform arrow, string text, float newWidth, TMP_FontAsset font)
    {
        if (arrow == null) { Debug.LogWarning("[VoD] Không tìm thấy ConfirmArrow để đổi hint."); return; }

        var tmp = arrow.GetComponent<TextMeshProUGUI>();
        if (tmp == null) { Debug.LogWarning($"[VoD] \"{arrow.name}\" không có TextMeshProUGUI."); return; }

        Undo.RecordObject(tmp, "Space Continue Hint");
        if (font != null) tmp.font = font;
        tmp.text = text;

        var rt = arrow.GetComponent<RectTransform>();
        if (rt != null)
        {
            Undo.RecordObject(rt, "Space Continue Hint");
            var size = rt.sizeDelta;
            size.x = newWidth;
            rt.sizeDelta = size;
        }
    }

    // BUG THẬT: DialogueAsset_Ch1_Intro.asset viết tay YAML với "m_Script: {fileID: 0} +
    // m_EditorClassIdentifier" (tưởng nhầm là pattern an toàn giống DialogueAsset.asset gốc) — xác nhận
    // qua Object Picker trong Inspector: Unity KHÔNG liệt kê được asset nào kiểu DialogueAsset viết theo
    // kiểu này, kể cả file gốc. Tool này tạo lại đúng cách qua ScriptableObject.CreateInstance +
    // AssetDatabase.CreateAsset (Unity tự ghi đúng m_Script, không đoán fileID tay nữa), rồi wire lại vào
    // IntroManager.introDialogue. Xoá + tạo lại nên GUID sẽ đổi — không sao vì chỉ IntroManager tham
    // chiếu tới asset này (không phải file DialogueAsset.asset gốc, không đụng gameplay khác).
    [MenuItem("VoD/Temp/Fix - Tạo lại DialogueAsset_Ch1_Intro (m_Script đúng cách)")]
    public static void RecreateIntroDialogueAsset()
    {
        string path = "Assets/_Project/Data/Dialogue/Chapter1/DialogueAsset_Ch1_Intro.asset";

        if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            AssetDatabase.DeleteAsset(path);

        // Câu "À... cửa sổ phòng ăn mở rồi..." (KH-INTRO-03 gốc) bị bỏ khỏi đây — Jok xác nhận nó chỉ nên
        // chạy khi tương tác với vật thể thật trong scene (chưa chốt gắn vào đâu, để sau), không phải
        // thoại tuần tự trong cutscene. Dòng đầu "Cuối cùng cũng đến nơi rồi." là ĐỀ XUẤT MỚI của Claude
        // (không có trong KỊCH_BẢN_LỒNG_TIẾNG_v1.md) — thêm nhịp thở/cảm xúc ngay lúc camera reveal xong,
        // trước khi vào câu thoại thuyết minh — Jok tự chỉnh lại nếu không ưng.
        var asset = ScriptableObject.CreateInstance<DialogueAsset>();
        asset.lines = new List<DialogueLine>
        {
            new DialogueLine { speakerName = "Minh Khoa", text = "Cuối cùng cũng đến nơi rồi.", hasVoice = true },
            new DialogueLine { speakerName = "Minh Khoa", text = "Biệt thự Đỗ Gia. Năm 1945, kiến trúc sư Pháp phối hợp ông Đỗ Văn Minh.", hasVoice = true },
            new DialogueLine { speakerName = "Minh Khoa", text = "Đề tài tốt nghiệp của mình không thể thiếu cái này.", hasVoice = true },
            new DialogueLine { speakerName = "Minh Khoa", text = "Chụp nhanh vài tấm, phác thảo mặt tiền, rồi về. Không dám ở lại đêm đâu.", hasVoice = true },
        };

        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var introGO = FindByNameIncludingInactive("IntroManager");
        if (introGO == null) { Debug.LogError("[VoD] Không tìm thấy \"IntroManager\" trong scene — tạo asset xong nhưng chưa wire được, tự kéo tay vào."); return; }

        var introComp = introGO.GetComponent<IntroManager>();
        if (introComp == null) { Debug.LogError("[VoD] \"IntroManager\" không có component IntroManager."); return; }

        var so = new SerializedObject(introComp);
        var prop = so.FindProperty("introDialogue");
        prop.objectReferenceValue = asset;

        // Tăng thời gian rotation camera reveal (2s → 3s) — nặng nề/từ từ hơn cho cảm giác điện ảnh,
        // đúng yêu cầu gốc "nặng nề từ từ có chủ ý" thay vì đổi tuỳ tiện, chỉ nới thêm 1s.
        var revealProp = so.FindProperty("revealRotationDuration");
        if (revealProp != null) revealProp.floatValue = 3f;

        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[VoD] Đã tạo lại DialogueAsset_Ch1_Intro (4 câu, bỏ câu cửa sổ, thêm câu mở đầu mới) + wire vào IntroManager.introDialogue + tăng revealRotationDuration lên 3s. Nhớ Save Scene rồi Play thử.");
    }

    // Dựng UI cho EyelidBlink — 2 panel đen (nửa trên/nửa dưới màn hình) trượt ra/vào giả mí mắt, thay
    // cho ScreenFader (alpha fade phẳng) trong đúng đoạn "chớp đen" của IntroManager. Panel dựng ở trạng
    // thái MỞ (trượt ra ngoài, không che gì) mặc định — để nếu Jok bật "Skip Intro Entirely" test phần
    // khác thì màn hình không bị đen vĩnh viễn (IntroManager tự SnapClosed() lúc RunIntro() bắt đầu).
    [MenuItem("VoD/Temp/Dựng EyelidBlink (2 panel mí mắt trên/dưới)")]
    public static void SetupEyelidBlink()
    {
        var dialoguePanel = FindByNameIncludingInactive("DialoguePanel");
        if (dialoguePanel == null) { Debug.LogError("[VoD] Không tìm thấy \"DialoguePanel\" để xác định Canvas."); return; }

        var canvasGO = dialoguePanel.transform.parent != null ? dialoguePanel.transform.parent.gameObject : null;
        if (canvasGO == null || canvasGO.GetComponent<Canvas>() == null)
        {
            Debug.LogError("[VoD] Không tìm thấy Canvas cha của \"DialoguePanel\".");
            return;
        }

        var existing = FindByNameIncludingInactive("EyelidBlink");
        GameObject rootGO;
        EyelidBlink eyelidComp;
        RectTransform topRT, bottomRT;

        if (existing != null)
        {
            rootGO = existing;
            eyelidComp = rootGO.GetComponent<EyelidBlink>();
            if (eyelidComp == null) eyelidComp = rootGO.AddComponent<EyelidBlink>();
            topRT = FindDeep(rootGO.transform, "TopPanel")?.GetComponent<RectTransform>();
            bottomRT = FindDeep(rootGO.transform, "BottomPanel")?.GetComponent<RectTransform>();
            if (topRT == null) topRT = CreateEyelidPanel(rootGO.transform, "TopPanel", new Vector2(0f, 0.5f), new Vector2(1f, 1f));
            if (bottomRT == null) bottomRT = CreateEyelidPanel(rootGO.transform, "BottomPanel", new Vector2(0f, 0f), new Vector2(1f, 0.5f));
            Debug.Log("[VoD] Đã có \"EyelidBlink\" trong scene — dùng lại, chỉ kiểm tra/tạo lại panel còn thiếu.");
        }
        else
        {
            rootGO = new GameObject("EyelidBlink");
            Undo.RegisterCreatedObjectUndo(rootGO, "Create EyelidBlink");
            rootGO.transform.SetParent(canvasGO.transform, false);
            var rootRT = rootGO.AddComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.offsetMin = Vector2.zero;
            rootRT.offsetMax = Vector2.zero;

            eyelidComp = rootGO.AddComponent<EyelidBlink>();
            topRT = CreateEyelidPanel(rootGO.transform, "TopPanel", new Vector2(0f, 0.5f), new Vector2(1f, 1f));
            bottomRT = CreateEyelidPanel(rootGO.transform, "BottomPanel", new Vector2(0f, 0f), new Vector2(1f, 0.5f));
        }

        // Luôn ở TRÊN CÙNG trong Canvas (che hết DialoguePanel/HUD) — hiển thị đúng lúc cinematic chạy.
        rootGO.transform.SetAsLastSibling();

        // Trạng thái MỞ mặc định (trượt ra ngoài, không che gì) — an toàn nếu skip intro.
        Canvas.ForceUpdateCanvases();
        topRT.anchoredPosition = new Vector2(0f, topRT.rect.height);
        bottomRT.anchoredPosition = new Vector2(0f, -bottomRT.rect.height);

        var eyelidSO = new SerializedObject(eyelidComp);
        SetIfEmpty(eyelidSO, "topPanel", topRT);
        SetIfEmpty(eyelidSO, "bottomPanel", bottomRT);
        eyelidSO.ApplyModifiedProperties();

        // Wire vào IntroManager.eyelidBlink
        var introGO = FindByNameIncludingInactive("IntroManager");
        if (introGO != null)
        {
            var introComp = introGO.GetComponent<IntroManager>();
            if (introComp != null)
            {
                var introSO = new SerializedObject(introComp);
                SetIfEmpty(introSO, "eyelidBlink", eyelidComp);
                introSO.ApplyModifiedProperties();
            }
        }
        else
        {
            Debug.LogWarning("[VoD] Không tìm thấy \"IntroManager\" — đã dựng EyelidBlink xong nhưng chưa wire được, tự kéo tay vào field \"Eyelid Blink\".");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[VoD] Đã dựng EyelidBlink (2 panel TopPanel/BottomPanel, trạng thái mở mặc định) + wire vào IntroManager. Nhớ Save Scene rồi Play thử.");
    }

    private static RectTransform CreateEyelidPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        Undo.RegisterCreatedObjectUndo(go, "Create Eyelid Panel");
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        var img = go.GetComponent<UnityEngine.UI.Image>();
        img.color = Color.black;
        img.raycastTarget = true;

        return rt;
    }

    // Dựng khung UI dùng chung cho mọi IInteractable (Piano/HideSpot/ExamineItem/PickupItem/vải đỏ...):
    // tâm màn hình có 1 ô vuông trắng đặt tạm (Phúc thay bằng sprite tròn thật) + chữ "E" bên dưới,
    // 2 cái nằm chung 1 container "PromptRoot" được InteractPromptUI.Show()/Hide() bật tắt theo raycast hover
    // trong InteractionSystem.cs. Root "InteractPrompt" LUÔN active (để Awake/Update chạy) — chỉ "PromptRoot" con bị tắt.
    [MenuItem("VoD/Temp/Setup - Interact Prompt UI (E + tâm tròn)")]
    public static void SetupInteractPromptUI()
    {
        GameObject canvasGO = FindByNameIncludingInactive("Canvas");
        if (canvasGO == null) { Debug.LogError("[VoD] Không tìm thấy Canvas trong scene."); return; }

        GameObject existing = FindByNameIncludingInactive("InteractPrompt");
        if (existing != null)
        {
            Debug.LogWarning("[VoD] 'InteractPrompt' đã tồn tại — xoá đối tượng cũ trước nếu muốn dựng lại từ đầu.");
            return;
        }

        var root = new GameObject("InteractPrompt", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(root, "Create InteractPrompt");
        root.transform.SetParent(canvasGO.transform, false);
        var rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        var promptRoot = new GameObject("PromptRoot", typeof(RectTransform));
        promptRoot.transform.SetParent(root.transform, false);
        var promptRt = promptRoot.GetComponent<RectTransform>();
        promptRt.anchorMin = new Vector2(0.5f, 0.5f);
        promptRt.anchorMax = new Vector2(0.5f, 0.5f);
        promptRt.pivot = new Vector2(0.5f, 0.5f);
        promptRt.anchoredPosition = Vector2.zero;
        promptRt.sizeDelta = new Vector2(60, 60);

        // Placeholder tâm tròn — Phúc thay Image này bằng sprite tròn trắng thật, chỉnh size/style tự do.
        var dotGO = new GameObject("Dot", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        dotGO.transform.SetParent(promptRoot.transform, false);
        var dotRt = dotGO.GetComponent<RectTransform>();
        dotRt.anchorMin = new Vector2(0.5f, 0.6f);
        dotRt.anchorMax = new Vector2(0.5f, 0.6f);
        dotRt.pivot = new Vector2(0.5f, 0.5f);
        dotRt.anchoredPosition = Vector2.zero;
        dotRt.sizeDelta = new Vector2(6, 6);
        var dotImg = dotGO.GetComponent<UnityEngine.UI.Image>();
        dotImg.color = Color.white;

        var labelGO = new GameObject("KeyLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(promptRoot.transform, false);
        var labelRt = labelGO.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0.5f, 0.6f);
        labelRt.anchorMax = new Vector2(0.5f, 0.6f);
        labelRt.pivot = new Vector2(0.5f, 1f);
        labelRt.anchoredPosition = new Vector2(0, -14);
        labelRt.sizeDelta = new Vector2(40, 30);
        var label = labelGO.GetComponent<TextMeshProUGUI>();
        label.text = "E";
        label.fontSize = 22;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        promptRoot.SetActive(false);

        var promptScript = root.AddComponent<InteractPromptUI>();
        var so = new SerializedObject(promptScript);
        so.FindProperty("promptRoot").objectReferenceValue = promptRoot;
        so.FindProperty("keyLabel").objectReferenceValue = label;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[VoD] Đã dựng InteractPrompt (Canvas/InteractPrompt/PromptRoot/Dot+KeyLabel). Phúc thay Image 'Dot' bằng sprite tròn trắng thật là xong.");
    }
}
