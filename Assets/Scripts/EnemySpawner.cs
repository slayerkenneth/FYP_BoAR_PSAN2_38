using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;
using CodeMonkey.HealthSystemCM;
using UnityEditor;

public class EnemySpawner : MonoBehaviour
{
    [Header("Controllers, Gameboard, Environment Reference")]
    [SerializeField] private ARController ARCtrl;
    [SerializeField] private GameFlowController GameFlowCtrl;
    private IGameboard _activeGameboard;
    [SerializeField] private CentralBattleController centralBattleCtrl;

    [Header("Spawner Setting")] 
    [SerializeField] public List<GameObject> EnemySpawnPrefabList;
    [SerializeField] public List<Vector3> EnemyAtkTowerPositionList;
    [SerializeField] public GameObject EnemyParentObj;
    [SerializeField] public int MaxEnemyCount;
    public List<Vector3> EnemySpawnLocationList;
    private bool EnemySpawnEnable = false;
    public List<GameObject> activeEnemies;

    public DefenceTarget DefenceTarget;
    public CaptureTargetSpawner captureTargetSpawner;
    private Vector3 towerPosition;
    public LayerMask whatIsGround; //ARDK_Gameboard
    public LayerMask whatIsPlayer; //add a layer mask on player
    public LayerMask whatIsEnemy;
    public GameObject towerPrefab;
    public GameObject playerPrefab;
    public static List<EventHandler> OnEnemyDead = new List<EventHandler>();

    void Start()
    {
        _activeGameboard = ARCtrl.GetActiveGameboard();
    }
    
    void Update()
    {
        // for (int i=0; i < EnemySpawnLocationList.Count; i++)
        // {
        //     if (!EnemySpawnEnable || EnemySpawnPrefabList.Count == 0 || EnemySpawnLocationList.Count == 0 || activeEnemies.Count >= MaxEnemyCount) return;
        //     
        //     if (GameFlowCtrl.BattleMode is GameFlowController.PVEBattleSceneState.DefencePointMode)
        //     {
        //         if (DefenceTarget == null) return; // Optimize later
        //         DefenceTarget.GetTowerPointsTransforms().ForEach(t =>
        //         {
        //             EnemyAtkTowerPositionList.Add(t.position);
        //         });
        //         StartCoroutine(SpawnEnemyAfterWaiting(1000, EnemySpawnPrefabList[i], EnemySpawnLocationList[i], EnemyAtkTowerPositionList[i]));
        //     }
        //     else if (GameFlowCtrl.BattleMode is GameFlowController.PVEBattleSceneState.CapturePointMode)
        //     {
        //         if (captureTargetSpawner == null) return; // Optimize later
        //         captureTargetSpawner.GetTowerPointsTransforms().ForEach(t =>
        //         {
        //             EnemySpawnLocationList.Add(t.position);        
        //         });
        //         // Need to review enemyPathfinding logic
        //         // Some Bug here!
        //         StartCoroutine(SpawnEnemyAfterWaiting(1000, EnemySpawnPrefabList[i], EnemySpawnLocationList[i], Vector3.zero));
        //     }
        //     else
        //     {
        //         // Need to review enemyPathfinding logic
        //         StartCoroutine(SpawnEnemyAfterWaiting(1000, EnemySpawnPrefabList[i], EnemySpawnLocationList[i], Vector3.zero));
        //     }
        // }
        //
    }

    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 SpawnLocationVec)
    {
        return Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(), EnemyParentObj.transform);
    }
    
    public IEnumerator SpawnEnemyAfterWaiting(float time, GameObject enemyPrefab, Vector3 SpawnLocationVec, Vector3 AtkTowerPos)
    {
        var e = Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(0,0,0,0), EnemyParentObj.transform);
        e.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        e.AddComponent<EnemyPathfinding>();
        e.GetComponent<EnemyBehavior>().GameFlowCtrl = GameFlowCtrl;
        e.GetComponent<EnemyPathfinding>().GameFlowCtrl = GameFlowCtrl;
        e.GetComponent<EnemyPathfinding>().AtkTowerPosition = AtkTowerPos;
        e.GetComponent<EnemyPathfinding>().whatIsGround = whatIsGround;
        e.GetComponent<EnemyPathfinding>().whatIsPlayer = whatIsPlayer;
        e.GetComponent<CombatHandler>().SetCentralCombatHandler(centralBattleCtrl);
        // e.GetComponent<EnemyPathfinding>().playerPrefab = playerPrefab;
        e.GetComponent<EnemyPathfinding>().whatIsEnemy = whatIsEnemy; 
        var healthSystem = e.GetComponent<HealthSystemComponent>().GetHealthSystem();
        foreach (EventHandler eventHandler in OnEnemyDead)
        {
            healthSystem.OnDead += eventHandler;
        }
        yield return new WaitForSeconds(time);
    }

    public void SetSpawner(bool sw)
    {
        EnemySpawnEnable = sw;
        if (!sw)
        {
            GameFlowCtrl.ResetEnemySpawnLocationList();
            ResetEnemySpawnLocationList();
            return;
        }
        GameFlowCtrl.SetRandomEnemySpawnLocationVectors(MaxEnemyCount);
        EnemySpawnLocationList = GameFlowCtrl.GetEnemySpawnLocationVectorList();
    }

    public Vector3 getTowerPosition()
    {
        return towerPosition;
    }

    private void ResetEnemySpawnLocationList()
    {
        EnemySpawnLocationList.Clear();
    }

    public void ClearEnemyOnScene()
    {
        activeEnemies.ForEach(Destroy);
        activeEnemies.Clear();
    }

    public void SetDefenseTarget(DefenceTarget dt)
    {
        DefenceTarget = dt;
    }

    public void SetCaptureTarget(CaptureTargetSpawner ct)
    {
        captureTargetSpawner = ct;
    }

    public void ResetTowerTargetReference()
    {
        DefenceTarget = null;
        captureTargetSpawner = null;
    }
}
