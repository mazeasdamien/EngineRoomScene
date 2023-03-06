using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Orbit_Camera : MonoBehaviour
{
    public Transform target;  // The object to orbit around
    [SerializeField] private Camera cam;
    [SerializeField] private float distanceToTarget = 20;
    [SerializeField] private float scrollSpeed = 5f;
    [SerializeField] private RawImage rawImage;

    private Vector3 previousPosition;


    public Camera targetCamera3;
    public RenderTexture renderTexture3;
    public Texture2D texture3;
    public RawImage imageCam3;

    public bool isEnter;

    public void iet()
    { 
        isEnter = true;
    }

    public void iex()
    { 
        isEnter = false;
    }
    void Start()
    {
        renderTexture3 = new RenderTexture(1280, 720, 24);
        targetCamera3.targetTexture = renderTexture3;
        texture3 = new Texture2D(renderTexture3.width, renderTexture3.height);
    }

    private void Update()
    {
        targetCamera3.Render();
        RenderTexture.active = renderTexture3;
        texture3.ReadPixels(new Rect(0, 0, renderTexture3.width, renderTexture3.height), 0, 0);
        imageCam3.texture = texture3;
        texture3.Apply();

        if (isEnter == true)
        {
            float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            distanceToTarget -= scrollAmount;
            distanceToTarget = Mathf.Max(0, distanceToTarget);

            if (Input.GetMouseButtonDown(0))
            {
                previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 newPosition = cam.ScreenToViewportPoint(Input.mousePosition);
                Vector3 direction = previousPosition - newPosition;

                float rotationAroundYAxis = -direction.x * 180; // camera moves horizontally
                float rotationAroundXAxis = direction.y * 180; // camera moves vertically

                cam.transform.position = target.position;

                cam.transform.Rotate(new Vector3(1, 0, 0), rotationAroundXAxis);
                cam.transform.Rotate(new Vector3(0, 1, 0), rotationAroundYAxis, Space.World); // <— This is what makes it work!

                cam.transform.Translate(new Vector3(0, 0, -distanceToTarget));

                previousPosition = newPosition;
            }
        }
    }
}
