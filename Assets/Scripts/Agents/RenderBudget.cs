using DDS_protocol;
using UnityEngine;

// 2026: the panel cameras of the 2D interface are enabled and have a target texture, so Unity
// renders them every frame, while Camera23 / CameraCopy also render them by hand at 20 Hz and only
// those manual renders reach the screen (ReadPixels into the RawImage textures). The automatic
// renders are wasted work (two HDRP instances saturate an integrated GPU). Disabling the
// components keeps the manual 20 Hz renders, so what is displayed is unchanged.
public class RenderBudget : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() => new GameObject("RenderBudget").AddComponent<RenderBudget>();

    void Start()
    {
        int n = 0;
        foreach (var c in FindObjectsByType<Camera23>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.targetCamera2 != null && c.targetCamera2.enabled) { c.targetCamera2.enabled = false; n++; }
            if (c.targetCamera3 != null && c.targetCamera3.enabled) { c.targetCamera3.enabled = false; n++; }
        }
        foreach (var c in FindObjectsByType<CameraCopy>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (c.targetCamera != null && c.targetCamera.enabled) { c.targetCamera.enabled = false; n++; }
        Debug.Log($"RenderBudget: {n} panel camera(s) left to their manual 20 Hz renders");
        Destroy(gameObject);
    }
}
