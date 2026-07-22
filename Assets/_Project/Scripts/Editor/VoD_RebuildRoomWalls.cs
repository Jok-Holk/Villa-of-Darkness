using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VoDVilla;

/// BUILD LẠI HOÀN TOÀN tường cho 8 phòng khối chính (không gồm Nha_Phu_Bep/
/// Nha_Kho — nhà phụ tách biệt, tường module đã ổn, không đụng; không gồm
/// CauThang nội thất — Jok tự dựng cầu thang bằng ProBuilder, ở đây chỉ xây 4
/// mặt tường bao quanh nó).
///
/// Lý do build lại thay vì sửa: tường cũ mỗi phòng 1 kiểu (mesh liền khối L/U
/// ở ThuPhong/TienSanh/TiepKhach/PhongAn/Salon, module rời ở Hanh_Lang) — cửa
/// giữa 2 phòng do 2 người vẽ riêng nên không khớp tâm với nhau. Bản build mới
/// này tính lỗ cửa tại ĐÚNG TRUNG ĐIỂM đường biên chung giữa 2 phòng (hình học
/// thuần từ bảng kích thước chuẩn, không phụ thuộc object cũ) — đảm bảo 2 bên
/// một cửa luôn thẳng hàng tuyệt đối.
///
/// Toàn bộ object tường CŨ (tên khớp "wall"/"tuong") trong 8 phòng này bị TẮT
/// (SetActive false, không xoá) để Jok move nội thất thủ công dựa theo layout
/// cũ mà không bị tường cũ che khuất. Nội thất/đồ đạc KHÔNG bị đụng.
public static class VoD_RebuildRoomWalls
{
    private struct RoomBox
    {
        public string name;
        public float cornerX, cornerZ, w, d, h;
        public RoomBox(string n, float cx, float cz, float w, float d, float h)
        { name = n; cornerX = cx; cornerZ = cz; this.w = w; this.d = d; this.h = h; }
    }

