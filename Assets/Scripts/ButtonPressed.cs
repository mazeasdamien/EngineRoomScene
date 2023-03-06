using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isHolding = false;
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