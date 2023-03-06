using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScenarioManager : MonoBehaviour
{
    public GameObject OriginalPipes;
    public GameObject BrokenPipes;
    public GameObject Rust1;
    public GameObject MissingPartScenario1;

    public GameObject Rust2;
    public GameObject BrokenPart2;

    public GameObject s1p1;
    public GameObject s1p2;
    public GameObject s1p3;
    public GameObject s1p4;

    public TMP_Text stepCounter;
    public TMP_Text buttonScenario;
    public TMP_Text distanceInfo;

    public Toggle VR;
    public Toggle Monitor;

    public List<Toggle> toggles;


    public GameObject takePhotoButton;

    public GameObject DefaultToReport;

    public Orbit_Camera orbit;


    public enum scenarios
    {
        scenario1,
        scenario2,
    }

    public enum scenario1_parts
    {
        start,
        p1,
        p2,
        p3,
        p4
    }

    public scenarios scenario; 
    public scenario1_parts parts;

    private void Update()
    {
        if (VR.isOn)
        {
            scenario = scenarios.scenario2;
        }
        else if (Monitor.isOn)
        {
            scenario = scenarios.scenario1;
        }

        switch (scenario)
        {
            case scenarios.scenario1:
                OriginalPipes.SetActive(false);
                BrokenPipes.SetActive(true);
                Rust1.SetActive(true);
                MissingPartScenario1.SetActive(false);
                BrokenPart2.SetActive(false);
                Rust2.SetActive(false);
                break;
            case scenarios.scenario2:
                OriginalPipes.SetActive(true);
                BrokenPipes.SetActive(false);
                Rust1.SetActive(false);
                MissingPartScenario1.SetActive(true);
                BrokenPart2.SetActive(true);
                Rust2.SetActive(true);
                break;
        }

        switch (parts)
        {
            case scenario1_parts.start:
                takePhotoButton.SetActive(false);
                DefaultToReport.SetActive(false);
                stepCounter.text = "";
                buttonScenario.text = "START";
                distanceInfo.text = "";
                s1p1.SetActive(false);
                s1p2.SetActive(false);
                s1p3.SetActive(false);
                s1p4.SetActive(false);
                break;
            case scenario1_parts.p1:
                orbit.target = s1p1.transform;
                DefaultToReport.SetActive(true);
                foreach (Toggle toggle in toggles)
                {
                    if (toggle.isOn == true)
                    {
                        takePhotoButton.SetActive(true);
                    }
                }
                stepCounter.text = "STEP 1 / 4";
                buttonScenario.text = "NEXT STEP [2]";
                distanceInfo.text = "15 cm";
                s1p1.SetActive(true);
                s1p2.SetActive(false);
                s1p3.SetActive(false);
                s1p4.SetActive(false);
                break;
            case scenario1_parts.p2:
                orbit.target = s1p2.transform;
                DefaultToReport.SetActive(true);
                foreach (Toggle toggle in toggles)
                {
                    if (toggle.isOn == true)
                    {
                        takePhotoButton.SetActive(true);
                    }
                }
                stepCounter.text = "STEP 2 / 4";
                buttonScenario.text = "NEXT STEP [3]";
                distanceInfo.text = "10 cm";
                s1p1.SetActive(false);
                s1p2.SetActive(true);
                s1p3.SetActive(false);
                s1p4.SetActive(false);
                break;
            case scenario1_parts.p3:
                orbit.target = s1p3.transform;
                DefaultToReport.SetActive(true);
                foreach (Toggle toggle in toggles)
                {
                    if (toggle.isOn == true)
                    {
                        takePhotoButton.SetActive(true);
                    }
                }
                stepCounter.text = "STEP 3 / 4";
                buttonScenario.text = "NEXT STEP [4]";
                distanceInfo.text = "14 cm";
                s1p1.SetActive(false);
                s1p2.SetActive(false);
                s1p3.SetActive(true);
                s1p4.SetActive(false);
                break;
            case scenario1_parts.p4:
                orbit.target = s1p4.transform;
                DefaultToReport.SetActive(true);
                foreach (Toggle toggle in toggles)
                {
                    if (toggle.isOn == true)
                    {
                        takePhotoButton.SetActive(true);
                    }
                }
                stepCounter.text = "STEP 4 / 4";
                buttonScenario.text = "SAVE AND EXIT";
                distanceInfo.text = "15 cm";
                s1p1.SetActive(false);
                s1p2.SetActive(false);
                s1p3.SetActive(false);
                s1p4.SetActive(true);
                break;
        }
    }

    public void UnselectAllToggles()
    {
        // Set the isOn property of each toggle to false
        foreach (Toggle toggle in toggles)
        {
            toggle.isOn = false;
        }
    }
}