    private const float WallThickness = 0.25f;
    private const float DoorWidth = 1.4f, DoorHeight = 2.3f;
    private static readonly Regex WallRegex = new Regex("wall|tuong", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private const string InteriorWallMatPath = "Assets/_Project/Materials/Architecture/Mat_Wall_Interior_Cream.mat";
    private const string ExteriorWallMatPath = "Assets/_Project/Materials/Architecture/Mat_Wall_Exterior_Ochre.mat";

    // Hành lang rộng gấp 2 (3m -> 6m, lấy về phía Salon/PhongAn) + xoá cửa
    // CauThang<->Salon (theo xác nhận của Jok) -- Salon/PhongAn dịch lùi 3m
    // (cornerZ 14.5 -> 17.5). CauThang cũng nới theo Hanh_Lang cho liền mạch
    // (không bị so le), dù không có cửa sang Salon nữa.
    // ThuPhong/TienSanh/TiepKhach lấn thêm 1.2m (=1/5 của hành lang 6m) về phía
    // Nam (vào phía hành lang) theo yêu cầu Jok -- d: 6 -> 7.2, cornerZ Bắc giữ
    // nguyên 5.5 (chỉ nới rộng thêm về phía Nam). Hành lang/CauThang nhường lại
    // đúng 1.2m đó -- cornerZ: 11.5 -> 12.7, d: 6 -> 4.8 (giữ nguyên mép Nam ở
    // 17.5 giáp Salon/PhongAn, không đụng).
    private static readonly RoomBox[] Rooms = new[]
    {
        new RoomBox("ThuPhong",       21.0f, 5.5f,  9f, 7.2f, 4.2f),
        new RoomBox("TienSanh",       12.0f, 5.5f,  9f, 7.2f, 4.2f),
        new RoomBox("TiepKhach",      3.0f,  5.5f,  9f, 7.2f, 4.2f),
        new RoomBox("Hanh_Lang_Trai", 3.0f,  12.7f, 9f, 4.8f, 4.2f),
        new RoomBox("CauThang",       12.0f, 12.7f, 9f, 4.8f, 4.2f),
        new RoomBox("Hanh_Lang_Phai", 21.0f, 12.7f, 9f, 4.8f, 4.2f),
        new RoomBox("Salon",          12.0f, 17.5f, 18f, 7f, 4.2f),
        new RoomBox("PhongAn",        3.0f,  17.5f, 9f, 7f, 4.2f),
        // MỚI (theo GDD: "Phòng Sân / Véranda -- Kết nối hành lang → sân sau. Cửa
        // sổ nhìn ra giếng." + "Galerie bao quanh từng tầng, rộng 2.5m, kết nối
        // TẤT CẢ cửa ra vào bên ngoài"). Bản đầu chỉ rộng 5m (đúng số ~15m² trong
        // GDD, gắn riêng Salon) -- Jok xác nhận muốn NGUYÊN CHIỀU RỘNG cả cụm
        // Salon+PhongAn (giống galerie thật, không phải 1 phòng đệm nhỏ). Giờ trải
        // suốt X:3-30 (27m, khớp bề rộng Salon+PhongAn cộng lại), sâu 2.5m (đúng
        // số "galerie rộng 2.5m" trong GDD). Sân sau/giếng ngoài trời thật vẫn CHƯA
        // tồn tại trong scene -- hạng mục riêng lớn hơn, làm sau.
        new RoomBox("Veranda",        3.0f, 24.5f, 27f, 2.5f, 4.2f),
    };

    // Salon/PhongAn dịch lùi 3m (Z) so với trước -- nội thất cũ (con của room root)
    // phải dịch theo CÙNG lượng để vẫn nằm trong tường mới, không bị "kẹt" ở
    // khoảng hành lang mở rộng. Dịch bằng cách di chuyển thẳng ROOT của phòng
    // (world position) -- mọi con bên trong tự trôi theo vì localPosition không đổi.
    private static readonly Dictionary<string, float> FurnitureShiftZ = new Dictionary<string, float>
    {
        { "Salon", 3f }, { "PhongAn", 3f },
    };

    // ThuPhong/TienSanh/TiepKhach GIÃN RA (không phải dịch nguyên khối) -- phòng
    // to thêm 1.2m theo Z chứ không di chuyển, nên nội thất bên trong cần DÃN
    // (kéo giãn vị trí theo Z, tính từ mép Bắc cố định = cornerZ 5.5) chứ không
    // dịch đều. Hệ số = depth mới / depth cũ = 7.2/6 = 1.2.
    private const float FrontRoomStretchFactor = 7.2f / 6f;
    private static readonly string[] FrontRoomsToStretch = { "ThuPhong", "TienSanh", "TiepKhach" };
    private const float FrontRoomNorthZ = 5.5f; // mép Bắc cố định, dùng làm gốc dãn

    // 11 cặp phòng liền kề trong khối chính (tính hình học thuần từ bảng trên) +
    // 1 cửa chính (TienSanh mặt Bắc, ra sân trước) + 1 cửa sổ (PhongAn mặt Tây).
    // (roomA, sideA, roomB, sideB, đoạn chồng min, đoạn chồng max, có cửa hay không, độ rộng cửa riêng -- 0 = dùng DoorWidth mặc định)
    // hasDoor=false: vẫn tính ranh giới (roomB bỏ mặt đó) nhưng roomA xây tường ĐẶC,
    // không khoét lỗ.
    //
    // SỬA theo yêu cầu Jok (bản vẽ tay): CauThang trở thành 1 khối RIÊNG BIỆT, chỉ
    // vào được từ TienSanh (mở toang, không cửa hẹp) — 2 hành lang KHÔNG còn nối
    // vào CauThang nữa (TiepKhach->Hanh_Lang_Trai->PhongAn và ThuPhong->Hanh_Lang_Phai
    // ->Salon là 2 tuyến hoàn toàn tách biệt, không đi qua khu cầu thang).
    //
    // CauThang đã bị Jok XOÁ tạm (chưa khớp scope, để làm sau) -- cả 4 cặp từng
    // dính tới CauThang (TienSanh mở toang sang CauThang, Hanh_Lang_Trai/Phai
    // seal sang CauThang, CauThang seal sang Salon) đã bỏ khỏi bảng này. Giờ
    // TienSanh/Hanh_Lang_Trai/Hanh_Lang_Phai/Salon đều tự xây tường ĐẶC bình
    // thường ở mặt từng giáp CauThang (không phụ thuộc CauThang tồn tại hay
    // không). Khi nào dựng lại CauThang thì thêm lại 4 cặp đó.
    private static readonly (string a, string sideA, string b, string sideB, float min, float max, bool hasDoor, float widthOverride)[] Adjacencies = new[]
    {
        ("TiepKhach", "East", "TienSanh", "West", 5.5f, 11.5f, true, 0f),          // biên X=12
        ("TienSanh", "East", "ThuPhong", "West", 5.5f, 11.5f, true, 0f),           // biên X=21
        ("TiepKhach", "South", "Hanh_Lang_Trai", "North", 3.0f, 12.0f, true, 0f),  // biên Z=11.5
        ("ThuPhong", "South", "Hanh_Lang_Phai", "North", 21.0f, 30.0f, true, 0f),  // biên Z=11.5
        ("Hanh_Lang_Trai", "South", "PhongAn", "North", 3.0f, 12.0f, true, 0f),    // biên Z=17.5
        // Đổi chủ (Salon xây thay vì Hanh_Lang_Phai): mặt Bắc Salon rộng NGUYÊN 18m
        // (X:12-30), trong khi Hanh_Lang_Phai chỉ rộng 9m (X:21-30) -- nếu để
        // Hanh_Lang_Phai xây thì chỉ có 1 nửa mặt Bắc Salon có tường (nửa còn lại,
        // giáp chỗ CauThang cũ X:12-21, sẽ trống hẳn -- đúng bug Jok phát hiện qua
        // ảnh first-person). Salon tự xây NGUYÊN cả mặt Bắc, chỉ khoét 1 cửa ở
        // đúng chỗ giáp Hanh_Lang_Phai (X=25.5).
        ("Salon", "North", "Hanh_Lang_Phai", "South", 21.0f, 30.0f, true, 0f),     // biên Z=17.5
        ("PhongAn", "East", "Salon", "West", 17.5f, 24.5f, true, 0f),              // biên X=12
        // Véranda giờ trải suốt X:3-30 -- Salon (X12-30) và PhongAn (X3-12) MỖI
        // BÊN tự xây trọn mặt Nam của mình (như cách Salon xây trọn mặt Bắc ở
        // trên), mỗi bên khoét 1 cửa riêng xuống Véranda. Véranda bỏ qua TOÀN BỘ
        // mặt Bắc (skipSidesByRoom cộng dồn từ cả 2 cặp, HashSet tự khử trùng).
        ("Salon", "South", "Veranda", "North", 12.0f, 30.0f, true, 1.8f),          // biên Z=24.5 -- cửa Salon -> Véranda (giữa X=21)
        ("PhongAn", "South", "Veranda", "North", 3.0f, 12.0f, true, 1.8f),         // biên Z=24.5 -- cửa PhongAn -> Véranda (giữa X=7.5)
    };

    // Khoảng trống CauThang (đã xoá tạm) vẫn cần xử lý riêng ở 4 mặt xung quanh:
    //   - TienSanh mặt Nam (hướng khoảng trống): MỞ TOANG hẳn, không xây gì cả.
    //   - Salon mặt Bắc (hướng khoảng trống): tường ĐẶC (đã đúng theo mặc định, giữ nguyên).
    //   - Hanh_Lang_Trai mặt Đông, Hanh_Lang_Phai mặt Tây (hướng khoảng trống):
    //     Jok bỏ ý định "tường có lỗ cửa" ở đây (cụm build ra bị lỗi hình dạng,
    //     xem VoD/Scan+Remove Void-Facing Hallway Walls) -- MỞ TOANG luôn như
    //     TienSanh, không xây gì cả nữa.
    private static readonly (string room, string side)[] ForceSkipSides = new[]
    {
        ("TienSanh", "South"),
        ("Hanh_Lang_Trai", "East"),
        ("Hanh_Lang_Phai", "West"),
    };
    private static readonly (string room, string side, float posAlong)[] StandaloneOpenings = new (string, string, float)[0];

    [MenuItem("VoD/Rebuild ALL Main Block Walls (8 Rooms, Full Redo)")]
    public static void RebuildAll()
    {
        // 1) Gom opening cho từng phòng từ bảng Adjacencies (mỗi cặp sinh 2 opening,
        //    1 bên mỗi phòng, CÙNG toạ độ trung điểm -- đảm bảo 2 bên thẳng hàng).
        //    BUG đã sửa: bản trước mỗi phòng trong 1 cặp đều TỰ XÂY tường tại đúng
        //    ranh giới chung -- 2 bức tường 2 phòng chồng khít lên nhau (z-fighting,
        //    "tường đụng nhau"). Giờ chỉ phòng `a` (đứng trước trong tuple) XÂY tường
        //    thật tại ranh giới đó, phòng `b` bỏ qua hẳn mặt đó (skipSides).
        var openingsByRoom = new Dictionary<string, List<OpeningModel>>();
        var skipSidesByRoom = new Dictionary<string, HashSet<string>>();
        foreach (var rb in Rooms) { openingsByRoom[rb.name] = new List<OpeningModel>(); skipSidesByRoom[rb.name] = new HashSet<string>(); }

        foreach (var adj in Adjacencies)
        {
            if (adj.hasDoor)
            {
                float mid = (adj.min + adj.max) * 0.5f;
                float width = adj.widthOverride > 0f ? adj.widthOverride : DoorWidth;
                // width = trọn đoạn chồng (mở toang, không tường 2 bên) khi widthOverride khớp đúng (max-min).
                float height = width >= (adj.max - adj.min) - 0.01f ? System.Array.Find(Rooms, r => r.name == adj.a).h : DoorHeight;
                openingsByRoom[adj.a].Add(new OpeningModel { room = adj.a, side = adj.sideA, posAlong = mid, width = width, sillY = 0f, openHeight = height });
            }
            // hasDoor=false: không thêm OpeningModel -> roomA tự xây tường ĐẶC (không lỗ) ở BuildSide.
            skipSidesByRoom[adj.b].Add(adj.sideB);
        }

        // Khoảng trống CauThang (đã xoá tạm): TienSanh mở toang hẳn (không xây gì),
        // Hanh_Lang_Trai/Phai xây tường có 1 lỗ cửa thường hướng vào khoảng trống đó.
        foreach (var f in ForceSkipSides) skipSidesByRoom[f.room].Add(f.side);
        foreach (var o in StandaloneOpenings)
            openingsByRoom[o.room].Add(new OpeningModel { room = o.room, side = o.side, posAlong = o.posAlong, width = DoorWidth, sillY = 0f, openHeight = DoorHeight });

        // Cửa chính TienSanh (mặt Bắc, ra sân trước) -- toạ độ đo thật từ object gốc.
        openingsByRoom["TienSanh"].Add(new OpeningModel { room = "TienSanh", side = "North", posAlong = 16.61f, width = 1.8f, sillY = 0f, openHeight = 2.6f });
        // Cửa sổ PhongAn (mặt Tây) -- ĐÃ SỬA LẦN 2: KhungCuaSo (object cha của
        // WindowFrame/WindowGlass) còn dính scale méo riêng (1.044, 0.708, 1.787 --
        // lỗi corrupt độc lập với root phòng) khiến bounds đo lần trước (3.77x2.33)
        // sai lệch. Đã reset KhungCuaSo về scale đều 0.45 qua MCP (scale=1 cho ra cửa
        // sổ cao 3.6m/rộng 4.32m -- vượt cả trần 4.2m và lấn góc tường Nam; 0.45 cho
        // kích thước hợp lý, đo lại chính xác qua get_gameobject: sillY=0.974,
        // openHeight=1.62 (đỉnh ở Y=2.594, còn dư 1.6m tới trần), width=1.944,
        // posAlong=22.681 (không đổi so với lần đo trước, vị trí luôn đúng).
        openingsByRoom["PhongAn"].Add(new OpeningModel { room = "PhongAn", side = "West", posAlong = 22.68f, width = 1.944f, sillY = 0.974f, openHeight = 1.62f, isWindow = true });
        // SỬA theo Jok: mặt Nam Véranda phải là CỬA ĐI THẬT ra sân sau (gameplay
        // cần đi bộ qua được -- Phần VII "Ra galerie phía sau... Qua sân sau"),
        // không phải cửa sổ (không đi xuyên được). Cửa Pháp 2 cánh sát đất
        // (sillY=0), cao gần hết tường, giữa mặt Nam MỚI (X:3-30, giữa = 16.5).
        // Giếng/sân sau ngoài trời CHƯA tồn tại trong scene -- hạng mục riêng, làm sau.
        openingsByRoom["Veranda"].Add(new OpeningModel { room = "Veranda", side = "South", posAlong = 16.5f, width = 2.4f, sillY = 0f, openHeight = 2.4f });

        // Mặt nào là "trong" (giáp phòng khác, kể cả khoảng trống CauThang) suy ra
        // TỰ ĐỘNG từ Adjacencies + ForceSkipSides -- mặt nào KHÔNG có trong 2 bảng
        // đó (VD ThuPhong mặt Bắc/Đông, PhongAn mặt Tây có cửa sổ, TienSanh mặt Bắc
        // có cửa chính...) mặc định là tường NGOÀI THẬT của villa, dùng material ochre.
        var interiorSides = new HashSet<(string room, string side)>();
        foreach (var adj in Adjacencies) { interiorSides.Add((adj.a, adj.sideA)); interiorSides.Add((adj.b, adj.sideB)); }
        foreach (var f in ForceSkipSides) interiorSides.Add((f.room, f.side));

        var exteriorSidesByRoom = new Dictionary<string, HashSet<string>>();
        foreach (var rb in Rooms)
        {
            var ext = new HashSet<string>();
            foreach (var side in new[] { "West", "East", "North", "South" })
                if (!interiorSides.Contains((rb.name, side))) ext.Add(side);
            exteriorSidesByRoom[rb.name] = ext;
        }

        var intMat = AssetDatabase.LoadAssetAtPath<Material>(InteriorWallMatPath);
        var extMat = AssetDatabase.LoadAssetAtPath<Material>(ExteriorWallMatPath);

        int wallsBuilt = 0, oldWallsHidden = 0;
        foreach (var rb in Rooms)
        {
            var room = GameObject.Find(rb.name);
            if (room == null)
            {
                if (rb.name == "CauThang") { Debug.Log($"[VoD] '{rb.name}' không tồn tại trong scene (đã xoá tạm, chưa khớp scope) -- bỏ qua như dự kiến."); continue; }
                if (rb.name == "Veranda")
                {
                    // Phòng MỚI, chưa từng tồn tại trong scene -- tự tạo GameObject
                    // trống (transform identity) để làm chỗ neo, không cần nội thất
                    // cũ để dịch/dãn như các phòng khác.
                    room = new GameObject("Veranda");
                    Undo.RegisterCreatedObjectUndo(room, "VoD Create Veranda Room");
                    Debug.Log("[VoD] 'Veranda' chưa tồn tại -- đã tạo GameObject trống mới.");
                }
                else { Debug.LogError($"[VoD] Không tìm thấy phòng '{rb.name}'."); continue; }
            }

            // Dịch nội thất cũ theo phòng đã dịch lùi (Salon/PhongAn) -- chỉ dịch 1 LẦN
            // (đánh dấu bằng marker con), tránh dịch chồng nếu bấm nút lại nhiều lần.
            if (FurnitureShiftZ.TryGetValue(rb.name, out float shiftZ) && room.transform.Find("_FurnitureShifted") == null)
            {
                Undo.RecordObject(room.transform, "VoD Shift Room Furniture");
                room.transform.position += new Vector3(0f, 0f, shiftZ);
                var marker = new GameObject("_FurnitureShifted");
                Undo.RegisterCreatedObjectUndo(marker, "VoD Shift Room Furniture Marker");
                marker.transform.SetParent(room.transform, worldPositionStays: false);
                Debug.Log($"[VoD] '{rb.name}': đã dịch nội thất +{shiftZ}m theo trục Z (khớp phòng dịch lùi cho hành lang rộng ra).");
            }

            // ThuPhong/TienSanh/TiepKhach GIÃN nội thất theo Z (phòng to ra 1.2m, không
            // phải dịch chỗ) -- lấy mép Bắc cố định (Z=5.5) làm gốc, kéo giãn từng món đồ
            // theo world Z hiện tại của CHÍNH nó (an toàn với transform cha bị xoay/lệch
            // scale, giống cách sửa tường). Chỉ chạy 1 lần (đánh dấu bằng marker).
            if (System.Array.IndexOf(FrontRoomsToStretch, rb.name) >= 0 && room.transform.Find("_FurnitureStretched") == null)
            {
                var toStretch = room.GetComponentsInChildren<Transform>(includeInactive: true);
                var captured = new List<(Transform t, Vector3 pos)>();
                foreach (var t in toStretch) if (t != room.transform) captured.Add((t, t.position));

                foreach (var (t, pos) in captured)
                {
                    Undo.RecordObject(t, "VoD Stretch Room Furniture");
                    float newZ = FrontRoomNorthZ + (pos.z - FrontRoomNorthZ) * FrontRoomStretchFactor;
                    t.position = new Vector3(pos.x, pos.y, newZ);
                }

                var stretchMarker = new GameObject("_FurnitureStretched");
                Undo.RegisterCreatedObjectUndo(stretchMarker, "VoD Stretch Room Furniture Marker");
                stretchMarker.transform.SetParent(room.transform, worldPositionStays: false);
                Debug.Log($"[VoD] '{rb.name}': đã dãn {captured.Count} object theo trục Z (hệ số x{FrontRoomStretchFactor:F2}, gốc Z={FrontRoomNorthZ}) cho khớp phòng mở rộng.");
            }

            // Xây lại từ đầu -- xoá bản build lỗi (tường đôi/méo do transform cha) từ lần chạy trước nếu có.
            string holderName = $"Walls_Rebuilt_{rb.name}";
            var oldHolder = GameObject.Find(holderName);
            if (oldHolder != null) Undo.DestroyObjectImmediate(oldHolder);

            // Tắt TOÀN BỘ object tường cũ (mesh liền khối lẫn module rời).
            // KHÔNG lấy material từ tường cũ nữa -- phát hiện hầu hết tường cũ các
            // phòng (Wall_LamViec, Wall_GiaDinh, Wall_Khach, Wall_An...) đều bị gán
            // NHẦM material Mat_Wall_Exterior_Ochre (màu vàng dành cho tường NGOÀI
            // nhà) từ trước, chỉ riêng Wall_Sanh (TienSanh) là đúng màu cream -- lấy
            // theo tường cũ vô tình lan luôn lỗi màu đó sang tường mới. Tường mới
            // giờ tự chọn material THEO TỪNG MẶT (interior/exterior tính tự động ở
            // trên), không dựa vào tường cũ nữa.
            var allT = room.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var t in allT)
            {
                if (t == room.transform || !WallRegex.IsMatch(t.name)) continue;
                var rend = t.GetComponent<Renderer>();
                if (rend == null) continue;
                if (rend.enabled) { rend.enabled = false; oldWallsHidden++; }
                var col = t.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }

            // QUAN TRỌNG: KHÔNG parent vào room -- root của phòng (sau bao lần fix
            // scale/rotation trong ngày) có thể mang rotation + scale KHÔNG ĐỀU cùng
            // lúc (VD PhongAn: xoay 270° + scale (1.196,1.095,1.196)), khiến box tường
            // con dùng localScale tuyệt đối bị BIẾN DẠNG theo cha (không còn đúng mét
            // thật) -- đây là lý do tường ẩn/méo không đều giữa các phòng bấy lâu.
            // Xây ở container trung lập (transform identity, đứng riêng ở gốc scene)
            // để toạ độ/kích thước tuyệt đối luôn đúng, không phụ thuộc phòng.
            var holder = new GameObject(holderName);
            Undo.RegisterCreatedObjectUndo(holder, "VoD Rebuild Room Walls");
            holder.transform.SetParent(GetOrCreateVillaWallsRoot(), worldPositionStays: true);

            wallsBuilt += VoD_WallBuilder.BuildRoomWalls(holder.transform, rb.cornerX, rb.cornerZ, rb.w, rb.d, rb.h,
                openingsByRoom[rb.name], intMat, extMat, WallThickness, skipSidesByRoom[rb.name], exteriorSidesByRoom[rb.name]);
            Debug.Log($"[VoD] '{rb.name}': xây {openingsByRoom[rb.name].Count} lỗ cửa/cửa sổ, bỏ {skipSidesByRoom[rb.name].Count} mặt (phòng liền kề đã xây), " +
                       $"{exteriorSidesByRoom[rb.name].Count} mặt ngoài (ochre) trong 4 mặt, tắt tường cũ.");
        }

        // Tầng 1 + Tầng 2: CHỈ tường BAO QUANH (vỏ ngoài), KHÔNG chia phòng trong
        // (Jok xác nhận: "tường bao quanh chứ không cần tường phía trong phân") --
        // 1 hộp rỗng full diện tích khối chính (X:3-30, Z:5.5-24.5, KHÔNG gồm
        // Véranda -- tầng trên không trùm lên phần 1 tầng đó), né khu vực giếng
        // trời cầu thang (CauThang cũ, X:12-21, Z:12.7-17.5) bằng cách KHOÉT 1 lỗ
        // to đúng bằng khu đó ở CẢ 4 mặt tường?? KHÔNG -- lỗ giếng trời nằm ở SÀN/
        // TRẦN (xem VoD_RebuildRoomFloors), tường bao quanh (4 mặt ngoài) không hề
        // đụng khu giếng trời (nó nằm giữa nhà, không giáp tường ngoài nào) nên
        // không cần khoét gì cho tường bao 2 tầng trên.
        BuildUpperFloorShell("Floor1", FloorToFloorHeight);
        BuildUpperFloorShell("Floor2", FloorToFloorHeight * 2f);

        // Tháp canh (GDD: "~12m², Góc đông, Tour de guet, safe zone, view 360°") --
        // đặt ở góc Đông-Bắc (giáp ThuPhong), nhô LÊN TRÊN mái tầng 2 (Y=12.6),
        // cao thêm 1 tầng nữa. Placeholder khô sơ -- vỏ rỗng, chưa nội thất.
        BuildWatchtowerShell();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.Refresh();
        Debug.Log($"[VoD] Xong -- tổng {wallsBuilt} khối tường mới (không còn chồng đôi, không còn méo theo transform cha), {oldWallsHidden} object tường cũ đã tắt (không xoá) ở 8 phòng khối chính " +
                   "+ vỏ tường bao Floor1/Floor2 (có khung cửa sổ khô sơ mỗi mặt) + tháp canh. Nhớ Ctrl+S. Nội thất KHÔNG bị đụng -- tự move thủ công theo layout cũ.");
    }

