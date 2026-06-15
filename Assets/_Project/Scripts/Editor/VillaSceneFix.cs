#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// VoD/SceneFix/* — One-shot scene automation for Chapter1.unity.
/// Fixes room names, adds lights, places gameplay systems, bakes NavMesh.
/// Run VoD/SceneFix/0 with Chapter1.unity open.
/// </summary>
public static class VillaSceneFix
{
    const string PREFAB_GHOST = "Assets/_Project/Prefabs/Ghosts/GhostCube.prefab";
    const string DATA_SANITY  = "Assets/_Project/Data/Sanity/SanityData.asset";
    const string DATA_GAZE    = "Assets/_Project/Data/AI/NewGazeSettings.asset";

    // ════════════════════════════════════════════════════════════════
    //  1. FIX ROOM NAMES
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/1 — Fix Room Names")]
    public static void FixRoomNames()
    {
        var renames = new Dictionary<string, string>
        {
            ["KITCHEN"]       = "Room_Kitchen",
            ["KitchenWing"]   = "Room_Kitchen",
            ["LIVING ROOM"]   = "Room_FamilyRoom",
            ["STUDY ROOM"]    = "Room_Study",
            ["Guest Room"]    = "Room_GuestRoom",
            ["Playroom"]      = "Room_Playroom",
            ["Basement"]      = "Room_Basement",
            ["reading room"]  = "Room_ReadingRoom",
            // Linh's room has stray apostrophes + trailing space
        };

        int count = 0;
        foreach (var go in FindAll())
        {
            if (renames.TryGetValue(go.name, out string newName))
            {
                Undo.RecordObject(go, "FixRoomName");
                go.name = newName;
                count++;
                continue;
            }
            // Handle Linh's room (name has quotes / spaces)
            if (go.name.Contains("Linh") && go.name.Contains("Room"))
            {
                Undo.RecordObject(go, "FixRoomName");
                go.name = "Room_LinhBedroom";
                count++;
            }
        }
        MarkDirty();
        Debug.Log($"[SceneFix] ✓ {count} rooms renamed to Room_* prefix");
    }

    // ════════════════════════════════════════════════════════════════
    //  2. CREATE HALLWAY OBJECTS
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/2 — Create Hallway Objects")]
    public static void CreateHallways()
    {
        if (GameObject.Find("Hallway_GroundFloor") != null)
        { Debug.Log("[SceneFix] Hallways already exist."); return; }

        var group = GetOrCreate("_Hallways", null);

        // Ground floor central corridor (between rooms along X axis)
        CreateHallway(group, "Hallway_GroundFloor",  new Vector3(31.75f, 33.5f, 27.67f), new Vector3(38f, 3.5f, 4f));
        CreateHallway(group, "Hallway_GroundFloor_Lo", new Vector3(31.75f, 33.5f, 14f),  new Vector3(38f, 3.5f, 3f));
        CreateHallway(group, "Hallway_GroundFloor_Hi", new Vector3(31.75f, 33.5f, 41f),  new Vector3(38f, 3.5f, 3f));
        // First floor landing
        CreateHallway(group, "Hallway_FirstFloor",   new Vector3(31.75f, 40.8f, 27.67f), new Vector3(38f, 3.5f, 4f));
        CreateHallway(group, "Hallway_Landing",      new Vector3(20f,    40.8f, 27.67f), new Vector3(8f,  3.5f, 8f));

        MarkDirty();
        Debug.Log("[SceneFix] ✓ 5 hallway trigger volumes created");
    }

    static void CreateHallway(GameObject parent, string name, Vector3 center, Vector3 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = center;
        var col = go.AddComponent<BoxCollider>();
        col.size = size;
        col.isTrigger = true;
        Undo.RegisterCreatedObjectUndo(go, "Hallway");
    }

