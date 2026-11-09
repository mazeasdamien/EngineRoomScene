using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DDS_protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ScenarioManager;

// 2026: agent-study server of the 2D interface / camera instance. Started only when the player is
// launched with  -agentport <port>  (default none) and/or  -condition monitor|vr.
// It replaces the 2023 experimenter flow (green start panel, participant id, Participants_data
// folder, Application.Quit after step 4); the interface itself (panels, buttons) is unchanged.
//
// Commands (JSON line, field "cmd"):
//   ping | state | screenshot | camera {w,h}
//   condition {value: "monitor"|"vr"}   start   step {n: 0..4}
//   report {label}   photo   reset_report
public class AgentServer : AgentLink
{
    static readonly float[] RequiredCm = { 15, 10, 14, 15 };

    ScenarioManager sm;
    CameraCopy cameraCopy;
    MoveRobot_Sub track;
    Recording recording;
    trial trialSwitch;
    string outDir;
    readonly List<Dictionary<string, object>> photos = new List<Dictionary<string, object>>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        int port = ArgInt("-agentport", 0);
        string condition = ArgString("-condition", "");
        if (port == 0 && condition == "") return;
        var go = new GameObject("AgentServer");
        var s = go.AddComponent<AgentServer>();
        s.outDir = ArgString("-agentout", Path.Combine(Application.persistentDataPath, "agent_photos"));
        if (port != 0) s.Listen(port);
    }

    IEnumerator Start()
    {
        sm = FindFirstObjectByType<ScenarioManager>(FindObjectsInactive.Include);
        cameraCopy = FindFirstObjectByType<CameraCopy>(FindObjectsInactive.Include);
        track = FindFirstObjectByType<MoveRobot_Sub>(FindObjectsInactive.Include);
        recording = FindFirstObjectByType<Recording>(FindObjectsInactive.Include);
        trialSwitch = FindFirstObjectByType<trial>(FindObjectsInactive.Include);
        Debug.Log($"AgentServer: scenario {sm != null}, camera {cameraCopy != null}, track {track != null}, recording {recording != null}");
        yield return null;
        string condition = ArgString("-condition", "");
        if (condition != "") { SetCondition(condition); StartRun(); }
    }

    void SetCondition(string value)
    {
        bool vr = value.Trim().ToLowerInvariant() == "vr";
        if (!vr && value.Trim().ToLowerInvariant() != "monitor") throw new ArgumentException("condition must be 'monitor' or 'vr'");
        if (trialSwitch != null && trialSwitch.trialToggle != null) trialSwitch.trialToggle.isOn = false;
        sm.VR.isOn = vr;
        sm.Monitor.isOn = !vr;
    }

    void StartRun()
    {
        if (recording != null && recording.popup != null) recording.popup.SetActive(false);
        sm.UnselectAllToggles();
        sm.parts = scenario1_parts.p1;
        photos.Clear();
    }

    bool IsVr => sm.VR.isOn;
    int Step => (int)sm.parts; // 0 = start, 1..4

    protected override IEnumerator Handle(AgentRequest req, Reply reply)
    {
        switch (req.cmd)
        {
            case "ping":
                reply["instance"] = "EngineRoomScene";
                break;
            case "state":
                FillState(reply);
                break;
            case "screenshot":
                yield return CaptureScreen(reply);
                FillState(reply);
                break;
            case "camera":
                yield return RenderCamera(cameraCopy.targetCamera, req.w > 0 ? req.w : 1280, req.h > 0 ? req.h : 720, reply);
                break;
            case "eye":
                {
                    // free camera for figures: same HDRP settings as the inspection camera, wide lens
                    var go = new GameObject("AgentEyeERS");
                    var cam = go.AddComponent<Camera>();
                    var study = cameraCopy.targetCamera;
                    cam.CopyFrom(study);
                    var src = study.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                    var dst = go.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() ?? go.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                    if (src != null) src.CopyTo(dst);
                    cam.fieldOfView = req.n > 0 ? req.n : 50f;
                    cam.nearClipPlane = 0.02f;
                    cam.farClipPlane = 200f;
                    cam.enabled = false;
                    go.transform.position = new Vector3(req.x, req.y, req.z);
                    go.transform.LookAt(new Vector3(req.lx, req.ly, req.lz));
                    yield return RenderCamera(cam, req.w > 0 ? req.w : 1920, req.h > 0 ? req.h : 1080, reply, settleFrames: 12);
                    Destroy(go);
                    break;
                }
            case "condition":
                SetCondition(req.value);
                yield return null;
                FillState(reply);
                break;
            case "start":
                StartRun();
                yield return null;
                FillState(reply);
                break;
            case "step":
                if (req.n < 0 || req.n > 4) { reply.Fail("n must be 0..4"); break; }
                sm.UnselectAllToggles();
                sm.parts = (scenario1_parts)req.n;
                yield return null;
                FillState(reply);
                break;
            case "report":
                {
                    var t = FindLabel(req.label);
                    if (t == null) { reply.Fail($"unknown label '{req.label}'"); reply["labels"] = Labels(); break; }
                    sm.UnselectAllToggles();
                    t.isOn = true;
                    yield return null;
                    FillState(reply);
                    break;
                }
            case "reset_report":
                sm.UnselectAllToggles();
                yield return null;
                FillState(reply);
                break;
            case "photo":
                yield return TakePhoto(reply);
                break;
            default:
                reply.Fail($"unknown command '{req.cmd}'");
                break;
        }
    }

    IEnumerator TakePhoto(Reply reply)
    {
        if (Step == 0) { reply.Fail("run not started"); yield break; }
        string label = SelectedLabel();
        if (label == null) { reply.Fail("no default reported: in 2023 the photo button only appears once a default is ticked"); yield break; }
        yield return RenderCamera(cameraCopy.targetCamera, 1280, 720, reply);
        var geo = Geometry();
        var rec = new Dictionary<string, object>(geo)
        {
            ["step"] = Step,
            ["label"] = label,
            ["displayed_distance"] = DisplayedDistance(),
            ["time"] = DateTime.UtcNow.ToString("o")
        };
        Directory.CreateDirectory(outDir);
        string file = Path.Combine(outDir, $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_{(IsVr ? "vr" : "monitor")}_p{Step}_{label.Replace(' ', '_')}.png");
        File.WriteAllBytes(file, Convert.FromBase64String((string)reply["image"]));
        rec["file"] = file;
        photos.Add(rec);
        reply["photo"] = rec;
        FillState(reply);
    }

    void FillState(Reply r)
    {
        r["condition"] = IsVr ? "vr" : "monitor";
        r["step"] = Step;
        r["required_cm"] = Step >= 1 ? RequiredCm[Step - 1] : 0f;
        r["displayed_distance"] = DisplayedDistance();
        r["out_of_range_warning"] = track != null && track.ColorModee == 3;
        r["limit_warning"] = track != null && track.ColorModee == 2;
        r["collisions"] = track != null ? track.collisionNumbers : -1;
        r["rail_x"] = track != null ? track.XValue : 0.0;
        r["label"] = SelectedLabel();
        r["labels"] = Labels();
        r["photos"] = photos;
        r["robot_settled"] = SimRobot.Instance == null || SimRobot.Instance.Settled;
        r["joints_deg"] = RobotHandler.UrOutputs.actual_q.Select(q => Math.Round(q * 180.0 / Math.PI, 2)).ToArray();
        foreach (var kv in Geometry()) r[kv.Key] = kv.Value;
    }

    // Ground-truth geometry of the inspection camera relative to the current target (not shown to the agent).
    Dictionary<string, object> Geometry()
    {
        var g = new Dictionary<string, object>();
        if (Step < 1 || cameraCopy == null || cameraCopy.targetCamera == null) return g;
        var cam = cameraCopy.targetCamera.transform;
        Vector3 p = GtTargets.Target(sm, IsVr, Step);
        Vector3 d = p - cam.position;
        g["gt_target_distance_cm"] = Math.Round(d.magnitude * 100f, 1);
        g["gt_off_axis_deg"] = Math.Round(Vector3.Angle(cam.forward, d), 1);
        Vector3 vp = cameraCopy.targetCamera.WorldToViewportPoint(p);
        g["gt_target_in_view"] = vp.z > 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
        g["gt_target_viewport"] = new[] { Math.Round(vp.x, 3), Math.Round(vp.y, 3) };
        var defect = GtTargets.Defect(sm, IsVr, Step);
        if (defect != null && GtFigure.TryWorldBounds(defect, out var db))
        {
            Vector3 dv = cameraCopy.targetCamera.WorldToViewportPoint(db.center);
            g["gt_zone"] = defect.name;
            g["gt_zone_viewport"] = new[] { Math.Round(dv.x, 3), Math.Round(dv.y, 3) };
            g["gt_zone_in_view"] = dv.z > 0 && dv.x >= 0 && dv.x <= 1 && dv.y >= 0 && dv.y <= 1;
            g["gt_zone_distance_cm"] = Math.Round(Vector3.Distance(cam.position, db.center) * 100f, 1);
        }
        else g["gt_zone"] = "target_point";
        g["camera_position"] = V3(cam.position);
        g["camera_forward"] = V3(cam.forward);
        g["camera_right"] = V3(cam.right);
        g["camera_up"] = V3(cam.up);
        return g;
    }

    string DisplayedDistance() => track != null && track.textMeshPro != null ? track.textMeshPro.text : "";

    List<string> Labels() => sm.toggles.Where(t => t != null).Select(LabelOf).ToList();

    static string LabelOf(Toggle t)
    {
        var tmp = t.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null && tmp.text.Trim().Length > 0) return tmp.text.Trim();
        var txt = t.GetComponentInChildren<Text>(true);
        if (txt != null && txt.text.Trim().Length > 0) return txt.text.Trim();
        return t.name;
    }

    static string Norm(string s) => new string((s ?? "").ToLowerInvariant().Where(char.IsLetter).ToArray());

    Toggle FindLabel(string label)
    {
        string n = Norm(label);
        return sm.toggles.FirstOrDefault(t => t != null && (Norm(LabelOf(t)) == n || Norm(t.name) == n));
    }

    string SelectedLabel()
    {
        var t = sm.toggles.FirstOrDefault(x => x != null && x.isOn);
        return t != null ? LabelOf(t) : null;
    }
}