    // Footprint khối chính (không gồm Véranda) -- PHẢI khớp VoD_RebuildRoomFloors
    // (MainMinX/MaxX/MinZ/MaxZ) -- copy thủ công theo đúng convention đã dùng.
    private const float MainBlockMinX = 3.0f, MainBlockMaxX = 30.0f;
    private const float MainBlockMinZ = 5.5f, MainBlockMaxZ = 24.5f;
    private const float FloorToFloorHeight = 4.2f;

    // Cửa sổ khô sơ (Jok: "kèm khung cửa sổ sẵn lõm trên tường xung quanh nhà") --
    // MỖI mặt (Bắc/Nam/Đông/Tây) 1 lỗ đặt giữa mặt đó -- placeholder rough, chưa
    // phải layout thật (WallBuilder hiện chỉ hỗ trợ 1 lỗ/1 mặt -- team dùng
    // ProBuilder khoét thêm lỗ thật theo layout phòng khi thiết kế chi tiết).
    //
    // SỬA theo Jok: mặt Bắc/Nam có ban công (xem VoD_RebuildRoomFloors BuildBalcony)
    // -- lỗ ở 2 mặt đó phải là CỬA PHÁP SÁT SÀN (sillY=0) để đi ra ban công được,
    // không phải cửa sổ có bậu cao (đứng trong nhà không bước ra ngoài được). Mặt
    // Đông/Tây không có ban công, giữ nguyên dạng cửa sổ bậu cao như cũ.
    private const float PlaceholderWindowWidth = 1.6f, PlaceholderWindowSill = 1.0f, PlaceholderWindowHeight = 1.6f;
    private const float BalconyDoorWidth = 2.2f, BalconyDoorHeight = 2.4f; // sillY=0 (sát sàn)