    // ════════════════════════════════════════════════════════════════
    //  3. MARK STRUCTURAL OBJECTS STATIC
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/3 — Mark Structural Objects Static")]
    public static void MarkStructuralStatic()
    {
        // Structural parent groups whose entire subtree should be static
        var structuralRoots = new[]
        {
            "StructuralCore", "FloorGeometry", "Floor_",
            "GroundFloor", "FirstFloor", "SecondFloor",
            "SupportPillars", "roof",
            "_Exterior_Arch", "_Galerie", "_Perron",
            "_Windows_Jalousie", "_Railings_Balcony",
            "Staircase_Main", "Staircase_Side",
            "StairRailing_A", "StairRailing_B",
            "Struct_Balcony_01", "Struct_Balcony_03",
        };

        // Also mark individual walls by name pattern
        var namePatterns = new[] { "wall", "Wall", "floor", "Floor", "ceil", "stair", "Stair", "roof", "Roof", "beam", "Beam" };

        int count = 0;
        foreach (var go in FindAll())
        {
            bool isStructural = structuralRoots.Any(r => go.name.StartsWith(r) || go.name == r);
            if (!isStructural)
                isStructural = namePatterns.Any(p => go.name.Contains(p));

            // Also mark all Balcony structs
            if (!isStructural && go.name.StartsWith("Struct_Balcony_"))
                isStructural = true;

            if (isStructural && go.GetComponent<MeshRenderer>() != null)
            {
                GameObjectUtility.SetStaticEditorFlags(go, (StaticEditorFlags)0x7FFFFFFF);
                count++;
            }
        }
        MarkDirty();
        Debug.Log($"[SceneFix] ✓ {count} structural mesh objects marked Static (Everything)");
    }

