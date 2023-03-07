using DDS_protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfRqngeHelper : MonoBehaviour
{
    public List<ButtonPressed> buttonPresseds = new List<ButtonPressed>();

    public GameObject last_pressed;
    public GameObject opposite_pressed;

    public MoveRobot_Sub moveRobot_Sub;

    void Update()
    {
        if (moveRobot_Sub.ColorModee == 1 || moveRobot_Sub.ColorModee == 2)
        {
            activeAll();
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.ispressed)
                {
                    last_pressed = button.gameObject;
                    getOpposite();
                }
            }
        }
        else if (moveRobot_Sub.ColorModee == 3)
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject != opposite_pressed)
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }

    public void activeAll()
    {
        foreach (ButtonPressed button in buttonPresseds)
        {
            button.gameObject.SetActive(true);
        }
    }

    public void getOpposite()
    {

        if (last_pressed.name == "right")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "left")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "left")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "right")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "forward")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "backward")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "backward")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "forward")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "DOWN")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "UP")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "UP")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "DOWN")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "1")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "4")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "4")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "1")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "2")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "3")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }

        if (last_pressed.name == "3")
        {
            foreach (ButtonPressed button in buttonPresseds)
            {
                if (button.gameObject.name == "2")
                {
                    opposite_pressed = button.gameObject;
                }
            }
        }
    }
}