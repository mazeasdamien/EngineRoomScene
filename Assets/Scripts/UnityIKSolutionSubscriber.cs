using DDS_protocol;
using Rti.Dds.Subscription;
using Rti.Types.Dynamic;
using UnityEngine;

public class UnityIKSolutionSubscriber : MonoBehaviour
{
    private protected DataReader<DynamicData> Reader { get; private set; }
    private bool init = false;

    private void Update()
    {
        if (!init)
        {
            init = true;
            var typeFactory = DynamicTypeFactory.Instance;

            var UnitySolutionTopic = typeFactory.BuildStruct()
               .WithName("UnitySolutionTopic")
               .AddMember(new StructMember("J1", typeFactory.GetPrimitiveType<double>()))
               .AddMember(new StructMember("J2", typeFactory.GetPrimitiveType<double>()))
               .AddMember(new StructMember("J3", typeFactory.GetPrimitiveType<double>()))
               .AddMember(new StructMember("J4", typeFactory.GetPrimitiveType<double>()))
               .AddMember(new StructMember("J5", typeFactory.GetPrimitiveType<double>()))
               .AddMember(new StructMember("J6", typeFactory.GetPrimitiveType<double>()))
               .Create();

            Reader = DDSHandler.SetupDataReader("UnitySolutionTopic", UnitySolutionTopic);
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

                var J1 = data.GetValue<double>("J1");
                var J2 = data.GetValue<double>("J2");
                var J3 = data.GetValue<double>("J3");
                var J4 = data.GetValue<double>("J4");
                var J5 = data.GetValue<double>("J5");
                var J6 = data.GetValue<double>("J6");

                RobotHandler.UrInputs.input_double_register_20 = J1;
                RobotHandler.UrInputs.input_double_register_21 = J2;
                RobotHandler.UrInputs.input_double_register_22 = J3;
                RobotHandler.UrInputs.input_double_register_23 = J4;
                RobotHandler.UrInputs.input_double_register_24 = J5;
                RobotHandler.UrInputs.input_double_register_25 = J6;
            }
        }
    }
}