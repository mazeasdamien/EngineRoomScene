using DDS_protocol;
using Rti.Dds.Publication;
using Rti.Types.Dynamic;
using UnityEngine;
using UnityEngine.Rendering;
namespace DDS_protocol
{
    internal class UR16eDataPublisher : MonoBehaviour
    {
        public const float Rad2Deg = 57.29578f;
        double[] robotValue = new double[12];

        private protected DataWriter<DynamicData> Writer { get; private set; }
        private DynamicData sample = null;
        private bool init = false;

        void Update()
        {
            if (!init)
            {
                init = true;

                var typeFactory = DynamicTypeFactory.Instance;

                var RobotStateTopic = typeFactory.BuildStruct()
                    .WithName("RobotStateTopic")
                    .AddMember(new StructMember("J1", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("J2", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("J3", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("J4", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("J5", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("J6", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("X", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("Y", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("Z", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("RX", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("RY", typeFactory.GetPrimitiveType<double>()))
                    .AddMember(new StructMember("RZ", typeFactory.GetPrimitiveType<double>()))
                    .Create();

                Writer = DDSHandler.SetupDataWriter("RobotStateTopic", RobotStateTopic);
                sample = new DynamicData(RobotStateTopic);


            }

            sample.SetValue("J1", RobotHandler.UrOutputs.actual_q[0]);
            sample.SetValue("J2", RobotHandler.UrOutputs.actual_q[1]);
            sample.SetValue("J3", RobotHandler.UrOutputs.actual_q[2]);
            sample.SetValue("J4", RobotHandler.UrOutputs.actual_q[3]);
            sample.SetValue("J5", RobotHandler.UrOutputs.actual_q[4]);
            sample.SetValue("J6", RobotHandler.UrOutputs.actual_q[5]);
            sample.SetValue("X", RobotHandler.UrOutputs.actual_TCP_pose[0]);
            sample.SetValue("Y", RobotHandler.UrOutputs.actual_TCP_pose[1]);
            sample.SetValue("Z", RobotHandler.UrOutputs.actual_TCP_pose[2]);
            sample.SetValue("RX", RobotHandler.UrOutputs.actual_TCP_pose[3]);
            sample.SetValue("RZ", RobotHandler.UrOutputs.actual_TCP_pose[4]);
            sample.SetValue("RY", RobotHandler.UrOutputs.actual_TCP_pose[5]);

            if (robotValue[0] != RobotHandler.UrOutputs.actual_q[0] ||
                robotValue[1] != RobotHandler.UrOutputs.actual_q[1] ||
                robotValue[2] != RobotHandler.UrOutputs.actual_q[2] ||
                robotValue[3] != RobotHandler.UrOutputs.actual_q[3] ||
                robotValue[4] != RobotHandler.UrOutputs.actual_q[4] ||
                robotValue[5] != RobotHandler.UrOutputs.actual_q[5])
            {
                robotValue[0] = RobotHandler.UrOutputs.actual_q[0];
                robotValue[1] = RobotHandler.UrOutputs.actual_q[1];
                robotValue[2] = RobotHandler.UrOutputs.actual_q[2];
                robotValue[3] = RobotHandler.UrOutputs.actual_q[3];
                robotValue[4] = RobotHandler.UrOutputs.actual_q[4];
                robotValue[5] = RobotHandler.UrOutputs.actual_q[5];
                robotValue[6] = RobotHandler.UrOutputs.actual_TCP_pose[0];
                robotValue[7] = RobotHandler.UrOutputs.actual_TCP_pose[1];
                robotValue[8] = RobotHandler.UrOutputs.actual_TCP_pose[2];
                robotValue[9] = RobotHandler.UrOutputs.actual_TCP_pose[3];
                robotValue[10] = RobotHandler.UrOutputs.actual_TCP_pose[4];
                robotValue[11] = RobotHandler.UrOutputs.actual_TCP_pose[5];

                Writer.Write(sample);
            }
        }
    }
}