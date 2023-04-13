using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushCarController : MonoBehaviour
{
    public GameObject CarPrefab;
    public GameObject ActiveCar;
    public GameFlowController GameFlowController;

    private void Start()
    {
        SpawnCar();
        var Position = PushCarCheckPoint.GlobalCheckPoints[1].transform.position;
        ActiveCar.transform.position = Position;
    }

    public void Update()
    {
        if (PushCarCheckPoint.GlobalCheckPoints.Count < 1) return;
        if (PushCarCheckPoint.GlobalCheckPoints.TrueForAll(car => car.CarPassed))
        {
            GameFlowController.BattleEndFlag = true;
            PushCarCheckPoint.GlobalCheckPoints.ForEach(i=> Destroy(i.transform));
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
