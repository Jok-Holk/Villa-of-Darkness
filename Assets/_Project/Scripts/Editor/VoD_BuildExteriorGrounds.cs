using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// GOM 1 NÚT DUY NHẤT cho toàn bộ ngoại thất quanh villa (thay cho 2 tool tách
/// rời trước đó, theo yêu cầu Jok "làm kĩ trong 2 3 nút thôi"):
///   1) Sân trước + sân sau (cỏ, CÙNG kích thước 27x15m, đối xứng Bắc/Nam quanh villa).
///   2) 2 dải "hành lang vườn" (cỏ) chạy dọc 2 bên hông villa (Đông/Tây), nối
///      liền sân trước -> sân sau vòng ngoài nhà (không xuyên qua trong nhà).
///   3) Hàng rào "hang_rao" (57 mảnh, đang lạc toạ độ xa villa) -- co giãn ĐỀU
///      CẢ 3 TRỤC (không kéo méo X/Z riêng như bản trước, tránh fence bị biến
///      dạng) rồi kéo về bao trọn khuôn viên.
public static class VoD_BuildExteriorGrounds
{
    // Khối chính villa: X 3-30 (27m), Z 5.5-24.5 (19m, chưa gồm Véranda).
    // Véranda nối thêm tới Z=27 (xem VoD_RebuildRoomWalls).
    private const float MainMinX = 3.0f, MainMaxX = 30.0f;
    private const float VerandaMaxZ = 27.0f;

    // Sân sau: ngay sau Véranda, sâu 15m ước lượng (đủ chỗ giếng đá sau này).
    private const float BackyardDepth = 15.0f;
    private static float BackyardMaxZ => VerandaMaxZ + BackyardDepth; // 42

    // Sân trước: CÙNG KÍCH THƯỚC với sân sau (27m x 15m, theo yêu cầu Jok), chỉ
    // khác vị trí -- đặt đối xứng phía Bắc, ngay trước cửa chính TienSanh (Z=5.5).
    private const float FrontYardDepth = BackyardDepth;
    private static float FrontYardMinZ => 5.5f - FrontYardDepth; // -9.5

    // Hành lang vườn 2 bên hông nhà -- Jok: "mở rộng thêm cỡ 1/3" (3m -> 4m).
    private const float SideGardenWidth = 4.0f;

    // Sân trước/sân sau: Jok muốn "mở rộng chiều rộng sân thêm" (KHÔNG kéo dài
    // sâu thêm, giữ nguyên FrontYardDepth/BackyardDepth) -- trải rộng ra ĐÚNG
    // bằng 2 dải hành lang vườn 2 bên (liền mạch 1 mảng cỏ, không còn đường nối
    // giữa sân trước/sau với hành lang bên hông).
    private static float YardMinX => MainMinX - SideGardenWidth;
    private static float YardMaxX => MainMaxX + SideGardenWidth;

    // Khung bao NGOÀI CÙNG cho hàng rào (thêm 1m lề ngoài 2 dải hành lang vườn).
    private static float FenceMinX => MainMinX - SideGardenWidth - 1f;   // -1
    private static float FenceMaxX => MainMaxX + SideGardenWidth + 1f;  // 34
    private static float FenceMinZ => FrontYardMinZ - 1f;               // -3.5
    private static float FenceMaxZ => BackyardMaxZ + 1f;                // 43

    private const string GrassMatPath = "Assets/_Project/Materials/Mat_Terrain_MainMenu_GrassFloor.mat";

