using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Đặt model kiến trúc chính xác theo toán học dựa trên spatial scan villa.
/// Menu: VoD > Villa > Fix Architecture
/// </summary>
public static class VillaArchitectureFix
{
    // ─── Building constants (từ spatial scan) ─────────────────────────────────
    // Front facade: Z ≈ 22.0 (wall outer surface, facing -Z = outward)
    // Left facade:  X ≈ 6.5  (wall outer surface, facing -X = outward)
    // Right facade: X ≈ 46.5
    // Back facade:  Z ≈ 44.0

    const float FRONT_Z    = 21.8f;
    const float LEFT_X     = 6.5f;
    const float RIGHT_X    = 46.5f;
    const float BACK_Z     = 44.0f;

    // Floor Y values (surface of each floor)
    const float Y_GROUND   = 32.5f;
    const float Y_1ST      = 40.0f;
    const float Y_2ND      = 47.5f;

    // Window center height above floor
    const float WIN_HEIGHT_OFFSET = 2.0f;   // center of window = floor + 2m
    const float WIN_DEPTH_INTO_WALL = 0.0f; // flush with outer face

    // Balcony front railing edge (from balcony scan: Z=23.2 at Y=40.8)
    const float BALCONY_RAILING_Z_1F  = 23.2f;
    const float BALCONY_RAILING_Y_1F  = 40.8f;
    const float BALCONY_RAILING_Z_2F  = 23.8f;
    const float BALCONY_RAILING_Y_2F  = 48.6f;
    const float BALCONY_RAILING_X_START = 23.5f;
    const float BALCONY_RAILING_X_END   = 45.5f;
    const float RAILING_STEP = 2.0f;        // 1 piece mỗi 2m

    // Main entrance X (no window here)
    const float ENTRANCE_X = 28.9f;
    const float ENTRANCE_CLEAR = 3.0f; // clear 3m either side of door

