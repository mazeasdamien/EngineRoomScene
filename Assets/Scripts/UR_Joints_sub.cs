using DDS_protocol;
using Rti.Dds.Subscription;
using Rti.Types.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UR_Joints_sub : MonoBehaviour
{
    public List<Transform> robot = new List<Transform>();

    void Update()
    {
        robot[0].localEulerAngles = new Vector3(0, 0, -(float)ConvertRadiansToDegrees(RobotHandler.UrOutputs.actual_q[0]));
        robot[1].localEulerAngles = new Vector3(-90, 0, -(float)ConvertRadiansToDegrees(RobotHandler.UrOutputs.actual_q[1]));
        robot[2].localEulerAngles = new Vector3(0, 0, -(float)ConvertRadiansToDegrees(RobotHandler.UrOutputs.actual_q[2]));
        robot[3].localEulerAngles = new Vector3(0, 0, -(float)ConvertRadiansToDegrees(RobotHandler.UrOutputs.actual_q[3]));
        robot[4].localEulerAngles = new Vector3(-90, 0, -(float)ConvertRadiansToDegrees(RobotHandler.UrOutputs.actual_q[4]));
        robot[5].localEulerAngles = new Vector3(90, 0, -(float)ConvertRadiansToDegrees(RobotHandler.UrOutputs.actual_q[5]));
    }

    public static double ConvertRadiansToDegrees(double radians)
    {
        double degrees = (180 / Math.PI) * radians;
        return degrees;
    }
}
