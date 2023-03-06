using DDS_protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class RobotHandler : MonoBehaviour
{
    public static UniversalRobot_Outputs UrOutputs = new UniversalRobot_Outputs();
    public static UniversalRobot_Inputs UrInputs = new UniversalRobot_Inputs();
    public static RtdeClient Ur3 = new RtdeClient();
    public string ipAdress = null!;
    public Toggle IsSimulator;


    public void StartApp()
    {
        if (IsSimulator.isOn)
        {
            ipAdress = "192.168.56.103";
        }
        else 
        {
            ipAdress = "169.254.56.120";
        }

        Ur3.Connect(ipAdress, 2);

        Ur3.Setup_Ur_Inputs(UrInputs);
        Ur3.Setup_Ur_Outputs(UrOutputs, 150);
        Ur3.Ur_ControlStart();

        UrInputs.input_double_register_20 = Math.PI / 180 * -96;
        UrInputs.input_double_register_21 = Math.PI / 180 * -60;
        UrInputs.input_double_register_22 = Math.PI / 180 * -90;
        UrInputs.input_double_register_23 = Math.PI / 180 * -110;
        UrInputs.input_double_register_24 = Math.PI / 180 * 90;
        UrInputs.input_double_register_25 = Math.PI / 180 * 0;

        var m = "def unity():\n" +
"  set_tcp(p[0,0,0.1493,0,0,0])\n" +
" while (True):\n" +
"  new_pose = [read_input_float_register(20), read_input_float_register(21), read_input_float_register(22), read_input_float_register(23), read_input_float_register(24), read_input_float_register(25)]\n" +
"  servoj(new_pose, t = 0.45, lookahead_time = 1, gain = 350)\n" +
"  sync()\n" +
" end\n" +
"end\n";

        RtdeClient.URscriptCommand(ipAdress, m);

        Ur3.OnDataReceive += Ur3_OnDataReceive;
    }

    private void Ur3_OnDataReceive(object sender, EventArgs e)
    {
    }

    private void OnApplicationQuit()
    {
        Ur3.Disconnect();
    }
}
