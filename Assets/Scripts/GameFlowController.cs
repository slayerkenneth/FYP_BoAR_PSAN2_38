using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;
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
        SpawningGameObject,
        SpawningPlayer,
        GameModeSelection,
        UISetup,
        PushCarBattleMode,
        CapturePointMode,
        DefencePointMode,
        DungeonMode,
        BossFight
    }

    [Header("Game Flow variable")] 
    [SerializeField] public static PVEBattleSceneState battleSceneState = PVEBattleSceneState.invalid;
    public static bool PlayerSpawnActive = false;
    
    [Header("Other Controller Reference")] 
    [SerializeField] private Camera arCamera;
    [SerializeField] private ARController ARCtrl;
    [SerializeField] private CharacterMovementController playerMovementCtrl;
    
    [Header("Debug logs")] 
    [SerializeField] private Text DebugText;
    [SerializeField] private Text AreaText;
    
    [Header("Gameboard environment reference")] 
    [SerializeField] public static IGameboard _activeGameboard; //Consider make a getter setter function instead
    [SerializeField] public SpatialTree SpatialTree;
    [SerializeField] private float AreaLimit = 100f;
    [SerializeField] private List<GridNode> AllGridNode;
    [SerializeField] public List<Vector2Int> AllGridNodeCoordinates;
    [SerializeField] public Dictionary<Vector2Int, List<int>> CoordinatesAdjacencyList;
    [SerializeField] public List<Vector2Int> WallCoordinates;
    private bool MapCoordinatesConfirmed = false;
    [SerializeField] private int maxX = 0, minX = 0, maxZ = 0, minZ = 0;

    public GameObject InvisibleWallPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_activeGameboard != null)
        {
            battleSceneState = PVEBattleSceneState.Scanning;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // var srcPosition = Utils.PositionToTile(tempPosition, _activeGameboard.Settings.TileSize);
        if (_activeGameboard == null) return;

        if (_activeGameboard.Area >= AreaLimit)
        {
            SpatialTree = _activeGameboard.GetSpatialTree();
            battleSceneState = PVEBattleSceneState.ScanCompleteForColliderBuilding;
            if (PlayerSpawnActive == false)
            {
                // make following function called once only
                GetAllTileCoordinatesAndMarkWalls();
                SetBoundaryColliders();
            }
            PlayerSpawnActive = true;
        }
    }

    void SetBoundaryColliders()
    {
        var WallSet = Instantiate(InvisibleWallPrefab, new Vector3(0, -0.1f, 0), new Quaternion());
        WallSet.GetComponent<BoxCollider>().enabled = false;
        foreach (var nodeCoord in WallCoordinates)
        {
            var wall = Instantiate(InvisibleWallPrefab, new Vector3(nodeCoord.x * _activeGameboard.Settings.TileSize,
                                                                                        0, 
                                                                                        nodeCoord.y * _activeGameboard.Settings.TileSize), new Quaternion(), WallSet.transform);
            wall.transform.localScale = new Vector3(1, 1, 1);
            var wallBox = wall.GetComponent<BoxCollider>();
            wallBox.size = new Vector3(_activeGameboard.Settings.TileSize, 30, _activeGameboard.Settings.TileSize);
            wallBox.center = new Vector3(_activeGameboard.Settings.TileSize / 2, 0, _activeGameboard.Settings.TileSize / 2);
        }
        battleSceneState = PVEBattleSceneState.ColliderBuilt;
    }

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

                    foreach (var dp in dummyPoints)
                    {
                        var nnn = new GridNode();
                        if (!SpatialTree.GetElement(Utils.PositionToTile(dp, _activeGameboard.Settings.TileSize), out nnn))
                        {
                            neighbourCount -= 1;
                            Debug.Log("NB Count: " + node.Coordinates.x + ", " +node.Coordinates.y);
                        };
                    }

                    if (neighbourCount < 4) WallCoordinates.Add(new Vector2Int(node.Coordinates.x, node.Coordinates.y));
                    if (node.Coordinates.x >= maxX) maxX = node.Coordinates.x;
                    if (node.Coordinates.x <= minX) minX = node.Coordinates.x;
                    if (node.Coordinates.y >= maxZ) maxZ = node.Coordinates.y;
                    if (node.Coordinates.y <= minZ)minZ = node.Coordinates.y;
                }
            }
        }
        MapCoordinatesConfirmed = true;
    }

    public List<Vector2Int> GetWallCoordinates()
    {
        return WallCoordinates;
    }
}
