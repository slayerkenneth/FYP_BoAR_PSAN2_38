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
using UnityEngine.Events;

public class GameFlowController : MonoBehaviour
{
    public enum PVEBattleSceneState
    {
        Invalid,
        Scanning,
        ScanCompleted,
        ColliderBuilt,
        SpawningEnemy,
        SpawningTower,
        SpawningGameObject,
        SpawningPlayer,
        UISetup,
        PushCarBattleMode,
        CapturePointMode,
        DefencePointMode,
        DungeonMode,
        BossFight,
        WinBattle,
        LossBattle,
        MapActive,
        ShopActive,
        EventActive,
        BattleConclusion,
        ChapterEnd
    }

    [Header("Game Flow variable")] 
    [SerializeField] public PVEBattleSceneState battleSceneState = PVEBattleSceneState.Invalid;
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
    public GameObject BattleUICanvasParent;
    public HealthBarUI PlayerHealthBarUI;
    public HealthBarUI EnemyHealthBarUI;
    public GameObject WinPopUpWindow;
    public GameObject LossPopUpWindow;

    [Header("Global Player status")]
    [SerializeField] public PlayerStatus playerGlobalStatus;
    [SerializeField] public static int EnemyKillCount = 0;

    [Header("Shop")] 
    [SerializeField] public GameObject ShopUIParent;
    
    [Header("Map")]
    [SerializeField] public GameObject MapParent;
    
    [Header("DefenseTower")]
    [SerializeField] public GameObject DefenseTowerParent;
    
    [Header("CaptureTower")]
    [SerializeField] public GameObject CaptureTowerParent;
    
    [Header("Dungeon")]
    [SerializeField] public GameObject DungeonParent;
    
    [Header("PushCar")]
    [SerializeField] public GameObject PushCarParent;
    
    [Header("BossFight")]
    [SerializeField] public GameObject BossFightParent;

    [Header("Events and Actions")]
    [SerializeField] UnityEvent DummyEvent;
    [SerializeField] Action DummyAction;


    public GameObject cloneTower;
    
    // Start is called before the first frame update
    // Set battleSceneState = Scanning
    void Start()
    {
        // Init or Get static Overall Player stats
        playerGlobalStatus = PlayerStatus.CurrentPlayer;
        
        // Scene State init
        // Set battleSceneState = Invalid
        // To be triggered by map Selection: On Click the icon
         battleSceneState = PVEBattleSceneState.Invalid;

        // Map Coordinate, (wall) list init
        CoordinatesAdjacencyList = new Dictionary<Vector2Int, List<Vector2Int>>();
        AllGridNodeCoordinates = new List<Vector2Int>();
        WallCoordinates = new List<Vector2Int>();
    }
    
    // Old Version
    // Update is called once per frame
    // void Update() // Old Version
    // {
    //     // Check if the game is end
    //     CheckGameEndCondition();
    //     
    //     // AreaText.text = "A:" + _activeGameboard.Area + " A / tile: " + _activeGameboard.Area / _activeGameboard.Settings.TileSize + battleSceneState;
    //     // var srcPosition = Utils.PositionToTile(tempPosition, _activeGameboard.Settings.TileSize);
    //     
    //     // if no active gameboard leave loop
    //     if (_activeGameboard == null) return;
    //
    //     // Area scanned is enough for the map
    //     if (_activeGameboard.Area / _activeGameboard.Settings.TileSize >= testScanArea)
    //     {
    //         // Start getting the gameboard map tiles
    //         SpatialTree = _activeGameboard.GetSpatialTree();
    //         if (PlayerSpawnActive == false)
    //         {
    //             // make following function called once only
    //             
    //             // change scene state
    //             battleSceneState = PVEBattleSceneState.ScanCompleteForColliderBuilding;
    //             
    //             // Build up and set up all walls and coordinates
    //             GetAllTileCoordinatesAndMarkWalls();
    //             SetBoundaryColliders();
    //             
    //             // Set enemy spawner On
    //             EnemySpawner.SetSpawner(true);
    //             SetRandomEnemySpawnLocationVectors(enemyRandomSpawnLocationsCount);
    //             
    //             // Set Tower Spawner On
    //             battleSceneState = PVEBattleSceneState.SpawningTower;
    //         }
    //         
    //         // Set Player spawn active
    //         PlayerSpawnActive = true;
    //     }
    // }

