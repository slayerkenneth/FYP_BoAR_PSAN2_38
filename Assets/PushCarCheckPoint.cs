using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushCarCheckPoint : MonoBehaviour
{
    public static List<PushCarCheckPoint> GlobalCheckPoints = new List<PushCarCheckPoint>();
    public bool CarPassed = false;
    // Start is called before the first frame update
    void Start()
    {
        GlobalCheckPoints.Add(this);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PushItem"))
        {
            CarPassed = true;
        }
    }
}