    // ════════════════════════════════════════════════════════════════
    //  4. ADD INTERIOR LIGHTS
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/4 — Add Interior Lights")]
    public static void AddInteriorLights()
    {
        if (GameObject.Find("_Lights_Interior") != null)
        { Debug.Log("[SceneFix] Interior lights group already exists."); return; }

        var group = GetOrCreate("_Lights_Interior", null);
        int count = 0;

        // Per-room warm amber point lights (1920s colonial electric)
        var roomLights = new (string room, float intensity, float range, Color col)[]
        {
            ("Room_DiningRoom",    3.5f,  9f,  new Color(1.0f, 0.82f, 0.55f)),
            ("Room_FamilyRoom",    2.8f,  8f,  new Color(1.0f, 0.82f, 0.55f)),
            ("Room_Kitchen",       2.0f,  7f,  new Color(0.95f, 0.88f, 0.70f)),
            ("Room_MasterBedroom", 2.0f,  7f,  new Color(1.0f, 0.80f, 0.50f)),
            ("Room_LinhBedroom",   1.8f,  6f,  new Color(1.0f, 0.80f, 0.50f)),
            ("Room_MrsLanRoom",    1.8f,  6f,  new Color(1.0f, 0.80f, 0.50f)),
            ("Room_SonRoom",       1.8f,  6f,  new Color(1.0f, 0.80f, 0.50f)),
            ("Room_GuestRoom",     1.8f,  6f,  new Color(1.0f, 0.80f, 0.50f)),
            ("Room_Study",         2.2f,  7f,  new Color(0.92f, 0.88f, 0.78f)),
            ("Room_ReadingRoom",   2.2f,  7f,  new Color(0.92f, 0.88f, 0.78f)),
            ("Room_Playroom",      2.0f,  7f,  new Color(1.0f, 0.82f, 0.55f)),
            ("Room_Bathroom",      1.5f,  5f,  new Color(0.90f, 0.92f, 1.00f)),
            ("Room_Bathroom_02",   1.5f,  5f,  new Color(0.90f, 0.92f, 1.00f)),
            ("Room_Closet",        0.8f,  3f,  new Color(1.0f, 0.80f, 0.50f)),
            ("Room_Entertainment", 2.5f,  8f,  new Color(1.0f, 0.82f, 0.55f)),
        };

        foreach (var (roomName, intensity, range, col) in roomLights)
        {
            var room = GameObject.Find(roomName);
            Vector3 pos = room != null ? GetRoomCenter(room) : Vector3.zero;
            if (pos == Vector3.zero) continue;

            var lightGo = new GameObject($"Light_{roomName}");
            lightGo.transform.SetParent(group.transform);
            lightGo.transform.position = pos;
            Undo.RegisterCreatedObjectUndo(lightGo, "InteriorLight");

            var lt = lightGo.AddComponent<Light>();
            lt.type      = LightType.Point;
            lt.intensity = intensity;
            lt.range     = range;
            lt.color     = col;
            lt.shadows   = LightShadows.Soft;
            count++;
        }

        // Hallway / corridor lights
        var corridorPositions = new[]
        {
            new Vector3(31.75f, 35.5f, 27.67f),  // GF centre corridor
            new Vector3(31.75f, 35.5f, 14f),
            new Vector3(31.75f, 35.5f, 41f),
            new Vector3(31.75f, 42.0f, 27.67f),  // 1F landing
        };
        foreach (var cpos in corridorPositions)
        {
            var lg = new GameObject("Light_Corridor");
            lg.transform.SetParent(group.transform);
            lg.transform.position = cpos;
            Undo.RegisterCreatedObjectUndo(lg, "CorridorLight");
            var lt = lg.AddComponent<Light>();
            lt.type = LightType.Point; lt.intensity = 1.2f; lt.range = 10f;
            lt.color = new Color(1f, 0.78f, 0.45f); lt.shadows = LightShadows.Soft;
            count++;
        }

        // Basement — spooky cool blue
        var basement = GameObject.Find("Room_Basement");
        if (basement != null)
        {
            Vector3 bpos = GetRoomCenter(basement);
            if (bpos != Vector3.zero)
            {
                var lg = new GameObject("Light_Basement");
                lg.transform.SetParent(group.transform);
                lg.transform.position = bpos;
                Undo.RegisterCreatedObjectUndo(lg, "BasementLight");
                var lt = lg.AddComponent<Light>();
                lt.type = LightType.Point; lt.intensity = 0.6f; lt.range = 8f;
                lt.color = new Color(0.55f, 0.65f, 1.0f); lt.shadows = LightShadows.Soft;
                count++;
            }
        }

        MarkDirty();
        Debug.Log($"[SceneFix] ✓ {count} interior lights placed");
    }

