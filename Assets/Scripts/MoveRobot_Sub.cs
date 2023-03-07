using DDS_protocol;
using Rti.Dds.Subscription;
using Rti.Types.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using TMPro;

namespace DDS_protocol
{
    public class MoveRobot_Sub : MonoBehaviour
    {
        public Transform robot;
        public TextMeshProUGUI textMeshPro;
        public float Ikx;
        public float Iky;
        public float Ikz;
        public double XValue;
        public int ColorModee;
        public GameObject warningMessage;
        public GameObject warning2Message;
        public int collisionNumbers;
        private protected DataReader<DynamicData> Reader { get; private set; }
        private bool init = false;

        void Update()
        {
            if (!init)
            {
                Application.runInBackground = true;
                init = true;

                var typeFactory = DynamicTypeFactory.Instance;

                var robotTrackTopic = typeFactory.BuildStruct()
                   .WithName("robotTrack")
                   .AddMember(new StructMember("xValue", typeFactory.GetPrimitiveType<float>()))
                   .AddMember(new StructMember("distance", typeFactory.GetPrimitiveType<float>()))
                   .AddMember(new StructMember("ikx", typeFactory.GetPrimitiveType<float>()))
                   .AddMember(new StructMember("iky", typeFactory.GetPrimitiveType<float>()))
                   .AddMember(new StructMember("ikz", typeFactory.GetPrimitiveType<float>()))
                   .AddMember(new StructMember("color", typeFactory.GetPrimitiveType<int>()))
                   .AddMember(new StructMember("collisionNumbers", typeFactory.GetPrimitiveType<int>()))
                   .Create();

                Reader = DDSHandler.SetupDataReader("robotTrackTopic", robotTrackTopic);
            }

            ProcessData(Reader);
        }

        void ProcessData(AnyDataReader anyReader)
        {
            var reader = (DataReader<DynamicData>)anyReader;
            using var samples = reader.Take();

            foreach (var sample in samples)
            {
                if (sample.Info.ValidData)
                {
                    DynamicData data = sample.Data;
                    robot.localPosition = new Vector3((float)data.GetValue<double>("xValue"), robot.localPosition.y, robot.localPosition.z);
                    textMeshPro.text = data.GetValue<float>("distance").ToString() + " cm";
                    XValue = data.GetValue<double>("xValue");
                    Ikx = data.GetValue<float>("ikx");
                    Iky = data.GetValue<float>("iky");
                    Ikz = data.GetValue<float>("ikz");
                    collisionNumbers = data.GetValue<int>("collisionNumbers");

                    if (data.GetValue<int>("color") == 2)
                    {
                        ColorModee = 2;
                        warningMessage.SetActive(true);
                        warning2Message.SetActive(false);
                    }
                    else if (data.GetValue<int>("color") == 3)
                    {
                        ColorModee = 3;
                        warning2Message.SetActive(true);
                        warningMessage.SetActive(false);
                    }
                    else
                    {
                        ColorModee = 1;
                        warningMessage.SetActive(false);
                        warning2Message.SetActive(false);
                    }
                }
            }
        }
    }
}
