using DDS_protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isHolding = false;
    public MoveRobot_Sub MoveRobot_Sub;
    public OutOfRqngeHelper OutOfRqngeHelper_Sub;
    public bool ispressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        // The button is being held down.
        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // The button is no longer being held down.
        isHolding = false;
    }

    void Update()
    {
        if(MoveRobot_Sub.ColorModee == 3 && gameObject.name != OutOfRqngeHelper_Sub.opposite_pressed.name)
        {
            isHolding = false;
        }

        if (isHolding)
        {
            ispressed = true;
        }
        else
        {
            ispressed = false;
        }
    }
}