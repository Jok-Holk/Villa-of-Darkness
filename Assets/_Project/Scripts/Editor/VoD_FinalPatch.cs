using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;

#pragma warning disable 618   // StaticEditorFlags.NavigationStatic deprecated in Unity 6 — still works

/// <summary>
/// VoD / Final Patch — chạy 1 lần trên Chapter1.unity đang mở:
///   1. Center villa: Player spawn về (0,0,0)
///   2. Materials toàn scene
///   3. Fix Ground_FrontYard → exterior + interior floor
///   4. Fix component refs: FootstepSystem._cc, GhostProximitySanity._sanitySystem, InventoryTabHandler
///   5. Place 47 models (nội + ngoại thất)
///   6. NavMesh static flags
/// </summary>
public static class VoD_FinalPatch
{
    // ── Offset tính từ scan thực tế: Player spawn (31.5, 33.9, 21.1) → (0,0,0)
    static readonly Vector3 CENTER_OFFSET = new Vector3(-31.5f, -33.9f, -21.1f);

    // ── Material rules (prefix → tên file .mat trong Assets/_Project/Materials/) ─
    static readonly (string pfx, string mat)[] MatRules =
    {
        // Mái
        ("HipRoof",         "Mat_Roof_TerraCotta"),
        ("Roof",            "Mat_Roof_TerraCotta"),
        // Tường ngoài
        ("Wall_Ext",        "Mat_Wall_Exterior_Ochre"),
        ("Struct_",         "Mat_Wall_Exterior_Ochre"),
        ("SH_Wall",         "Mat_Wall_Exterior_Ochre"),
        ("Arch_Block",      "Mat_Wall_Exterior_Ochre"),
        ("SupportPillar",   "Mat_Wall_Exterior_Ochre"),
        ("StructuralCore",  "Mat_Wall_Exterior_Ochre"),
        // Tường trong
        ("Wall_Int",        "Mat_Wall_Interior_Cream"),
        // Phào chỉ / cornice
        ("Ped_",            "Mat_Cornice_White"),
        ("Cornice",         "Mat_Cornice_White"),
        ("Promotion",       "Mat_Cornice_White"),
        // Đá / stone
        ("Quoin",           "Mat_Wall_Stone"),
        ("Q_",              "Mat_Wall_Stone"),
        ("Chimney",         "Mat_Wall_Stone"),
        ("WT_",             "Mat_Wall_Stone"),
        ("FencePillar",     "Mat_Wall_Stone"),
        ("GatePillar",      "Mat_Wall_Stone"),
        ("PathFlag",        "Mat_Wall_Stone"),
        ("GardenBed_",      "Mat_Wall_Stone"),
        ("Prop_DryFountain","Mat_Wall_Stone"),
        ("Prop_Vase",       "Mat_Wall_Stone"),
        ("Well",            "Mat_Wall_Stone"),
        ("Obelisk",         "Mat_Wall_Stone"),
        ("Arch_Fireplace",  "Mat_Wall_Stone"),
        ("Arch_Well",       "Mat_Wall_Stone"),
        // Sắt
        ("Balcony_",        "Mat_Iron_Fence"),
        ("FenceRail",       "Mat_Iron_Fence"),
        ("Fence_",          "Mat_Iron_Fence"),
        ("Arch_Gate",       "Mat_Iron_Fence"),
        ("Arch_Railing",    "Mat_Iron_Fence"),
        ("StairRailing",    "Mat_Iron_Fence"),
        ("LadderFolding",   "Mat_Iron_Fence"),
        ("LadderFixed",     "Mat_Iron_Fence"),
        ("Lamp_",           "Mat_Iron_Fence"),
        // Sàn bê tông
        ("Slab_",           "Mat_Floor_CementTile"),
        ("Floor_",          "Mat_Floor_CementTile"),
        ("Stair",           "Mat_Floor_CementTile"),
        ("Ground_",         "Mat_Floor_CementTile"),
        ("FloorGeometry",   "Mat_Floor_CementTile"),
        ("GroundFloor",     "Mat_Floor_CementTile"),
        ("FirstFloor",      "Mat_Floor_CementTile"),
        ("SecondFloor",     "Mat_Floor_CementTile"),
        ("Room_",           "Mat_Floor_CementTile"),
        ("Staircase",       "Mat_Floor_CementTile"),
        // Sân ngoài
        ("Approach_",       "Mat_Floor_Courtyard"),
        ("Path_",           "Mat_Floor_Courtyard"),
        ("StonePath",       "Mat_Floor_Courtyard"),
        ("Parterre",        "Mat_Floor_Courtyard"),
        // Cỏ / vườn
        ("Lawn_",           "Mat_Garden_Grass"),
        ("Shrub_",          "Mat_Garden_Hedge"),
        ("DeadTree",        "Mat_Palm_Trunk"),
        ("Bed_Flower",      "Mat_Garden_FlowerY"),
        ("Bed_Hedge",       "Mat_Garden_Hedge"),
        // Gỗ nội thất
        ("Furn_",           "Mat_Wood_Furniture"),
        ("PSX_",            "Mat_Wood_Furniture"),
        ("Arch_Door",       "Mat_Wood_Furniture"),
        ("Arch_Window",     "Mat_Jalousie_Green"),
        ("Prop_Curtain",    "Mat_Jalousie_Green"),
        ("Curtain_",        "Mat_Jalousie_Green"),
        ("Prop_Lamp",       "Mat_Iron_Fence"),
        ("Prop_Frame",      "Mat_Wood_Furniture"),
        ("Prop_Portrait",   "Mat_Wood_Furniture"),
        // Generic fallback
        ("wall",            "Mat_Wall_Interior_Cream"),
        ("floor",           "Mat_Floor_CementTile"),
        ("door",            "Mat_Wood_Furniture"),
        ("Door",            "Mat_Wood_Furniture"),
    };

