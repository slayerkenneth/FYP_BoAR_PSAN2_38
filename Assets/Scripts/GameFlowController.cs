using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeMonkey.HealthSystemCM;
using Niantic.ARDK.Extensions.Gameboard;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class GameFlowController : MonoBehaviour
{
    public enum SceneTheme
    {
        Cyberpunk,
        PostApocalypse,
        Fantasy,
        Xianxia,
        Modern
    }
    
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
    [SerializeField] public bool BattleEndFlag = false;

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
    public DefenceTarget DTower;

    [Header("Capture Point reference")] public CaptureTarget CTower;

    [Header("AR theme")] public SceneTheme currentSceneTheme;

    [Header("Route, Item Gen Reference")] 
    public float MaxRouteDisplacement;
    public float MinRouteDisplacement;
    public float AverageMainPointDistance;
    public float MainPointDistanceVariance;
    public List<Vector3> PossibleRouteMainEndPoints;
    public List<Vector3> PossibleRouteMainSecPoints;
    public List<Vector3> ActiveRouteTilesLocations;
    public List<GameObject> ActiveRouteTiles;
    public GameObject TileParent;
    public int MaxTilesCount;
    public GameObject CyberpunkTilePrefab,
        PostApocalypseTilePrefab,
        FantasyTilePrefab,
        XianxiaTilePrefab,
        ModernTilePrefab;

    [Header("UI / Canvas elements")] 
    public GameObject BattleUICanvasParent;
    public HealthBarUI PlayerHealthBarUI;
    public HealthBarUI EnemyHealthBarUI;
    public GameObject WinPopUpWindow;
    public GameObject LossPopUpWindow;
    public GameObject BackgroundCanvasParent;
    public GameObject startFightUI;

    [Header("Global Player status")]
    [SerializeField] public PlayerStatus playerGlobalStatus;
    [SerializeField] public PlayerSpawner PlayerSpawner;
    [SerializeField] public static int EnemyKillCount = 0;
    [SerializeField] private Vector3 PlayerSpawnLocation;
    [SerializeField] public GameObject PlayerSpawnParent;

    [Header("Shop")] 
    [SerializeField] public GameObject ShopUIParent;
    
    [Header("Map")]
    [SerializeField] public GameObject MapParent;
    
    [Header("DefenseTower")]
    [SerializeField] public GameObject DefenseTowerParentPrefab;
    [SerializeField] public GameObject ActiveDefenseTowerParent;
    [SerializeField] private DefenceTarget DefTarget;
    
    [Header("CaptureTower")]
    [SerializeField] public GameObject CaptureTowerParentPrefab;
    [SerializeField] public GameObject ActiveCaptureTowerParent;
    [SerializeField] private CaptureTarget CapTarget;
    
    [Header("Dungeon")]
    [SerializeField] public GameObject DungeonParentPrefab;
    [SerializeField] public GameObject ActiveDungeonParent;
    [SerializeField] public float tileInterval;
    [SerializeField] public GameObject EndDungeonTile;
    
    [Header("PushCar")]
    [SerializeField] public GameObject PushCarParentPrefab;
    [SerializeField] public GameObject ActivePushCarParent;
    [SerializeField] private PushTarget PushTarget;
    [SerializeField] public GameObject CheckPointPrefab;
    [SerializeField] public List<GameObject> CarCheckPointGameObjects;

    [Header("BossFight")]
    [SerializeField] public GameObject BossFightParentPrefab;
    [SerializeField] public GameObject ActiveBossFightParent;

    [Header("Events and Actions")]
    [SerializeField] UnityEvent DummyEvent;
    [SerializeField] Action DummyAction;

    public bool restartBattle = false;
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

        EnemySpawnPositionList = new List<Vector3>();
        
        BattleUICanvasParent.SetActive(false);
        startFightUI.SetActive(false);
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
    //             // Set DTower Spawner On
    //             battleSceneState = PVEBattleSceneState.SpawningTower;
    //         }
    //         
    //         // Set Player spawn active
    //         PlayerSpawnActive = true;
    //     }
    // }
    private void CheckPlayerAndGameEndCondition()
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
        }
    }

    void Update()
    {
        // Game Flow: 
        // Chapter selection
        // Event (Story) 
        // Scene scanning: handled by AR Controller
        
        // Map generation and selection 
        // !~! Warning: Now Skipped Collider Generation, later before testing need to re-implement the collider logics
        
        // // now Only call once in AR controller after scan completed, so this if condition check no longer needed
        if (battleSceneState == PVEBattleSceneState.ScanCompleted)
        {
            // now Only call once in AR controller after scan completed
            // GetAllTileCoordinatesAndMarkWalls();
            startFightUI.SetActive(true);
            GenerateFixedMapRoutePointsList();
        }
        
        // Only if all map tile and coordinates are confirm, the game flow continues
        if (!MapCoordinatesConfirmed) return;
        
        // battleSceneState = BattleMode;

        /*
         * Battle Sessions(entering Battle status): 4 main mode + Boss fight
         */
        if (battleSceneState is PVEBattleSceneState.CapturePointMode)
        {
            // update loop
            /* CP
             * - Spawn DTower (not Enabled)
             *   -->Set DTower to Capture Mode, Enemy Spawn Point nearby the tower, the 4 pillars
             *  - Activate Capture Percentage Gauge
             *   - Spawn Enemy (not Enabled)
             *   - Spawn (Object Recognition Related) buff or Items (not Enabled)
             *   - Ask start? Start Waiting
             *   - Start by spawn Player, and enable all spawned elements
             *  - Enable UIs: Capture Gauge, player HP bar...
             * - Loop
             *   -->Detect game end condition
             */
            CheckPlayerAndGameEndCondition();

            if (BattleEndFlag)
            {
                battleSceneState = PVEBattleSceneState.WinBattle;
                DebugText.text = " Player Win !";
                Destroy(ActiveCaptureTowerParent);
                RewardNextStage();
                battleSceneState = PVEBattleSceneState.BattleConclusion;
            }
        }
        
        else if (battleSceneState is PVEBattleSceneState.DefencePointMode)
        {
            // update loop
            /* DP
             * Defense Point
                - Get all spawn locations
                - Spawn Tower (not Enabled)
                -->Set Tower to defense Mode
                - Activate Objective gauge (Tower HP percentage & Time remaining)
                - Spawn Enemy (not Enabled)
                - Spawn (Object Recognition Related) buff or Items (not Enabled)
                - Ask start? Start Waiting
                - Start by spawn Player, and enable all spawned elements
                - Enable UIs: Tower Gauge, player HP bar...
             * - Loop
             *   -->Detect game end condition
             */
            CheckPlayerAndGameEndCondition();

            if (BattleEndFlag)
            {
                battleSceneState = PVEBattleSceneState.WinBattle;
                DebugText.text = " Player Win !";
                Destroy(ActiveDefenseTowerParent);
                RewardNextStage();
                battleSceneState = PVEBattleSceneState.BattleConclusion;
            }
            // Conclusion State
        }
        else if (battleSceneState is PVEBattleSceneState.DungeonMode)
        {
            // update loop
            /* D
             * - Get all spawn locations
             * - Set Start and exit point, pre-set routes
                - Spawn Enemy
                - Spawn (Object Recognition Related) buff or Items (not Enabled)
                - Ask start? Start Waiting
                - Start by spawn Player, and enable all spawned elements
                - Enable UIs: player HP bar...
             * - Loop
             *   -->Detect game end condition
             */
            CheckPlayerAndGameEndCondition();

            if (BattleEndFlag)
            {
                battleSceneState = PVEBattleSceneState.WinBattle;
                DebugText.text = " Player Win !";
                ClearActiveRouteTiles();
                Destroy(ActiveDungeonParent);
                RewardNextStage();
                battleSceneState = PVEBattleSceneState.BattleConclusion;
            }
            // Conclusion State
        }
        
        else if (battleSceneState is PVEBattleSceneState.PushCarBattleMode)
        {
            // update loop
            /* PC
             * - Get all spawn locations
                - spawn Car and car push route
                - Spawn Enemy
                - Spawn (Object Recognition Related) buff or Items (not Enabled)
                - Ask start? Start Waiting
                - Start by spawn Player, and enable all spawned elements
                - Enable UIs: push progress bar (Objective gauge), player HP bar...
             * - Loop
             *   -->Detect game end condition
             */
            CheckPlayerAndGameEndCondition();

            if (BattleEndFlag)
            {
                battleSceneState = PVEBattleSceneState.WinBattle;
                DebugText.text = " Player Win !";
                ClearActiveRouteTiles();
                ActivePushCarParent.GetComponentInChildren<PushCarController>().DespawnCar();
                Destroy(ActivePushCarParent);
                RewardNextStage();
                battleSceneState = PVEBattleSceneState.BattleConclusion;
            }
            // Conclusion State
        }
        
        else if (battleSceneState is PVEBattleSceneState.BossFight)
        {
            // update loop
            /* BF
             * - Spawn Boss
                - Spawn  (Object Recognition Related) buff or Items (not Enabled)
                - Ask start? Start Waiting
                - Start by spawn Player, and enable all spawned elements
                - Enable UIs: boss UP (Objective gauge) player HP bar...
             * - Loop
             *   -->Detect game end condition
             */
            CheckPlayerAndGameEndCondition();

            if (BattleEndFlag)
            {
                battleSceneState = PVEBattleSceneState.WinBattle;
                DebugText.text = " Player Win !";
                Destroy(ActiveBossFightParent);
                RewardNextStage();
                battleSceneState = PVEBattleSceneState.BattleConclusion;
            }
            // Conclusion State
            
            // Save character build
        
            // Chapter selection
        }

        // Map selection  (returns to map)
        else if (battleSceneState is PVEBattleSceneState.MapActive)
        {
            // BattleUICanvasParent.SetActive(false);
            ShopUIParent.SetActive(false);
            BackgroundCanvasParent.SetActive(true);
            MapParent.SetActive(true);
        }
        
        // Shop / Event (story) mode
        else if (battleSceneState is PVEBattleSceneState.ShopActive)
        {
            // BattleUICanvasParent.SetActive(false);
            MapParent.SetActive(false);
            ShopUIParent.SetActive(true);
            ShopUIParent.SetActive(true);
        }
        
        else if (battleSceneState is PVEBattleSceneState.EventActive)
        {
            // BattleUICanvasParent.SetActive(false);
            ShopUIParent.SetActive(false);
            MapParent.SetActive(false);
            BackgroundCanvasParent.SetActive(true);
        }
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
        SpatialTree = _activeGameboard.GetSpatialTree();
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

    /*
     * Call After all fixed point are found from initial scan
     * And Call before every battle, including first battle (D, PC mode)
     * Generate a route
     * Then Spawn those map tile with corresponding theme
     */
    public void GenerateSpawnRouteTile()
    {
        GenerateAndSpawnRouteTile();
    }
    
    public void GenerateAndSpawnRouteTile()
    {
        if (BattleMode == PVEBattleSceneState.BossFight ||
            BattleMode == PVEBattleSceneState.CapturePointMode ||
            BattleMode == PVEBattleSceneState.DefencePointMode) return;
        
        if (PossibleRouteMainEndPoints.Count < 1 || PossibleRouteMainSecPoints.Count < 1) return;

        ClearActiveRouteTileLocations();
        var startRoutePoint = new Vector3(0, -1.1f, 0);
        ActiveRouteTilesLocations.Add(startRoutePoint);

        // Randomly set Second and Last point
        int index = Random.Range(1, PossibleRouteMainSecPoints.Count-1);
        var secondMainPoint = PossibleRouteMainSecPoints[index];
        // Add second Main Point
        ActiveRouteTilesLocations.Add(secondMainPoint);
        
        // Generating end point
        var endMainPoint = new Vector3();
        for(var i = PossibleRouteMainEndPoints.Count; i>1; i--)
        {
            index = Random.Range(1, i-1);
            var dist1 = Vector3.Distance(secondMainPoint, PossibleRouteMainEndPoints[index]);
            var dist2 = Vector3.Distance(startRoutePoint, PossibleRouteMainEndPoints[index]);
            if (dist2 >= dist1)
            {
                endMainPoint = PossibleRouteMainEndPoints[index];
                break;
            }
        }
        if (endMainPoint.x == 0 && endMainPoint.z == 0)
        {
            Debug.Log("End invalid");
            endMainPoint = PossibleRouteMainEndPoints[PossibleRouteMainEndPoints.Count/2];
        }
        
        // Generate remaining main points along the axis:
        List<Vector3> IntermediateMains = GenerateIntermediateMainPoints(secondMainPoint, endMainPoint);
        IntermediateMains.ForEach(vec3 => ActiveRouteTilesLocations.Add(vec3));
        ActiveRouteTilesLocations.Add(endMainPoint);

        if (BattleMode == PVEBattleSceneState.DungeonMode)
        {
            var final = GenerateConnectingTileLocations(ActiveRouteTilesLocations, tileInterval);
            ClearActiveRouteTileLocations();
            foreach (var point in final)
            {
                ActiveRouteTilesLocations.Add(point);
            }
            var tile = CyberpunkTilePrefab;
            switch (currentSceneTheme)
            {
                case SceneTheme.Cyberpunk:
                    tile = CyberpunkTilePrefab;
                    break;
                case SceneTheme.Fantasy:
                    tile = FantasyTilePrefab;
                    break;
                case SceneTheme.PostApocalypse:
                    tile = PostApocalypseTilePrefab;
                    break;
                case SceneTheme.Xianxia:
                    tile = XianxiaTilePrefab;
                    break;
                case SceneTheme.Modern:
                    tile = ModernTilePrefab;
                    break;
            }

            var rotQ = new Quaternion(0, 0, 0, 0);
            var count = 0;

            foreach (var location in ActiveRouteTilesLocations)
            {
                if (count == ActiveRouteTilesLocations.Count-1)
                {
                    Instantiate(tile, location, rotQ , TileParent.transform); // Dummy tile
                    var t = Instantiate(EndDungeonTile, location, rotQ , TileParent.transform);
                    ActiveRouteTiles.Add(t);
                    var bpd = t.GetComponent<BattlePathDest>();
                    bpd.SetBattleMode(BattleMode, this);
                }
                else
                {
                    var t = Instantiate(tile, location, rotQ , TileParent.transform);
                    ActiveRouteTiles.Add(t);
                }
                count++;
            }
        } 
        else if (BattleMode == PVEBattleSceneState.PushCarBattleMode)
        {
            var rotQ = new Quaternion(0, 0, 0, 0);
            
            foreach (var checkpoint in ActiveRouteTilesLocations)
            {
                var checkObject = Instantiate(CheckPointPrefab, checkpoint, rotQ , TileParent.transform);
                CarCheckPointGameObjects.Add(checkObject);
            }
            
            // Scene init
            ActivePushCarParent = Instantiate(PushCarParentPrefab);
            if (ActivePushCarParent != null) ActivePushCarParent.SetActive(true);
            var pcCtrl = ActivePushCarParent.GetComponentInChildren<PushCarController>();
            pcCtrl.GameFlowController = this;
            pcCtrl.SetCheckPoint(CarCheckPointGameObjects);

            ActivePushCarParent.transform.position = Vector3.zero;
            
            var final = GenerateConnectingTileLocations(ActiveRouteTilesLocations, tileInterval);
            ClearActiveRouteTileLocations();
            foreach (var point in final)
            {
                ActiveRouteTilesLocations.Add(point);
            }

            var tile = CyberpunkTilePrefab;
            switch (currentSceneTheme)
            {
                case SceneTheme.Cyberpunk:
                    tile = CyberpunkTilePrefab;
                    break;
                case SceneTheme.Fantasy:
                    tile = FantasyTilePrefab;
                    break;
                case SceneTheme.PostApocalypse:
                    tile = PostApocalypseTilePrefab;
                    break;
                case SceneTheme.Xianxia:
                    tile = XianxiaTilePrefab;
                    break;
                case SceneTheme.Modern:
                    tile = ModernTilePrefab;
                    break;
            }
            foreach (var location in ActiveRouteTilesLocations)
            {
                var t = Instantiate(tile, location, rotQ , TileParent.transform);
                ActiveRouteTiles.Add(t);
            }
        }
    }
    
    private List<Vector3> GenerateIntermediateMainPoints(Vector3 second, Vector3 end)
    {
        // // Generate third Main point s.t. third to start > second to start
        // var biasedRange = Random.Range(AverageMainPointDistance - MainPointDistanceVariance,
        //     AverageMainPointDistance - MainPointDistanceVariance);
        // _activeGameboard.FindNearestFreePosition(secondMainPoint, 
        //     biasedRange, out thirdMain);
        // while (Vector3.Distance(thirdMain, startRoutePoint) < SecToStartDistance)
        // {
        //     biasedRange = Random.Range(AverageMainPointDistance - MainPointDistanceVariance,
        //         AverageMainPointDistance - MainPointDistanceVariance);
        //     _activeGameboard.FindNearestFreePosition(secondMainPoint, 
        //         biasedRange, out thirdMain);
        // }
        //
        // // Generate fifth Main point s.t. fifth to start < end to start
        // biasedRange = Random.Range(AverageMainPointDistance - MainPointDistanceVariance,
        //     AverageMainPointDistance - MainPointDistanceVariance);
        // _activeGameboard.FindNearestFreePosition(endMainPoint, 
        //     biasedRange, out fifthMain);
        // while (Vector3.Distance(fifthMain, startRoutePoint) > EndToStartDistance)
        // {
        //     biasedRange = Random.Range(AverageMainPointDistance - MainPointDistanceVariance,
        //         AverageMainPointDistance - MainPointDistanceVariance);
        //     _activeGameboard.FindNearestFreePosition(endMainPoint, 
        //         biasedRange, out fifthMain);
        // }
        //
        // // Generate fourth Main point interpolating third and fifth Main point
        // fourthMain = Vector3.Lerp(thirdMain, fifthMain, 0.5f);
        var finalList = new List<Vector3>();

        Vector3 middle, front, back;
        if (end.magnitude > second.magnitude)
        {
            middle = (end - second)/2 + second;
            front = (middle - second)/2 + second;
            back = (end - middle)/2 + middle;     
        }
        else
        {
            middle = (second - end)/2 + end;
            front = (second - middle)/2 + middle;
            back = (middle - end)/2 + end;     
        }

        var VarVec = new Vector3(Random.Range(-MainPointDistanceVariance, MainPointDistanceVariance), -1.1f,
            Random.Range(-MainPointDistanceVariance, MainPointDistanceVariance));
        _activeGameboard.FindNearestFreePosition(front + VarVec, AverageMainPointDistance, out front);
        
        VarVec = new Vector3(Random.Range(-MainPointDistanceVariance, MainPointDistanceVariance), -1.1f,
            Random.Range(-MainPointDistanceVariance, MainPointDistanceVariance));
        _activeGameboard.FindNearestFreePosition(middle + VarVec, AverageMainPointDistance, out middle);
        
        VarVec = new Vector3(Random.Range(-MainPointDistanceVariance, MainPointDistanceVariance), -1.1f,
            Random.Range(-MainPointDistanceVariance, MainPointDistanceVariance));
        _activeGameboard.FindNearestFreePosition(back + VarVec, AverageMainPointDistance, out back);

        finalList.Add(front);
        finalList.Add(middle);
        finalList.Add(back);
        
        return finalList;
    }

    /*
     * Generate 5-th and 2-nd possible points and store them into list
     */
    private void GenerateFixedMapRoutePointsList()
    {
        // Player Starting Position (0,y,0) should always be the starting position
        Vector3 StartPoint = new Vector3(0, -1.1f, 0);
        foreach (var coord in AllGridNodeCoordinates)
        {
            var CoordVec3 = new Vector3(coord.x * _activeGameboard.Settings.TileSize, -1.1f,
                coord.y * _activeGameboard.Settings.TileSize);

            var distanceToStart = Vector3.Distance(CoordVec3, StartPoint);
            if (distanceToStart > MinRouteDisplacement - MainPointDistanceVariance 
                && distanceToStart <= MaxRouteDisplacement + MainPointDistanceVariance 
                && !PossibleRouteMainEndPoints.Contains(CoordVec3))
            {
                PossibleRouteMainEndPoints.Add(CoordVec3);
            }
            else if (distanceToStart > AverageMainPointDistance - MainPointDistanceVariance 
                     && distanceToStart <= AverageMainPointDistance + MainPointDistanceVariance 
                     && !PossibleRouteMainSecPoints.Contains(CoordVec3))
            { 
                PossibleRouteMainSecPoints.Add(CoordVec3);
            }
        }
    }

    public void ClearActiveRouteTileLocations()
    {
        if (ActiveRouteTilesLocations.Capacity != 0)
        {
            ActiveRouteTilesLocations.Clear();
        }
    }

    public void ClearActiveRouteTiles()
    {
        ActiveRouteTiles.ForEach(Destroy);
        ActiveRouteTiles.Clear();
    }

    private List<Vector3> GenerateConnectingTileLocations(List<Vector3> Mains, float intervalLength)
    {
        var finalList = new List<Vector3>();

        for (var i = 1; i < Mains.Count; i++)
        {
            finalList.Add(Mains[i-1]);
            var dist = (Mains[i] - Mains[i - 1]).magnitude;
            var direction = (Mains[i] - Mains[i - 1]).normalized;

            int numIntervals = Mathf.CeilToInt(dist / intervalLength);
            float actualIntervalLength = dist / numIntervals;
            for (int j = 1; j < numIntervals; j++)
            {
                Vector3 point = Mains[i-1] + direction * (j * actualIntervalLength);
                finalList.Add(point);
            }
            finalList.Add(Mains[i]);
        }
        return finalList;
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
    public void SetRandomEnemySpawnLocationVectors(int MaxRandomSpawnPositionCount)
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
    }

    public void ResetEnemySpawnLocationList()
    {
        EnemySpawnPositionList.Clear();
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
        }
        // Set battle mode (not the current state)
        switch (levelType)
        {
            /*
             * Battle Sessions: Also call the initialisation of each mode!! (Only call once)
             */
            case LevelType.CapturePointBattleMode:
                BattleMode = PVEBattleSceneState.CapturePointMode;
                InitCaptureTowerMode();
                Debug.Log("Battle CP");
                break;
            
            case LevelType.DefensePointBattleMode:
                BattleMode = PVEBattleSceneState.DefencePointMode;
                InitDefenseTowerMode();
                Debug.Log("Battle DP");
                break;
            
            case LevelType.PushCarBattleMode:
                BattleMode = PVEBattleSceneState.PushCarBattleMode;
                InitPushCarMode();
                Debug.Log("Battle CA");
                break;
            
            case LevelType.DungeonMode:
                BattleMode = PVEBattleSceneState.DungeonMode;
                InitDungeonMode();
                Debug.Log("Battle D");
                break;
            
            /*
             * Shop, event, Boss session
             */
            case LevelType.Shop:
                battleSceneState = PVEBattleSceneState.ShopActive;
                BattleMode = PVEBattleSceneState.ShopActive;
                
                BattleUICanvasParent.SetActive(false);
                MapParent.SetActive(false);
                ShopUIParent.SetActive(true);
                BattleEndFlag = false;
                break;
            
            case LevelType.Event:
                BattleMode = PVEBattleSceneState.EventActive;
                break;
            
            case LevelType.Boss:
                BattleMode = PVEBattleSceneState.BossFight;
                InitBossFight();
                break;
        }
        // Inital Set up UI for battle / scan mode
        if (battleSceneState is 
            (PVEBattleSceneState.Scanning or PVEBattleSceneState.BossFight or PVEBattleSceneState.CapturePointMode 
            or PVEBattleSceneState.DefencePointMode or PVEBattleSceneState.DungeonMode or PVEBattleSceneState.PushCarBattleMode or PVEBattleSceneState.BossFight)
           )
        {
            BattleUICanvasParent.SetActive(true);
            MapParent.SetActive(false);
            BackgroundCanvasParent.SetActive(false);
            ShopUIParent.SetActive(false);
            BattleEndFlag = false;
        }
        
        // Re entering the scene with different mode:
        else if (battleSceneState is PVEBattleSceneState.MapActive)
        {
            battleSceneState = BattleMode;
            restartBattle = true;
            BattleEndFlag = false;
            BattleUICanvasParent.SetActive(true);
            startFightUI.SetActive(true);
            
            MapParent.SetActive(false);
            BackgroundCanvasParent.SetActive(false);
            ShopUIParent.SetActive(false);
        }
    }


    IEnumerator IsGameboardReady()
    {
        yield return new WaitUntil(() => _activeGameboard != null);
    }

    // IEnumerator WaitForScanComplete()
    // {
    //     yield return new WaitUntil(() =>
    //     {
    //         return battleSceneState == PVEBattleSceneState.ScanCompleted;
    //     });
    //
    // }

    public void ExitShopReEnterMap()
    {
        MapParent.SetActive(true);
        ShopUIParent.SetActive(false);
        battleSceneState = PVEBattleSceneState.MapActive;
        BattleEndFlag = false;
    }

    public void EnterNewBattle()
    {
        if (!restartBattle) return;
        // Reset Spawner then Set again
        EnemySpawner.SetSpawner(false);
        EnemySpawner.SetSpawner(true);
        switch (BattleMode)
        {
            case PVEBattleSceneState.DefencePointMode:
                ActiveDefenseTowerParent.SetActive(true);
                restartBattle = false;
                break;
            case PVEBattleSceneState.CapturePointMode:
                ActiveCaptureTowerParent.SetActive(true);
                restartBattle = false;
                break;
            case PVEBattleSceneState.DungeonMode:
                battleSceneState = BattleMode;
                ActiveDungeonParent.SetActive(true);
                restartBattle = false;
                break;
            case PVEBattleSceneState.PushCarBattleMode:
                battleSceneState = BattleMode;
                ActivePushCarParent.SetActive(true);
                restartBattle = false;
                break;
            case PVEBattleSceneState.BossFight:
                battleSceneState = BattleMode;
                ActiveBossFightParent.SetActive(true);
                restartBattle = false;
                break;
        }
    }
    #endregion

    #region Spawn Object
    
    /*
     * Find Random position if it is in correct mode
     * check if it's in the valid map tile
     * 
     */
    public bool GetTowerSpawnLocationVector(out Vector3 towerLocation)
    {
        towerLocation = new Vector3();
        if (BattleMode == PVEBattleSceneState.CapturePointMode || BattleMode == PVEBattleSceneState.DefencePointMode)
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

    public void SpawnPlayer()
    {
        if (PlayerSpawner.activeCharacter != null) return;
        battleSceneState = PVEBattleSceneState.SpawningPlayer;
        StartCoroutine(CalculatePlayerSpawnPoint());
        PlayerSpawner.SpawnPlayer(playerGlobalStatus, PlayerSpawnParent.transform, PlayerSpawnLocation);
        startFightUI.SetActive(false);
        battleSceneState = BattleMode;
    }

    private IEnumerator CalculatePlayerSpawnPoint()
    {
        var spawnpoint = new Vector3();
        //
        // var ray = ARCtrl._arCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        // RaycastHit hit;
        //
        // if (Physics.Raycast(ray, out hit))
        // {
        //     _activeGameboard.FindNearestFreePosition(hit.point, out spawnpoint);
        //     // var node = new GridNode();
        //     // if (SpatialTree.GetElement(
        //     //         Utils.PositionToTile(spawnpoint, _activeGameboard.Settings.TileSize), out node))
        //     // {
        //     //     Vector3 spawnPoint = GetEnemySpawnLocationVectorList()[(int) GetEnemySpawnLocationVectorList().Count/2];
        //     //     
        //     //     spawnPoint = new Vector3(spawnPoint.x * _activeGameboard.Settings.TileSize, 1f, spawnPoint.z * _activeGameboard.Settings.TileSize);
        //     //     
        //     //     // Have the prefab face towards camera
        //     //     var rotation = Vector3.ProjectOnPlane(ARCtrl.transform.forward, Vector3.up).normalized;
        //     //     var QRot = Quaternion.LookRotation(-rotation);
        //     // }
        // }

        spawnpoint = new Vector3(0, 0.5f, 0);
        PlayerSpawnLocation = spawnpoint;
        yield return new WaitUntil( () => PlayerSpawnLocation == spawnpoint);
    }
    #endregion
    
    #region Capture DTower Mode

    void InitCaptureTowerMode()
    {
        CalculateTowerLocation();
        ActiveCaptureTowerParent = Instantiate(CaptureTowerParentPrefab);
        CapTarget = ActiveCaptureTowerParent.GetComponentInChildren<CaptureTarget>();
        ActiveCaptureTowerParent.transform.position = Vector3.zero;
        startFightUI.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (ActiveCaptureTowerParent != null) ActiveCaptureTowerParent.SetActive(true);
            if (BattleMode != PVEBattleSceneState.CapturePointMode) return;
            battleSceneState = PVEBattleSceneState.SpawningTower;
            var ct = ActiveCaptureTowerParent.GetComponentInChildren<CaptureTarget>();
            ct.SpawnTower();
            battleSceneState = PVEBattleSceneState.CapturePointMode;
        });
    }
    
    #endregion

    #region Defense DTower Mode

    void InitDefenseTowerMode()
    {
        ActiveDefenseTowerParent = Instantiate(DefenseTowerParentPrefab);
        DefTarget = ActiveDefenseTowerParent.GetComponentInChildren<DefenceTarget>();
        ActiveDefenseTowerParent.transform.position = Vector3.zero;
        startFightUI.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (ActiveDefenseTowerParent != null) ActiveDefenseTowerParent.SetActive(true);
            if (BattleMode != PVEBattleSceneState.DefencePointMode) return;
            battleSceneState = PVEBattleSceneState.SpawningTower;
            var dt = ActiveDefenseTowerParent.GetComponentInChildren<DefenceTarget>();
            dt.SpawnTower();
            dt.StartCountDown();
            battleSceneState = PVEBattleSceneState.DefencePointMode;
        });
    }
    
    #endregion
    
    #region Dungeon Mode

    void InitDungeonMode()
    {
        ActiveDungeonParent = Instantiate(DungeonParentPrefab);
        if (ActiveDungeonParent != null) ActiveDungeonParent.SetActive(true);
        ActiveDungeonParent.transform.position = Vector3.zero;
    }

    #endregion
    
    #region Push Car Mode

    void InitPushCarMode()
    {
        // ActivePushCarParent = Instantiate(PushCarParentPrefab);
        // if (ActivePushCarParent != null) ActivePushCarParent.SetActive(true);
        // var pcCtrl = ActivePushCarParent.GetComponentInChildren<PushCarController>();
        // pcCtrl.GameFlowController = this;
        // pcCtrl.SetCheckPoint(CarCheckPointGameObjects);
        //
        // ActivePushCarParent.transform.position = Vector3.zero;
    }

    #endregion
    
    #region BossFight Mode

    void InitBossFight()
    {
        ActiveBossFightParent = Instantiate(BossFightParentPrefab);
        ActiveBossFightParent.transform.position = Vector3.zero;
    }

    #endregion

    #region Getter / Setter
    public CharacterMovementController GetPlayerMovementCtrl ()
    {
        if (PlayerSpawner.activeCharacter is null) return null;
        
        var movementCtrl = PlayerSpawner.activeCharacter.GetComponent<CharacterMovementController>();
        playerMovementCtrl = movementCtrl;
        return movementCtrl;
    }

    public CentralBattleController GetCentralBattleController()
    {
        return CentralBattleCtrl;
    }

    public void SetDefenseTower(DefenceTarget tower)
    {
        DTower = tower;
        EnemySpawner.SetDefenseTarget(tower);
    }

    public void SetCaptureTower(CaptureTarget tower)
    {
        CTower = tower;
        EnemySpawner.SetCaptureTarget(tower);
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
        if (PlayerSpawner.activeCharacter is null) return null;
        return playerMovementCtrl;
    }

    public void SetPlayerMovementCtrl(CharacterMovementController cmc)
    {
        playerMovementCtrl = cmc;
    }

    public void SetPlayerLocation(Vector3 location)
    {
        PlayerSpawnLocation = location;
    }
    
    public Vector3 GetPlayerLocation()
    {
        return PlayerSpawnLocation;
    }

    public void SetBattleSceneTheme(SceneTheme theme)
    {
        currentSceneTheme = theme;
    }

    public SceneTheme GetBattleSceneTheme()
    {
        return currentSceneTheme;

    }

    public CombatHandler GetPlayerCombatHandler()
    {       
        return playerOwnCombatHandler;
    }

    public PVEBattleSceneState GetBattleSceneState()
    {       
        return battleSceneState;
    }

    public GameObject GetActivePushCarParent()
    {
        return ActivePushCarParent;
    }

    public GameObject GetActiveDefenseTowerPrefab()
    {
        return ActiveDefenseTowerParent;
    }

    public GameObject GetActiveCaptureTowerPrefab()
    {
        return ActiveCaptureTowerParent;
    }

    #endregion

    #region End Battle Sequence Conclusion(Reward or Die to return start)

    public void RewardNextStage()
    {
        WinPopUpWindow.SetActive(true);
        EnemySpawner.ClearEnemyOnScene();
        PlayerSpawner.DespawnPlayer((int) playerMovementCtrl.GetPlayerCombatHandler().GetCurrentHP(), playerGlobalStatus.money+1, playerGlobalStatus.weaponLv);
        ActiveDefenseTowerParent = null;
        ActiveCaptureTowerParent = null;
        ActiveDungeonParent = null;
        ActivePushCarParent = null;
        ActiveBossFightParent = null;
        EnemySpawner.ResetTowerTargetReference();
    }

    public void LossRestartFromBeginning()
    {
        LossPopUpWindow.SetActive(true);
        EnemySpawner.ResetTowerTargetReference();
        EnemySpawner.ClearEnemyOnScene();
        switch (BattleMode)
        {
            case PVEBattleSceneState.DefencePointMode:
                Destroy(ActiveDefenseTowerParent);
                break;
            case PVEBattleSceneState.CapturePointMode:
                Destroy(ActiveCaptureTowerParent);
                break;
            case PVEBattleSceneState.DungeonMode:
                Destroy(ActiveDungeonParent);
                ClearActiveRouteTiles();
                break;
            case PVEBattleSceneState.PushCarBattleMode:
                Destroy(ActivePushCarParent);
                ClearActiveRouteTiles();
                break;
            case PVEBattleSceneState.BossFight:
                Destroy(ActiveBossFightParent);
                break;
        }

        ActiveDefenseTowerParent = null;
        ActiveCaptureTowerParent = null;
        ActiveDungeonParent = null;
        ActivePushCarParent = null;
        ActiveBossFightParent = null;
        
        PlayerSpawner.DespawnPlayer(100, 0, 0);
    }


    public void ReturningToMap()
    {
        WinPopUpWindow.SetActive(false);
        LossPopUpWindow.SetActive(false);
        
        BackgroundCanvasParent.SetActive(true);
        BattleUICanvasParent.SetActive(false);
        MapParent.SetActive(true);
        
        battleSceneState = PVEBattleSceneState.MapActive;
    }

    public void ReturnToTitlePage()
    {
        SceneManager.LoadScene(0);
    }
    #endregion

    #region ShopUI

    

    #endregion
}
