using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeMonkey.HealthSystemCM;
using Niantic.ARDK.Extensions.Gameboard;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFlowController : MonoBehaviour
{
    public enum PVEBattleSceneState
    {
        invalid,
        Scanning,
        ScanCompleteForColliderBuilding,
        ColliderBuilt,
        SpawningEnemy,
        SpawningTower,
        SpawningGameObject,
        SpawningPlayer,
        GameModeSelection,
        UISetup,
        PushCarBattleMode,
        CapturePointMode,
        DefencePointMode,
        DungeonMode,
        BossFight,
        Win,
        Loss
    }

    [Header("Game Flow variable")] 
    [SerializeField] public PVEBattleSceneState battleSceneState = PVEBattleSceneState.invalid;
    public bool PlayerSpawnActive = false;
    public PVEBattleSceneState BattleMode;
    
    [Header("Other Controller Reference")] 
    [SerializeField] private Camera arCamera;
    [SerializeField] private ARController ARCtrl;
    [SerializeField] private CharacterMovementController playerMovementCtrl;
    [SerializeField] private CentralBattleController CentralBattleCtrl;
    [SerializeField] private CombatHandler playerOwnCombatHandler;
    [SerializeField] private DefenceTarget DefTarget;
    
    [Header("Debug logs")] 
    [SerializeField] private Text DebugText;
    [SerializeField] private Text AreaText;
    
    [Header("Gameboard environment reference")] 
    [SerializeField] public static IGameboard _activeGameboard; //Consider make a getter setter function instead
    [SerializeField] public SpatialTree SpatialTree;
    [SerializeField] public float AreaLimit = 100f;
    public float testScanArea;
    public List<Vector2Int> AllGridNodeCoordinates;
    public Dictionary<Vector2Int, List<Vector2Int>> CoordinatesAdjacencyList;
    public List<Vector2Int> WallCoordinates;
    private bool MapCoordinatesConfirmed = false;
    [SerializeField] private int maxX = 0, minX = 0, maxZ = 0, minZ = 0;

    public GameObject InvisibleWallPrefab;

    [Header("Enemy Related")] 
    public EnemySpawner EnemySpawner;
    public List<Vector3> EnemySpawnPositionList;
    public int enemyRandomSpawnLocationsCount;

    [Header("Defense Point reference")] 
    public DefenceTarget Tower;

    [Header("UI / Canvas elements")] 
    public HealthBarUI PlayerHealthBarUI;
    public HealthBarUI EnemyHealthBarUI;
    public GameObject WinPopUpWindow;
    public GameObject LossPopUpWindow;

    [Header("Global Player status")]
    [SerializeField] public PlayerStatus playerGlobalStatus;
    [SerializeField] public static int EnemyKillCount = 0;

    


    public GameObject cloneTower;
    
    // Start is called before the first frame update
    void Start()
    {
        playerGlobalStatus = PlayerStatus.CurrentPlayer;
        if (_activeGameboard != null)
        {
            battleSceneState = PVEBattleSceneState.Scanning;
        }
        CoordinatesAdjacencyList = new Dictionary<Vector2Int, List<Vector2Int>>();
        AllGridNodeCoordinates = new List<Vector2Int>();
        WallCoordinates = new List<Vector2Int>();
    }
    
    // Update is called once per frame
    void Update()
    {
        CheckGameEndCondition();
        AreaText.text = "A:" + _activeGameboard.Area + " A / tile: " + _activeGameboard.Area / _activeGameboard.Settings.TileSize + battleSceneState;
        // var srcPosition = Utils.PositionToTile(tempPosition, _activeGameboard.Settings.TileSize);
        if (_activeGameboard == null) return;

        if (_activeGameboard.Area / _activeGameboard.Settings.TileSize >= testScanArea)
        {
            SpatialTree = _activeGameboard.GetSpatialTree();
            if (PlayerSpawnActive == false)
            {
                // make following function called once only
                battleSceneState = PVEBattleSceneState.ScanCompleteForColliderBuilding;
                GetAllTileCoordinatesAndMarkWalls();
                SetBoundaryColliders();
                EnemySpawner.SetSpawner(true);
                SetRandomEnemySpawnLocationVectors(enemyRandomSpawnLocationsCount);
                battleSceneState = PVEBattleSceneState.SpawningTower;
            }
            PlayerSpawnActive = true;
        }
    }

    #region Maptiles, Map boundaries And Collider Setting
    void SetBoundaryColliders()
    {
        var WallSet = Instantiate(InvisibleWallPrefab, new Vector3(0, -0.1f, 0), new Quaternion());
        WallSet.GetComponent<BoxCollider>().enabled = false;
        foreach (var nodeCoord in WallCoordinates)
        {
            var wall = Instantiate(InvisibleWallPrefab, new Vector3(nodeCoord.x * _activeGameboard.Settings.TileSize,
                                                                                        0, 
                                                                                        nodeCoord.y * _activeGameboard.Settings.TileSize), new Quaternion(), WallSet.transform);
            wall.transform.localScale = new Vector3(1f,1f,1f);
            var wallBox = wall.GetComponent<BoxCollider>();
            wallBox.size = new Vector3(_activeGameboard.Settings.TileSize, 30, _activeGameboard.Settings.TileSize);
            wallBox.center = new Vector3(_activeGameboard.Settings.TileSize / 2, 0, _activeGameboard.Settings.TileSize / 2);
            
            // disable all border control like what government did (sorry actually becoz it's buggy
            wall.SetActive(false);
        }
        battleSceneState = PVEBattleSceneState.ColliderBuilt;
    }

    public void GetAllTileCoordinatesAndMarkWalls() // Problematic if Area < 4, from the beginning is buggy and wrong to use the AreaLimit
    {
        if (SpatialTree == null || MapCoordinatesConfirmed) return;

        for (var i = -AreaLimit; i < AreaLimit; i++)
        {
            for (var j = -AreaLimit; j < AreaLimit; j++)
            {
                var node = new GridNode();
                if (SpatialTree.GetElement(new Vector2Int((int) i, (int)j), out node))
                {
                    var currentV2I = new Vector2Int(node.Coordinates.x, node.Coordinates.y);
                    AllGridNodeCoordinates.Add(currentV2I);
                    var dummyPoints = new List<Vector3>();
                    var neighbourCount = 4;
                    dummyPoints.Add(new Vector3((i + 1) * _activeGameboard.Settings.TileSize, 0 , (j + 1) * _activeGameboard.Settings.TileSize));
                    dummyPoints.Add(new Vector3((i + 1) * _activeGameboard.Settings.TileSize, 0 , (j - 1) * _activeGameboard.Settings.TileSize));
                    dummyPoints.Add(new Vector3((i - 1) * _activeGameboard.Settings.TileSize, 0 , (j + 1) * _activeGameboard.Settings.TileSize));
                    dummyPoints.Add(new Vector3((i - 1) * _activeGameboard.Settings.TileSize, 0 , (j - 1) * _activeGameboard.Settings.TileSize));
                    
                    var AdjList = new List<Vector2Int>();
                    foreach (var dp in dummyPoints)
                    {
                        var nnn = new GridNode();
                        if (!SpatialTree.GetElement(Utils.PositionToTile(dp, _activeGameboard.Settings.TileSize), out nnn))
                        {
                            neighbourCount -= 1;
                        }
                        else
                        {
                            AdjList.Add(nnn.Coordinates);
                        }
                    }
                    CoordinatesAdjacencyList.Add(currentV2I, AdjList);

                    if (neighbourCount < 4) WallCoordinates.Add(currentV2I);
                    if (node.Coordinates.x >= maxX) maxX = node.Coordinates.x;
                    if (node.Coordinates.x <= minX) minX = node.Coordinates.x;
                    if (node.Coordinates.y >= maxZ) maxZ = node.Coordinates.y;
                    if (node.Coordinates.y <= minZ) minZ = node.Coordinates.y;
                }
            }
        }
        MapCoordinatesConfirmed = true;
    }

    public List<Vector2Int> GetWallCoordinates()
    {
        return WallCoordinates;
    }
    #endregion

    #region Enemy Spawning
    public List<Vector3> GetEnemySpawnLocationVectorList()
    {
        battleSceneState = PVEBattleSceneState.SpawningEnemy;
        return EnemySpawnPositionList;
    }

    private void SetRandomEnemySpawnLocationVectors(int MaxRandomSpawnPositionCount)
    {
        int count = 0;

        var pos = new Vector3();
        while (_activeGameboard.FindRandomPosition(out pos) && count != MaxRandomSpawnPositionCount)
        {
            var v = Utils.PositionToTile(pos, _activeGameboard.Settings.TileSize);
            if (!WallCoordinates.Contains(v) && AllGridNodeCoordinates.Contains(v))
            {
                EnemySpawnPositionList.Add(new Vector3(v.x * _activeGameboard.Settings.TileSize, -1f, v.y * _activeGameboard.Settings.TileSize));
                count++;
            }
        }

        battleSceneState = BattleMode;
    }
    #endregion

    #region CapturePoint / Defence Point
    public bool GetTowerSpawnLocationVector(out Vector3 towerLocation)
    {
        towerLocation = new Vector3();
        if (BattleMode is PVEBattleSceneState.CapturePointMode or PVEBattleSceneState.DefencePointMode)
        {
            // towerLocation = CalculateTowerLocation();
            _activeGameboard.FindRandomPosition(out towerLocation);
                var v = Utils.PositionToTile(towerLocation, _activeGameboard.Settings.TileSize);
                if (!WallCoordinates.Contains(v) && AllGridNodeCoordinates.Contains(v))
                {
                    towerLocation = new Vector3(v.x * _activeGameboard.Settings.TileSize, -1.15f, v.y * _activeGameboard.Settings.TileSize); 
                }
            towerLocation = CalculateTowerLocation();
            // if (DefTarget.GetSpawnedTower() != null) cloneTower = DefTarget.GetSpawnedTower();
            return true;
        }
        return false;
    }

    private Vector3 CalculateTowerLocation()
    {
        float x, z;
        var TPos = new Vector3();
        x = (maxX + minX) / 2 * _activeGameboard.Settings.TileSize;
        z = (maxZ + minZ) / 2 * _activeGameboard.Settings.TileSize;
        _activeGameboard.FindNearestFreePosition(new Vector3(x, -1, z), out TPos);
        return TPos;
    }
    #endregion

    #region Flow Condition Check

    private void CheckGameEndCondition()
    {
        if (playerMovementCtrl == null) return;
        if (playerMovementCtrl.GetPlayerCombatHandler().GetCurrentHP() <= 0)
        {
            battleSceneState = PVEBattleSceneState.Loss;
            DebugText.text = " Player Died and loss! Back to Start Point";
            playerGlobalStatus.currentLevel = 0;
            LossRestartFromBeginning();
            return;
        }
        
        if (BattleMode == PVEBattleSceneState.CapturePointMode)
        {
            
        }
        else if (BattleMode == PVEBattleSceneState.DefencePointMode)
        {
            if (Tower.GetRemainingTime() <= 0)
            {
                battleSceneState = PVEBattleSceneState.Win;
                DebugText.text = " Player Win !";
                RewardNextStage();
                return;
            }
            
        }
        else if (BattleMode == PVEBattleSceneState.DungeonMode)
        {
            
        }
        else if (BattleMode == PVEBattleSceneState.PushCarBattleMode)
        {
            
        }
        else
        {
            // not possible
        }
    }

    #endregion
    
    #region Getter / Setter
    public CharacterMovementController GetPlayerMovementCtrl ()
    {
        var movementCtrl = ARCtrl.GetActivePlayerMovementCtrl();
        playerMovementCtrl = movementCtrl;
        return movementCtrl;
    }

    public CentralBattleController GetCentralBattleController()
    {
        return CentralBattleCtrl;
    }

    public void SetDefenseTower(DefenceTarget tower)
    {
        Tower = tower;
    }

    public void SetCurrentEnemyBeenAttacked(CombatHandler enemyCombatHandler)
    {
        EnemyHealthBarUI.SetHealthSystem(enemyCombatHandler.GetHealthSystemComponent().GetHealthSystem());
    }

    public GameObject GetCloneTower()
    {
        return cloneTower;
    }

    public ARController getARCtrl()
    {
        return ARCtrl;
    }

    public CharacterMovementController getPlayerMovementCtrl()
    {
        return playerMovementCtrl;
    }

    public void SetPlayerMovementCtrl(CharacterMovementController cmc)
    {
        playerMovementCtrl = cmc;
    }
    #endregion

    #region End Battle Sequence (Reward or Die to return start)

    public void RewardNextStage()
    {
        playerGlobalStatus.money++;
        playerGlobalStatus.speed++;
        playerGlobalStatus.currentHP = (int) playerMovementCtrl.GetPlayerCombatHandler().GetCurrentHP();
        // playerGlobalStatus.currentLevel++; // cannot change this
        playerGlobalStatus.normalAttackDamage++;
        playerGlobalStatus.specialAttackDamage++;
        
        // WinPopUpWindow.SetActive(true);
        ReturningToMapScene();
    }

    public void LossRestartFromBeginning()
    {
        // LossPopUpWindow.SetActive(true);
        ReturningToMapScene();
    }


    public void ReturningToMapScene()
    {
        // WinPopUpWindow.SetActive(false);
        // LossPopUpWindow.SetActive(false);
        SceneManager.LoadScene(1);
    }
    #endregion
}