    // ════════════════════════════════════════════════════════════════
    //  5. ADD SANITY ZONES + AMBIENT TRIGGERS
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/5 — Add Sanity Zones + Ambient Triggers")]
    public static void AddGameplayZones()
    {
        if (GameObject.Find("_GameplayZones") != null)
        { Debug.Log("[SceneFix] Gameplay zones already exist."); return; }

        var group = GetOrCreate("_GameplayZones", null);
        int sanCount = 0, ambCount = 0;

        // ── Safe zones: well-lit rooms where sanity recovers ────────────────
        var safeRooms = new[] { "Room_DiningRoom", "Room_FamilyRoom", "Room_MasterBedroom",
                                "Room_Kitchen", "Room_Study", "Room_Entertainment" };
        foreach (var rn in safeRooms)
        {
            var room = GameObject.Find(rn);
            if (room == null) continue;
            Vector3 c = GetRoomCenter(room);
            if (c == Vector3.zero) continue;

            var sz = CreateTriggerVolume(group, $"SanityZone_Safe_{rn}", c, new Vector3(8f, 4f, 8f));
            var zone = sz.AddComponent<SanityZone>();
            // _zoneType is private — set via SerializedObject (Safe = 0)
            var so = new SerializedObject(zone);
            var ztp = so.FindProperty("_zoneType"); if (ztp != null) ztp.enumValueIndex = 0;  // ZoneType.Safe = 0
            so.ApplyModifiedProperties();
            sanCount++;
        }

        // ── Danger zones: dark areas drain sanity ───────────────────────────
        var dangerAreas = new (string name, Vector3 pos, Vector3 size)[]
        {
            ("SanityZone_Danger_Basement", new Vector3(31.75f, 29f, 27.67f), new Vector3(20f, 5f, 20f)),
            ("SanityZone_Danger_WatchTower", new Vector3(52f, 49f, 27.67f), new Vector3(6f, 6f, 6f)),
            ("SanityZone_Danger_Well", new Vector3(3f, 32.5f, 27.67f), new Vector3(6f, 4f, 6f)),
        };
        foreach (var (dname, dpos, dsize) in dangerAreas)
        {
            var dz = CreateTriggerVolume(group, dname, dpos, dsize);
            var zone = dz.AddComponent<SanityZone>();
            var so = new SerializedObject(zone);
            var ztp = so.FindProperty("_zoneType"); if (ztp != null) ztp.enumValueIndex = 1;  // ZoneType.Danger = 1
            so.ApplyModifiedProperties();
            sanCount++;
        }

        // ── RandomAmbientTrigger per room ────────────────────────────────────
        var ambRooms = new[] {
            "Room_DiningRoom", "Room_FamilyRoom", "Room_Kitchen", "Room_MasterBedroom",
            "Room_Study", "Room_Playroom", "Room_Bathroom", "Room_Entertainment",
            "Room_GuestRoom", "Room_LinhBedroom", "Room_Basement",
        };
        foreach (var rn in ambRooms)
        {
            var room = GameObject.Find(rn);
            if (room == null) continue;
            Vector3 c = GetRoomCenter(room);
            if (c == Vector3.zero) continue;

            var av = CreateTriggerVolume(group, $"AmbientZone_{rn}", c, new Vector3(9f, 4f, 9f));
            av.AddComponent<RandomAmbientTrigger>();
            // Note: _sfxClips left empty — Tân sẽ assign AudioClip[] trong Inspector
            ambCount++;
        }

        MarkDirty();
        Debug.Log($"[SceneFix] ✓ {sanCount} SanityZones + {ambCount} RandomAmbientTriggers placed");
    }

    // ════════════════════════════════════════════════════════════════
    //  6. PLACE GAMEPLAY SYSTEMS (GameManager, SanitySystem, Player)
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/6 — Place Gameplay Systems + Player")]
    public static void PlaceGameplaySystems()
    {
        // ── _GameplaySystems group ───────────────────────────────────────────
        var sys = GetOrCreate("_GameplaySystems", null);

        // GameManager (singleton)
        if (!sys.GetComponentInChildren<GameManager>())
        {
            var gmGo = GetOrCreate("GameManager", sys);
            gmGo.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gmGo, "GameManager");
            Debug.Log("[SceneFix]  + GameManager");
        }

        // SanitySystem + data
        if (!sys.GetComponentInChildren<SanitySystem>())
        {
            var ssGo = GetOrCreate("SanitySystem", sys);
            var ss   = ssGo.AddComponent<SanitySystem>();
            Undo.RegisterCreatedObjectUndo(ssGo, "SanitySystem");
            var sd = AssetDatabase.LoadAssetAtPath<SanityData>(DATA_SANITY);
            if (sd != null)
            {
                var so   = new SerializedObject(ss);
                var prop = so.FindProperty("_data");
                if (prop != null) { prop.objectReferenceValue = sd; so.ApplyModifiedProperties(); }
            }
            Debug.Log("[SceneFix]  + SanitySystem" + (AssetDatabase.LoadAssetAtPath<SanityData>(DATA_SANITY) != null ? " (data wired)" : " — WARNING: SanityData.asset not found"));
        }

        // AudioManager
        if (!sys.GetComponentInChildren<AudioManager>())
        {
            var amGo = GetOrCreate("AudioManager", sys);
            amGo.AddComponent<AudioManager>();
            Undo.RegisterCreatedObjectUndo(amGo, "AudioManager");
            Debug.Log("[SceneFix]  + AudioManager");
        }

