using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.IO;

public static class VillaLighting
{
    // ─── POST-PROCESS VOLUMES ───────────────────────────────────────────

    [MenuItem("VoD/Lighting/1 Setup Horror Lighting")]
    public static void SetupHorrorLighting()
    {
        // Directional light — moonlight, dim cold blue
        var dirLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in dirLights)
        {
            if (l.type != LightType.Directional) continue;
            l.color     = new Color(0.30f, 0.35f, 0.50f);
            l.intensity = 0.06f;
            l.shadows   = LightShadows.Soft;
            l.shadowStrength = 0.85f;
            EditorUtility.SetDirty(l);
        }

        // Ambient — near-black cool blue-gray
        RenderSettings.ambientMode      = AmbientMode.Flat;
        RenderSettings.ambientLight     = new Color(0.05f, 0.05f, 0.08f);
        RenderSettings.ambientIntensity = 0.08f;

        // Fog — exponential, dark blue-gray
        RenderSettings.fog             = true;
        RenderSettings.fogMode         = FogMode.Exponential;
        RenderSettings.fogColor        = new Color(0.04f, 0.04f, 0.06f);
        RenderSettings.fogDensity      = 0.025f;

        // Realtime GI off
        RenderSettings.realtimeGI = false;

        // Shadow distance & cascades via QualitySettings
        QualitySettings.shadowDistance  = 50f;
        QualitySettings.shadowCascades  = 2;

        Debug.Log("[Lighting] Directional light, ambient and fog configured.");
    }

    [MenuItem("VoD/Lighting/2 Setup GlobalVolume PostProcess")]
    public static void SetupGlobalVolume()
    {
        // Find or create GlobalVolume
        GameObject volGO = GameObject.Find("GlobalVolume");
        if (volGO == null)
        {
            volGO = new GameObject("GlobalVolume");
            volGO.layer = LayerMask.NameToLayer("Default");
        }

        var vol = volGO.GetComponent<Volume>() ?? volGO.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 10;

        // Create or reuse a VolumeProfile
        string profilePath = "Assets/_Project/Materials/PP_HorrorProfile.asset";
        VolumeProfile profile;
        if (File.Exists(profilePath))
        {
            profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        }
        else
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }
        vol.sharedProfile = profile;

        // ─── Bloom ───────────────────────────────────────────────────────
        if (!profile.TryGet<Bloom>(out var bloom))
            bloom = profile.Add<Bloom>(false);
        bloom.active = true;
        bloom.intensity.Override(0.4f);
        bloom.threshold.Override(0.9f);
        bloom.scatter.Override(0.3f);

        // ─── Film Grain ──────────────────────────────────────────────────
        if (!profile.TryGet<FilmGrain>(out var grain))
            grain = profile.Add<FilmGrain>(false);
        grain.active = true;
        grain.type.Override(FilmGrainLookup.Thin1);
        grain.intensity.Override(0.3f);
        grain.response.Override(0.8f);

        // ─── Vignette ────────────────────────────────────────────────────
        if (!profile.TryGet<Vignette>(out var vig))
            vig = profile.Add<Vignette>(false);
        vig.active = true;
        vig.intensity.Override(0.35f);
        vig.smoothness.Override(0.5f);

        // ─── Color Adjustments (cool, desaturated) ───────────────────────
        if (!profile.TryGet<ColorAdjustments>(out var ca))
            ca = profile.Add<ColorAdjustments>(false);
        ca.active = true;
        ca.postExposure.Override(-0.3f);
        ca.saturation.Override(-25f);
        ca.contrast.Override(15f);
        ca.colorFilter.Override(new Color(0.78f, 0.85f, 1.0f)); // cool blue tint

        // ─── Shadows Midtones Highlights ─────────────────────────────────
        if (!profile.TryGet<ShadowsMidtonesHighlights>(out var smh))
            smh = profile.Add<ShadowsMidtonesHighlights>(false);
        smh.active = true;
        smh.shadows.Override(new Vector4(0.85f, 0.85f, 1.0f, 0f));   // cool shadows
        smh.midtones.Override(new Vector4(0.92f, 0.92f, 1.0f, 0f));

        // ─── Chromatic Aberration ─────────────────────────────────────────
        if (!profile.TryGet<ChromaticAberration>(out var chrom))
            chrom = profile.Add<ChromaticAberration>(false);
        chrom.active = true;
        chrom.intensity.Override(0.05f);

        // ─── Lens Distortion ─────────────────────────────────────────────
        if (!profile.TryGet<LensDistortion>(out var lens))
            lens = profile.Add<LensDistortion>(false);
        lens.active = true;
        lens.intensity.Override(-0.08f); // subtle barrel distortion

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(volGO);

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("[Lighting] GlobalVolume post-process profile configured.");
    }

    [MenuItem("VoD/Lighting/3 Setup URP Renderer Performance")]
    public static void SetupURPPerformance()
    {
        // Find URP asset
        var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            Debug.LogError("[Lighting] No URP asset found in Graphics Settings.");
            return;
        }

        // Render scale 0.85 for perf
        urpAsset.renderScale = 0.85f;

        // Shadow settings
        urpAsset.shadowDistance  = 50f;
        urpAsset.shadowCascadeCount = 2;

        // MSAA off (post-process handles AA)
        urpAsset.msaaSampleCount = 1;

        // HDR on for bloom
        urpAsset.supportsHDR = true;

        EditorUtility.SetDirty(urpAsset);
        AssetDatabase.SaveAssets();
        Debug.Log("[Lighting] URP asset: renderScale=0.85, shadow=50m, cascades=2, MSAA=off, HDR=on.");
    }

    [MenuItem("VoD/Lighting/4 Enable GPU Instancing on All Materials")]
    public static void EnableGPUInstancing()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/_Project/Materials" });
        int count = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            if (!mat.enableInstancing)
            {
                mat.enableInstancing = true;
                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Lighting] GPU instancing enabled on {count} materials.");
    }

    [MenuItem("VoD/Lighting/5 Add Candle Point Lights to Scene")]
    public static void AddCandleLights()
    {
        // Warm amber candle-like point lights in key horror spots
        var candlePositions = new (Vector3 pos, string label)[]
        {
            (new Vector3(31.75f, 34.4f, 27.67f), "CandleLight_Center"),
            (new Vector3(20f,    34.4f, 27.67f), "CandleLight_West"),
            (new Vector3(43f,    34.4f, 27.67f), "CandleLight_East"),
            (new Vector3(31.75f, 34.4f, 15f),    "CandleLight_South"),
            (new Vector3(31.75f, 34.4f, 40f),    "CandleLight_North"),
            // upstairs
            (new Vector3(31.75f, 38.4f, 27.67f), "CandleLight_F1_Center"),
        };

        var parent = GetOrCreate("Lighting_Candles", null);
        foreach (var (pos, label) in candlePositions)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.6f, 0.2f); // warm amber
            light.intensity = 1.2f;
            light.range = 6f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.7f;
            EditorUtility.SetDirty(go);
        }

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"[Lighting] {candlePositions.Length} candle point lights placed.");
    }

    [MenuItem("VoD/Lighting/0 Run All Lighting Setup")]
    public static void RunAll()
    {
        SetupHorrorLighting();
        SetupGlobalVolume();
        SetupURPPerformance();
        EnableGPUInstancing();
        AddCandleLights();

        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Lighting] Full lighting setup complete — scene saved.");
    }

    // ─── helpers ──────────────────────────────────────────────────────────

    static GameObject GetOrCreate(string name, Transform parent)
    {
        var existing = GameObject.Find(name);
        if (existing != null) return existing;
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent);
        return go;
    }
}
