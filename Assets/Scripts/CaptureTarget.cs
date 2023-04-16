using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CaptureTargert : MonoBehaviour
{
    public CaptureTargetSpawner spawner;
    
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!spawner.enemyCollidersInsideTower.Contains(other))
            {
                spawner.enemyCollidersInsideTower.Add(other);
            }
        }
        if (other.CompareTag("Player") && spawner.enemyCollidersInsideTower.Count == 0)
        {
            spawner.captureProgress += Time.deltaTime;
        }

        if (spawner.captureProgress >= spawner.captureRequirement)
        {
            spawner.GameFlowCtrl.BattleEndFlag = true;
        }
    }
}