using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// 2026: runtime capture of the four inspection targets in both conditions, with the player's
// real lighting. Only active when the player is started with:  -gtcapture <output folder>
// The 2023 scene is not modified: this object is created at start-up from code.
public class GtCapture : MonoBehaviour
{
    string dir;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var args = Environment.GetCommandLineArgs();
        int i = Array.IndexOf(args, "-gtcapture");
        if (i < 0 || i + 1 >= args.Length) return;
        var go = new GameObject("GtCapture");
        go.AddComponent<GtCapture>().dir = args[i + 1];
    }

    static readonly Vector3[] Monitor =
    {
        new Vector3(395.397f, 2.411f, 0.338f), new Vector3(396.031f, 2.440f, 0.340f),
        new Vector3(395.767f, 2.407f, 0.334f), new Vector3(396.483f, 2.426f, 0.398f)
    };
    static readonly Vector3[] Vr =
    {
        new Vector3(395.341f, 2.321f, 0.436f), new Vector3(395.142f, 2.428f, 0.462f),
        new Vector3(396.560f, 2.433f, 0.331f), new Vector3(394.920f, 2.401f, 0.299f)
    };

    IEnumerator Start()
    {
        Directory.CreateDirectory(dir);
        var sm = FindFirstObjectByType<ScenarioManager>(FindObjectsInactive.Include);
        var copy = FindFirstObjectByType<DDS_protocol.CameraCopy>(FindObjectsInactive.Include);
        var study = copy != null ? copy.targetCamera : null;
        Debug.Log($"GtCapture: scenario manager {(sm != null)}, study camera {(study != null ? study.name : "none")}");

        var go = new GameObject("GtCaptureCamera");
        var cam = go.AddComponent<Camera>();
        if (study != null)
        {
            cam.CopyFrom(study);
            var src = study.GetComponent<HDAdditionalCameraData>();
            var dst = go.GetComponent<HDAdditionalCameraData>() ?? go.AddComponent<HDAdditionalCameraData>();
            if (src != null) src.CopyTo(dst);
        }
        cam.fieldOfView = 50f; cam.nearClipPlane = 0.005f; // framing of our own, lighting settings from the study camera
        cam.enabled = false;
        var rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);

        for (int c = 0; c < 2; c++)
        {
            bool vr = c == 1;
            if (sm != null) { sm.VR.isOn = vr; sm.Monitor.isOn = !vr; }
            for (int f = 0; f < 30; f++) yield return null; // let the scenario apply and lighting settle
            var pts = vr ? Vr : Monitor;
            for (int i = 0; i < 4; i++)
            {
                foreach (var (suffix, eye) in new[] { ("near", new Vector3(0f, 0.18f, 0.32f)), ("wide", new Vector3(0f, 0.6f, 0.9f)) })
                {
                    go.transform.position = pts[i] + eye;
                    go.transform.LookAt(pts[i]);
                    for (int f = 0; f < 10; f++) { yield return new WaitForEndOfFrame(); cam.Render(); }
                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                    tex.Apply();
                    RenderTexture.active = null;
                    File.WriteAllBytes(Path.Combine(dir, $"{(vr ? "vr" : "monitor")}_step{i + 1}_{suffix}.png"), tex.EncodeToPNG());
                }
            }
        }
        Debug.Log("GtCapture: done");
        Application.Quit();
    }
}
