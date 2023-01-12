using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;

public class DefenceTarget : MonoBehaviour
{
    [Header("Gameboard and other controller reference")]
    public IGameboard _activeGameboard;
    public GameFlowController GameFlowCtrl;
    public ARController arCtrl;

    [Header("Tower Parameters")] 
    private float hp = 100f;
    public float DefenceRange;
    public GameObject TowerPrefab;
    public Vector3 TowerPosition;
    [SerializeField] private float DefenceTime;

    [Header("UI")] 
    public Canvas DefenceModeUICanvas;

    void Update()
    {
        SpawnTower();
        DefenceTimerUpdate();
    }

    private void SpawnTower()
    {
        if (GameFlowCtrl.GetTowerSpawnLocationVector(out TowerPosition) && GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
        {
            Instantiate(TowerPrefab, TowerPosition, new Quaternion(0, 0, 0, 0), transform);
        }
    }

    public IEnumerable DefenceTimerUpdate()
    {
        yield return new WaitForSeconds(1);
    }
}