    // Asset paths
    const string ARCH = "Assets/_Project/Models/Props/Architecture/";
    const string FURN = "Assets/_Project/Models/Props/Furniture/";

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/1 - Fix Windows (Jalousie)")]
    public static void PlaceWindows()
    {
        var group = GetOrCreate("_Windows_Jalousie", null);

        // Ground floor front windows — avoid entrance zone
        var gfFrontX = CalcWindowPositions(LEFT_X + 2f, RIGHT_X - 2f, spacing: 5.5f, avoidX: ENTRANCE_X, avoidClear: ENTRANCE_CLEAR);
        foreach (float wx in gfFrontX)
            PlaceWindow(group, wx, Y_GROUND + WIN_HEIGHT_OFFSET, FRONT_Z, Quaternion.Euler(0, 180, 0), $"Win_GF_Front_{(int)wx}");

        // 1st floor front windows (balcony → window behind railing)
        var f1FrontX = CalcWindowPositions(LEFT_X + 2f, RIGHT_X - 2f, spacing: 4.5f, avoidX: ENTRANCE_X, avoidClear: 2f);
        foreach (float wx in f1FrontX)
            PlaceWindow(group, wx, Y_1ST + WIN_HEIGHT_OFFSET, FRONT_Z + 1.0f, Quaternion.Euler(0, 180, 0), $"Win_1F_Front_{(int)wx}");

        // 2nd floor front windows
        var f2FrontX = CalcWindowPositions(LEFT_X + 2f, RIGHT_X - 2f, spacing: 4.5f, avoidX: ENTRANCE_X, avoidClear: 2f);
        foreach (float wx in f2FrontX)
            PlaceWindow(group, wx, Y_2ND + WIN_HEIGHT_OFFSET, FRONT_Z + 1.0f, Quaternion.Euler(0, 180, 0), $"Win_2F_Front_{(int)wx}");

        // Left-side windows (X face, facing -X)
        float[] sideZ = { 27f, 32f, 37f, 42f };
        foreach (float sz in sideZ)
        {
            PlaceWindow(group, LEFT_X, Y_GROUND + WIN_HEIGHT_OFFSET, sz, Quaternion.Euler(0, 90, 0),  $"Win_GF_Left_{(int)sz}");
            PlaceWindow(group, LEFT_X, Y_1ST   + WIN_HEIGHT_OFFSET, sz, Quaternion.Euler(0, 90, 0),  $"Win_1F_Left_{(int)sz}");
            PlaceWindow(group, LEFT_X, Y_2ND   + WIN_HEIGHT_OFFSET, sz, Quaternion.Euler(0, 90, 0),  $"Win_2F_Left_{(int)sz}");
        }

        // Right-side windows (X face, facing +X)
        float[] sideZRight = { 28f, 33f, 38f, 43f };
        foreach (float sz in sideZRight)
        {
            PlaceWindow(group, RIGHT_X, Y_GROUND + WIN_HEIGHT_OFFSET, sz, Quaternion.Euler(0, -90, 0), $"Win_GF_Right_{(int)sz}");
            PlaceWindow(group, RIGHT_X, Y_1ST    + WIN_HEIGHT_OFFSET, sz, Quaternion.Euler(0, -90, 0), $"Win_1F_Right_{(int)sz}");
        }

        // Back wall windows
        float[] backX = { 13f, 22f, 35f, 44f };
        foreach (float bx in backX)
        {
            PlaceWindow(group, bx, Y_GROUND + WIN_HEIGHT_OFFSET, BACK_Z, Quaternion.Euler(0, 0, 0),   $"Win_GF_Back_{(int)bx}");
            PlaceWindow(group, bx, Y_1ST    + WIN_HEIGHT_OFFSET, BACK_Z, Quaternion.Euler(0, 0, 0),   $"Win_1F_Back_{(int)bx}");
        }

        Debug.Log($"[VoD] Placed {group.transform.childCount} jalousie windows.");
        MarkDirty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/2 - Fix Balcony Railings")]
    public static void PlaceRailings()
    {
        var group = GetOrCreate("_Railings_Balcony", null);

        // 1F front railing — runs along X from 23.5 to 45.5 at Z=23.2
        PlaceRailingRow(group, BALCONY_RAILING_X_START, BALCONY_RAILING_X_END,
                        BALCONY_RAILING_Y_1F, BALCONY_RAILING_Z_1F,
                        Quaternion.Euler(0, 90, 0), "Railing_1F_Front");

        // 1F left-wing railing (from scan: X=7.8, Z=21.6 to 34.4)
        PlaceRailingRowZ(group, 21.6f, 34.4f, BALCONY_RAILING_Y_1F, 7.8f,
                         Quaternion.Euler(0, 0, 0), "Railing_1F_Left");

        // 2F front railing
        PlaceRailingRow(group, 22.7f, 30.2f,
                        BALCONY_RAILING_Y_2F, BALCONY_RAILING_Z_2F,
                        Quaternion.Euler(0, 90, 0), "Railing_2F_Front");

        // 2F left-wing railing (X=7.9, Z=21.6 to 34.4)
        PlaceRailingRowZ(group, 21.6f, 34.4f, BALCONY_RAILING_Y_2F, 7.9f,
                         Quaternion.Euler(0, 0, 0), "Railing_2F_Left");

        Debug.Log($"[VoD] Placed {group.transform.childCount} railing sections.");
        MarkDirty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/3 - Place Interior Doors")]
    public static void PlaceDoors()
    {
        var group = GetOrCreate("_Doors_Interior", null);

        // Main entrance — front center (X=28.9, Z=22, ground floor)
        PlaceDoor(group, ENTRANCE_X, Y_GROUND, FRONT_Z,
                  Quaternion.Euler(0, 180, 0), "Door_MainEntrance",
                  ARCH + "Arch_Door_Interior.glb");

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
        var group = GetOrCreate("_Exterior_Arch", null);

        // Front gate (entrance from street, south of villa at Z≈10)
        var gate = LoadAndPlace(ARCH + "Arch_Gate_Colonial.glb", "Gate_Colonial_Main",
                                new Vector3(28.9f, 32.5f, 10f), Quaternion.Euler(0, 0, 0), Vector3.one);
        if (gate) gate.transform.SetParent(group.transform, true);

        // Well (there's already a "well" at (58,32,27) — place model over it)
        var well = LoadAndPlace(ARCH + "Arch_Well_Stone.glb", "Well_Stone_Model",
                                new Vector3(58.0f, 32.5f, 27.0f), Quaternion.Euler(0, 45, 0), Vector3.one);
        if (well) well.transform.SetParent(group.transform, true);

        // Front yard curtain/arch elements
        var curtain1 = LoadAndPlace(ARCH + "Prop_Curtain_Torn.glb", "Curtain_Entrance_L",
                                    new Vector3(22f, 35f, 22f), Quaternion.Euler(0, 90, 0), Vector3.one);
        if (curtain1) curtain1.transform.SetParent(group.transform, true);

        var curtain2 = LoadAndPlace(ARCH + "Prop_Curtain_Torn.glb", "Curtain_Entrance_R",
                                    new Vector3(35f, 35f, 22f), Quaternion.Euler(0, 90, 0), Vector3.one);
        if (curtain2) curtain2.transform.SetParent(group.transform, true);

        Debug.Log("[VoD] Gate, well, exterior elements placed.");
        MarkDirty();
    }

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("VoD/Villa/6 - Add Exterior Decor")]
    public static void AddExteriorDecor()
    {
        var group = GetOrCreate("_Exterior_Decor", null);

        // ── Cột đèn lối vào (entrance lantern pillars) ─────────────────────
        // Đặt 2 cột lồng đèn hai bên lối vào, Z≈16 (giữa cổng và cửa chính)
        AddLanternPillar(group, 23.5f, Y_GROUND, 16f, "Lantern_L");
        AddLanternPillar(group, 34.3f, Y_GROUND, 16f, "Lantern_R");

        // ── Cây / bụi cây ngoài nhà (vegetation markers) ─────────────────
        AddVegetation(group, 10f,  Y_GROUND, 15f, 1.2f, 2.5f, "Tree_FL");
        AddVegetation(group, 47f,  Y_GROUND, 15f, 1.0f, 2.2f, "Tree_FR");
        AddVegetation(group, 5f,   Y_GROUND, 32f, 0.8f, 1.5f, "Bush_SL_01");
        AddVegetation(group, 5f,   Y_GROUND, 36f, 0.7f, 1.3f, "Bush_SL_02");
        AddVegetation(group, 55f,  Y_GROUND, 30f, 0.9f, 1.8f, "Bush_SR_01");
        AddVegetation(group, 58f,  Y_GROUND, 35f, 0.8f, 1.6f, "Bush_SR_02");
        AddVegetation(group, 20f,  Y_GROUND, 12f, 1.5f, 3.0f, "Tree_BL");
        AddVegetation(group, 38f,  Y_GROUND, 12f, 1.3f, 2.8f, "Tree_BR");

        // ── Đường đá lát sân (stone path stepping stones) ────────────────
        // Lối đi từ cổng Z=10 đến cửa chính Z=22, dọc theo X=28.9
        for (int si = 0; si < 6; si++)
        {
            float sz = 11.5f + si * 1.8f;
            AddSteppingStone(group, 28.9f, Y_GROUND, sz, $"PathStone_{si:00}");
        }

        // ── Chậu hoa sân trước (flower pots at entrance corners) ──────────
        AddFlowerPot(group, 25f, Y_GROUND, 21f, "FlowerPot_L");
        AddFlowerPot(group, 33f, Y_GROUND, 21f, "FlowerPot_R");

        // ── Tường rào thấp (low boundary wall markers) ────────────────────
        AddBoundaryWall(group, 8f,   Y_GROUND, 10f,  6f, 18f, "BoundaryWall_FL");
        AddBoundaryWall(group, 48f,  Y_GROUND, 10f,  6f, 18f, "BoundaryWall_FR");

        Debug.Log("[VoD] Exterior decor placed.");
        MarkDirty();
    }

    // ── Exterior primitive builders ───────────────────────────────────────────

    static void AddLanternPillar(GameObject parent, float x, float y, float z, string name)
    {
        var pivot = new GameObject(name);
        pivot.transform.position = new Vector3(x, y, z);
        pivot.transform.SetParent(parent.transform, true);
        Undo.RegisterCreatedObjectUndo(pivot, name);

        // Base pillar — stone cube 0.4×1.2m
        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillar.name = "Pillar";
        pillar.transform.SetParent(pivot.transform, false);
        pillar.transform.localPosition = new Vector3(0, 0.6f, 0);
        pillar.transform.localScale = new Vector3(0.4f, 1.2f, 0.4f);
        Undo.RegisterCreatedObjectUndo(pillar, "Pillar");

        // Lantern cap — smaller cube on top
        var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = "LanternCap";
        cap.transform.SetParent(pivot.transform, false);
        cap.transform.localPosition = new Vector3(0, 1.4f, 0);
        cap.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        Undo.RegisterCreatedObjectUndo(cap, "LanternCap");
    }

    static void AddVegetation(GameObject parent, float x, float y, float z,
                               float radius, float height, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.position = new Vector3(x, y + height * 0.5f, z);
        go.transform.localScale = new Vector3(radius * 2f, height, radius * 2f);
        go.transform.SetParent(parent.transform, true);
        // Đánh dấu là vegetation để dễ nhận biết
        go.tag = "Untagged";
        Undo.RegisterCreatedObjectUndo(go, name);

        // Thân cây (trunk) cho cây cao
        if (height > 2f)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name + "_Trunk";
            trunk.transform.position = new Vector3(x, y + height * 0.2f, z);
            trunk.transform.localScale = new Vector3(0.15f, height * 0.25f, 0.15f);
            trunk.transform.SetParent(parent.transform, true);
            Undo.RegisterCreatedObjectUndo(trunk, name + "_Trunk");
        }
    }

