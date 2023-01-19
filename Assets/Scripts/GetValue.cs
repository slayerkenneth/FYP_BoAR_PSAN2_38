using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetValue : MonoBehaviour
{
    public GameFlowController GameFlowCtrl;
    private Vector3 towerPosition;
    // public DefenceTarget target;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Vector3 getTowerPosition()
    {
        return towerPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
        {
            GameFlowCtrl.GetTowerSpawnLocationVector(out towerPosition);
        }
    }
}
