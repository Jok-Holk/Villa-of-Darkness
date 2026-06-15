using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Đặt model kiến trúc chính xác theo toán học dựa trên spatial scan villa.
/// Menu: VoD > Villa > Fix Architecture
/// </summary>
public static class VillaArchitectureFix
{
    // ─── Building constants (đo THẬT + Jok xác nhận hướng, 2026-06-15) ────────
    // Footprint GroundFloor: X[10.7 .. 52.8]  Z[8.0 .. 47.2]  tâm X=31.7 Z=27.6
    // MẶT TIỀN THẬT = mặt -X (X≈10.7) — có ban công/loggia + lối vào dựng sẵn,
    //   ĐỐI DIỆN nhà bếp phụ + giếng (ở +X). Trục nhà chạy dọc X.
    //   → cửa mặt tiền/sau phân bố theo Z, đối xứng quanh CENTER_Z=27.6.

    const float FRONT_X    = 10.7f;   // MẶT TIỀN (-X), cửa quay -X (rotY=90)
    const float BACK_X     = 52.8f;   // mặt sau (+X, phía bếp/giếng), quay +X (rotY=270)
    const float SIDE_ZLOW  = 7.78f;   // hông -Z (rotY=180)
    const float SIDE_ZHIGH = 47.56f;  // hông +Z (rotY=0)

    const float CENTER_X   = 31.7f;   // trục đối xứng hai hông
    const float CENTER_Z   = 27.6f;   // trục đối xứng MẶT TIỀN/SAU (cửa phân bố theo Z)

    // Đặt cửa hơi ngoài mặt tường để mặt kính lộ ra (flush + 0.3m)
    const float FACE_OFFSET = 0.3f;

    // Floor Y values (surface of each floor) — đã xác nhận đúng từ scan
    const float Y_GROUND   = 32.5f;
    const float Y_1ST      = 40.0f;
    const float Y_2ND      = 47.5f;

    // Balcony/galerie front railing Y (mặt tiền -X)
    const float BALCONY_RAILING_Y_1F  = 40.8f;
    const float BALCONY_RAILING_Y_2F  = 48.3f;
    const float RAILING_STEP = 2.0f;        // 1 piece mỗi 2m

    // Cửa chính — giữa mặt tiền -X (Z=CENTER_Z). KHÔNG đặt cửa sổ ở bay này.
    const float ENTRANCE_Z = CENTER_Z;

    // ─── Layout đối xứng theo PHÒNG (villa, không phải chung cư) ──────────────
    // Mặt tiền/sau (rộng ~39m theo Z): 3 bay mỗi bên + bay giữa (cửa chính/ban công)
    static readonly float[] FRONT_OFFSETS = { 5f, 11f, 17f };
    // Hông (rộng ~42m theo X): 4 cửa/tầng
    static readonly float[] SIDE_OFFSETS  = { 8f, 16f };

    // Asset paths
    const string ARCH   = "Assets/_Project/Models/Props/Architecture/";
    const string FURN   = "Assets/_Project/Models/Props/Furniture/";
    const string NATURE = "Assets/_Project/Models/Props/Nature/";

    // FBX model scales (Blender cm-export: at scale 1 door = 0.19m×1.0m×0.63m)
    // Target door: 1.1m wide × 2.3m tall × 0.16m deep
    static readonly Vector3 DOOR_SCALE      = new Vector3(5.8f, 2.3f, 0.25f);
    static readonly Vector3 DOOR_MAIN_SCALE = new Vector3(10f,  2.3f, 0.25f);  // grand entrance
    // Pivot at center of model → lift by half scaled height so bottom sits on floor
    const float DOOR_Y_OFFSET = 1.15f;  // 0.998f/2 * 2.3f