    const string MAT_BASE = "Assets/_Project/Materials/";

    // ── 39 placements: ngoại thất PH + horror (nội thất đã do team đặt: 161 objects) ──
    // REQUIRES: chạy _BlenderExport/run_export.ps1 trước để có PH_*.glb trong Nature/
    // Layout sau centering (Player tại 0,0,0):
    //   Ground floor Y≈0,   doors Y≈4-6   → First floor walk Y≈5
    //   Second floor Y≈13,  doors Y≈14    → Third floor  walk Y≈21
    //   Kitchen wing X≈30   Back wall Z≈-28   Left wall X≈-32   Front Z≈+8
    struct P { public string path, name, grp; public float x, y, z, ry; }

    static readonly P[] Props = new P[]
    {
        // ── EXTERIOR — Cổng vào, lan can, cửa sổ mặt tiền, rèm, giếng ──
        // (Rooms already furnished by team: Living/Dining/Kitchen/Study/Bedrooms = 161 objects)
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Gate_Colonial.glb",     name="Arch_Gate_Main",          grp="_Exterior_Arch",    x=-11f, y=0f,   z=24f,  ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Railing_Balcony.glb",   name="Arch_Railing_1F_A",       grp="_Railings_Balcony", x=-5f,  y=5.5f, z=4.5f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Railing_Balcony.glb",   name="Arch_Railing_1F_B",       grp="_Railings_Balcony", x=-15f, y=5.5f, z=4.5f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Railing_Balcony.glb",   name="Arch_Railing_2F_A",       grp="_Railings_Balcony", x=-5f,  y=14f,  z=4.5f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Well_Stone.glb",        name="Arch_Well_Stone",         grp="_Exterior_Arch",    x=14.4f,y=0f,   z=-10.3f,ry=0f  },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Window_Jalousie.glb",   name="Arch_Window_GF_A",        grp="_Exterior_Arch",    x=-5f,  y=1.8f, z=5.2f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Window_Jalousie.glb",   name="Arch_Window_GF_B",        grp="_Exterior_Arch",    x=-13f, y=1.8f, z=5.2f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Window_Jalousie.glb",   name="Arch_Window_GF_C",        grp="_Exterior_Arch",    x=-21f, y=1.8f, z=5.2f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Window_Jalousie.glb",   name="Arch_Window_1F_A",        grp="_Exterior_Arch",    x=-5f,  y=7.0f, z=5.2f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Window_Jalousie.glb",   name="Arch_Window_1F_B",        grp="_Exterior_Arch",    x=-13f, y=7.0f, z=5.2f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Prop_Curtain_Torn.glb",      name="Prop_Curtain_Front_A",    grp="_Exterior_Arch",    x=-5f,  y=0f,   z=5f,   ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Prop_Curtain_Torn.glb",      name="Prop_Curtain_Front_B",    grp="_Exterior_Arch",    x=-13f, y=0f,   z=5f,   ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Prop_Curtain_Torn.glb",      name="Prop_Curtain_Front_C",    grp="_Exterior_Arch",    x=-21f, y=0f,   z=5f,   ry=0f   },
        new P { path="Assets/_Project/Models/Props/Architecture/Arch_Fireplace_Stone.glb",   name="Arch_Fireplace_Main",     grp="_Exterior_Arch",    x=-26f, y=0f,   z=-5f,  ry=90f  },

        // ── HORROR PROP ───────────────────────────────────────────────────────
        new P { path="Assets/_Project/Models/Props/Horror/Prop_Doll_Porcelain.glb",              name="Prop_Doll_1F",              grp="_Props_Chapter1",   x=-5f,  y=6.0f, z=-12f,  ry=180f },

        // ── NGOẠI THẤT: CÂY CHẾT (PolyHaven exports) ─────────────────────────
        // Trục giữa villa X≈-11. Driveway Z=8→24, cổng Z≈24.
        // Layout asymmetric theo design doc: bên TRÁI nặng, bên PHẢI thưa hơn.
        // Cần chạy _BlenderExport/run_export.ps1 trước để có GLB.
        //
        // TRÁI driveway (X càng âm) — 3 cây: 2 dead, 1 đang chết nghiêng
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadTree_01.glb",  name="PH_Dead_L0",  grp="_Landscape_Plants", x=-4f,   y=0f, z=10f, ry=15f  },
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadTree_02.glb",  name="PH_Dead_L3",  grp="_Landscape_Plants", x=-4f,   y=0f, z=18f, ry=200f },
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadBranch_01.glb",name="PH_Dead_L6",  grp="_Landscape_Plants", x=-4f,   y=0f, z=23f, ry=340f },
        //
        // PHẢI driveway (X càng dương) — 5 cây: 2 dead, 2 héo, 1 jacaranda còn sống gần cổng
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadTree_01.glb",  name="PH_Dead_R0",  grp="_Landscape_Plants", x=-18f,  y=0f, z=10f, ry=160f },
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadBranch_02.glb",name="PH_Dead_R1",  grp="_Landscape_Plants", x=-18f,  y=0f, z=14f, ry=80f  },
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadTree_02.glb",  name="PH_Dead_R2",  grp="_Landscape_Plants", x=-18f,  y=0f, z=18f, ry=110f },
        new P { path="Assets/_Project/Models/Props/Nature/PH_IslandTree_01.glb",name="PH_Dead_R4",  grp="_Landscape_Plants", x=-20f,  y=0f, z=21f, ry=270f },
        new P { path="Assets/_Project/Models/Props/Nature/PH_Jacaranda.glb",    name="PH_Jac_Gate", grp="_Landscape_Plants", x=-20f,  y=0f, z=24f, ry=30f  },
        //
        // Sân HÔNG TRÁI (tường villa trải dài X=−32) — 2 cây chết lớn
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadTree_01.glb",  name="PH_Dead_SL_A",grp="_Landscape_Plants", x=-38f,  y=0f, z=-3f, ry=30f  },
        new P { path="Assets/_Project/Models/Props/Nature/PH_DeadBranch_01.glb",name="PH_Dead_SL_B",grp="_Landscape_Plants", x=-38f,  y=0f, z=-16f,ry=220f },
        //
        // Sân SAU (bên giếng) — 2 cây nhiệt đới + 1 fir
        new P { path="Assets/_Project/Models/Props/Nature/PH_IslandTree_02.glb",name="PH_Isle_Back_A",grp="_Landscape_Plants",x=18f,  y=0f, z=-15f,ry=170f },
        new P { path="Assets/_Project/Models/Props/Nature/PH_FirTree_01.glb",   name="PH_Fir_Back",  grp="_Landscape_Plants", x=12f,  y=0f, z=-22f,ry=90f  },

        // ── CỎ DẠI & BỤI (PolyHaven exports) ────────────────────────────────
        // Bụi chết 2 bên perron (đối xứng mặt tiền)
        new P { path="Assets/_Project/Models/Props/Nature/PH_Shrub_03.glb",     name="PH_Shrub_Perron_R",grp="_Landscape_Plants",x=0f,  y=0f, z=8.5f,ry=0f   },
        new P { path="Assets/_Project/Models/Props/Nature/PH_Shrub_04.glb",     name="PH_Shrub_Perron_L",grp="_Landscape_Plants",x=-22f,y=0f, z=8.5f,ry=0f   },
        // Bụi rải rác dọc driveway
        new P { path="Assets/_Project/Models/Props/Nature/PH_Shrub_01.glb",     name="PH_Shrub_D_R_A",   grp="_Landscape_Plants",x=-3f, y=0f, z=9.5f,ry=0f   },
        new P { path="Assets/_Project/Models/Props/Nature/PH_Shrub_02.glb",     name="PH_Shrub_D_L_A",   grp="_Landscape_Plants",x=-19f,y=0f, z=9.5f,ry=0f   },
        new P { path="Assets/_Project/Models/Props/Nature/PH_Shrub_01.glb",     name="PH_Shrub_D_R_B",   grp="_Landscape_Plants",x=5f,  y=0f, z=14f, ry=30f  },
        new P { path="Assets/_Project/Models/Props/Nature/PH_Shrub_04.glb",     name="PH_Shrub_D_L_B",   grp="_Landscape_Plants",x=-27f,y=0f, z=14f, ry=160f },
        // Bụi gần giếng & hông
        new P { path="Assets/_Project/Models/Props/Nature/PH_Shrub_02.glb",     name="PH_Shrub_Well",    grp="_Landscape_Plants",x=16f, y=0f, z=-8f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Nature/PH_WeedPlant_02.glb", name="PH_Weed_SL",       grp="_Landscape_Plants",x=-32f,y=0f, z=-20f,ry=200f },
        new P { path="Assets/_Project/Models/Props/Nature/PH_Nettle.glb",       name="PH_Nettle_Corner", grp="_Landscape_Plants",x=-30f,y=0f, z=2f,  ry=0f   },
        // Cành khô rơi trên đất
        new P { path="Assets/_Project/Models/Props/Nature/PH_DryBranches_01.glb",name="PH_DryBranch_Yard",grp="_Landscape_Plants",x=-6f,y=0f, z=12f, ry=45f  },
        // Cụm cỏ cao 2 bên cổng
        new P { path="Assets/_Project/Models/Props/Nature/PH_Grass_01.glb",     name="PH_Grass_Gate_R",  grp="_Landscape_Plants",x=-5f, y=0f, z=22f, ry=0f   },
        new P { path="Assets/_Project/Models/Props/Nature/PH_Grass_02.glb",     name="PH_Grass_Gate_L",  grp="_Landscape_Plants",x=-17f,y=0f, z=22f, ry=90f  },
    };

    // ═══════════════════════════════════════════════════════════════════════
    [MenuItem("VoD/★ Final Patch – Fix Everything")]
    public static void Run()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.path.Contains("Chapter1"))
        {
            Debug.LogError("[VoD] Mở Chapter1.unity trước khi chạy patch này.");
            return;
        }