        // ── Player rig ───────────────────────────────────────────────────────
        if (GameObject.FindFirstObjectByType<PlayerController>() == null)
        {
            // Find spawn position
            var spawnGo = GameObject.Find("PlayerSpawn");
            Vector3 spawnPos = spawnGo != null ? spawnGo.transform.position : new Vector3(12f, 33.5f, 27.67f);

            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = spawnPos;
            Undo.RegisterCreatedObjectUndo(player, "Player");

            // CharacterController
            var cc      = player.AddComponent<CharacterController>();
            cc.height   = 1.8f;
            cc.radius   = 0.3f;
            cc.center   = new Vector3(0, 0.9f, 0);

            // Player scripts
            player.AddComponent<PlayerController>();
            player.AddComponent<InteractionSystem>();
            player.AddComponent<GhostProximitySanity>();
            player.AddComponent<FootstepSystem>();

            // Camera child
            var camGo = new GameObject("PlayerCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(player.transform);
            camGo.transform.localPosition = new Vector3(0, 1.65f, 0);
            var cam       = camGo.AddComponent<Camera>();
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.05f;
            camGo.AddComponent<AudioListener>();
            // Remove AudioListener from Main Camera if one exists separately
            var oldCam = GameObject.Find("Main Camera");
            if (oldCam != null)
            {
                var al = oldCam.GetComponent<AudioListener>();
                if (al != null) UnityEngine.Object.DestroyImmediate(al);
            }

            // SanityPostProcess on camera
            camGo.AddComponent<SanityPostProcess>();

            // SanityShake on camera child (CLAUDE.md checklist requirement)
            var shakeCamGo = new GameObject("CameraShake");
            shakeCamGo.transform.SetParent(camGo.transform);
            shakeCamGo.transform.localPosition = Vector3.zero;
            shakeCamGo.AddComponent<SanityShake>();

            // Wire _cameraTransform on PlayerController
            var pcSo = new SerializedObject(player.GetComponent<PlayerController>());
            var camProp = pcSo.FindProperty("_cameraTransform");
            if (camProp != null) { camProp.objectReferenceValue = camGo.transform; pcSo.ApplyModifiedProperties(); }

            Debug.Log($"[SceneFix]  + Player rig at {spawnPos}");
        }
        else
        {
            Debug.Log("[SceneFix]  Player already exists — skipping");
        }

        MarkDirty();
        Debug.Log("[SceneFix] ✓ Gameplay systems placed");
    }

    // ════════════════════════════════════════════════════════════════
    //  7. PLACE GHOST PREFAB
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/7 — Place Ghost Prefab")]
    public static void PlaceGhost()
    {
        if (GameObject.FindFirstObjectByType<GhostAI>() != null)
        { Debug.Log("[SceneFix] GhostAI already in scene."); return; }

        var ghostPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_GHOST);
        if (ghostPrefab == null)
        { Debug.LogError($"[SceneFix] Ghost prefab not found at {PREFAB_GHOST}"); return; }

        var spawnGo  = GameObject.Find("GhostSpawn");
        Vector3 pos  = spawnGo != null ? spawnGo.transform.position : new Vector3(31.75f, 33.5f, 15f);

        var ghost = (GameObject)PrefabUtility.InstantiatePrefab(ghostPrefab);
        ghost.transform.position = pos;
        Undo.RegisterCreatedObjectUndo(ghost, "GhostAI");

