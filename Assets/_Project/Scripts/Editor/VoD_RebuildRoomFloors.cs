using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// BUILD LẠI sàn + trần cho 9 phòng khối chính (8 phòng cũ + Véranda mới) --
/// cùng tinh thần với VoD_RebuildRoomWalls: xây khối phẳng đơn giản theo ĐÚNG
/// bảng kích thước phòng (không phụ thuộc mesh sàn/trần cũ, không phụ thuộc
/// transform phòng có thể bị méo), tắt hết sàn/trần cũ (không xoá).
///
/// Bảng RoomBox PHẢI khớp nguyên văn với VoD_RebuildRoomWalls.Rooms -- copy thủ
/// công (đúng convention đã dùng ở VoD_ScanRoomProps), nhớ đồng bộ nếu đổi sau.
public static class VoD_RebuildRoomFloors
{
    private struct RoomBox
    {
        public string name;
        public float cornerX, cornerZ, w, d, h;
        public RoomBox(string n, float cx, float cz, float w, float d, float h)
        { name = n; cornerX = cx; cornerZ = cz; this.w = w; this.d = d; this.h = h; }
    }

    private static readonly RoomBox[] Rooms = new[]
    {
        new RoomBox("ThuPhong",       21.0f, 5.5f,  9f, 7.2f, 4.2f),
        new RoomBox("TienSanh",       12.0f, 5.5f,  9f, 7.2f, 4.2f),
        new RoomBox("TiepKhach",      3.0f,  5.5f,  9f, 7.2f, 4.2f),
        new RoomBox("Hanh_Lang_Trai", 3.0f,  12.7f, 9f, 4.8f, 4.2f),
        new RoomBox("Hanh_Lang_Phai", 21.0f, 12.7f, 9f, 4.8f, 4.2f),
        new RoomBox("Salon",          12.0f, 17.5f, 18f, 7f, 4.2f),
        new RoomBox("PhongAn",        3.0f,  17.5f, 9f, 7f, 4.2f),
        new RoomBox("Veranda",        3.0f,  24.5f, 27f, 2.5f, 4.2f),
    };

    private const float FloorThickness = 0.15f;
    private const float CeilingThickness = 0.15f;

    // Sàn gỗ tếch cho phòng chính (khớp lore GDD -- "bàn ăn gỗ tếch", villa Đông
    // Dương khá giả dùng parquet); hành lang dùng gạch bông (đi lại nhiều, bền
    // hơn); Véranda dùng vật liệu sân/hiên có sẵn (Mat_Floor_Courtyard) vì đây là
    // khu bán ngoài trời nối ra sân sau, không phải phòng trong nhà.
    private static readonly Dictionary<string, string> FloorMatPathByRoom = new Dictionary<string, string>
    {
        { "ThuPhong",       "Assets/_Project/Materials/Architecture/Mat_Floor_Teak.mat" },
        { "TienSanh",       "Assets/_Project/Materials/Architecture/Mat_Floor_Teak.mat" },
        { "TiepKhach",      "Assets/_Project/Materials/Architecture/Mat_Floor_Teak.mat" },
        { "Salon",          "Assets/_Project/Materials/Architecture/Mat_Floor_Teak.mat" },
        { "PhongAn",        "Assets/_Project/Materials/Architecture/Mat_Floor_Teak.mat" },
        { "Hanh_Lang_Trai", "Assets/_Project/Materials/Architecture/Mat_Floor_CementTile.mat" },
        { "Hanh_Lang_Phai", "Assets/_Project/Materials/Architecture/Mat_Floor_CementTile.mat" },
        { "Veranda",        "Assets/_Project/Materials/Architecture/Mat_Floor_Courtyard.mat" },
    };

    // Không có material trần riêng trong project -- dùng lại màu sơn tường trong
    // (cream) cho mặt dưới trần, giống cách nhiều villa Đông Dương sơn trần đồng
    // màu tường trong.
    private const string CeilingMatPath = "Assets/_Project/Materials/Architecture/Mat_Wall_Interior_Cream.mat";

