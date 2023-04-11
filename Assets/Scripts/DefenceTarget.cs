using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;
using UnityEngine.UI;

public class DefenceTarget : MonoBehaviour
{
    [Header("Gameboard and other controller reference")]
    public IGameboard _activeGameboard;
    public GameFlowController GameFlowCtrl;
    public ARController arCtrl;

    [Header("DTower Parameters")] 
    private float hp = 100f;
    public float DefenceRange;
    public GameObject TowerPrefab;
    public Vector3 TowerPosition;
    [SerializeField] private float DefenceTime;
    private bool stopWatchActive;
    private float CurrentTime;
    private bool hasTowerSpawned = false;
    public int startMinute;
    public CombatHandler towerCombatHandler;
    private GameObject SpawnedTower;
    public List<Transform> TowerPointsTransforms;


    [Header("UI")] 
    public Canvas DefenceModeUICanvas;
    public Text CurrentTimeText;

    private void Start()
    {
        CurrentTime = DefenceTime;
        GameFlowCtrl.SetDefenseTower(this);
    }

    void Update()
    {
        //Debug.Log(TowerPosition);
        if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningPlayer) StartCountDown();
        if (stopWatchActive)
        {
            CurrentTime = CurrentTime - Time.deltaTime;
            if (CurrentTime <= 0)
            {
                stopWatchActive = false;
                GameFlowCtrl.BattleEndFlag = true;
            }
        }
        TimeSpan timeSpan = TimeSpan.FromSeconds(CurrentTime);
        CurrentTimeText.text = "Defense Time Remaining: " + timeSpan.ToString(@"mm\:ss");
    }

    public void SpawnTower()
    {
        if (GameFlowCtrl.GetTowerSpawnLocationVector(out TowerPosition) && !hasTowerSpawned && GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningTower)
        {
            SpawnedTower = Instantiate(TowerPrefab, TowerPosition, new Quaternion(0, 0, 0, 0), transform);
            TowerPointsTransforms = SpawnedTower.GetComponent<Tower>().GetTowerPoints();
            GameFlowCtrl.battleSceneState = GameFlowController.PVEBattleSceneState.DefencePointMode;
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

    public GameObject GetSpawnedTower()
    {
        return SpawnedTower;
    }

    public Vector3 GetTowerPosition()
    {
        return TowerPosition;
    }

    public void StartCountDown()
    {
        stopWatchActive = true;
    }

    public void StopCountDown()
    {
        stopWatchActive = false;
    }

    public float GetRemainingTime()
    {
        return CurrentTime;
    }

    public List<Transform> GetTowerPointsTransforms()
    {
        return TowerPointsTransforms;
    }
}