    void Update()
    {
        // Game Flow: 
        // Chapter selection
        // Event (Story) 
        // Scene scanning: handled by AR Controller
        
        // Map generation and selection 
        // !~! Warning: Now Skipped Collider Generation, later before testing need to re-implement the collider logics
        if (battleSceneState == PVEBattleSceneState.ScanCompleted)
        {
            GetAllTileCoordinatesAndMarkWalls();
        }
        
        // Only if all map tile and coordinates are confirm, the game flow continues
        if (!MapCoordinatesConfirmed) return;
        
        // Battle (entering Battle status): 4 main mode + Boss fight
        if (battleSceneState is 
            (PVEBattleSceneState.CapturePointMode or PVEBattleSceneState.DefencePointMode
            or PVEBattleSceneState.DungeonMode or PVEBattleSceneState.PushCarBattleMode or PVEBattleSceneState.BossFight))
        {
            // init
            
            // update
            
            // Conclusion State
        }

        // Map selection  (returns to map)
        if (battleSceneState is PVEBattleSceneState.MapActive)
        {
            
        }
        // Shop / Event (story) mode
        
        // Save character build
        
        // Chapter selection
    }
    
    
    #region Maptiles, Map boundaries And Collider Setting
    /*
     * Instantiate All wall boundary colliders
     * Then Set battleSceneState = PVEBattleSceneState.ColliderBuilt
     */
    void SetBoundaryColliders() // disable all border control like what government did (sorry actually becoz it's buggy
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
        