    // Blockout có tầng cao ~7.5m → cửa phải cao theo để giữ TỶ LỆ lịch sử (~55-60% tường).
    // Cửa sổ jalousie tầng trên: ~1.3m rộng × 3.4m cao (GLB bounds 0.2156×1.0006×0.8170)
    static readonly Vector3 WIN_SCALE    = new Vector3(6.0f, 3.4f, 0.18f);
    // Cửa kính kiểu Pháp tầng trệt (mở ra galerie): cao gần trần, bệ sát sàn
    static readonly Vector3 WIN_SCALE_GF = new Vector3(6.6f, 4.0f, 0.18f);
    const float WIN_CENTER_UPPER = 2.20f;  // tâm cửa = sàn + 2.20 (bệ ~0.5m, cao 3.4m)
    const float WIN_CENTER_GF    = 2.10f;  // tâm cửa Pháp = sàn + 2.10 (bệ sát sàn, cao 4.0m)

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/1 - Fix Windows (Jalousie)")]
    public static void PlaceWindows()
    {
        // Xoá group cũ để không còn cửa "mẫu"/lệch sót lại
        var existing = GameObject.Find("_Windows_Jalousie");
        if (existing != null) Undo.DestroyObjectImmediate(existing);
        var group = GetOrCreate("_Windows_Jalousie", null);

        float frontX = FRONT_X - FACE_OFFSET;    // ra ngoài mặt tiền -X
        float backX  = BACK_X  + FACE_OFFSET;
        float sideZA = SIDE_ZLOW  - FACE_OFFSET; // hông -Z
        float sideZB = SIDE_ZHIGH + FACE_OFFSET; // hông +Z

        // Mặt tiền/sau: cửa phân bố theo Z, đối xứng quanh CENTER_Z, bỏ bay giữa (cửa chính)
        var frontZs = Sym(CENTER_Z, FRONT_OFFSETS);
        // Hông: cửa phân bố theo X, đối xứng quanh CENTER_X
        var sideXs  = Sym(CENTER_X, SIDE_OFFSETS);

        // Tầng trệt = cửa kính Pháp cao (ra galerie); tầng trên = jalousie chuẩn
        var floors = new (string tag, float y, Vector3 scale)[] {
            ("GF", Y_GROUND + WIN_CENTER_GF,    WIN_SCALE_GF),
            ("1F", Y_1ST    + WIN_CENTER_UPPER, WIN_SCALE),
            ("2F", Y_2ND    + WIN_CENTER_UPPER, WIN_SCALE),
        };

        foreach (var (tag, y, scale) in floors)
        {
            // Mặt tiền (-X, quay -X) — bỏ bay trục giữa (cửa chính + ban công dựng sẵn)
            foreach (float z in frontZs)
                PlaceWindow(group, frontX, y, z, Quaternion.Euler(0, 90, 0), $"Win_{tag}_Front_{(int)z}", scale);

            // Mặt sau (+X, quay +X)
            foreach (float z in frontZs)
                PlaceWindow(group, backX, y, z, Quaternion.Euler(0, -90, 0), $"Win_{tag}_Back_{(int)z}", scale);

            // Hông -Z (quay -Z) / hông +Z (quay +Z) — đối xứng nhau
            foreach (float x in sideXs)
            {
                PlaceWindow(group, x, y, sideZA, Quaternion.Euler(0, 180, 0), $"Win_{tag}_SideA_{(int)x}", scale);
                PlaceWindow(group, x, y, sideZB, Quaternion.Euler(0, 0, 0),   $"Win_{tag}_SideB_{(int)x}", scale);
            }
        }

        Debug.Log($"[VoD] Placed {group.transform.childCount} windows (front=-X, room-based bays).");
        MarkDirty();
    }

