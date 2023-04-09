using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathDest : MonoBehaviour
{
    public GameFlowController.PVEBattleSceneState battleMode = GameFlowController.PVEBattleSceneState.Invalid;
    public GameFlowController GameFlowCtrl;
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
                GameFlowCtrl.BattleEndFlag = true;
            }
        }
        else if (battleMode == GameFlowController.PVEBattleSceneState.PushCarBattleMode)
        {
            if (other.transform.CompareTag("PushItem"))
            {
                GameFlowCtrl.BattleEndFlag = true;
            }
        }
        
    }

    public void SetBattleMode(GameFlowController.PVEBattleSceneState mode, GameFlowController that)
    {
        GameFlowCtrl = that;
        battleMode = mode;
    }
}
