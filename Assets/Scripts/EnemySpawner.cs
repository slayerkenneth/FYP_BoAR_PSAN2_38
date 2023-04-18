using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;
using CodeMonkey.HealthSystemCM;
using Unity.VisualScripting;
using UnityEditor;
using Random = UnityEngine.Random;

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
    public float spawnInterval;
    public float spawnTimeCounter;

    public DefenceTarget DefenceTarget;
    public CaptureTargetSpawner captureTargetSpawner;
    private Vector3 towerPosition;
    public LayerMask whatIsGround; //ARDK_Gameboard
    public LayerMask whatIsPlayer; //add a layer mask on player
    public LayerMask whatIsEnemy;
    public GameObject towerPrefab;
    public GameObject playerPrefab;
    public static List<EventHandler> OnEnemyDead = new List<EventHandler>();

    public Tower ActiveTower;
    void Start()
    {
        
    }
    
    void Update()
    {
        // location list ready, counter start
        if (spawnTimeCounter >= 0 && EnemySpawnEnable)
        {
            spawnTimeCounter -= Time.deltaTime;
            return;
        }

        if (EnemySpawnEnable && spawnTimeCounter <= 0)
        {
            if (GameFlowCtrl.BattleMode is GameFlowController.PVEBattleSceneState.DefencePointMode)
            {
                if (ActiveTower == null) ActiveTower = DefenceTarget.GetComponentInChildren<Tower>();
                ActiveTower.TowerPointsTransforms.ForEach(t =>
                {
                    var towerpos = t.position;
                    if (!EnemyAtkTowerPositionList.Contains(towerpos)) EnemyAtkTowerPositionList.Add(towerpos);
                });
                // Not Ready for spawning any enemy
                if (EnemySpawnLocationList.Count == 0) return;
                
                for (int i = 1; i < MaxEnemyCount; i++)
                {
                    var Enemy = GetRandomEnemyFromPrefabList();
                    if (Enemy == null) break;
                    SpawnEnemy(Enemy,
                        EnemySpawnLocationList[i % EnemySpawnLocationList.Count],
                        EnemyAtkTowerPositionList[i % EnemyAtkTowerPositionList.Count]);
                }
            } 
            else if (GameFlowCtrl.BattleMode is GameFlowController.PVEBattleSceneState.CapturePointMode)
            {
                if (ActiveTower == null) ActiveTower = captureTargetSpawner.ActiveTower;
                ActiveTower.TowerPointsTransforms.ForEach(t =>
                {
                    var pos = new Vector3(t.position.x, -1.1f, t.position.z);
                    if (!EnemySpawnLocationList.Contains(pos)) EnemySpawnLocationList.Add(pos);
                });
                
                // Not Ready for spawning any enemy
                if (EnemySpawnLocationList.Count == 0) return;
                for (int i = 1; i < MaxEnemyCount; i++)
                {
                    var Enemy = GetRandomEnemyFromPrefabList();
                    if (Enemy == null) break;
                    SpawnEnemy(Enemy,
                        EnemySpawnLocationList[i % EnemySpawnLocationList.Count],
                        Vector3.zero);
                }
            }
            else if (GameFlowCtrl.BattleMode is GameFlowController.PVEBattleSceneState.PushCarBattleMode)
            {
                // Not Ready for spawning any enemy
                if (EnemySpawnLocationList.Count == 0) return;
                // spawn according to the list 
                for (int i = 1; i < MaxEnemyCount; i++)
                {
                    var Enemy = GetRandomEnemyFromPrefabList();
                    if (Enemy == null) break;
                    SpawnEnemy(Enemy,
                        EnemySpawnLocationList[i % EnemySpawnLocationList.Count],
                        Vector3.zero);
                }
                // spawn more when the main point is nearer to player
                Vector3 NearestTile = GameFlowCtrl.ActiveRouteTilesLocations.Find(vec3 =>
                {
                    return Vector3.Distance(vec3, GameFlowCtrl.GetPlayerLocation()) <=
                        2 * GameFlowCtrl.MainPointDistanceVariance;
                });
                var EnemyExtra = GetRandomEnemyFromPrefabList();
                if (EnemyExtra == null) return;
                SpawnEnemy(EnemyExtra,NearestTile, Vector3.zero);
            }
            else if (GameFlowCtrl.BattleMode is GameFlowController.PVEBattleSceneState.DungeonMode)
            {
                // Not Ready for spawning any enemy
                if (EnemySpawnLocationList.Count == 0) return;
                // spawn according to the list 
                for (int i = 1; i < MaxEnemyCount; i++)
                {
                    SpawnEnemy(GetRandomEnemyFromPrefabList(),
                        EnemySpawnLocationList[i % EnemySpawnLocationList.Count],
                        Vector3.zero);
                }
                // spawn more when the main point is nearer to player
                Vector3 NearestTile = GameFlowCtrl.ActiveRouteTilesLocations.Find(vec3 =>
                {
                    return Vector3.Distance(vec3, GameFlowCtrl.GetPlayerLocation()) <=
                           2 * GameFlowCtrl.MainPointDistanceVariance;
                });
                SpawnEnemy(GetRandomEnemyFromPrefabList(),NearestTile, Vector3.zero);
            }
            
            // reset spawn count down timer
            spawnTimeCounter = spawnInterval;
        }
        // for (int i=0; i < EnemySpawnLocationList.Count; i++)
        // {
        //     if (!EnemySpawnEnable || EnemySpawnPrefabList.Count == 0 (not possible) || EnemySpawnLocationList.Count == 0|| activeEnemies.Count >= MaxEnemyCount) return;
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

    // public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 SpawnLocationVec)
    // {
    //     return Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(), EnemyParentObj.transform);
    // }
    
    public void SpawnEnemy(GameObject enemyPrefab, Vector3 SpawnLocationVec, Vector3 AtkTowerPos)
    {
        var e = Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(0,0,0,0), EnemyParentObj.transform);
        e.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        e.AddComponent<EnemyPathfinding>();
        e.GetComponent<EnemyBehavior>().GameFlowCtrl = GameFlowCtrl;
        e.GetComponent<EnemyPathfinding>().GameFlowCtrl = GameFlowCtrl;
        // e.GetComponent<EnemyPathfinding>().enemyID = ID;
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
        activeEnemies.Add(e);
    }

    public void SetEnemySpawner(bool sw)
    {
        _activeGameboard = ARCtrl.GetActiveGameboard();
        EnemySpawnEnable = sw;
        if (!sw)
        {
            ActiveTower = null;
            ResetEnemySpawnLocationList();
            return;
        }
        // SetRandomEnemySpawnLocationVectors(MaxEnemyCount);
        switch (GameFlowCtrl.BattleMode)
        {
            case GameFlowController.PVEBattleSceneState.DefencePointMode:
                MaxEnemyCount = 5;
                spawnInterval = MaxEnemyCount * 5;
                GameFlowCtrl.ActiveDefenseTowerParent.SetActive(true);
                DefenceTarget = GameFlowCtrl.ActiveDefenseTowerParent.GetComponentInChildren<DefenceTarget>();
                EnemyAtkTowerPositionList.Clear();

                SetRandomEnemySpawnLocation(MaxEnemyCount);
                break;
            
            case GameFlowController.PVEBattleSceneState.CapturePointMode:
                MaxEnemyCount = 5;
                spawnInterval = MaxEnemyCount * 8;
                GameFlowCtrl.ActiveCaptureTowerParent.SetActive(true);
                captureTargetSpawner =
                    GameFlowCtrl.ActiveCaptureTowerParent.GetComponentInChildren<CaptureTargetSpawner>();
                
                EnemyAtkTowerPositionList.Clear();
                break;
            
            case GameFlowController.PVEBattleSceneState.DungeonMode:
                MaxEnemyCount = 5;
                spawnInterval = MaxEnemyCount * 5;
                break;
            
            case GameFlowController.PVEBattleSceneState.PushCarBattleMode:
                MaxEnemyCount = 5;
                spawnInterval = MaxEnemyCount * 5;
                break;
        }
    }

    private GameObject GetRandomEnemyFromPrefabList()
    {
        if (EnemySpawnPrefabList.Count == 0) return null;
        int seed = Random.Range(0, EnemySpawnPrefabList.Count);
        return EnemySpawnPrefabList[seed];
    }
    
    public void SetRandomEnemySpawnLocation(int MaxRandomSpawnPositionCount)
    {
        int count = 0;
        Vector3 pos = new Vector3();
        while (_activeGameboard.FindRandomPosition(out pos) && count != MaxRandomSpawnPositionCount)
        {
            var v = Utils.PositionToTile(pos, _activeGameboard.Settings.TileSize);
            if (!GameFlowCtrl.WallCoordinates.Contains(v) 
                && GameFlowCtrl.AllGridNodeCoordinates.Contains(v)) 
            {
                EnemySpawnLocationList.Add(new Vector3(v.x * _activeGameboard.Settings.TileSize, -1.1f, v.y * _activeGameboard.Settings.TileSize));
                count++;
            }
        }
    }

    private void ResetEnemySpawnLocationList()
    {
        EnemySpawnLocationList.Clear();
        spawnTimeCounter = 0;
    }

    public void ClearEnemyOnScene()
    {
        activeEnemies.ForEach(Destroy);
        var wait = new WaitUntil( () => activeEnemies.Count == 0);
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
