using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 2026: dumps, for each condition of the 2023 study, which defect object lies at each of the
// four green inspection targets. Run: Unity -batchmode -executeMethod GroundTruth.Dump
public static class GroundTruth
{
    static Bounds? BoundsOf(GameObject go)
    {
        if (go == null) return null;
        var rs = go.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rs[0].bounds;
        foreach (var r in rs.Skip(1)) b.Encapsulate(r.bounds);
        return b;
    }

    static float Gap(Bounds a, Bounds b)
    {
        // distance between two axis-aligned boxes (0 if they overlap)
        var d = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            float lo = Mathf.Max(a.min[i], b.min[i]), hi = Mathf.Min(a.max[i], b.max[i]);
            d[i] = lo > hi ? lo - hi : 0f;
        }
        return d.magnitude;
    }

    static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;
    static string V(Vector3 v) => $"[{v.x:F3},{v.y:F3},{v.z:F3}]";

    public static void Dump()
    {
        var scene = EditorSceneManager.OpenScene("Assets/main2D.unity");
        var sm = Object.FindFirstObjectByType<ScenarioManager>(FindObjectsInactive.Include);
        var defects = new Dictionary<string, GameObject>
        {
            { "OriginalPipes", sm.OriginalPipes }, { "BrokenPipes", sm.BrokenPipes }, { "Rust1", sm.Rust1 },
            { "MissingPartScenario1", sm.MissingPartScenario1 }, { "Rust2", sm.Rust2 }, { "BrokenPart2", sm.BrokenPart2 }
        };
        var targets = new[] { sm.s1p1, sm.s1p2, sm.s1p3, sm.s1p4 };
        // active state of each object per scenario, copied from ScenarioManager.Update (2023)
        var scen = new Dictionary<string, Dictionary<string, bool>>
        {
            { "scenario1_MONITOR", new Dictionary<string, bool> { { "OriginalPipes", false }, { "BrokenPipes", true }, { "Rust1", true }, { "MissingPartScenario1", false }, { "BrokenPart2", false }, { "Rust2", false } } },
            { "scenario2_VR", new Dictionary<string, bool> { { "OriginalPipes", true }, { "BrokenPipes", false }, { "Rust1", false }, { "MissingPartScenario1", true }, { "BrokenPart2", true }, { "Rust2", true } } }
        };

        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"targets\": [");
        for (int i = 0; i < 4; i++)
        {
            var t = targets[i];
            var b = BoundsOf(t).Value;
            sb.AppendLine($"    {{\"step\": {i + 1}, \"path\": \"{Path(t.transform)}\", \"center\": {V(b.center)}, \"size\": {V(b.size)}, \"gaps_m\": {{" +
                string.Join(", ", defects.Select(kv => $"\"{kv.Key}\": {(kv.Value == null ? "null" : Gap(b, BoundsOf(kv.Value).Value).ToString("F3", System.Globalization.CultureInfo.InvariantCulture))}")) + "}}" + (i < 3 ? "," : ""));
        }
        sb.AppendLine("  ],");
        sb.AppendLine("  \"defects\": {");
        int k = 0;
        foreach (var kv in defects)
        {
            var b = kv.Value == null ? (Bounds?)null : BoundsOf(kv.Value);
            sb.AppendLine($"    \"{kv.Key}\": {{\"path\": \"{(kv.Value == null ? "" : Path(kv.Value.transform))}\", \"center\": {(b == null ? "null" : V(b.Value.center))}, \"size\": {(b == null ? "null" : V(b.Value.size))}}}" + (++k < defects.Count ? "," : ""));
        }
        sb.AppendLine("  },");
        sb.AppendLine("  \"scenarios\": {");
        k = 0;
        foreach (var s in scen)
        {
            sb.AppendLine($"    \"{s.Key}\": {{" + string.Join(", ", s.Value.Select(x => $"\"{x.Key}\": {(x.Value ? "true" : "false")}")) + "}" + (++k < scen.Count ? "," : ""));
        }
        sb.AppendLine("  }");
        sb.AppendLine("}");
        var outPath = System.Environment.GetEnvironmentVariable("GROUND_TRUTH_OUT") ?? "ground_truth.json";
        File.WriteAllText(outPath, sb.ToString());
        Debug.Log("GroundTruth: written " + outPath);
        EditorApplication.Exit(0);
    }

    static void Tree(StringBuilder sb, Transform t, int depth, int max)
    {
        var b = BoundsOf(t.gameObject);
        sb.AppendLine(new string(' ', depth * 2) + t.name + (t.gameObject.activeSelf ? "" : " (off)") + "  pos=" + V(t.position) + " lscale=" + V(t.lossyScale) + (b.HasValue ? " bc=" + V(b.Value.center) + " bs=" + V(b.Value.size) : ""));
        if (depth < max) foreach (Transform c in t) Tree(sb, c, depth + 1, max);
    }

    public static void DumpTrees()
    {
        EditorSceneManager.OpenScene("Assets/main2D.unity");
        var sb = new StringBuilder();
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == "digital mockup" || root.name == "environment" || root.name == "broken-PART") Tree(sb, root.transform, 0, 3);
        var outPath = System.Environment.GetEnvironmentVariable("GROUND_TRUTH_OUT") ?? "trees.txt";
        File.WriteAllText(outPath, sb.ToString());
        EditorApplication.Exit(0);
    }

    static void Apply(ScenarioManager sm, bool vr)
    {
        if (sm.OriginalPipes) sm.OriginalPipes.SetActive(vr);
        if (sm.BrokenPipes) sm.BrokenPipes.SetActive(!vr);
        if (sm.Rust1) sm.Rust1.SetActive(!vr);
        if (sm.MissingPartScenario1) sm.MissingPartScenario1.SetActive(vr);
        if (sm.BrokenPart2) sm.BrokenPart2.SetActive(vr);
        if (sm.Rust2) sm.Rust2.SetActive(vr);
    }

    static void Shot(Vector3 target, Vector3 eye, string file)
    {
        var go = new GameObject("gt_cam");
        var cam = go.AddComponent<Camera>();
        cam.fieldOfView = 50; cam.nearClipPlane = 0.005f; cam.farClipPlane = 50;
        go.transform.position = eye; go.transform.LookAt(target);
        var rt = new RenderTexture(800, 600, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        for (int i = 0; i < 3; i++) cam.Render(); // HDRP needs a few frames for exposure history
        RenderTexture.active = rt;
        var tex = new Texture2D(800, 600, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 800, 600), 0, 0); tex.Apply();
        File.WriteAllBytes(file, tex.EncodeToPNG());
        RenderTexture.active = null; cam.targetTexture = null;
        Object.DestroyImmediate(go); Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
    }

    public static void Render()
    {
        EditorSceneManager.OpenScene("Assets/main2D.unity");
        var sm = Object.FindFirstObjectByType<ScenarioManager>(FindObjectsInactive.Include);
        var dir = System.Environment.GetEnvironmentVariable("GROUND_TRUTH_OUT") ?? "gt_renders";
        Directory.CreateDirectory(dir);
        // targets on EngineRoomScene's engine, in world coordinates (see docs/protocole-agents.md)
        var mon = new[] { new Vector3(395.397f, 2.411f, 0.338f), new Vector3(396.031f, 2.440f, 0.340f), new Vector3(395.767f, 2.407f, 0.334f), new Vector3(396.483f, 2.426f, 0.398f) };
        var vr = new[] { new Vector3(395.341f, 2.321f, 0.436f), new Vector3(395.142f, 2.428f, 0.462f), new Vector3(396.560f, 2.433f, 0.331f), new Vector3(394.920f, 2.401f, 0.299f) };
        for (int c = 0; c < 2; c++)
        {
            bool isVr = c == 1; Apply(sm, isVr);
            var pts = isVr ? vr : mon; var tag = isVr ? "vr" : "monitor";
            for (int i = 0; i < 4; i++)
            {
                Shot(pts[i], pts[i] + new Vector3(0f, 0.18f, 0.32f), System.IO.Path.Combine(dir, $"{tag}_step{i + 1}_near.png"));
                Shot(pts[i], pts[i] + new Vector3(0f, 0.6f, 0.9f), System.IO.Path.Combine(dir, $"{tag}_step{i + 1}_wide.png"));
            }
        }
        EditorApplication.Exit(0);
    }
}
