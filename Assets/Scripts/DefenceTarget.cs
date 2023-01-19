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

    [Header("Tower Parameters")] 
    private float hp = 100f;
    public float DefenceRange;
    public GameObject TowerPrefab;
    public Vector3 TowerPosition;
    [SerializeField] private float DefenceTime;
    private bool stopWatchActive;
    private float CurrentTime;
    public int startMinute;
    

    [Header("UI")] 
    public Canvas DefenceModeUICanvas;
    public Text CurrentTimeText;

    private void Start()
    {
        CurrentTime = DefenceTime;
    }

    void Update()
    {
        //Debug.Log(TowerPosition);
        SpawnTower();
        if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningPlayer) StartCountDown();
        if (stopWatchActive == true)
        {
            CurrentTime = CurrentTime - Time.deltaTime;
            if (CurrentTime <= 0)
            {
                stopWatchActive = false;
                
            }
        }
        TimeSpan timeSpan = TimeSpan.FromSeconds(CurrentTime);
        CurrentTimeText.text = "Defense Time Remaining: " + timeSpan.ToString(@"mm\:ss");
    }

    private void SpawnTower()
    {
        if (GameFlowCtrl.GetTowerSpawnLocationVector(out TowerPosition) && GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
        {
            Instantiate(TowerPrefab, TowerPosition, new Quaternion(0, 0, 0, 0), transform);
        }
    }

    public Vector3 getTowerPosition()
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
}
