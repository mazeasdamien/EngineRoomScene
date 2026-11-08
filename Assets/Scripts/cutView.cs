using DDS_protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cutView : MonoBehaviour
{
    public Transform camera;
    public Transform inspectionCamera;
    public Transform robot;
    public MoveRobot_Sub MoveRobot_Sub;

    void Update()
    {
        camera.position = new Vector3(inspectionCamera.position.x, camera.position.y, camera.position.z);
        robot.localPosition = new Vector3((float)MoveRobot_Sub.XValue, robot.localPosition.y, robot.localPosition.z);
    }
}
