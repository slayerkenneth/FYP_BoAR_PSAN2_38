using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CaptureTargetSpawner : MonoBehaviour
{
    [Header("Gameboard and other controller reference")]
    public IGameboard _activeGameboard;
    public GameFlowController GameFlowCtrl;
    public ARController ARCtrl;

    [Header("C Tower Parameters")] 
    private float hp = 100f;
    public GameObject TowerPrefab;
    public Vector3 TowerPosition;
    private bool hasTowerSpawned = false;
    public CombatHandler towerCombatHandler;
    private GameObject SpawnedTower;
    public List<Transform> TowerPointsTransforms;
    public List<Collider> enemyCollidersInsideTower;
    public float captureProgress;
    public float captureRequirement;

    [Header("UI")] 
    public Canvas CaptureModeUICanvas;

    private void Start()
    {
        GameFlowCtrl.SetCaptureTower(this);
    }

    private void Update()
    {
        enemyCollidersInsideTower.RemoveAll(item => item == null);
    }

    public void SpawnTower()
    {
        if (GameFlowCtrl.GetTowerSpawnLocationVector(out TowerPosition) && !hasTowerSpawned && GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningTower)
        {
            SpawnedTower = Instantiate(TowerPrefab, TowerPosition, new Quaternion(0, 0, 0, 0), transform);
            var ct = SpawnedTower.AddComponent<CaptureTargert>();
            ct.spawner = this;
            TowerPointsTransforms = SpawnedTower.GetComponent<Tower>().GetTowerPoints();
            GameFlowCtrl.battleSceneState = GameFlowController.PVEBattleSceneState.CapturePointMode;
            hasTowerSpawned = true;
            GameFlowCtrl.cloneTower = SpawnedTower;
        }
        // Adjust DTower Y-axis:
        // if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningPlayer)
        // {
        //     var CalibrateY = GameFlowCtrl.GetPlayerMovementCtrl().transform.position.y;
        //     if (CalibrateY <= 0)
        //         activeTower.transform.localPosition = new Vector3(activeTower.transform.localPosition.x, CalibrateY, activeTower.transform.localPosition.z);
        // }
    }

    public List<Transform> GetTowerPointsTransforms()
    {
        return TowerPointsTransforms;
    }
}