        float t = 0f;
        void Prog(string msg, float pct) { t = pct; EditorUtility.DisplayProgressBar("VoD Final Patch", msg, t); }

        try
        {
            Prog("Centering villa…", 0.05f);
            DoCenterVilla(scene);

            Prog("Applying materials…", 0.25f);
            DoMaterials();

            Prog("Fixing Ground_FrontYard…", 0.40f);
            DoFixGround();

            Prog("Fixing component references…", 0.55f);
            DoFixRefs();

            Prog("Removing stale duplicates + placeholder trees…", 0.58f);
            DoCleanDuplicates();
            DoRemovePlaceholderTrees();

            Prog("Placing PolyHaven nature + horror (39 items)…", 0.68f);
            DoPlaceModels();

            Prog("Setting NavMesh static flags…", 0.88f);
            DoNavMeshSetup();

            Prog("Saving scene…", 0.97f);
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.Refresh();

            Prog("Done!", 1f);
            Debug.Log("[VoD] ★ Final Patch complete — 7 bước xong. Kiểm tra Console để xem chi tiết.");
            Debug.Log("[VoD] NavMesh: chạy thủ công Window → AI → Navigation → Bake sau khi Tuấn Anh đặt xong furniture.");
        }
        finally { EditorUtility.ClearProgressBar(); }
    }

    // ── 1. Center villa ──────────────────────────────────────────────────────
    static void DoCenterVilla(UnityEngine.SceneManagement.Scene scene)
    {
        // Guard: nếu Player đã ở gần origin thì skip (tránh center 2 lần)
        var playerGO = GameObject.Find("Player");
        if (playerGO != null && playerGO.transform.root == playerGO.transform)
        {
            float dist = playerGO.transform.position.magnitude;
            if (dist < 2f)
            {
                Debug.Log("[VoD] Villa already centered (Player ≈ origin). Centering skipped.");
                return;
            }
        }

        static bool Skip(string n) =>
            n == "[NEW_PROPS]" || n == "_UI" || n == "EventSystem" ||
            n == "AudioManager" || n.StartsWith("VoD");

        int moved = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (Skip(root.name)) continue;
            root.transform.position += CENTER_OFFSET;
            EditorUtility.SetDirty(root);
            moved++;
        }
        Debug.Log($"[VoD] Centered {moved} root objects. Offset={CENTER_OFFSET}. Player tại (0,0,0).");
    }

    // ── 2. Apply materials ───────────────────────────────────────────────────
    static void DoMaterials()
    {
        var cache = new Dictionary<string, Material>();
        Material GetMat(string name) {
            string path = MAT_BASE + name + ".mat";
            if (!cache.TryGetValue(path, out var m))
                cache[path] = m = AssetDatabase.LoadAssetAtPath<Material>(path);
            return m;
        }

        int hit = 0, miss = 0;
        foreach (var mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string n = mr.gameObject.name;
            bool matched = false;
            foreach (var (pfx, matName) in MatRules)
            {
                if (n.StartsWith(pfx, System.StringComparison.OrdinalIgnoreCase))
                {
                    var mat = GetMat(matName);
                    if (mat == null) continue;
                    var slots = new Material[mr.sharedMaterials.Length];
                    for (int i = 0; i < slots.Length; i++) slots[i] = mat;
                    mr.sharedMaterials = slots;
                    EditorUtility.SetDirty(mr);
                    hit++; matched = true; break;
                }
            }
            if (!matched) miss++;
        }
        Debug.Log($"[VoD] Materials: {hit} applied, {miss} kept original.");
    }

    // ── 3. Fix Ground_FrontYard ──────────────────────────────────────────────
    static void DoFixGround()
    {
        var gfy = GameObject.Find("Ground_FrontYard");
        if (gfy == null) { Debug.LogWarning("[VoD] Ground_FrontYard not found."); return; }

        var extMat = AssetDatabase.LoadAssetAtPath<Material>(MAT_BASE + "Mat_Floor_Courtyard.mat");
        var mr = gfy.GetComponent<MeshRenderer>();
        if (mr != null && extMat != null)
        {
            mr.sharedMaterial = extMat;
            EditorUtility.SetDirty(mr);
            Debug.Log("[VoD] Ground_FrontYard → Mat_Floor_Courtyard.");
        }

        const string IFLOOR = "_InteriorFloor_VoD";
        if (GameObject.Find(IFLOOR) == null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = IFLOOR;
            go.transform.position = new Vector3(-14f, 0.02f, -9f);
            go.transform.localScale = new Vector3(4.5f, 1f, 3.5f);
            var intMat = AssetDatabase.LoadAssetAtPath<Material>(MAT_BASE + "Mat_Floor_CementTile.mat");
            var pmr = go.GetComponent<MeshRenderer>();
            if (intMat != null) pmr.sharedMaterial = intMat;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            Debug.Log("[VoD] _InteriorFloor_VoD created (interior floor plane 45×35m).");
        }
    }

    // ── 4. Fix component references ──────────────────────────────────────────
    static void DoFixRefs()
    {
        GameObject player = null;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (go.name == "Player" && go.transform.parent == null) { player = go; break; }

        if (player == null) { Debug.LogWarning("[VoD] Player root not found."); return; }

        // FootstepSystem._cc
        var fs = player.GetComponent("FootstepSystem") as MonoBehaviour;
        var cc = player.GetComponentInChildren<CharacterController>();
        if (fs != null && cc != null)
        {
            var so = new SerializedObject(fs);
            var p  = so.FindProperty("_cc");
            if (p != null && p.objectReferenceValue == null)
            { p.objectReferenceValue = cc; so.ApplyModifiedProperties(); Debug.Log("[VoD] FootstepSystem._cc → assigned."); }
        }

        // GhostProximitySanity._sanitySystem
        MonoBehaviour sanSys = null;
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (mb.GetType().Name == "SanitySystem") { sanSys = mb; break; }

        var gps = player.GetComponent("GhostProximitySanity") as MonoBehaviour;
        if (gps != null && sanSys != null)
        {
            var so = new SerializedObject(gps);
            var p  = so.FindProperty("_sanitySystem");
            if (p != null && p.objectReferenceValue == null)
            { p.objectReferenceValue = sanSys; so.ApplyModifiedProperties(); Debug.Log("[VoD] GhostProximitySanity._sanitySystem → assigned."); }
        }

        // InventoryTabHandler trên Player (copy từ Capsule nếu có)
        if (player.GetComponent("InventoryTabHandler") == null)
        {
            MonoBehaviour existITH = null;
            foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (mb.GetType().Name == "InventoryTabHandler" && mb.gameObject != player)
                { existITH = mb; break; }

            if (existITH != null)
            {
                var newITH = player.AddComponent(existITH.GetType()) as MonoBehaviour;
                var soOld  = new SerializedObject(existITH);
                var soNew  = new SerializedObject(newITH);
                var uiRef  = soOld.FindProperty("_inventoryUI");
                var nRef   = soNew.FindProperty("_inventoryUI");
                if (uiRef != null && nRef != null)
                { nRef.objectReferenceValue = uiRef.objectReferenceValue; soNew.ApplyModifiedProperties(); }
                Debug.Log("[VoD] InventoryTabHandler added to Player.");
            }
        }

        EditorUtility.SetDirty(player);
    }

    // ── 5a. Remove duplicates from previous patch run ────────────────────────
    static void DoCleanDuplicates()
    {
        // Interior furniture that was placed by an earlier version of this patch
        // but is already present via team's room subgroups (LivingRoom_Props etc.)
        var stale = new HashSet<string>
        {
            "Furn_Sofa_Salon","Furn_Chair_Salon_Left","Furn_Chair_Salon_Right",
            "Prop_Lamp_Salon_A","Prop_Vase_Salon",
            "Furn_Table_Dining","Furn_Chair_Dining_A","Furn_Chair_Dining_B",
            "Furn_Chair_Dining_C","Furn_Chair_Dining_D","Furn_Sideboard_Dining",
            "Prop_Lamp_Dining_A","Prop_Lamp_Dining_B",
            "Prop_Frame_GF_A","Prop_Portrait_Main","Prop_Frame_GF_B",
            "Furn_Bookshelf_A","Furn_Bookshelf_B",
            "Furn_Desk_Study","Furn_Chair_Study","Prop_Lamp_Study","Prop_Vase_Study",
            "Furn_Cabinet_Kitchen_A","Furn_Cabinet_Locked_GF",
            "Furn_Bedside_1F_A","Furn_Bedside_1F_B",
            "Furn_Chair_1F_Corner","Furn_Bench_Piano","Furn_Bookshelf_1F",
            "Prop_Lamp_1F_A","Prop_Lamp_1F_B","Prop_Portrait_1F",
            "Prop_Frame_1F_A","Prop_Frame_1F_B","Prop_Vase_1F",
            "Furn_Cabinet_Locked_2F","Prop_Frame_2F","Prop_Lamp_2F",
            "Prop_Vase_Entry_A","Prop_Vase_Entry_B",
            "Furn_Table_Kitchen","Prop_Stove_Kitchen","Prop_Shelf_Kitchen",
            "Prop_Jar_Kitchen_A","Prop_Jar_Kitchen_B",
            "Prop_Candle_Salon","Prop_Candle_Dining","Prop_Candle_Fireplace",
            "Prop_Candle_Study","Prop_Candle_1F_Hall",
            "Prop_SheetMusic_Piano","Prop_Board_Study",
            "Kenney_BedDouble_1F","Kenney_BedSingle_1F",
            "Kenney_Bathtub_1F","Kenney_Sink_1F","Kenney_Toilet_1F",
            "Kenney_Plant_Entry_R","Kenney_Plant_Entry_L",
        };

        int removed = 0;
        foreach (var go in Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (stale.Contains(go.name))
            {
                // Only remove if it's a root-level or direct child of _Props_Chapter1
                // (don't accidentally touch team's room-group objects with same names)
                var root = go.transform.root.name;
                var parent = go.transform.parent?.name ?? "";
                if (root == go.name || parent == "_Props_Chapter1" || root == "[NEW_PROPS]")
                {
                    Debug.Log($"[VoD] Removing stale duplicate: {go.name}");
                    Object.DestroyImmediate(go);
                    removed++;
                }
            }
        }
        Debug.Log($"[VoD] DoCleanDuplicates — {removed} stale objects removed.");
    }

    // ── 5b. Remove placeholder teal palm trees from original scene ───────────
    static void DoRemovePlaceholderTrees()
    {
        // Tên pattern của cọ teal placeholder (driveway_L_0, driveway_R_0, tree_L_*, ...)
        // Xóa bất kỳ GameObject nào match pattern này ở root hoặc dưới _Landscape group
        int removed = 0;
        var all = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var go in all)
        {
            string n = go.name;
            bool isPlaceholder =
                (n.StartsWith("driveway_") && (n.Contains("_L_") || n.Contains("_R_"))) ||
                (n.StartsWith("tree_L_") || n.StartsWith("tree_R_")) ||
                (n.StartsWith("Tree_L") || n.StartsWith("Tree_R")) ||
                (n.StartsWith("Palm_") || n.StartsWith("Cactus_") || n.StartsWith("PalmTeal")) ||
                (n.StartsWith("PlaceholderTree") || n.StartsWith("Placeholder_Tree"));

            if (!isPlaceholder) continue;

            // Chỉ xóa nếu là root hoặc con của landscape group (không xóa objects của team)
            var parent = go.transform.parent?.name ?? "";
            bool safeToRemove = go.transform.parent == null
                || parent.Contains("Landscape") || parent.Contains("Plants")
                || parent.Contains("Tree") || parent.Contains("Exterior")
                || parent.Contains("Garden") || parent.Contains("Driveway");

            if (safeToRemove)
            {
                Debug.Log($"[VoD] Removing placeholder tree: {go.name}");
                Object.DestroyImmediate(go);
                removed++;
            }
        }
        Debug.Log($"[VoD] DoRemovePlaceholderTrees — {removed} placeholder trees removed.");
    }

    // ── 5c. Place PolyHaven nature + horror props ─────────────────────────────
    static void DoPlaceModels()
    {
        // Thu thập tên đã tồn tại để tránh duplicate
        var existing = new HashSet<string>(
            Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                  .Select(g => g.name));

        // Xóa [NEW_PROPS] cũ (tạm thời từ patch cũ) nếu còn tồn tại
        var oldNewProps = GameObject.Find("[NEW_PROPS]");
        if (oldNewProps != null)
        {
            Object.DestroyImmediate(oldNewProps);
            Debug.Log("[VoD] Removed stale [NEW_PROPS] group.");
        }

        int placed = 0, skipped = 0, missing = 0;

        foreach (var e in Props)
        {
            if (existing.Contains(e.name)) { skipped++; continue; }

            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(e.path);
            if (asset == null)
            {
                Debug.LogWarning($"[VoD] Model not found: {e.path}");
                missing++;
                continue;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            if (go == null) go = Object.Instantiate(asset);

            if (go == null) continue;

            go.name = e.name;
            go.transform.position = new Vector3(e.x, e.y, e.z);
            go.transform.rotation = Quaternion.Euler(0f, e.ry, 0f);

            // Parent vào group, giữ nguyên world position
            var grpGO = !string.IsNullOrEmpty(e.grp) ? GameObject.Find(e.grp) : null;
            if (grpGO != null)
                go.transform.SetParent(grpGO.transform, true);

            // Unpack prefab connection để tránh lỗi khi edit scene
            if (PrefabUtility.IsPartOfPrefabInstance(go))
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            EditorUtility.SetDirty(go);
            placed++;
        }

        Debug.Log($"[VoD] Models placed={placed}, skipped(already exist)={skipped}, missing assets={missing}.");
    }

    // ── 6. NavMesh — Unity 6.3 dùng NavMeshSurface (com.unity.ai.navigation 2.x) ─
    static void DoNavMeshSetup()
    {
        // Xóa NavMesh data cũ bị lệch (baked trước khi center villa)
        UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();

        // Đánh dấu geometry là walkable (legacy flag, vẫn dùng được)
        var walkable = new HashSet<string> {
            "Ground_FrontYard","FloorGeometry","GroundFloor","FirstFloor","SecondFloor",
            "Room_Basement","Room_Kitchen","StonePath","_InteriorFloor_VoD"
        };
        int flagged = 0;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!walkable.Contains(go.name) && !go.name.StartsWith("Floor_") && !go.name.StartsWith("Slab_"))
                continue;
            var cur = GameObjectUtility.GetStaticEditorFlags(go);
            if ((cur & StaticEditorFlags.NavigationStatic) != 0) continue;
            GameObjectUtility.SetStaticEditorFlags(go, cur | StaticEditorFlags.NavigationStatic);
            EditorUtility.SetDirty(go);
            flagged++;
        }

        // Tạo NavMeshSurface nếu chưa có (Unity 6 approach)
        var surface = Object.FindFirstObjectByType<NavMeshSurface>(FindObjectsInactive.Include);
        if (surface == null)
        {
            // Đặt vào _Systems group nếu có, không thì root
            var systems = GameObject.Find("_Systems");
            var surfGO  = new GameObject("NavMeshSurface");
            if (systems != null) surfGO.transform.SetParent(systems.transform, false);
            surface = surfGO.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry    = NavMeshCollectGeometry.RenderMeshes;
            EditorUtility.SetDirty(surfGO);
            Debug.Log("[VoD] NavMeshSurface created in _Systems.");
        }

        // Bake
        surface.BuildNavMesh();
        EditorUtility.SetDirty(surface.gameObject);
        Debug.Log($"[VoD] NavMesh baked via NavMeshSurface ({flagged} objects flagged walkable). GhostAI sẽ pathfind đúng vị trí mới.");
    }
}

#pragma warning restore 618