    [MenuItem("VoD/Build Exterior Grounds (Backyard + Side Gardens + Fence)")]
    public static void BuildAll()
    {
        var root = GetOrCreateVillaGroundsRoot();
        var grassMat = AssetDatabase.LoadAssetAtPath<Material>(GrassMatPath);
        if (grassMat == null) Debug.LogWarning($"[VoD] Không tìm thấy material cỏ tại '{GrassMatPath}'.");

        // 1) Sân sau + sân trước -- CÙNG kích thước, rộng bằng đúng bề ngang tính
        //    cả 2 dải hành lang vườn 2 bên (liền mạch, không lệch seam).
        BuildGrassSlab(root, "SanSau_Grass", YardMinX, VerandaMaxZ, YardMaxX - YardMinX, BackyardDepth, grassMat);
        BuildGrassSlab(root, "SanTruoc_Grass", YardMinX, FrontYardMinZ, YardMaxX - YardMinX, FrontYardDepth, grassMat);

        // 2) 2 hành lang vườn 2 bên hông (Tây: trước MainMinX: Đông: sau MainMaxX),
        //    chạy suốt từ sân trước tới sân sau (bọc ngoài nhà, không xuyên qua trong).
        float sideZLen = BackyardMaxZ - FrontYardMinZ;
        BuildGrassSlab(root, "HanhLangVuon_Tay", MainMinX - SideGardenWidth, FrontYardMinZ, SideGardenWidth, sideZLen, grassMat);
        BuildGrassSlab(root, "HanhLangVuon_Dong", MainMaxX, FrontYardMinZ, SideGardenWidth, sideZLen, grassMat);

        // 3) Hàng rào -- co giãn ĐỀU cả 3 trục (tránh méo dạng segment như lần
        //    trước dùng scale X/Z riêng biệt), lấy hệ số lớn hơn trong 2 trục để
        //    đảm bảo bao trọn hết khung, thà dư biên còn hơn thiếu.
        FitFence();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();
        Debug.Log($"[VoD] Xong -- sân trước + sân sau (rộng {YardMaxX - YardMinX}m x sâu {BackyardDepth}m, liền mạch với 2 dải hành lang vườn {SideGardenWidth}m mỗi bên) + hàng rào. Nhớ Ctrl+S.");
    }

    private static void BuildGrassSlab(Transform root, string name, float cornerX, float cornerZ, float w, float d, Material mat)
    {
        var old = root.Find(name);
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        Undo.RegisterCreatedObjectUndo(go, "VoD Build Exterior Ground Slab");
        go.transform.SetParent(root, worldPositionStays: true);
        go.transform.position = new Vector3(cornerX + w * 0.5f, 0f, cornerZ + d * 0.5f);
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = new Vector3(w / 10f, 1f, d / 10f); // Plane primitive mặc định 10x10

        if (mat != null)
        {
            var rend = go.GetComponent<MeshRenderer>();
            var instMat = new Material(mat);
            instMat.mainTextureScale = new Vector2(w, d);
            rend.sharedMaterial = instMat;
        }
    }

    private static void FitFence()
    {
        var fence = GameObject.Find("hang_rao");
        if (fence == null) { Debug.LogWarning("[VoD] Không tìm thấy 'hang_rao', bỏ qua bước hàng rào."); return; }

        var renderers = fence.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (renderers.Length == 0) { Debug.LogWarning("[VoD] 'hang_rao' không có Renderer con, bỏ qua."); return; }

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        if (bounds.size.x < 0.01f || bounds.size.z < 0.01f) { Debug.LogWarning("[VoD] Bounds hàng rào quá nhỏ/phẳng, bỏ qua."); return; }

        float targetW = FenceMaxX - FenceMinX;
        float targetD = FenceMaxZ - FenceMinZ;
        float scaleX = targetW / bounds.size.x;
        float scaleZ = targetD / bounds.size.z;
        float uniformScale = Mathf.Max(scaleX, scaleZ); // đủ bao trọn, thà dư còn hơn méo dạng

        Vector3 oldScale = fence.transform.localScale;
        Vector3 oldPos = fence.transform.position;
        Vector3 pivotOffsetFromCenter = oldPos - bounds.center; // world-space, chưa scale

        Vector3 targetCenter = new Vector3((FenceMinX + FenceMaxX) * 0.5f, oldPos.y, (FenceMinZ + FenceMaxZ) * 0.5f);

        Undo.RecordObject(fence.transform, "VoD Fit Fence To Villa");
        fence.transform.localScale = oldScale * uniformScale;
        fence.transform.position = targetCenter + pivotOffsetFromCenter * uniformScale;

        Debug.Log($"[VoD] Hàng rào: bounds cũ {bounds.size.x:F1}m x {bounds.size.z:F1}m -> scale đều x{uniformScale:F3} (giữ tỉ lệ, không méo) để bao trọn khung {targetW:F1}m x {targetD:F1}m.");
    }

    private static Transform GetOrCreateVillaGroundsRoot()
    {
        var go = GameObject.Find("VoD_Villa_Grounds_Root");
        if (go == null)
        {
            go = new GameObject("VoD_Villa_Grounds_Root");
            Undo.RegisterCreatedObjectUndo(go, "VoD Create Villa Grounds Root");
        }
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }
}