    // 1 hộp rỗng full diện tích, có 1 lỗ khô sơ mỗi mặt (cửa ra ban công ở Bắc/
    // Nam, cửa sổ ở Đông/Tây), KHÔNG chia phòng -- vỏ placeholder cho Jok/team
    // ước lượng tỉ lệ trước khi thiết kế layout thật. Xây ở cao độ 0-4.2 như bình
    // thường rồi dịch nguyên khối holder lên đúng elevationY (đơn giản hơn sửa
    // VoD_WallBuilder để nhận thêm tham số Y gốc).
    private static void BuildUpperFloorShell(string levelName, float elevationY)
    {
        string holderName = $"Walls_Rebuilt_{levelName}_Shell";
        var oldHolder = GameObject.Find(holderName);
        if (oldHolder != null) Undo.DestroyObjectImmediate(oldHolder);

        var holder = new GameObject(holderName);
        Undo.RegisterCreatedObjectUndo(holder, "VoD Build Upper Floor Shell");
        holder.transform.SetParent(GetOrCreateVillaWallsRoot(), worldPositionStays: true);

        var extMat = AssetDatabase.LoadAssetAtPath<Material>(ExteriorWallMatPath);
        var allExterior = new HashSet<string> { "West", "East", "North", "South" };
        float midX = (MainBlockMinX + MainBlockMaxX) * 0.5f, midZ = (MainBlockMinZ + MainBlockMaxZ) * 0.5f;
        var windows = new List<OpeningModel>
        {
            new OpeningModel { side = "North", posAlong = midX, width = BalconyDoorWidth, sillY = 0f, openHeight = BalconyDoorHeight },
            new OpeningModel { side = "South", posAlong = midX, width = BalconyDoorWidth, sillY = 0f, openHeight = BalconyDoorHeight },
            new OpeningModel { side = "East",  posAlong = midZ, width = PlaceholderWindowWidth, sillY = PlaceholderWindowSill, openHeight = PlaceholderWindowHeight, isWindow = true },
            new OpeningModel { side = "West",  posAlong = midZ, width = PlaceholderWindowWidth, sillY = PlaceholderWindowSill, openHeight = PlaceholderWindowHeight, isWindow = true },
        };
        int count = VoD_WallBuilder.BuildRoomWalls(holder.transform, MainBlockMinX, MainBlockMinZ,
            MainBlockMaxX - MainBlockMinX, MainBlockMaxZ - MainBlockMinZ, FloorToFloorHeight,
            windows, extMat, extMat, WallThickness, skipSides: null, exteriorSides: allExterior);

        holder.transform.position += new Vector3(0f, elevationY, 0f);
        Debug.Log($"[VoD] '{levelName}': xây {count} khối tường bao (placeholder, không chia phòng, cửa ra ban công sát sàn ở Bắc/Nam + cửa sổ ở Đông/Tây) ở cao độ Y={elevationY}-{elevationY + FloorToFloorHeight}.");
    }

