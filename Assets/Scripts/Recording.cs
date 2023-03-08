using System.IO;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ScenarioManager;

namespace DDS_protocol
{
    public class Recording : MonoBehaviour
    {
        private string filePath;
        public MoveRobot_Sub moveRobot_Sub;
        public GameObject popup;
        public TMP_InputField id;
        public Toggle VR;
        public Toggle MONITOR;
        private float time = 0f;
        private bool isRunning;
        public string folderPath;
        public ScenarioManager SM;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                popup.SetActive(true);
            }

            if (isRunning)
            {
                time += Time.deltaTime;
            }
        }

        public void StartStopExperiment()
        {
            if (SM.parts == scenario1_parts.start)
            {

                isRunning = true;
                popup.SetActive(false);

                folderPath = $"{Application.dataPath}/Participants_data/participant_{id.text}";
                Directory.CreateDirectory(folderPath);


                    filePath = $"{folderPath}/log_MONITOR.txt";

                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath)) { }
                }

                SM.parts = scenario1_parts.p1;

                // start of part
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("part1");
                }

                // record position every 0.5 seconds
                InvokeRepeating("RecordPosition", 0.0f, 0.5f);
            }
            else if(SM.parts == scenario1_parts.p1)
            {
                // start of part
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("part1 end collisions count " + moveRobot_Sub.collisionNumbers);
                    writer.WriteLine("part2");
                }
                SM.parts = scenario1_parts.p2;
            }
            else if (SM.parts == scenario1_parts.p2)
            {
                // start of part
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("part2 end collisions count " + moveRobot_Sub.collisionNumbers);
                    writer.WriteLine("part3");
                }
                SM.parts = scenario1_parts.p3;
            }
            else if (SM.parts == scenario1_parts.p3)
            {
                // start of part
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("part3 end collisions count " + moveRobot_Sub.collisionNumbers);
                    writer.WriteLine("part4");
                }
                SM.parts = scenario1_parts.p4;
            }
            else if (SM.parts == scenario1_parts.p4)
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("part4 end collisions count " + moveRobot_Sub.collisionNumbers);
                }
                isRunning = false;
                CancelInvoke("RecordPosition");
                time = 0;
                Application.Quit();
            }
        }

        void RecordPosition()
        {
            // write position to file
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(
                    Math.Round(time, 2)
                    + "," + Math.Round(moveRobot_Sub.XValue, 2)
                    + "," + Math.Round(moveRobot_Sub.Ikx, 2)
                    + "," + Math.Round(moveRobot_Sub.Iky, 2)
                    + "," + Math.Round(moveRobot_Sub.Ikz, 2)
                    + "," + Math.Round(RobotHandler.UrOutputs.actual_q[0] * Mathf.Rad2Deg, 2)
                    + "," + Math.Round(RobotHandler.UrOutputs.actual_q[1] * Mathf.Rad2Deg, 2)
                    + "," + Math.Round(RobotHandler.UrOutputs.actual_q[2] * Mathf.Rad2Deg, 2)
                    + "," + Math.Round(RobotHandler.UrOutputs.actual_q[3] * Mathf.Rad2Deg, 2)
                    + "," + Math.Round(RobotHandler.UrOutputs.actual_q[4] * Mathf.Rad2Deg, 2)
                    + "," + Math.Round(RobotHandler.UrOutputs.actual_q[5] * Mathf.Rad2Deg, 2));
            }
        }
    }
}