        // Wire waypoints to GhostAI via SerializedObject
        var ai   = ghost.GetComponent<GhostAI>();
        if (ai != null)
        {
            var waypoints = new List<Transform>();
            foreach (var go in FindAll())
                if (go.name.StartsWith("GhostWaypoint_"))
                    waypoints.Add(go.transform);

            if (waypoints.Count > 0)
            {
                var so   = new SerializedObject(ai);
                var prop = so.FindProperty("_waypoints") ?? so.FindProperty("waypoints") ?? so.FindProperty("_patrolPoints");
                if (prop != null && prop.isArray)
                {
                    prop.ClearArray();
                    for (int i = 0; i < waypoints.Count; i++)
                    {
                        prop.InsertArrayElementAtIndex(i);
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];
                    }
                    so.ApplyModifiedProperties();
                    Debug.Log($"[SceneFix]  + {waypoints.Count} waypoints wired to GhostAI");
                }
                else
                    Debug.LogWarning("[SceneFix]  GhostAI waypoint property not found — wire manually");
            }
        }

        MarkDirty();
        Debug.Log($"[SceneFix] ✓ Ghost placed at {pos}");
    }

    // ════════════════════════════════════════════════════════════════
    //  8. ADD HIDESPOTS
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/8 — Add HideSpots")]
    public static void AddHideSpots()
    {
        if (GameObject.Find("_HideSpots") != null)
        { Debug.Log("[SceneFix] HideSpots already exist."); return; }

        var group = GetOrCreate("_HideSpots", null);
        var pc    = GameObject.FindFirstObjectByType<PlayerController>();

        var spots = new (string name, Vector3 pos, Vector3 hidePos)[]
        {
            ("HideSpot_Closet",          new Vector3(20f,   33.5f, 10f),   new Vector3(20f,   33.5f, 10.5f)),
            ("HideSpot_UnderStairs_GF",  new Vector3(18f,   33.5f, 27.67f), new Vector3(18.5f, 33.5f, 27.67f)),
            ("HideSpot_Basement_Corner", new Vector3(25f,   30f,   18f),   new Vector3(25.5f, 30f,   18f)),
            ("HideSpot_Wardrobe_Master", new Vector3(45f,   40.8f, 12f),   new Vector3(45f,   40.8f, 12.5f)),
            ("HideSpot_UnderBed_Linh",   new Vector3(40f,   40.8f, 42f),   new Vector3(40f,   40.5f, 42f)),
        };

        int count = 0;
        foreach (var (name, pos, hpos) in spots)
        {
            var go = new GameObject(name);
            go.transform.SetParent(group.transform);
            go.transform.position = pos;
            Undo.RegisterCreatedObjectUndo(go, "HideSpot");

            // Trigger collider
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = new Vector3(1.5f, 2f, 1.5f);

            // HidePosition child
            var hidePosGo = new GameObject("HidePosition");
            hidePosGo.transform.SetParent(go.transform);
            hidePosGo.transform.position = hpos;
            Undo.RegisterCreatedObjectUndo(hidePosGo, "HidePosition");

            // HideSpot component
            var hs   = go.AddComponent<HideSpot>();
            var so   = new SerializedObject(hs);
            var pHide = so.FindProperty("_hidePosition");
            if (pHide != null) { pHide.objectReferenceValue = hidePosGo.transform; }
            var pPC  = so.FindProperty("_playerController");
            if (pPC != null && pc != null) { pPC.objectReferenceValue = pc; }
            so.ApplyModifiedProperties();

            count++;
        }

        MarkDirty();
        Debug.Log($"[SceneFix] ✓ {count} HideSpots created" + (pc == null ? " — WARNING: PlayerController not found, wire _playerController manually" : ""));
    }

    // ════════════════════════════════════════════════════════════════
    //  9. FIX GENERIC NAMES
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/9 — Fix Generic Object Names")]
    public static void FixGenericNames()
    {
        var counters = new Dictionary<string, int>();
        int total = 0;

        foreach (var go in FindAll())
        {
            string n = go.name;
            string prefix = null;

            if (n == "Cube" || n.StartsWith("Cube ("))     prefix = "Arch_Block";
            else if (n == "Cylinder" || n.StartsWith("Cylinder (")) prefix = "Arch_Cylinder";
            else if (n == "Sphere" || n.StartsWith("Sphere ("))   prefix = "Arch_Sphere";
            else if (n == "n")                                     prefix = "Arch_Unknown";
            // Normalise "Wash and prepare the table" triggers
            else if (n.Contains("Wash and prepare"))
            {
                if (!counters.ContainsKey("Trigger_PrepTable")) counters["Trigger_PrepTable"] = 0;
                Undo.RecordObject(go, "FixName");
                go.name = $"Trigger_PrepTable_{++counters["Trigger_PrepTable"]:00}";
                total++;
                continue;
            }
            // Remove trailing spaces/quotes from names
            else if (n != n.Trim('\'', ' '))
            {
                Undo.RecordObject(go, "FixName");
                go.name = n.Trim('\'', ' ');
                total++;
                continue;
            }

            if (prefix != null)
            {
                if (!counters.ContainsKey(prefix)) counters[prefix] = 0;
                Undo.RecordObject(go, "FixName");
                go.name = $"{prefix}_{++counters[prefix]:00}";
                total++;
            }
        }

        MarkDirty();
        Debug.Log($"[SceneFix] ✓ {total} generic names cleaned up");
    }

    // ════════════════════════════════════════════════════════════════
    //  A. BAKE NAVMESH
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/A — Bake NavMesh")]
    public static void BakeNavMesh()
    {
        // Step 3 must have run first to mark floors as NavigationStatic
#pragma warning disable CS0618
        int staticCount = FindAll().Count(go =>
            (GameObjectUtility.GetStaticEditorFlags(go) & StaticEditorFlags.NavigationStatic) != 0);
        if (staticCount == 0)
        {
            Debug.LogWarning("[SceneFix] No NavigationStatic objects found — running Step 3 first…");
            MarkStructuralStatic();
        }

        try
        {
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            Debug.Log("[SceneFix] ✓ NavMesh baked successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneFix] NavMesh bake failed: {e.Message}");
        }
#pragma warning restore CS0618
        MarkDirty();
    }

    // ════════════════════════════════════════════════════════════════
    //  0. RUN ALL
    // ════════════════════════════════════════════════════════════════
    [MenuItem("VoD/SceneFix/0 — RUN ALL SCENE FIX")]
    public static void RunAll()
    {
        if (!EditorUtility.DisplayDialog("VoD Scene Fix",
            "Chạy toàn bộ scene fix cho Chapter1:\n" +
            "1. Fix Room Names\n2. Create Hallways\n3. Mark Static\n" +
            "4. Interior Lights\n5. Sanity+Ambient Zones\n6. Gameplay Systems + Player\n" +
            "7. Ghost Prefab\n8. HideSpots\n9. Fix Generic Names\nA. Bake NavMesh\n\n" +
            "Scene phải đang mở. Không thể undo toàn bộ.",
            "Run All", "Cancel")) return;

        FixRoomNames();
        CreateHallways();
        MarkStructuralStatic();
        AddInteriorLights();
        AddGameplayZones();
        PlaceGameplaySystems();
        PlaceGhost();
        AddHideSpots();
        FixGenericNames();
        BakeNavMesh();

        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[SceneFix] ══ All scene fixes complete — scene saved ══");
    }

    // ════════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════════

    static IEnumerable<GameObject> FindAll() =>
        UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

    static Vector3 GetRoomCenter(GameObject room)
    {
        var renderers = room.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return room.transform.position;
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        // Return ceiling-midpoint for light placement
        return new Vector3(b.center.x, b.max.y - 0.6f, b.center.z);
    }

    static GameObject GetOrCreate(string name, GameObject parent)
    {
        var existing = GameObject.Find(name);
        if (existing != null) return existing;
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent.transform);
        Undo.RegisterCreatedObjectUndo(go, name);
        return go;
    }

    static GameObject CreateTriggerVolume(GameObject parent, string name, Vector3 pos, Vector3 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;
        Undo.RegisterCreatedObjectUndo(go, name);
        return go;
    }

    static void MarkDirty() =>
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
}
#endif
