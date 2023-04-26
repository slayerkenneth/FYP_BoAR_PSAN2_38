using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaptureProgressSlider : MonoBehaviour
{
    public Slider slider;
    public CaptureTargetSpawner ctSpawner;
    

    void Update()
    {
        slider.value = ctSpawner.captureProgress / ctSpawner.captureRequirement;
    }
}
