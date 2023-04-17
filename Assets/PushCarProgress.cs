using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PushCarProgress : MonoBehaviour
{
    public Slider slider;
    public int PassedPointCount;
    public int totalPointCount;

    void Update()
    {
        int temp = 0;
        totalPointCount = PushCarCheckPoint.GlobalCheckPoints.Count;
        slider.maxValue = totalPointCount;
        PushCarCheckPoint.GlobalCheckPoints.ForEach(point =>
        {
            if (point.CarPassed) temp++;
        });
        PassedPointCount = temp;
        slider.value = PassedPointCount;
    }
}
