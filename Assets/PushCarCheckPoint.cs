using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushCarCheckPoint : MonoBehaviour
{
    public static List<PushCarCheckPoint> GlobalCheckPoints = new List<PushCarCheckPoint>();
    public bool CarPassed = false;
    public PushCarCheckPoint nextCheckPoint;
    // Start is called before the first frame update
    void Start()
    {
        GlobalCheckPoints.Add(this);
        if (GlobalCheckPoints.Count > 1)
        {
            GlobalCheckPoints[GlobalCheckPoints.Count - 2].nextCheckPoint = this;
        }
    }
}