    // Tháp canh: đặt góc Đông-Bắc (giáp ThuPhong, X:27-30) -- 3.5m x 3.5m
    // (~12m² đúng số GDD), nhô lên trên mái tầng 2 (Y=12.6), cao thêm 1 tầng.
    private const float TowerMinX = 26.5f, TowerMaxX = 30.0f; // 3.5m
    private const float TowerMinZ = 5.5f, TowerMaxZ = 9.0f;   // 3.5m
    private static void BuildWatchtowerShell()
    {
        string holderName = "Walls_Rebuilt_ThapCanh_Shell";
        var oldHolder = GameObject.Find(holderName);
        if (oldHolder != null) Undo.DestroyObjectImmediate(oldHolder);

        var holder = new GameObject(holderName);
        Undo.RegisterCreatedObjectUndo(holder, "VoD Build Watchtower Shell");
        holder.transform.SetParent(GetOrCreateVillaWallsRoot(), worldPositionStays: true);

        var extMat = AssetDatabase.LoadAssetAtPath<Material>(ExteriorWallMatPath);
        var allExterior = new HashSet<string> { "West", "East", "North", "South" };
        // View 360 độ (GDD) -- cửa sổ rộng cả 4 mặt thay vì 1 lỗ nhỏ.
        float midX = (TowerMinX + TowerMaxX) * 0.5f, midZ = (TowerMinZ + TowerMaxZ) * 0.5f;
        var windows = new List<OpeningModel>
        {
            new OpeningModel { side = "North", posAlong = midX, width = 2.2f, sillY = 0.9f, openHeight = 1.8f, isWindow = true },
            new OpeningModel { side = "South", posAlong = midX, width = 2.2f, sillY = 0.9f, openHeight = 1.8f, isWindow = true },
            new OpeningModel { side = "East",  posAlong = midZ, width = 2.2f, sillY = 0.9f, openHeight = 1.8f, isWindow = true },
            new OpeningModel { side = "West",  posAlong = midZ, width = 2.2f, sillY = 0.9f, openHeight = 1.8f, isWindow = true },
        };
        int count = VoD_WallBuilder.BuildRoomWalls(holder.transform, TowerMinX, TowerMinZ,
            TowerMaxX - TowerMinX, TowerMaxZ - TowerMinZ, FloorToFloorHeight,
            windows, extMat, extMat, WallThickness, skipSides: null, exteriorSides: allExterior);

        // ĐÃ SỬA: dịch lên FloorToFloorHeight*3 (12.6), KHÔNG PHẢI *2 (8.4) -- *2 sẽ
        // trùng cao độ với chính tường tầng 2 (Y:8.4-12.6, xem BuildUpperFloorShell),
        // tháp canh phải đứng TRÊN mái tầng 2 (Y=12.6 trở lên), không đè lên tầng 2.
        holder.transform.position += new Vector3(0f, FloorToFloorHeight * 3f, 0f);
        Debug.Log($"[VoD] Tháp canh: xây {count} khối tường (placeholder, cửa sổ 360°) ở góc Đông-Bắc, cao độ Y={FloorToFloorHeight * 3f}-{FloorToFloorHeight * 4f}.");
    }

    // Container trung lập đứng ở gốc scene, LUÔN transform identity (position 0,
    // rotation 0, scale 1) -- nơi chứa mọi Walls_Rebuilt_<Phòng>, tách biệt hoàn
    // toàn khỏi transform (có thể méo) của từng phòng.
    private static Transform GetOrCreateVillaWallsRoot()
    {
        var go = GameObject.Find("VoD_Villa_Walls_Root");
        if (go == null)
        {
            go = new GameObject("VoD_Villa_Walls_Root");
            Undo.RegisterCreatedObjectUndo(go, "VoD Create Villa Walls Root");
        }
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

}
