using UnityEngine;
using Rti.Dds.Publication;
using Rti.Types.Dynamic;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine.UI;
using DDS_protocol;

public class TeleopCommands : MonoBehaviour
{
    private protected DataWriter<DynamicData> Writer { get; private set; }
    private DynamicData sample = null;
    private bool init = false;


    public ButtonPressed Rleft;
    public ButtonPressed Lleft;
    public ButtonPressed Left;
    public ButtonPressed Right;
    public ButtonPressed Forward;
    public ButtonPressed Backward;
    public ButtonPressed zoomin;
    public ButtonPressed zoomout;
    public ButtonPressed t1;
    public ButtonPressed t2;
    public ButtonPressed t3;
    public ButtonPressed t4;

    public Toggle VR;

    void Update()
    {
        if (!init)
        {
            init = true;

            var typeFactory = DynamicTypeFactory.Instance;

            var Teleop = typeFactory.BuildStruct()
                .WithName("DirectTeleop")
                .AddMember(new StructMember("Rleft", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("Lleft", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("Left", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("Right", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("Forward", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("Backward", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("zoomin", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("zoomout", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("t1", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("t2", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("t3", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("t4", typeFactory.GetPrimitiveType<bool>()))
                .AddMember(new StructMember("isVR", typeFactory.GetPrimitiveType<bool>()))
                .Create();

            Writer = DDSHandler.SetupDataWriter("DirectTeleopTopic", Teleop);
            sample = new DynamicData(Teleop);
        }

        sample.SetValue("Rleft", Rleft.ispressed);
        sample.SetValue("Lleft", Lleft.ispressed);
        sample.SetValue("Left", Left.ispressed);
        sample.SetValue("Right", Right.ispressed);
        sample.SetValue("Forward", Forward.ispressed);
        sample.SetValue("Backward", Backward.ispressed);
        sample.SetValue("zoomin", zoomin.ispressed);
        sample.SetValue("zoomout", zoomout.ispressed);
        sample.SetValue("t1", t1.ispressed);
        sample.SetValue("t2", t2.ispressed);
        sample.SetValue("t3", t3.ispressed);
        sample.SetValue("t4", t4.ispressed);
        if (VR.isOn)
        {
            sample.SetValue("isVR", true);
        }
        else
        {
            sample.SetValue("isVR", false);
        }
        Writer.Write(sample);
    }
}