    /// <summary>Sinh vị trí đối xứng quanh tâm từ list offsets (cả + và -), đã sort.</summary>
    static List<float> Sym(float center, float[] offsets)
    {
        var r = new List<float>();
        foreach (float o in offsets)
        {
            if (Mathf.Approximately(o, 0f)) { r.Add(center); continue; }
            r.Add(center - o);
            r.Add(center + o);
        }
        r.Sort();
        return r;
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/2 - Fix Balcony Railings")]
    public static void PlaceRailings()
    {
        var existing = GameObject.Find("_Railings_Balcony");
        if (existing != null) Undo.DestroyObjectImmediate(existing);
        var group = GetOrCreate("_Railings_Balcony", null);

        // Galerie/ban công chạy dọc mặt tiền -X (theo trục Z), đối xứng quanh CENTER_Z.
        float railX  = FRONT_X - 0.5f;     // hơi ngoài mặt tiền
        float zStart = CENTER_Z - 18f;     // ≈9.6
        float zEnd   = CENTER_Z + 18f;     // ≈45.6

        // 1F front railing (quay -X)
        PlaceRailingRowZ(group, zStart, zEnd, BALCONY_RAILING_Y_1F, railX,
                         Quaternion.Euler(0, 0, 0), "Railing_1F_Front");

        // 2F front railing
        PlaceRailingRowZ(group, zStart, zEnd, BALCONY_RAILING_Y_2F, railX,
                         Quaternion.Euler(0, 0, 0), "Railing_2F_Front");

        Debug.Log($"[VoD] Placed {group.transform.childCount} railing sections (front -X galerie).");
        MarkDirty();
    }

    /// <summary>Đặt row railing dọc trục Z (cho mặt tiền -X).</summary>
    static void PlaceRailingRowZ(GameObject parent, float zStart, float zEnd,
                                  float y, float x, Quaternion rot, string prefix)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH + "Arch_Railing_Balcony.glb");
        if (asset == null) { Debug.LogWarning("Arch_Railing_Balcony.glb not found"); return; }
        int i = 0;
        for (float z = zStart; z <= zEnd + 0.1f; z += RAILING_STEP, i++)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            go.name = $"{prefix}_{i:00}";
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = rot;
            go.transform.SetParent(parent.transform, true);
            Undo.RegisterCreatedObjectUndo(go, go.name);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/3 - Place Interior Doors")]
    public static void PlaceDoors()
    {
        var existing = GameObject.Find("_Doors_Interior");
        if (existing != null) Undo.DestroyObjectImmediate(existing);
        var group = GetOrCreate("_Doors_Interior", null);

        // Main entrance — giữa mặt tiền -X (Z=27.6), cửa quay -X
        PlaceDoor(group, FRONT_X - FACE_OFFSET, Y_GROUND, ENTRANCE_Z,
                  Quaternion.Euler(0, 90, 0), "Door_MainEntrance",
                  ARCH + "Arch_Door_Interior.glb", isMain: true);

        // Ground floor interior doors (between rooms based on room positions)
        // Living Room → Hallway (X≈22, Z≈35)
        PlaceDoor(group, 22f, Y_GROUND, 35f, Quaternion.Euler(0, 0, 0),   "Door_GF_LivingToHall",   ARCH + "Arch_Door_Interior.glb");
        // Dining Room → Hallway
        PlaceDoor(group, 38f, Y_GROUND, 35f, Quaternion.Euler(0, 0, 0),   "Door_GF_DiningToHall",   ARCH + "Arch_Door_Interior.glb");
        // Study → Hallway
        PlaceDoor(group, 16f, Y_GROUND, 22f, Quaternion.Euler(0, 90, 0),  "Door_GF_StudyToHall",    ARCH + "Arch_Door_Interior.glb");
        // Kitchen → Dining
        PlaceDoor(group, 63f, Y_GROUND, 41f, Quaternion.Euler(0, 90, 0),  "Door_GF_KitchenEntry",   ARCH + "Arch_Door_Interior.glb");

        // 1F interior doors
        PlaceDoor(group, 15f, Y_1ST, 35f, Quaternion.Euler(0, 90, 0),     "Door_1F_MasterBed",      ARCH + "Arch_Door_Interior.glb");
        PlaceDoor(group, 32f, Y_1ST, 38f, Quaternion.Euler(0, 0, 0),      "Door_1F_MrsLanRoom",     ARCH + "Arch_Door_Interior.glb");
        PlaceDoor(group, 50f, Y_1ST, 25f, Quaternion.Euler(0, 90, 0),     "Door_1F_GuestRoom",      ARCH + "Arch_Door_Interior.glb");

        // Storage door (with padlock in game)
        PlaceDoor(group, 28f, Y_GROUND, 28f, Quaternion.Euler(0, 0, 0),   "Door_Storage_Locked",    ARCH + "Arch_Door_StorageClean.glb");

        Debug.Log($"[VoD] Placed {group.transform.childCount} doors.");
        MarkDirty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/4 - Place Gate & Well")]
    public static void PlaceGateAndWell()
    {
        var existing = GameObject.Find("_Exterior_Arch");
        if (existing != null) Undo.DestroyObjectImmediate(existing);
        var group = GetOrCreate("_Exterior_Arch", null);

        // Cổng trước — sân -X, trên trục cửa chính (Z=27.6), mở theo hướng vào +X
        var gate = LoadAndPlace(ARCH + "Arch_Gate_Colonial.glb", "Gate_Colonial_Main",
                                new Vector3(3f, 32.5f, CENTER_Z), Quaternion.Euler(0, 90, 0), Vector3.one);
        if (gate) gate.transform.SetParent(group.transform, true);

        // Giếng — phía +X (sân sau/gia nhân), giữ nguyên vị trí có sẵn
        var well = LoadAndPlace(ARCH + "Arch_Well_Stone.glb", "Well_Stone_Model",
                                new Vector3(58.0f, 32.5f, 27.0f), Quaternion.Euler(0, 45, 0), Vector3.one);
        if (well) well.transform.SetParent(group.transform, true);

        // Rèm rách hai bên sảnh — ngay sau cửa chính -X, đối xứng quanh trục Z giữa
        var curtain1 = LoadAndPlace(ARCH + "Prop_Curtain_Torn.glb", "Curtain_Entrance_L",
                                    new Vector3(FRONT_X + 0.8f, 35f, CENTER_Z - 4f), Quaternion.Euler(0, 90, 0), Vector3.one);
        if (curtain1) curtain1.transform.SetParent(group.transform, true);

        var curtain2 = LoadAndPlace(ARCH + "Prop_Curtain_Torn.glb", "Curtain_Entrance_R",
                                    new Vector3(FRONT_X + 0.8f, 35f, CENTER_Z + 4f), Quaternion.Euler(0, 90, 0), Vector3.one);
        if (curtain2) curtain2.transform.SetParent(group.transform, true);

        Debug.Log("[VoD] Gate, well, exterior elements placed.");
        MarkDirty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/6b - Clear and Redo Exterior Decor")]
    public static void ClearAndRedoExteriorDecor()
    {
        var old = GameObject.Find("_Exterior_Decor");
        if (old != null) { Undo.DestroyObjectImmediate(old); }
        AddExteriorDecor();
    }

    [MenuItem("VoD/Villa/6 - Add Exterior Decor")]
    public static void AddExteriorDecor()
    {
        var existing = GameObject.Find("_Exterior_Decor");
        if (existing != null) Undo.DestroyObjectImmediate(existing);
        // Dọn luôn các placeholder cũ (perron/đèn/chậu/lối đá) nếu còn sót
        var oldPerron = GameObject.Find("_Perron");
        if (oldPerron != null) Undo.DestroyObjectImmediate(oldPerron);
        var group = GetOrCreate("_Exterior_Decor", null);

        // CHỈ dùng model GLB thật (cây/bụi Kenney). Không còn primitive placeholder.
        // Mặt tiền = -X; bố cục đối xứng quanh trục cửa CENTER_Z=27.6.

        // ── Cây cổ thụ hai góc sân trước -X (đối xứng quanh Z=27.6) ──────
        AddVegetation(group, 3.5f, Y_GROUND, CENTER_Z - 15f, 1.4f, 4.5f, "Tree_FrontL");
        AddVegetation(group, 3.5f, Y_GROUND, CENTER_Z + 15f, 1.4f, 4.5f, "Tree_FrontR");

        // ── Bụi cây nền móng dọc mặt tiền -X (đối xứng) ──────────────────
        AddVegetation(group, FRONT_X - 1.2f, Y_GROUND, CENTER_Z - 12f, 0.9f, 1.6f, "Bush_FL");
        AddVegetation(group, FRONT_X - 1.2f, Y_GROUND, CENTER_Z + 12f, 0.9f, 1.6f, "Bush_FR");

        // ── Cây bóng mát hai hông (đối xứng quanh tâm Z) ─────────────────
        AddVegetation(group, CENTER_X, Y_GROUND, SIDE_ZLOW  - 4f, 1.3f, 4.0f, "Tree_SideA");
        AddVegetation(group, CENTER_X, Y_GROUND, SIDE_ZHIGH + 4f, 1.3f, 4.0f, "Tree_SideB");

        Debug.Log("[VoD] Exterior decor placed (GLB only, front=-X, symmetric).");
        MarkDirty();
    }

    // ── Exterior model placer (chỉ GLB thật, không primitive) ─────────────────

    static void AddVegetation(GameObject parent, float x, float y, float z,
                               float radius, float height, string name)
    {
        // Try actual low-poly models from Nature/ folder first (Kenney Nature Kit GLB)
        // Place downloaded GLBs as: Tree_Large.glb, Tree_Small.glb, Bush_Small.glb, Bush_Large.glb
        bool isTall = height > 2f;
        string modelKey = isTall ? (radius > 1f ? "Tree_Large" : "Tree_Small")
                                 : (radius > 0.8f ? "Bush_Large" : "Bush_Small");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NATURE + modelKey + ".glb");
        if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NATURE + modelKey + ".fbx");

        if (prefab != null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.position = new Vector3(x, y, z);
            // Kenney models native height ~1.3m at scale 1; multiply to reach target height
            go.transform.localScale = Vector3.one * (height * 1.2f);
            go.transform.SetParent(parent.transform, true);
            Undo.RegisterCreatedObjectUndo(go, name);
            return;
        }

        // Fallback: Unity primitive placeholder
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.position = new Vector3(x, y + height * 0.5f, z);
        sphere.transform.localScale = new Vector3(radius * 2f, height, radius * 2f);
        sphere.transform.SetParent(parent.transform, true);
        Undo.RegisterCreatedObjectUndo(sphere, name);

        if (isTall)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name + "_Trunk";
            trunk.transform.position = new Vector3(x, y + height * 0.2f, z);
            trunk.transform.localScale = new Vector3(0.15f, height * 0.25f, 0.15f);
            trunk.transform.SetParent(parent.transform, true);
            Undo.RegisterCreatedObjectUndo(trunk, name + "_Trunk");
        }
    }


    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/5 - Rename Hierarchy")]
    public static void RenameHierarchy()
    {
        var renames = new Dictionary<string, string>
        {
            { "ground floor",    "GroundFloor"       },
            { "1st floor",       "FirstFloor"        },
            { "2nd floor",       "SecondFloor"       },
            { "BASEMENT",        "Basement"          },
            { "OUTBUILDING",     "KitchenWing"       },
            { "STAIR (1)",       "Staircase_Main"    },
            { "STAIR (2)",       "Staircase_Side"    },
            { "stair fence",     "StairRailing_A"    },
            { "stair fence (1)", "StairRailing_B"    },
            { "folding ladder",  "LadderFolding"     },
            { "ladder (3)",      "LadderFixed"       },
            { "Watch Tower",     "WatchTower"        },
            { "well",            "Well"              },
            { "Front yard",      "FrontYard"         },
            { "Stone walkway",   "StonePath"         },
            { "Wall, column",    "StructuralCore"    },
            { "Floor",           "FloorGeometry"     },
            { "' house pillar'", "SupportPillars"    },
        };

        int count = 0;
        foreach (var go in Object.FindObjectsOfType<GameObject>())
        {
            if (renames.TryGetValue(go.name, out var newName) && go.transform.parent == null)
            {
                Undo.RecordObject(go, "Rename " + go.name);
                go.name = newName;
                count++;
            }
        }
        Debug.Log($"[VoD] Renamed {count} hierarchy items.");
        MarkDirty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/0 - Run All Fixes")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("VoD Architecture Fix",
            "Chạy toàn bộ:\n1. Jalousie windows\n2. Balcony railings\n3. Interior doors\n4. Gate & well\n5. Rename hierarchy\n6. Exterior decor\n\nĐảm bảo Chapter1 đang mở.",
            "Run All", "Cancel")) return;

        PlaceWindows();
        PlaceRailings();
        PlaceDoors();
        PlaceGateAndWell();
        AddExteriorDecor();
        RenameHierarchy();
    }

    // ─── Math helpers ─────────────────────────────────────────────────────────

    /// <summary>Đặt row của railing dọc theo trục X.</summary>
    static void PlaceRailingRow(GameObject parent, float xStart, float xEnd,
                                 float y, float z, Quaternion rot, string prefix)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH + "Arch_Railing_Balcony.glb");
        if (asset == null) { Debug.LogWarning("Arch_Railing_Balcony.glb not found"); return; }

        int i = 0;
        for (float x = xStart; x <= xEnd + 0.1f; x += RAILING_STEP, i++)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            go.name = $"{prefix}_{i:00}";
            go.transform.position = new Vector3(x, y, z);
            go.transform.rotation = rot;
            go.transform.SetParent(parent.transform, true);
            Undo.RegisterCreatedObjectUndo(go, go.name);
        }
    }

    static void PlaceWindow(GameObject parent, float x, float y, float z, Quaternion rot, string goName, Vector3 scale)
    {
        var go = LoadAndPlace(ARCH + "Arch_Window_Jalousie.glb", goName,
                              new Vector3(x, y, z), rot, scale);
        if (go) go.transform.SetParent(parent.transform, true);
    }

    static void PlaceDoor(GameObject parent, float x, float y, float z,
                           Quaternion rot, string goName, string assetPath, bool isMain = false)
    {
        var scale = isMain ? DOOR_MAIN_SCALE : DOOR_SCALE;
        var go = LoadAndPlace(assetPath, goName, new Vector3(x, y + DOOR_Y_OFFSET, z), rot, scale);
        if (go) go.transform.SetParent(parent.transform, true);
    }

    static GameObject LoadAndPlace(string assetPath, string goName, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null && assetPath.EndsWith(".glb"))
        {
            // Try same-name .fbx first
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath.Replace(".glb", ".fbx"));
            // FBX files drop the "Arch_" prefix (e.g. Arch_Door_Interior.glb → Door_Interior.fbx)
            if (prefab == null)
            {
                var dir   = System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/') + "/";
                var fname = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                if (fname.StartsWith("Arch_"))
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(dir + fname.Substring(5) + ".fbx");
            }
        }
        if (prefab == null)
        {
            Debug.LogWarning($"[VoD] Not found: {assetPath}");
            return null;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = goName;
        go.transform.position = pos;
        go.transform.rotation = rot;
        go.transform.localScale = scale;
        Undo.RegisterCreatedObjectUndo(go, "Place " + goName);
        return go;
    }

    static GameObject GetOrCreate(string name, GameObject parent)
    {
        var found = GameObject.Find(name);
        if (found) return found;
        var go = new GameObject(name);
        if (parent) go.transform.SetParent(parent.transform, false);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go;
    }

    static void MarkDirty() =>
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
}
