using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class Orbit_Camera : MonoBehaviour
{
    public Transform target;  // The object to orbit around
    [SerializeField] private Camera cam;
    [SerializeField] private RawImage rawImage;

    public float distance = 10.0f; // the initial distance between the camera and target
    public float xSpeed = 120.0f; // the horizontal speed of the orbit
    public float ySpeed = 120.0f; // the vertical speed of the orbit
    public float yMinLimit = -20f; // the minimum vertical angle of the orbit
    public float yMaxLimit = 80f; // the maximum vertical angle of the orbit
    public float distanceMin = 0.5f; // the minimum distance between the camera and target
    public float distanceMax = 40f; // the maximum distance between the camera and target

    private float x = 0.0f;
    private float y = 0.0f;
    private bool isRotating = false;
    private Vector3 lastMousePosition;

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

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
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
            if (target)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isRotating = true;
                    lastMousePosition = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    isRotating = false;
                }

                if (isRotating)
                {
                    float mouseX = Input.mousePosition.x - lastMousePosition.x;
                    float mouseY = Input.mousePosition.y - lastMousePosition.y;

                    x += mouseX * xSpeed * distance * 0.02f;
                    y -= mouseY * ySpeed * 0.02f;

                    y = ClampAngle(y, yMinLimit, yMaxLimit);

                    lastMousePosition = Input.mousePosition;
                }

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

                Quaternion rotation = Quaternion.Euler(y, x, 0);
                Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

                transform.rotation = rotation;
                transform.position = position;
            }
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle += 360f;
        }

        if (angle > 360f)
        {
            angle -= 360f;
        }

        return Mathf.Clamp(angle, min, max);
    }
}