    static void AddSteppingStone(GameObject parent, float x, float y, float z, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = new Vector3(x, y + 0.05f, z);
        go.transform.localScale = new Vector3(0.8f, 0.1f, 0.6f);
        go.transform.SetParent(parent.transform, true);
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void AddFlowerPot(GameObject parent, float x, float y, float z, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.position = new Vector3(x, y + 0.3f, z);
        go.transform.localScale = new Vector3(0.4f, 0.3f, 0.4f);
        go.transform.SetParent(parent.transform, true);
        Undo.RegisterCreatedObjectUndo(go, name);
    }

    static void AddBoundaryWall(GameObject parent, float x, float y, float z,
                                 float width, float depth, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position = new Vector3(x, y + 0.5f, z + depth * 0.5f);
        go.transform.localScale = new Vector3(width, 1.0f, depth);
        go.transform.SetParent(parent.transform, true);
        Undo.RegisterCreatedObjectUndo(go, name);
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

    /// <summary>Tính danh sách X positions đều nhau, bỏ qua entrance zone.</summary>
    static List<float> CalcWindowPositions(float xMin, float xMax, float spacing, float avoidX = -999f, float avoidClear = 0f)
    {
        var result = new List<float>();
        float totalWidth = xMax - xMin;
        int count = Mathf.FloorToInt(totalWidth / spacing);
        float actualSpacing = totalWidth / count;
        for (int i = 0; i <= count; i++)
        {
            float x = xMin + i * actualSpacing;
            if (Mathf.Abs(x - avoidX) < avoidClear) continue;
            result.Add(x);
        }
        return result;
    }

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

    /// <summary>Đặt row của railing dọc theo trục Z.</summary>
    static void PlaceRailingRowZ(GameObject parent, float zStart, float zEnd,
                                  float y, float x, Quaternion rot, string prefix)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH + "Arch_Railing_Balcony.glb");
        if (asset == null) return;

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

    static void PlaceWindow(GameObject parent, float x, float y, float z, Quaternion rot, string goName)
    {
        var go = LoadAndPlace(ARCH + "Arch_Window_Jalousie.glb", goName,
                              new Vector3(x, y, z), rot, Vector3.one);
        if (go) go.transform.SetParent(parent.transform, true);
    }

    static void PlaceDoor(GameObject parent, float x, float y, float z,
                           Quaternion rot, string goName, string assetPath)
    {
        var go = LoadAndPlace(assetPath, goName, new Vector3(x, y, z), rot, Vector3.one);
        if (go) go.transform.SetParent(parent.transform, true);
    }

    static GameObject LoadAndPlace(string assetPath, string goName, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        // FBX fallback: nếu GLB chưa được import (cần glTFast), thử .fbx cùng tên
        if (prefab == null && assetPath.EndsWith(".glb"))
        {
            var fbx = assetPath.Replace(".glb", ".fbx");
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbx);
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
