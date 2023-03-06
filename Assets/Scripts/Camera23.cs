using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Camera23 : MonoBehaviour
{
    public Camera targetCamera3;
    public RenderTexture renderTexture3;
    public Texture2D texture3;
    public RawImage imageCam3;

    public Camera targetCamera2;
    public RenderTexture renderTexture2;
    public Texture2D texture2;
    public RawImage imageCam2;


    void Start()
    {
        renderTexture3 = new RenderTexture(1280 / 2,  720 / 2, 24);
        targetCamera3.targetTexture = renderTexture3;
        texture3 = new Texture2D(renderTexture3.width, renderTexture3.height);

        renderTexture2 = new RenderTexture(1280/2, 720 / 2, 24);
        targetCamera2.targetTexture = renderTexture2;
        texture2 = new Texture2D(renderTexture2.width, renderTexture2.height);

        StartCoroutine(ExecuteScript());
    }

    private IEnumerator ExecuteScript()
    {
        while (true)
        {
            targetCamera3.Render();
            RenderTexture.active = renderTexture3;
            texture3.ReadPixels(new Rect(0, 0, renderTexture3.width, renderTexture3.height), 0, 0);
            imageCam3.texture = texture3;
            texture3.Apply();

            targetCamera2.Render();
            RenderTexture.active = renderTexture2;
            texture2.ReadPixels(new Rect(0, 0, renderTexture2.width, renderTexture2.height), 0, 0);
            imageCam2.texture = texture2;
            texture2.Apply();

            yield return new WaitForSeconds(0.05f); // Wait for 0.05 seconds (20 times per second)
        }
    }
}
