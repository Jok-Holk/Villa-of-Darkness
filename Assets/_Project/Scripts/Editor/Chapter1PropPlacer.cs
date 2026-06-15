using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class Chapter1PropPlacer
{
    // ─── Room world positions (từ spatial scan) ───────────────────────────────
    static readonly Vector3 POS_LIVING_ROOM     = new Vector3(20.2f,  32.5f, 41.1f);
    static readonly Vector3 POS_DINING_ROOM     = new Vector3(47.0f,  32.5f, 40.4f);
    static readonly Vector3 POS_KITCHEN         = new Vector3(79.4f,  32.5f, 41.0f);
    static readonly Vector3 POS_STUDY           = new Vector3(15.8f,  32.5f, 13.4f);
    static readonly Vector3 POS_BATHROOM_GF     = new Vector3(50.4f,  32.5f, 16.2f);
    static readonly Vector3 POS_MASTER_BED      = new Vector3(15.4f,  40.0f, 41.6f);
    static readonly Vector3 POS_MRS_LAN_ROOM    = new Vector3(32.6f,  40.0f, 41.8f);
    static readonly Vector3 POS_GUEST_ROOM      = new Vector3(50.1f,  40.0f, 16.8f);
    static readonly Vector3 POS_LINHS_ROOM      = new Vector3(16.5f,  40.0f, 14.6f);
    static readonly Vector3 POS_FAMILY_ROOM     = new Vector3(13.9f,  40.0f, 27.4f);
    static readonly Vector3 POS_PLAYROOM        = new Vector3(12.5f,  48.0f, 27.2f);
    static readonly Vector3 POS_READING_ROOM    = new Vector3(35.7f,  48.0f, 42.3f);
    static readonly Vector3 POS_ENTERTAINMENT   = new Vector3(16.9f,  48.5f, 14.3f);
    static readonly Vector3 POS_BASEMENT        = new Vector3(24.5f,  27.5f, 26.9f);

    // ─── Model paths ──────────────────────────────────────────────────────────
    const string ARCH     = "Assets/_Project/Models/Props/Architecture/";
    const string FURN     = "Assets/_Project/Models/Props/Furniture/";
    const string DECOR    = "Assets/_Project/Models/Props/Decor/";
    const string GAMEPLAY = "Assets/_Project/Models/Props/Gameplay/";
    const string HORROR   = "Assets/_Project/Models/Props/Horror/";
    const string KITCHEN  = "Assets/_Project/Models/Props/Kitchen/";

    [MenuItem("VoD/Chapter1/Fix Missing Dining Table and Sofa")]
    public static void FixMissingFurniture()
    {
        var rootGO = GameObject.Find("_Props_Chapter1");
        if (rootGO == null) { Debug.LogWarning("[VoD] _Props_Chapter1 not found — run Place All Props first."); return; }

        bool added = false;
        if (GameObject.Find("DiningTable") == null)
        {
            var g = GetOrCreateGroup("DiningRoom_Props", rootGO);
            Place(FURN + "Furn_Table_Dining.glb", "DiningTable", g,
                  POS_DINING_ROOM, Rot(0, 0, 0), Vector3.one);
            added = true;
        }
        if (GameObject.Find("Sofa_LR") == null)
        {
            var g = GetOrCreateGroup("LivingRoom_Props", rootGO);
            Place(FURN + "Furn_Sofa_Colonial.glb", "Sofa_LR", g,
                  POS_LIVING_ROOM + new Vector3(0, 0, 2f), Rot(0, 180, 0), Vector3.one);
            added = true;
        }
        if (!added) { Debug.Log("[VoD] DiningTable and Sofa_LR already present — no changes made."); return; }

        EditorUtility.SetDirty(rootGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[VoD] Added missing dining table and/or sofa.");
    }

    [MenuItem("VoD/Chapter1/Place All Props")]
    public static void PlaceAllProps()
    {
        if (!ConfirmDialog()) return;

        var root = GetOrCreateGroup("_Props_Chapter1", null);

        PlaceLivingRoom(root);
        PlaceDiningRoom(root);
        PlaceKitchen(root);
        PlaceStudyRoom(root);
        PlaceMasterBedroom(root);
        PlaceMrsLanRoom(root);
        PlaceGuestRoom(root);
        PlaceLinhRoom(root);
        PlacePlayroom(root);
        PlaceReadingRoom(root);
        PlaceBasement(root);
        PlaceAtmosphericProps(root);

        EditorUtility.SetDirty(root);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[VoD] Chapter1 props placed. Save scene to persist.");
    }

    // ─── LIVING ROOM ──────────────────────────────────────────────────────────
    static void PlaceLivingRoom(GameObject parent)
    {
        var g = GetOrCreateGroup("LivingRoom_Props", parent);
        Place(ARCH     + "Arch_Fireplace_Stone.glb",  "Fireplace",       g, POS_LIVING_ROOM + new Vector3(0, 0, -3f),      Quaternion.identity, Vector3.one);
        Place(FURN     + "Furn_Sofa_Colonial.glb",    "Sofa_LR",         g, POS_LIVING_ROOM + new Vector3(0, 0, 2f),        Rot(0, 180, 0),      Vector3.one);
        Place(FURN     + "Furn_Chair_Armchair.glb",   "Armchair_LR_01",  g, POS_LIVING_ROOM + new Vector3(3f,   0,    0),   Rot(0, -90, 0),      Vector3.one);
        Place(FURN     + "Furn_Chair_Armchair.glb",   "Armchair_LR_02",  g, POS_LIVING_ROOM + new Vector3(-3f,  0,    0),   Rot(0, 90, 0),       Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",     "Candle_LR_01",    g, POS_LIVING_ROOM + new Vector3(-1.5f, 0.8f, 1f), Quaternion.identity, Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",     "Candle_LR_02",    g, POS_LIVING_ROOM + new Vector3(1.5f,  0.8f, 1f), Rot(0, 45, 0),       Vector3.one);
        Place(DECOR    + "Prop_Lamp_TableOil.glb",    "OilLamp_LR",      g, POS_LIVING_ROOM + new Vector3(2f,   0.7f,-1.5f),Rot(0, 90, 0),       Vector3.one);
        Place(DECOR    + "Prop_Portrait_Family.glb",  "Portrait_Family", g, POS_LIVING_ROOM + new Vector3(0,    2.5f,-3.5f),Rot(0, 0, 0),        Vector3.one);
        Place(DECOR    + "Prop_Vase_Ceramic.glb",     "Vase_LR",         g, POS_LIVING_ROOM + new Vector3(-2f,  0.5f, 0.5f),Rot(0, 135, 0),      Vector3.one);
    }

    // ─── DINING ROOM ──────────────────────────────────────────────────────────
    static void PlaceDiningRoom(GameObject parent)
    {
        var g = GetOrCreateGroup("DiningRoom_Props", parent);
        Place(FURN     + "Furn_Table_Dining.glb",     "DiningTable",     g, POS_DINING_ROOM + new Vector3(0, 0, 0),        Rot(0, 0, 0),   Vector3.one);
        Place(FURN     + "Furn_Sideboard_Dining.glb", "Sideboard_DR",    g, POS_DINING_ROOM + new Vector3(-3f, 0, 0),      Rot(0, 90, 0),  Vector3.one);
        Place(FURN     + "Furn_Chair_Dining.glb",     "Chair_DR_01",     g, POS_DINING_ROOM + new Vector3(-1.5f, 0, 1.5f), Rot(0, 0, 0),   Vector3.one);
        Place(FURN     + "Furn_Chair_Dining.glb",     "Chair_DR_02",     g, POS_DINING_ROOM + new Vector3(1.5f,  0, 1.5f), Rot(0, 0, 0),   Vector3.one);
        Place(FURN     + "Furn_Chair_Dining.glb",     "Chair_DR_03",     g, POS_DINING_ROOM + new Vector3(-1.5f, 0, -1.5f),Rot(0, 180, 0), Vector3.one);
        Place(FURN     + "Furn_Chair_Dining.glb",     "Chair_DR_04",     g, POS_DINING_ROOM + new Vector3(1.5f,  0, -1.5f),Rot(0, 180, 0), Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",     "Candle_DR",       g, POS_DINING_ROOM + new Vector3(0, 0.85f, 0),   Rot(0, 0, 0),   Vector3.one);
        Place(DECOR    + "Prop_Vase_Ceramic.glb",     "Vase_DR",         g, POS_DINING_ROOM + new Vector3(3f,  0.9f, 0),  Rot(0, 200, 0), Vector3.one);
    }

    // ─── KITCHEN ──────────────────────────────────────────────────────────────
    static void PlaceKitchen(GameObject parent)
    {
        var g = GetOrCreateGroup("Kitchen_Props", parent);
        Place(KITCHEN  + "Furn_Table_Kitchen.glb",    "KitchenTable",    g, POS_KITCHEN + new Vector3(0, 0, 0),          Rot(0, 90, 0),  Vector3.one);
        Place(KITCHEN  + "Prop_Shelf_Kitchen.glb",    "KitchenShelf_01", g, POS_KITCHEN + new Vector3(-3f, 1.5f, -1.5f), Rot(0, 90, 0),  Vector3.one);
        Place(KITCHEN  + "Prop_Shelf_Kitchen.glb",    "KitchenShelf_02", g, POS_KITCHEN + new Vector3(-3f, 0.5f, -1.5f), Rot(0, 90, 0),  Vector3.one);
        Place(KITCHEN  + "Prop_Stove_WoodBurning.glb","WoodStove",       g, POS_KITCHEN + new Vector3(2f,  0,    -2f),   Rot(0, 0, 0),   Vector3.one);
        Place(KITCHEN  + "Prop_Jar_Ceramic.glb",      "Jar_01",          g, POS_KITCHEN + new Vector3(-3f, 2.5f, -2.0f), Rot(0, 0, 0),   Vector3.one);
        Place(KITCHEN  + "Prop_Jar_Ceramic.glb",      "Jar_02",          g, POS_KITCHEN + new Vector3(-3f, 2.5f, -1.5f), Rot(0, 60, 0),  Vector3.one);
    }

    // ─── STUDY ROOM ───────────────────────────────────────────────────────────
    static void PlaceStudyRoom(GameObject parent)
    {
        var g = GetOrCreateGroup("StudyRoom_Props", parent);
        Place(FURN     + "Furn_Desk_Study.glb",        "Desk_Study",     g, POS_STUDY + new Vector3(0, 0, 0),         Rot(0, 90, 0),  Vector3.one);
        Place(FURN     + "Furn_Chair_Study.glb",        "Chair_Study",   g, POS_STUDY + new Vector3(0, 0, 1.5f),      Rot(0, 180, 0), Vector3.one);
        Place(FURN     + "Furn_Bookshelf_Colonial.glb", "Bookshelf_01",  g, POS_STUDY + new Vector3(-2.5f, 0, -1.5f), Rot(0, 90, 0),  Vector3.one);
        Place(FURN     + "Furn_Bookshelf_Colonial.glb", "Bookshelf_02",  g, POS_STUDY + new Vector3(-2.5f, 0, 0),     Rot(0, 90, 0),  Vector3.one);
        Place(DECOR    + "Prop_Lamp_TableOil.glb",      "OilLamp_Study", g, POS_STUDY + new Vector3(0.5f, 0.8f, -0.5f),Rot(0,0,0),   Vector3.one);
        Place(DECOR    + "Prop_Frame_Portrait.glb",     "Frame_Study",   g, POS_STUDY + new Vector3(-2.5f, 2.0f, 0),  Rot(0, 90, 0),  Vector3.one);
        Place(GAMEPLAY + "Prop_Board_Chalk.glb",        "ChalkBoard",    g, POS_STUDY + new Vector3(0, 1.2f, -2.5f),  Rot(0, 0, 0),   Vector3.one);
        Place(GAMEPLAY + "Prop_SheetMusic.glb",         "SheetMusic_S",  g, POS_STUDY + new Vector3(0.2f, 0.8f,-0.3f),Rot(0,15,0),   Vector3.one);
    }

    // ─── MASTER BEDROOM ───────────────────────────────────────────────────────
    static void PlaceMasterBedroom(GameObject parent)
    {
        var g = GetOrCreateGroup("MasterBedroom_Props", parent);
        Place(FURN     + "Furn_Sideboard_Bedside.glb", "Bedside_Left",  g, POS_MASTER_BED + new Vector3(-2f, 0, 0),    Rot(0, 90, 0),  Vector3.one);
        Place(FURN     + "Furn_Sideboard_Bedside.glb", "Bedside_Right", g, POS_MASTER_BED + new Vector3(2f,  0, 0),    Rot(0,-90, 0),  Vector3.one);
        Place(DECOR    + "Prop_Lamp_TableOil.glb",     "OilLamp_MB_L", g, POS_MASTER_BED + new Vector3(-2f, 0.7f,0),  Rot(0, 0, 0),   Vector3.one);
        Place(DECOR    + "Prop_Lamp_TableOil.glb",     "OilLamp_MB_R", g, POS_MASTER_BED + new Vector3(2f,  0.7f,0),  Rot(0, 180, 0), Vector3.one);
        Place(DECOR    + "Prop_Frame_Portrait.glb",    "Frame_MB",     g, POS_MASTER_BED + new Vector3(0, 2.5f,-2.5f), Rot(0,0,0),    Vector3.one);
        Place(DECOR    + "Prop_Vase_Ceramic.glb",      "Vase_MB",      g, POS_MASTER_BED + new Vector3(2.5f,0.6f,2f),  Rot(0,45,0),   Vector3.one);
    }

    // ─── MRS LAN'S ROOM ───────────────────────────────────────────────────────
    static void PlaceMrsLanRoom(GameObject parent)
    {
        var g = GetOrCreateGroup("MrsLanRoom_Props", parent);
        Place(FURN     + "Furn_Sideboard_Bedside.glb", "Bedside_ML",   g, POS_MRS_LAN_ROOM + new Vector3(-1.5f,0,0.5f),  Rot(0,90,0),  Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",       "Candle_ML",   g, POS_MRS_LAN_ROOM + new Vector3(-1.5f,0.7f,0.5f),Rot(0,30,0), Vector3.one);
        Place(FURN     + "Furn_Chair_Armchair.glb",     "Chair_ML",    g, POS_MRS_LAN_ROOM + new Vector3(2f,0,-1f),       Rot(0,-45,0), Vector3.one);
        Place(DECOR    + "Prop_Frame_Portrait.glb",     "Frame_ML",    g, POS_MRS_LAN_ROOM + new Vector3(0,2.2f,-2.5f),   Rot(0,0,0),  Vector3.one);
    }

    // ─── GUEST ROOM ───────────────────────────────────────────────────────────
    static void PlaceGuestRoom(GameObject parent)
    {
        var g = GetOrCreateGroup("GuestRoom_Props", parent);
        Place(FURN     + "Furn_Sideboard_Bedside.glb", "Bedside_GR",   g, POS_GUEST_ROOM + new Vector3(-2f, 0, 0),   Rot(0, 90, 0), Vector3.one);
        Place(DECOR    + "Prop_Lamp_TableOil.glb",     "OilLamp_GR",   g, POS_GUEST_ROOM + new Vector3(-2f, 0.7f,0), Rot(0,0,0),    Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",      "Candle_GR",    g, POS_GUEST_ROOM + new Vector3(2f, 0.5f, 2f), Rot(0,75,0),  Vector3.one);
    }

    // ─── LINH'S ROOM (child) ──────────────────────────────────────────────────
    static void PlaceLinhRoom(GameObject parent)
    {
        var g = GetOrCreateGroup("LinhRoom_Props", parent);
        Place(HORROR   + "Prop_Doll_Porcelain.glb",    "Doll_01",      g, POS_LINHS_ROOM + new Vector3(0, 0.5f,-1f),   Rot(0, 180, 0), Vector3.one);
        Place(HORROR   + "Prop_Doll_Porcelain.glb",    "Doll_02",      g, POS_LINHS_ROOM + new Vector3(1.5f,0.5f,-1f), Rot(0,-30,0),   Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",      "Candle_LR",    g, POS_LINHS_ROOM + new Vector3(-1f, 0.5f,0),   Rot(0, 0, 0),   Vector3.one);
        Place(FURN     + "Furn_Chair_Study.glb",       "Chair_Linh",   g, POS_LINHS_ROOM + new Vector3(1f, 0, 1.5f),   Rot(0,-45,0),   Vector3.one);
    }

    // ─── PLAYROOM (2F) ────────────────────────────────────────────────────────
    static void PlacePlayroom(GameObject parent)
    {
        var g = GetOrCreateGroup("Playroom_Props", parent);
        Place(HORROR   + "Prop_Doll_Porcelain.glb",    "Doll_Play_01", g, POS_PLAYROOM + new Vector3(-1f,  0, -1f),   Rot(0, 45, 0),  Vector3.one);
        Place(HORROR   + "Prop_Doll_Porcelain.glb",    "Doll_Play_02", g, POS_PLAYROOM + new Vector3(1f,   0, -0.5f), Rot(0,-20,0),   Vector3.one);
        Place(GAMEPLAY + "Prop_MusicBox_Cylinder.glb", "MusicBox_Play",g, POS_PLAYROOM + new Vector3(0, 0.5f, 1f),    Rot(0, 90, 0),  Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",      "Candle_Play",  g, POS_PLAYROOM + new Vector3(-2f,0.5f, 0),    Rot(0, 0, 0),   Vector3.one);
    }

    // ─── READING ROOM (2F) ────────────────────────────────────────────────────
    static void PlaceReadingRoom(GameObject parent)
    {
        var g = GetOrCreateGroup("ReadingRoom_Props", parent);
        Place(FURN     + "Furn_Bookshelf_Colonial.glb","Bookshelf_RR_01",g, POS_READING_ROOM + new Vector3(-2.5f,0,-1f),Rot(0,90,0), Vector3.one);
        Place(FURN     + "Furn_Bookshelf_Colonial.glb","Bookshelf_RR_02",g, POS_READING_ROOM + new Vector3(-2.5f,0,1f), Rot(0,90,0), Vector3.one);
        Place(FURN     + "Furn_Chair_Armchair.glb",   "Armchair_RR",    g, POS_READING_ROOM + new Vector3(2f,0,0),      Rot(0,-90,0),Vector3.one);
        Place(DECOR    + "Prop_Lamp_TableOil.glb",    "OilLamp_RR",     g, POS_READING_ROOM + new Vector3(-2.5f,2.5f,-1f),Rot(0,0,0),Vector3.one);
    }

    // ─── BASEMENT ─────────────────────────────────────────────────────────────
    static void PlaceBasement(GameObject parent)
    {
        var g = GetOrCreateGroup("Basement_Props", parent);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",     "Candle_BS_01",  g, POS_BASEMENT + new Vector3(-1f,0.5f, 0),   Rot(0, 0, 0),  Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",     "Candle_BS_02",  g, POS_BASEMENT + new Vector3(2f, 0.5f, 1f),  Rot(0, 60, 0), Vector3.one);
        Place(FURN     + "Furn_Cabinet_Locked.glb",   "Cabinet_BS",    g, POS_BASEMENT + new Vector3(-2f, 0, -2f),   Rot(0, 90, 0), Vector3.one);
        Place(KITCHEN  + "Prop_Jar_Ceramic.glb",      "Jar_BS",        g, POS_BASEMENT + new Vector3(-2f,0.6f,-1.5f),Rot(0,30,0),  Vector3.one);
    }

    // ─── ATMOSPHERIC (doors, curtains, misc throughout) ───────────────────────
    static void PlaceAtmosphericProps(GameObject parent)
    {
        var g = GetOrCreateGroup("Atmospheric_Props", parent);

        // Curtains at windows on ground floor
        Place(ARCH     + "Prop_Curtain_Torn.glb",     "Curtain_LR_01", g, POS_LIVING_ROOM  + new Vector3(-4f,2f,-3f),Rot(0,0,0), Vector3.one);
        Place(ARCH     + "Prop_Curtain_Torn.glb",     "Curtain_LR_02", g, POS_LIVING_ROOM  + new Vector3(4f, 2f,-3f),Rot(0,0,0), Vector3.one);
        Place(ARCH     + "Prop_Curtain_Torn.glb",     "Curtain_DR",    g, POS_DINING_ROOM   + new Vector3(-4f,2f,-3f),Rot(0,0,0), Vector3.one);

        // Hallway candles
        var hallCenter = new Vector3(28.9f, 32.8f, 28.3f);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",     "Candle_Hall_01",g, hallCenter + new Vector3(-2f,1f,0), Rot(0,0,0),  Vector3.one);
        Place(GAMEPLAY + "Prop_Candle_Brass.glb",     "Candle_Hall_02",g, hallCenter + new Vector3(2f, 1f,0), Rot(0,45,0), Vector3.one);

        // Portraits in hallway
        Place(DECOR    + "Prop_Frame_Portrait.glb",   "Frame_Hall_01", g, hallCenter + new Vector3(-3f,2.5f,0), Rot(0,90,0),  Vector3.one);
        Place(DECOR    + "Prop_Frame_Portrait.glb",   "Frame_Hall_02", g, hallCenter + new Vector3(3f, 2.5f,0), Rot(0,-90,0), Vector3.one);

        // Vase near entrance
        Place(DECOR    + "Prop_Vase_Ceramic.glb",     "Vase_Entrance", g, new Vector3(28.5f,32.5f,25f), Rot(0,0,0), Vector3.one);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    static void Place(string assetPath, string goName, GameObject parent,
                      Vector3 worldPos, Quaternion rot, Vector3 scale)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        // FBX fallback: khi glTFast chưa cài, thử .fbx cùng tên
        if (prefab == null && assetPath.EndsWith(".glb"))
        {
            var fbxPath = assetPath.Replace(".glb", ".fbx");
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            // Đặc biệt: Furn_Cabinet_Locked.glb → Furn_Cabinet.fbx (tên khác)
            if (prefab == null && assetPath.Contains("Furn_Cabinet_Locked"))
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    assetPath.Replace("Furn_Cabinet_Locked.glb", "Furn_Cabinet.fbx"));
        }
        if (prefab == null)
        {
            Debug.LogWarning($"[VoD PropPlacer] Asset not found: {assetPath}");
            return;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = goName;
        go.transform.position = worldPos;
        go.transform.rotation = rot;
        go.transform.localScale = scale;
        go.transform.SetParent(parent.transform, worldPositionStays: true);
        Undo.RegisterCreatedObjectUndo(go, "Place " + goName);
    }

    static GameObject GetOrCreateGroup(string name, GameObject parent)
    {
        var existing = GameObject.Find(name);
        if (existing != null) return existing;
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent.transform, false);
        Undo.RegisterCreatedObjectUndo(go, "Create group " + name);
        return go;
    }

    static Quaternion Rot(float x, float y, float z) => Quaternion.Euler(x, y, z);

    static bool ConfirmDialog() =>
        EditorUtility.DisplayDialog(
            "VoD: Place Chapter1 Props",
            "Đặt toàn bộ props vào Chapter1 scene theo spatial map.\n\nĐảm bảo scene Chapter1 đang mở.",
            "Proceed", "Cancel");
}