        // scene state done collider
        battleSceneState = PVEBattleSceneState.ColliderBuilt;
    }

    /*
     * MapCoordinatesConfirmed then not go into the loop: only call once
     * From spatial tree of the gameboard
     * scan through from - Area Limit to Area Limit
     * Find all tile coordinate and mark wall coordinates
     * Wall confirmed = true
     */
    public void GetAllTileCoordinatesAndMarkWalls()
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
    
    /*
     * Set battleSceneState = PVEBattleSceneState.SpawningEnemy
     * return EnemySpawnPositionList
     */
    public List<Vector3> GetEnemySpawnLocationVectorList()
    {
        return EnemySpawnPositionList;
    }

    /*
     * Select and add random non wall coordinates as enemy spawn point
     * Param max position count
     * Set Position List
     * with y-axis coordinate of 2f
     * 
     * Set battleSceneState = BattleMode;
     */
    private void SetRandomEnemySpawnLocationVectors(int MaxRandomSpawnPositionCount)
    {
        int count = 0;

        var pos = new Vector3();
        while (_activeGameboard.FindRandomPosition(out pos) && count != MaxRandomSpawnPositionCount)
        {
            var v = Utils.PositionToTile(pos, _activeGameboard.Settings.TileSize);
            if (!WallCoordinates.Contains(v) && AllGridNodeCoordinates.Contains(v))
            {
                EnemySpawnPositionList.Add(new Vector3(v.x * _activeGameboard.Settings.TileSize, 2f, v.y * _activeGameboard.Settings.TileSize));
                count++;
            }
        }

        battleSceneState = BattleMode;
    }
    #endregion
    
    #region Map Route Node Generation and Selection

    public void EnterMapNode(LevelType levelType)
    {
        if (battleSceneState == PVEBattleSceneState.Invalid)
        {
            // Wait until Game board session is init and started
            StartCoroutine(IsGameboardReady()); 
            // Then Start Scanning
            battleSceneState = PVEBattleSceneState.Scanning;
            StartCoroutine(WaitForScanComplete());
        }
        switch (levelType)
        {
            /*
             * Battle Sessions
             */
            case LevelType.CapturePointBattleMode:
                battleSceneState = PVEBattleSceneState.CapturePointMode;
                break;
            
            case LevelType.DefensePointBattleMode:
                battleSceneState = PVEBattleSceneState.DefencePointMode;
                break;
            
            case LevelType.PushCarBattleMode:
                battleSceneState = PVEBattleSceneState.PushCarBattleMode;
                break;
            
            case LevelType.DungeonMode:
                battleSceneState = PVEBattleSceneState.DungeonMode;
                break;
            
            /*
             * Shop, event, Boss session
             */
            case LevelType.Shop:
                battleSceneState = PVEBattleSceneState.ShopActive;
                break;
            
            case LevelType.Event:
                battleSceneState = PVEBattleSceneState.EventActive;
                break;
            
            case LevelType.Boss:
                battleSceneState = PVEBattleSceneState.BossFight;
                break;
        }
    }


    IEnumerator IsGameboardReady()
    {
        yield return new WaitUntil(() => _activeGameboard != null);
    }

    IEnumerator WaitForScanComplete()
    {
        yield return new WaitUntil(() => battleSceneState == PVEBattleSceneState.ScanCompleted);

    }
    #endregion

    #region Capture Point / Defense Point
    
    /*
     * Find Random position if it is in correct mode
     * check if it's in the valid map tile
     * 
     */
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
                    towerLocation = new Vector3(v.x * _activeGameboard.Settings.TileSize, -2f, v.y * _activeGameboard.Settings.TileSize); 
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

    #region Dungeon Mode

    

    #endregion
    
    #region Push Car Mode

    

    #endregion
    
    #region BossFight Mode

    

    #endregion

    #region Flow Condition Check

    private void CheckGameEndCondition()
    {
        // no player exist or inside battle conclusion stage then no need to check
        if (playerMovementCtrl == null || battleSceneState == PVEBattleSceneState.BattleConclusion) return;
        if (playerMovementCtrl.GetPlayerCombatHandler().GetCurrentHP() <= 0)
        {
            battleSceneState = PVEBattleSceneState.LossBattle;
            DebugText.text = " Player Died and loss! Back to Start Point";
            playerGlobalStatus.currentLevel = 0;
            LossRestartFromBeginning();
            battleSceneState = PVEBattleSceneState.BattleConclusion;
            return;
        }
        
        if (BattleMode == PVEBattleSceneState.CapturePointMode)
        {
            
        }
        else if (BattleMode == PVEBattleSceneState.DefencePointMode)
        {
            if (Tower.GetRemainingTime() <= 0)
            {
                battleSceneState = PVEBattleSceneState.WinBattle;
                DebugText.text = " Player Win !";
                RewardNextStage();
                battleSceneState = PVEBattleSceneState.BattleConclusion;

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

    #region End Battle Sequence Conclusion(Reward or Die to return start)

    public void RewardNextStage()
    {
        playerGlobalStatus.money++;
        playerGlobalStatus.speed++;
        playerGlobalStatus.currentHP = (int) playerMovementCtrl.GetPlayerCombatHandler().GetCurrentHP();
        // playerGlobalStatus.currentLevel++; // cannot change this
        playerGlobalStatus.normalAttackDamage++;
        playerGlobalStatus.specialAttackDamage++;
        
        WinPopUpWindow.SetActive(true);
    }

    public void LossRestartFromBeginning()
    {
        LossPopUpWindow.SetActive(true);
    }


    public void ReturningToMapScene()
    {
        // WinPopUpWindow.SetActive(false);
        // LossPopUpWindow.SetActive(false);
        SceneManager.LoadScene(1);
    }

    public void ReturnToTitlePage()
    {
        SceneManager.LoadScene(0);
    }
    #endregion

    #region ShopUI

    

    #endregion
}
