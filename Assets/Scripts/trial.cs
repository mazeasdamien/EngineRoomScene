using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class trial : MonoBehaviour
{
    public Toggle trialToggle;

    public List<GameObject> expe;
    public List<GameObject> trialObj;

    // Update is called once per frame
    void Update()
    {
        if (trialToggle.isOn)
        {
            foreach (GameObject o in expe)
            { 
            o.SetActive(false);
            }
            foreach (GameObject o in trialObj)
            {
                o.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject o in expe)
            {
                o.SetActive(true);
            }
            foreach (GameObject o in trialObj)
            {
                o.SetActive(false);
            }
        }
        
    }
}
