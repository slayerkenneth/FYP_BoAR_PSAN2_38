using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathDest : MonoBehaviour
{
    public GameFlowController.PVEBattleSceneState battleMode = GameFlowController.PVEBattleSceneState.Invalid;
    
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (battleMode == GameFlowController.PVEBattleSceneState.DungeonMode)
        {
            if (other.transform.CompareTag("Player"))
            {
                
            }
        }
        else if (battleMode == GameFlowController.PVEBattleSceneState.PushCarBattleMode)
        {
            if (other.transform.CompareTag("PushItem"))
            {
                
            }
        }
        
    }

    public void SetBattleMode(GameFlowController.PVEBattleSceneState mode)
    {
        battleMode = mode;
    }
}
