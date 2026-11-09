using System;
using UnityEngine;

// 2026: stand-in for the physical UR16e. In 2023 the robot of this scene (and so the inspection
// camera) followed the joint angles read back from the real controller over RTDE (actual_q).
// Without the robot, actual_q never changes and the camera stays frozen. This component copies
// the IK set-points received from ur_Grenoble (input registers 20-25) into actual_q, with a joint
// speed limit, i.e. an ideal controller. Disabled when the player is started with -realrobot.
public class SimRobot : MonoBehaviour
{
    public static SimRobot Instance { get; private set; }

    // Home pose written by RobotHandler.StartApp in 2023 (degrees).
    static readonly double[] HomeDeg = { -96, -60, -90, -110, 90, 0 };

    public float maxJointSpeed = Mathf.PI / 2f; // rad/s
    public bool Settled { get; private set; } = true;
    public double MaxError { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (Array.IndexOf(Environment.GetCommandLineArgs(), "-realrobot") >= 0) return;
        var go = new GameObject("SimRobot");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<SimRobot>();
    }

    void Awake()
    {
        var inp = RobotHandler.UrInputs;
        var q = RobotHandler.UrOutputs.actual_q;
        double[] home = new double[6];
        for (int i = 0; i < 6; i++) { home[i] = HomeDeg[i] * Math.PI / 180.0; q[i] = home[i]; }
        inp.input_double_register_20 = home[0];
        inp.input_double_register_21 = home[1];
        inp.input_double_register_22 = home[2];
        inp.input_double_register_23 = home[3];
        inp.input_double_register_24 = home[4];
        inp.input_double_register_25 = home[5];
        Debug.Log("SimRobot: simulated UR16e active (start with -realrobot to use the physical robot)");
    }

    // Unity 6 / HDRP 17 renders the camera housing model (camera.fbx, child of the tool TARGET)
    // inside the inspection camera's view as an opaque white disk; the 2023 images do not show it.
    // Put the housing on a spare layer that only the inspection camera ignores.
    const int HousingLayer = 31;

    void Start()
    {
        var copy = FindFirstObjectByType<DDS_protocol.CameraCopy>(FindObjectsInactive.Include);
        var cam = copy != null ? copy.targetCamera : null;
        if (cam == null || cam.transform.parent == null) return;
        int hidden = 0;
        foreach (Transform sibling in cam.transform.parent)
        {
            if (sibling == cam.transform) continue;
            foreach (var r in sibling.GetComponentsInChildren<Renderer>(true))
            {
                int old = r.gameObject.layer;
                r.gameObject.layer = HousingLayer;
                hidden++;
                foreach (var other in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (other != cam && (other.cullingMask & (1 << old)) != 0) other.cullingMask |= 1 << HousingLayer;
            }
        }
        cam.cullingMask &= ~(1 << HousingLayer);
        Debug.Log($"SimRobot: {hidden} housing renderer(s) hidden from the inspection camera");
    }

    void Update()
    {
        var inp = RobotHandler.UrInputs;
        double[] target =
        {
            inp.input_double_register_20, inp.input_double_register_21, inp.input_double_register_22,
            inp.input_double_register_23, inp.input_double_register_24, inp.input_double_register_25
        };
        var q = RobotHandler.UrOutputs.actual_q;
        double step = maxJointSpeed * Time.deltaTime;
        double maxErr = 0;
        for (int i = 0; i < 6; i++)
        {
            if (double.IsNaN(target[i])) continue;
            double e = target[i] - q[i];
            q[i] += Math.Max(-step, Math.Min(step, e));
            maxErr = Math.Max(maxErr, Math.Abs(target[i] - q[i]));
        }
        MaxError = maxErr;
        Settled = maxErr < 1e-3;
    }
}
