using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DDS_protocol;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// 2026: renders of the 8 ground-truth cells for the paper figure, as the robot camera sees them:
// same orientation and field of view as the inspection camera at the robot's start pose, placed at
// the required distance from the step target ("near") and at 35 cm ("wide"), robot hidden.
// Writes {monitor|vr}_step{n}[_wide].png and zones.json (pixel box of the zone of interest: the
// defect object for Rust / Damaged / Missing, the target point for No default).
// Only active with:  -gtfigure <output folder>
public class GtFigure : MonoBehaviour
{
    string dir;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, "-gtfigure");
        if (i < 0 || i + 1 >= args.Length) return;
        new GameObject("GtFigure").AddComponent<GtFigure>().dir = args[i + 1];
    }

    static readonly float[] RequiredM = { 0.15f, 0.10f, 0.14f, 0.15f };
    static readonly string[] TruthMonitor = { "Rust", "Damaged part", "No default", "Missing part" };
    static readonly string[] TruthVr = { "Rust", "No default", "Damaged part", "Missing part" };
    const int W = 1920, H = 1080;

    IEnumerator Start()
    {
        Directory.CreateDirectory(dir);
        var sm = FindFirstObjectByType<ScenarioManager>(FindObjectsInactive.Include);
        var copy = FindFirstObjectByType<CameraCopy>(FindObjectsInactive.Include);
        var track = FindFirstObjectByType<MoveRobot_Sub>(FindObjectsInactive.Include);
        var study = copy.targetCamera;
        var trialSwitch = FindFirstObjectByType<trial>(FindObjectsInactive.Include);
        if (trialSwitch != null && trialSwitch.trialToggle != null) trialSwitch.trialToggle.isOn = false;
        for (int f = 0; f < 20; f++) yield return null; // simulated robot reaches its start pose

        var go = new GameObject("GtFigureCamera");
        var cam = go.AddComponent<Camera>();
        cam.CopyFrom(study);
        var src = study.GetComponent<HDAdditionalCameraData>();
        var dst = go.GetComponent<HDAdditionalCameraData>() ?? go.AddComponent<HDAdditionalCameraData>();
        if (src != null) src.CopyTo(dst);
        cam.enabled = false;
        cam.nearClipPlane = 0.005f;
        cam.cullingMask = study.cullingMask;
        Quaternion rot = study.transform.rotation; // robot camera orientation at start
        var rt = new RenderTexture(W, H, 24);
        cam.targetTexture = rt;
        var tex = new Texture2D(W, H, TextureFormat.RGB24, false);

        // hide the robot (it would stand between the camera and the engine)
        var robotRenderers = new List<Renderer>();
        if (track != null && track.robot != null)
            foreach (var r in track.robot.GetComponentsInChildren<Renderer>()) if (r.enabled) { r.enabled = false; robotRenderers.Add(r); }

        var json = new StringBuilder("[\n");
        bool first = true;
        for (int c = 0; c < 2; c++)
        {
            bool vr = c == 1;
            sm.VR.isOn = vr; sm.Monitor.isOn = !vr;
            for (int f = 0; f < 30; f++) yield return null; // scenario applied, lighting settled
            for (int s = 0; s < 4; s++)
            {
                Vector3 p = GtTargets.Target(sm, vr, s + 1);
                string label = (vr ? TruthVr : TruthMonitor)[s];
                GameObject defect = GtTargets.Defect(sm, vr, s + 1);
                foreach (var (suffix, dist) in new[] { ("", RequiredM[s]), ("_wide", 0.35f) })
                {
                    go.transform.SetPositionAndRotation(p - rot * Vector3.forward * dist, rot);
                    for (int f = 0; f < 10; f++) { yield return new WaitForEndOfFrame(); cam.Render(); }
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                    tex.Apply();
                    RenderTexture.active = null;
                    string name = $"{(vr ? "vr" : "monitor")}_step{s + 1}{suffix}.png";
                    File.WriteAllBytes(Path.Combine(dir, name), tex.EncodeToPNG());

                    string zone;
                    string check = "";
                    if (defect != null && TryWorldBounds(defect, out var db)) check = $", \"target_to_defect_centre_cm\": {F(Vector3.Distance(p, db.center) * 100f)}";
                    if (defect != null && TryWorldBounds(defect, out var b))
                        zone = Box(cam, b, "object", defect.name, defect.activeInHierarchy);
                    else
                        zone = Box(cam, new Bounds(p, Vector3.one * 0.06f), "target_point", "", true);
                    if (!first) json.Append(",\n");
                    first = false;
                    json.Append($"  {{\"file\": \"{name}\", \"condition\": \"{(vr ? "vr" : "monitor")}\", \"step\": {s + 1}, " +
                                $"\"label\": \"{label}\", \"distance_m\": {F(dist)}, \"target\": [{F(p.x)}, {F(p.y)}, {F(p.z)}]{check}, \"zone\": {zone}}}");
                }
            }
        }
        json.Append("\n]\n");
        File.WriteAllText(Path.Combine(dir, "zones.json"), json.ToString());
        foreach (var r in robotRenderers) r.enabled = true;
        Debug.Log("GtFigure: done");
        Application.Quit();
    }

    static string F(float v) => v.ToString("0.####", CultureInfo.InvariantCulture);

    // World bounds of an object's meshes, also when it is inactive (renderer bounds are then empty).
    public static bool TryWorldBounds(GameObject g, out Bounds b)
    {
        b = default;
        bool any = false;
        foreach (var mf in g.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.sharedMesh == null) continue;
            var lb = mf.sharedMesh.bounds;
            var m = mf.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                var corner = lb.center + Vector3.Scale(lb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var w = m.MultiplyPoint3x4(corner);
                if (!any) { b = new Bounds(w, Vector3.zero); any = true; } else b.Encapsulate(w);
            }
        }
        return any;
    }

    // Pixel box (origin top-left) of world bounds seen by the camera, clipped to the image.
    static string Box(Camera cam, Bounds b, string kind, string name, bool visibleInScene)
    {
        float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
        bool front = false;
        for (int i = 0; i < 8; i++)
        {
            var corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            var v = cam.WorldToViewportPoint(corner);
            if (v.z <= 0) continue;
            front = true;
            float px = v.x * W, py = (1f - v.y) * H;
            x0 = Mathf.Min(x0, px); x1 = Mathf.Max(x1, px); y0 = Mathf.Min(y0, py); y1 = Mathf.Max(y1, py);
        }
        if (!front) return $"{{\"kind\": \"{kind}\", \"object\": \"{name}\", \"in_frame\": false}}";
        bool inFrame = x1 > 0 && x0 < W && y1 > 0 && y0 < H;
        float cx0 = Mathf.Clamp(x0, 0, W), cx1 = Mathf.Clamp(x1, 0, W), cy0 = Mathf.Clamp(y0, 0, H), cy1 = Mathf.Clamp(y1, 0, H);
        float cx = (cx0 + cx1) / 2, cy = (cy0 + cy1) / 2, rad = Mathf.Max(cx1 - cx0, cy1 - cy0) / 2;
        return $"{{\"kind\": \"{kind}\", \"object\": \"{name}\", \"active_in_scene\": {(visibleInScene ? "true" : "false")}, \"in_frame\": {(inFrame ? "true" : "false")}, " +
               $"\"bbox\": [{Mathf.RoundToInt(cx0)}, {Mathf.RoundToInt(cy0)}, {Mathf.RoundToInt(cx1)}, {Mathf.RoundToInt(cy1)}], " +
               $"\"center\": [{Mathf.RoundToInt(cx)}, {Mathf.RoundToInt(cy)}], \"radius\": {Mathf.RoundToInt(rad)}, " +
               $"\"bbox_unclipped\": [{Mathf.RoundToInt(x0)}, {Mathf.RoundToInt(y0)}, {Mathf.RoundToInt(x1)}, {Mathf.RoundToInt(y1)}]}}";
    }
}
