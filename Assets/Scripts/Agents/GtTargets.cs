using UnityEngine;

// 2026: the 8 inspection targets in this scene's world coordinates, computed from the real objects.
// - 2D (scenario 1): the green parts s1p1..s1p4 are children of the miniature ("digital mockup",
//   scale 10.36) but their LOCAL positions are engine coordinates in the "environment" frame
//   (s1p1 local = RUST1 local to 2 mm). Target = environment.TransformPoint(s1pN.localPosition).
// - VR (scenario 2): the green parts live in ur_Grenoble, whose world frame is this scene's
//   "environment" frame. Target = environment.TransformPoint(ur_Grenoble world position).
// Replaces the hand-transferred points of the first ground-truth pass (5-8 cm off).
public static class GtTargets
{
    static readonly Vector3[] VrInTwin =
    {
        new Vector3(-0.0575f, 1.0348f, -0.612f),   // s1p1 Cube
        new Vector3(-0.253f, 1.1461f, -0.5874f),   // s1p2 part3
        new Vector3(1.1115f, 1.134f, -0.6886f),    // s1p3 Cylinder (2)
        new Vector3(-0.4785f, 1.1033f, -0.7406f),  // s1p4 Cylinder (1), child of ENGINE (2.485, 0.119, -0.893)
    };

    static Transform environment;

    public static Transform Environment(ScenarioManager sm)
    {
        if (environment != null) return environment;
        if (sm != null && sm.Rust1 != null) environment = sm.Rust1.transform.parent != null ? sm.Rust1.transform.parent.parent : null;
        if (environment == null) { var g = GameObject.Find("environment"); if (g != null) environment = g.transform; }
        return environment;
    }

    static Vector3[] s1Local;   // original local positions of s1p1..s1p4 (scenario 1)

    static GameObject Part(ScenarioManager sm, int step) =>
        step == 1 ? sm.s1p1 : step == 2 ? sm.s1p2 : step == 3 ? sm.s1p3 : sm.s1p4;

    static void CacheScenario1(ScenarioManager sm)
    {
        if (s1Local != null) return;
        s1Local = new Vector3[4];
        for (int i = 0; i < 4; i++) s1Local[i] = Part(sm, i + 1).transform.localPosition;
    }

    // "scenario2" = defects and inspected parts of scenario 2 (the 2023 VR scenario).
    public static Vector3 Target(ScenarioManager sm, bool scenario2, int step /* 1..4 */)
    {
        var env = Environment(sm);
        if (scenario2) return env.TransformPoint(VrInTwin[step - 1]);
        CacheScenario1(sm);
        return env.TransformPoint(s1Local[step - 1]);
    }

    // 2026 (agent study, scenario crossed with the interface): the green parts of the 2D
    // interface (miniature) move to the parts of the chosen scenario. Their local positions are
    // environment coordinates, so scenario 2 uses the twin positions directly. The part shapes stay
    // those of scenario 1; only their positions change.
    public static void PlaceGreenParts(ScenarioManager sm, bool scenario2)
    {
        CacheScenario1(sm);
        for (int i = 0; i < 4; i++)
            Part(sm, i + 1).transform.localPosition = scenario2 ? VrInTwin[i] : s1Local[i];
    }

    // Defect object of a ground-truth cell (null for "No default" and scenario-2 step 4, whose
    // missing part does not exist in this scene).
    public static GameObject Defect(ScenarioManager sm, bool scenario2, int step)
    {
        if (!scenario2) return step == 1 ? sm.Rust1 : step == 2 ? sm.BrokenPipes : step == 4 ? sm.MissingPartScenario1 : null;
        return step == 1 ? sm.Rust2 : step == 3 ? sm.BrokenPart2 : null;
    }
}