    private static readonly Regex FloorCeilingPrefixRegex = new Regex("^(Floor|Ceiling)_", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Footprint TOÀN KHỐI CHÍNH (không gồm Véranda -- veranda là phần nối 1 tầng
    // ra sân sau, tầng trên không xây trùm lên đó) -- dùng làm placeholder ước
    // lượng cho sàn/trần tầng 1 và tầng 2 (chưa chia phòng, Jok sẽ tự chia sau).
    private const float MainBlockMinX = 3.0f, MainBlockMaxX = 30.0f;   // 27m
    private const float MainBlockMinZ = 5.5f, MainBlockMaxZ = 24.5f;  // 19m
    private const float FloorToFloorHeight = 4.2f; // trệt Y:0-4.2, tầng1 Y:4.2-8.4, tầng2 Y:8.4-12.6

    // Giếng trời cầu thang (footprint CauThang cũ) -- sàn/trần placeholder tầng
    // 1-2 phải KHOÉT LỖ đúng chỗ này để sau còn chỗ cho cầu thang xuyên qua (Jok:
    // "sàn/trần né khu vực cầu thang ra"). KHÔNG áp dụng cho trần tầng 2 (mái) --
    // cầu thang theo kế hoạch dừng ở tầng 2, không lên tiếp áp mái.
    private const float StairwellMinX = 12.0f, StairwellMaxX = 21.0f;
    private const float StairwellMinZ = 12.7f, StairwellMaxZ = 17.5f;

    [MenuItem("VoD/Rebuild ALL Room Floors+Ceilings (9 Rooms + Floor1/2 Placeholder)")]
    public static void RebuildAll()
    {
        var floorsRoot = GetOrCreateVillaFloorsRoot();
        var ceilingMat = AssetDatabase.LoadAssetAtPath<Material>(CeilingMatPath);

        // 2 container GOM RIÊNG toàn bộ sàn / toàn bộ trần TẦNG TRỆT (theo yêu cầu
        // Jok: "tách ra 2 object celling tầng trệt, và floor tầng trệt") -- chọn/ẩn
        // 1 lần là xong cả tầng, không phải ẩn từng phòng một.
        var groundFloorHolder = GetOrCreateChild(floorsRoot, "Floor_GroundFloor");
        var groundCeilingHolder = GetOrCreateChild(floorsRoot, "Ceiling_GroundFloor");

        int hiddenOld = 0, built = 0;
        foreach (var rb in Rooms)
        {
            // Tắt sàn/trần cũ của phòng (nếu phòng đã tồn tại trong scene -- Véranda
            // là phòng mới, chưa có object cũ nào để tắt, bỏ qua bước này cho nó).
            var room = GameObject.Find(rb.name);
            if (room != null)
            {
                var allT = room.GetComponentsInChildren<Transform>(includeInactive: true);
                foreach (var t in allT)
                {
                    if (t == room.transform || !FloorCeilingPrefixRegex.IsMatch(t.name)) continue;
                    var rend = t.GetComponent<Renderer>();
                    if (rend != null && rend.enabled) { rend.enabled = false; hiddenOld++; }
                    var col = t.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                }
            }

            string floorMatPath;
            if (!FloorMatPathByRoom.TryGetValue(rb.name, out floorMatPath))
            {
                Debug.LogError($"[VoD] Không có material sàn cho '{rb.name}', bỏ qua phòng này.");
                continue;
            }
            var floorMat = AssetDatabase.LoadAssetAtPath<Material>(floorMatPath);

            // Xoá bản cũ (từ lần chạy trước, có thể còn ở holder-theo-phòng kiểu cũ).
            var oldPerRoomHolder = GameObject.Find($"FloorCeiling_Rebuilt_{rb.name}");
            if (oldPerRoomHolder != null) Undo.DestroyObjectImmediate(oldPerRoomHolder);
            var oldFloor = groundFloorHolder.Find($"Floor_{rb.name}");
            if (oldFloor != null) Undo.DestroyObjectImmediate(oldFloor.gameObject);
            var oldCeiling = groundCeilingHolder.Find($"Ceiling_{rb.name}");
            if (oldCeiling != null) Undo.DestroyObjectImmediate(oldCeiling.gameObject);

            CreateSlab(groundFloorHolder, $"Floor_{rb.name}", rb.cornerX, rb.cornerZ, rb.w, rb.d, -FloorThickness, 0f, floorMat);
            CreateSlab(groundCeilingHolder, $"Ceiling_{rb.name}", rb.cornerX, rb.cornerZ, rb.w, rb.d, rb.h, rb.h + CeilingThickness, ceilingMat);
            built += 2;
        }

        // Tầng 1 + Tầng 2 -- CHƯA chia phòng (đợi Jok/team thiết kế layout riêng),
        // đặt placeholder FULL diện tích khối chính để ước lượng tỉ lệ trước, ẩn
        // đi sau khi nhóm bắt đầu dựng props/phòng thật.
        var floorMatDefault = AssetDatabase.LoadAssetAtPath<Material>(FloorMatPathByRoom["Salon"]);
        float mbW = MainBlockMaxX - MainBlockMinX, mbD = MainBlockMaxZ - MainBlockMinZ;

        // Floor1: cả sàn (Y=4.2) lẫn trần (Y=8.4) đều giáp giếng trời cầu thang -> khoét lỗ cả 2.
        // Floor2: sàn (Y=8.4, cùng cao độ trần Floor1) khoét lỗ; trần (Y=12.6, MÁI) thì KHÔNG -- cầu thang dừng ở tầng 2.
        BuildPlaceholderLevel("Floor1", MainBlockMinX, MainBlockMinZ, mbW, mbD, FloorToFloorHeight, floorsRoot, floorMatDefault, ceilingMat, floorHasHole: true, ceilingHasHole: true, ref built);
        BuildPlaceholderLevel("Floor2", MainBlockMinX, MainBlockMinZ, mbW, mbD, FloorToFloorHeight * 2f, floorsRoot, floorMatDefault, ceilingMat, floorHasHole: true, ceilingHasHole: false, ref built);

        // Ban công khô sơ (Jok: "cả ban công mặt tiền nhà luôn không chỉ mặt sau,
        // đâm lồi ra 1 tí ở tầng 1") -- theo GDD: tầng 1 nhìn sân trước (Bắc),
        // tầng 2 nhìn vườn/giếng (Nam); Jok muốn ĐẦY ĐỦ cả 2 mặt ở CẢ 2 tầng.
        // Mỗi ban công: sàn nhô ra 2.5m + lan can thấp (3 cạnh hở, cạnh giáp nhà
        // không cần lan can vì đã có tường).
        var balconyMat = AssetDatabase.LoadAssetAtPath<Material>(FloorMatPathByRoom["Salon"]);
        float bcCenterX = (MainBlockMinX + MainBlockMaxX) * 0.5f;
        const float bcWidth = 5f, bcDepth = 2.5f, bcRailHeight = 1.0f, bcRailThickness = 0.1f;
        BuildBalcony(floorsRoot, "BanCong_Floor1_Bac", bcCenterX, MainBlockMinZ, bcWidth, bcDepth, FloorToFloorHeight, north: true, balconyMat, bcRailHeight, bcRailThickness, ref built);
        BuildBalcony(floorsRoot, "BanCong_Floor1_Nam", bcCenterX, MainBlockMaxZ, bcWidth, bcDepth, FloorToFloorHeight, north: false, balconyMat, bcRailHeight, bcRailThickness, ref built);
        BuildBalcony(floorsRoot, "BanCong_Floor2_Bac", bcCenterX, MainBlockMinZ, bcWidth, bcDepth, FloorToFloorHeight * 2f, north: true, balconyMat, bcRailHeight, bcRailThickness, ref built);
        BuildBalcony(floorsRoot, "BanCong_Floor2_Nam", bcCenterX, MainBlockMaxZ, bcWidth, bcDepth, FloorToFloorHeight * 2f, north: false, balconyMat, bcRailHeight, bcRailThickness, ref built);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();
        Debug.Log($"[VoD] Xong -- xây {built} khối sàn/trần mới ({Rooms.Length} phòng tầng trệt + placeholder Floor1/Floor2), tắt {hiddenOld} object cũ. " +
                   "Cấu trúc: Floor_GroundFloor / Ceiling_GroundFloor (theo phòng) + Floor_Floor1/Ceiling_Floor1 + Floor_Floor2/Ceiling_Floor2 (placeholder full khối chính, chưa chia phòng). Nhớ Ctrl+S.");
    }

    // 1 tầng placeholder = 1 sàn full diện tích ở đáy tầng (Y=levelBaseY) + 1 trần
    // full diện tích ở đỉnh tầng (Y=levelBaseY+FloorToFloorHeight) -- CHƯA chia
    // phòng, chỉ ước lượng khối hộp để có tỉ lệ tham chiếu trước khi nhóm dựng
    // layout thật. floorHasHole/ceilingHasHole: khoét giếng trời cầu thang hay không.
    private static void BuildPlaceholderLevel(string levelName, float minX, float minZ, float w, float d, float levelBaseY,
        Transform floorsRoot, Material floorMat, Material ceilingMat, bool floorHasHole, bool ceilingHasHole, ref int built)
    {
        var floorHolder = GetOrCreateChild(floorsRoot, $"Floor_{levelName}");
        var ceilingHolder = GetOrCreateChild(floorsRoot, $"Ceiling_{levelName}");

        var oldFloorSlab = floorHolder.Find($"Floor_{levelName}_Placeholder");
        if (oldFloorSlab != null) Undo.DestroyObjectImmediate(oldFloorSlab.gameObject);
        var oldFloorFrame = floorHolder.Find($"Floor_{levelName}_Placeholder_Frame");
        if (oldFloorFrame != null) Undo.DestroyObjectImmediate(oldFloorFrame.gameObject);
        var oldCeilingSlab = ceilingHolder.Find($"Ceiling_{levelName}_Placeholder");
        if (oldCeilingSlab != null) Undo.DestroyObjectImmediate(oldCeilingSlab.gameObject);
        var oldCeilingFrame = ceilingHolder.Find($"Ceiling_{levelName}_Placeholder_Frame");
        if (oldCeilingFrame != null) Undo.DestroyObjectImmediate(oldCeilingFrame.gameObject);

        built += BuildSlabMaybeWithHole(floorHolder, $"Floor_{levelName}_Placeholder", minX, minZ, w, d, levelBaseY - FloorThickness, levelBaseY, floorMat, floorHasHole);
        built += BuildSlabMaybeWithHole(ceilingHolder, $"Ceiling_{levelName}_Placeholder", minX, minZ, w, d, levelBaseY + FloorToFloorHeight, levelBaseY + FloorToFloorHeight + CeilingThickness, ceilingMat, ceilingHasHole);
    }

    // Không khoét lỗ -> 1 khối như cũ. Có khoét lỗ -> 4 khối viền bao quanh giếng
    // trời cầu thang (Stairwell*), giống cách VoD_WallBuilder tách tường quanh
    // 1 lỗ cửa -- North/South chạy full bề ngang, West/East chỉ chạy đúng đoạn
    // giữa (tránh chồng lấn ở 4 góc).
    private static int BuildSlabMaybeWithHole(Transform parent, string name, float cornerX, float cornerZ, float w, float d, float yMin, float yMax, Material mat, bool hasHole)
    {
        if (!hasHole)
        {
            CreateSlab(parent, name, cornerX, cornerZ, w, d, yMin, yMax, mat);
            return 1;
        }

        var frame = new GameObject(name + "_Frame");
        Undo.RegisterCreatedObjectUndo(frame, "VoD Build Floor/Ceiling Frame (Stairwell Hole)");
        frame.transform.SetParent(parent, worldPositionStays: true);

        float maxX = cornerX + w, maxZ = cornerZ + d;
        // North: từ mép Bắc tới mép Bắc giếng trời, full bề ngang.
        CreateSlab(frame.transform, name + "_North", cornerX, cornerZ, w, StairwellMinZ - cornerZ, yMin, yMax, mat);
        // South: từ mép Nam giếng trời tới mép Nam, full bề ngang.
        CreateSlab(frame.transform, name + "_South", cornerX, StairwellMaxZ, w, maxZ - StairwellMaxZ, yMin, yMax, mat);
        // West: chỉ đoạn giữa (ngang tầm giếng trời theo Z), từ mép Tây tới mép Tây giếng trời.
        CreateSlab(frame.transform, name + "_West", cornerX, StairwellMinZ, StairwellMinX - cornerX, StairwellMaxZ - StairwellMinZ, yMin, yMax, mat);
        // East: tương tự, từ mép Đông giếng trời tới mép Đông.
        CreateSlab(frame.transform, name + "_East", StairwellMaxX, StairwellMinZ, maxX - StairwellMaxX, StairwellMaxZ - StairwellMinZ, yMin, yMax, mat);
        return 4;
    }

    // Ban công khô sơ: 1 sàn nhô ra khỏi tường (Bắc hoặc Nam) + lan can thấp 3
    // cạnh (cạnh trong giáp tường nhà không cần lan can). north=true -> nhô ra
    // phía Bắc (Z nhỏ hơn wallZ); false -> nhô ra phía Nam (Z lớn hơn wallZ).
    private static void BuildBalcony(Transform floorsRoot, string name, float centerX, float wallZ, float width, float depth, float baseY, bool north, Material mat, float railHeight, float railThickness, ref int built)
    {
        var old = floorsRoot.Find(name);
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        var holder = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(holder, "VoD Build Balcony");
        holder.transform.SetParent(floorsRoot, worldPositionStays: true);
        holder.transform.position = Vector3.zero;
        holder.transform.rotation = Quaternion.identity;
        holder.transform.localScale = Vector3.one;

        float cornerX = centerX - width * 0.5f;
        float outerZ = north ? wallZ - depth : wallZ + depth;
        float floorCornerZ = north ? outerZ : wallZ;

        CreateSlab(holder.transform, "Floor", cornerX, floorCornerZ, width, depth, baseY - FloorThickness, baseY, mat);
        built++;

        // Cạnh ngoài (song song tường nhà, xa nhất).
        CreateSlab(holder.transform, "Rail_Outer", cornerX, outerZ - railThickness * 0.5f, width, railThickness, baseY, baseY + railHeight, mat);
        // 2 cạnh bên (nối tường nhà ra cạnh ngoài).
        CreateSlab(holder.transform, "Rail_Left", cornerX - railThickness * 0.5f, floorCornerZ, railThickness, depth, baseY, baseY + railHeight, mat);
        CreateSlab(holder.transform, "Rail_Right", cornerX + width - railThickness * 0.5f, floorCornerZ, railThickness, depth, baseY, baseY + railHeight, mat);
        built += 3;
    }

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "VoD Create Floor/Ceiling Level Group");
        go.transform.SetParent(parent, worldPositionStays: true);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    private static void CreateSlab(Transform parent, string name, float cornerX, float cornerZ, float w, float d, float yMin, float yMax, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Undo.RegisterCreatedObjectUndo(go, "VoD Build Floor/Ceiling Slab");
        go.transform.SetParent(parent, worldPositionStays: true);

        float xCenter = cornerX + w * 0.5f;
        float zCenter = cornerZ + d * 0.5f;
        float yCenter = (yMin + yMax) * 0.5f;
        float yLen = yMax - yMin;

        go.transform.position = new Vector3(xCenter, yCenter, zCenter);
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = new Vector3(w, yLen, d);

        if (mat != null)
        {
            var rend = go.GetComponent<MeshRenderer>();
            // Material instance riêng + tiling theo đúng kích thước thật (giống
            // cách VoD_WallBuilder làm cho tường) -- tránh hoạ tiết bị kéo giãn
            // khác nhau giữa phòng to/nhỏ.
            var instMat = new Material(mat);
            instMat.mainTextureScale = new Vector2(w, d);
            rend.sharedMaterial = instMat;
        }
    }

    // Theo yêu cầu Jok: 1 nút ẩn hết cụm floor/ceiling placeholder (6 group: Floor/
    // Ceiling x GroundFloor/Floor1/Floor2) sau khi đã xem tạm ổn, để nhóm dựng
    // props không bị vướng hình khối che khuất. Chỉ SetActive false (không xoá) --
    // bật lại dễ dàng qua Hierarchy nếu cần xem lại.
    [MenuItem("VoD/Villa/Hide All Floor+Ceiling Placeholders")]
    public static void HideAllPlaceholders()
    {
        var root = GameObject.Find("VoD_Villa_Floors_Root");
        if (root == null) { Debug.LogWarning("[VoD] Không tìm thấy 'VoD_Villa_Floors_Root'."); return; }

        string[] groupNames = { "Floor_GroundFloor", "Ceiling_GroundFloor", "Floor_Floor1", "Ceiling_Floor1", "Floor_Floor2", "Ceiling_Floor2" };
        int hidden = 0;
        foreach (var n in groupNames)
        {
            var t = root.transform.Find(n);
            if (t == null) continue;
            Undo.RecordObject(t.gameObject, "VoD Hide Floor/Ceiling Placeholder");
            t.gameObject.SetActive(false);
            hidden++;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[VoD] Đã ẩn {hidden}/{groupNames.Length} nhóm sàn/trần placeholder (SetActive false, chưa xoá -- bật lại qua Hierarchy nếu cần). Nhớ Ctrl+S.");
    }

    private static Transform GetOrCreateVillaFloorsRoot()
    {
        var go = GameObject.Find("VoD_Villa_Floors_Root");
        if (go == null)
        {
            go = new GameObject("VoD_Villa_Floors_Root");
            Undo.RegisterCreatedObjectUndo(go, "VoD Create Villa Floors Root");
        }
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }
}
