using UnityEngine;
using Rti.Dds.Publication;
using Rti.Types.Dynamic;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using static ScenarioManager;
using TMPro;
using Unity.VisualScripting;
using System.Linq;

namespace DDS_protocol
{
    public class CameraCopy : MonoBehaviour
    {
        private protected DataWriter<DynamicData> Writer { get; private set; }
        private DynamicData sample = null;
        private bool init = false;

        public Camera targetCamera;
        public RenderTexture renderTexture;
        public Texture2D texture;
        public RawImage imageCam;
        public RawImage savedImage;
        public Recording Recording;
        private byte[] snapshotdata;
        public ScenarioManager scenarioManager;
        private string fileName;
        public TMP_Text distanceInfo;

        public ToggleGroup partsDefault;
        private string getDefault;


        private void Start()
        {
            renderTexture = new RenderTexture(1280 / 2, 720 / 2, 24);
            targetCamera.targetTexture = renderTexture;
            texture = new Texture2D(renderTexture.width, renderTexture.height);
        }

        void Update()
        {
            // Get the list of active toggles in the group
            List<Toggle> activeToggles = new List<Toggle>(partsDefault.ActiveToggles());

            // If there's only one active toggle, get its name
            if (activeToggles.Count == 1)
            {
                Toggle activeToggle = activeToggles[0];
                getDefault = activeToggle.name;
            }

            if (!init)
            {
                init = true;

                var typeFactory = DynamicTypeFactory.Instance;

                var VideoFeed = typeFactory.BuildStruct()
                    .WithName("VideoFeedSimu")
                    .AddMember(new StructMember("Memory", typeFactory.CreateSequence(typeFactory.GetPrimitiveType<byte>(), 1500000)))
                    .Create();

                Writer = DDSHandler.SetupDataWriter("VideoFeedSimu", VideoFeed);
                sample = new DynamicData(VideoFeed);
                StartCoroutine(ExecuteScript());
            }

        }
        private IEnumerator ExecuteScript()
        {
            while (true)
            {
                targetCamera.Render();
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                imageCam.texture = texture;
                texture.Apply();
                byte[] bytes = texture.EncodeToJPG();
                if (bytes.Length != 0)
                {
                    sample.SetValue("Memory", bytes);
                    Writer.Write(sample);
                }

                yield return new WaitForSeconds(0.05f); // Wait for 0.05 seconds (20 times per second)
            }
        }

        public void saveImage()
        {
            switch (scenarioManager.parts)
            {
                case scenario1_parts.p1:
                    fileName = "p1_" + distanceInfo.text +"_" + getDefault + ".jpg";
                    break;
                case scenario1_parts.p2:
                    fileName = "p2_" + distanceInfo.text + "_" + getDefault + ".jpg";
                    break;
                case scenario1_parts.p3:
                    fileName = "p3_" + distanceInfo.text + "_" + getDefault + ".jpg";
                    break;
                case scenario1_parts.p4:
                    fileName = "p4_" + distanceInfo.text + "_" + getDefault + ".jpg";
                    break;
                default:
                    fileName = "invalidPhoto.jpg";
                    break;
            }


            string filePath = System.IO.Path.Combine(Recording.folderPath, fileName); // combine the file name with the specified path

            Texture2D snapshot = (Texture2D)imageCam.texture;
            snapshot.Apply();
            snapshotdata = snapshot.EncodeToJPG();

            File.WriteAllBytes(filePath, snapshotdata);
            // Load the image from the saved file path
            byte[] imageData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(imageData);

            // Set the texture to the RawImage component
            savedImage.texture = texture;
            texture.Apply();
        }
    }
}