using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PushCarController : MonoBehaviour
{
    public GameObject CarPrefab;
    public GameObject ActiveCar;
    public GameFlowController GameFlowController;
    
    private bool gameEnd = true;
    private void Start()
    {
        SpawnCar();
        var Position = PushCarCheckPoint.GlobalCheckPoints[0].transform.position;
        ActiveCar.transform.position = Position;
        ActiveCar.GetComponent<PushTarget>().SetOrientation(PushCarCheckPoint.GlobalCheckPoints[1].transform);
    }

    public void Update()
    {
        // Assume game end 
        gameEnd = true;
        PushCarCheckPoint.GlobalCheckPoints.ForEach(point =>
        {
            if (!point.CarPassed) gameEnd = false;
        });

        if (gameEnd)
        {
            GameFlowController.BattleEndFlag = true;
            PushCarCheckPoint.GlobalCheckPoints.ForEach(o=>Destroy(o.GameObject()));
            PushCarCheckPoint.GlobalCheckPoints.Clear();
        }
    }

    public void SpawnCar()
    {
        ActiveCar = Instantiate(CarPrefab, new Vector3(0, -1f, 0), new Quaternion(0, 0, 0, 0), transform);
    }

    public void DespawnCar()
    {
        Destroy(ActiveCar);
        ActiveCar = null;
    }

    public void SetCheckPoint(List<GameObject> checkpoints)
    {
        ActiveCar.GetComponent<PushTarget>().checkPoints = checkpoints;
    }
}
