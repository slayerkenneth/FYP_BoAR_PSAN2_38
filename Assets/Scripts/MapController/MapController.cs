using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using Gists;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum LevelType
{
    DefensePointBattleMode,
    CapturePointBattleMode,
    DungeonMode,
    PushCarBattleMode,
    Shop,
    Event,
    Boss
}

public struct level
{
    public int id;
    public LevelType levelType;
    public Vector2 pos;
    public int sequence;    //the distance to the start level. -1 represent end level
}

public struct Map {
    public Dictionary<int, level> levels;
    public Dictionary<Vector2, int> pos2Id;
    public Dictionary<int, List<int>> edges; //key is the level id and value is the list connected levels id that can be proceed
}

public class MapController : MonoBehaviour
{
    public GameFlowController GameFlowCtrl;
    public RectTransform graphContainer;
    public GameObject IconPrefab;
    public GameObject EndPrefab;
    public Sprite DefenceImage;
    public Sprite ShopImage;
    public GameObject LinkPrefab;
    public GameObject CirclePrefab;
    public static Map currentMap;
    public bool endSceen = false;


    public void Start()
    {
        if (currentMap.levels == null) GenerateMap();
        drawMap();
        if (currentMap.levels[PlayerStatus.CurrentPlayer.currentLevel].sequence == -1)
        {
            EndPrefab.SetActive(true);
            endSceen = true;
        }
    }

    public void GenerateMap() {
        currentMap.levels = new Dictionary<int, level>();
        currentMap.edges = new Dictionary<int, List<int>>();
        currentMap.pos2Id = new Dictionary<Vector2, int>();
        float width = graphContainer.rect.width;
        float height = graphContainer.rect.height;
        var points = FastPoissonDiskSampling.Sampling(new Vector2(-70, 70), new Vector2(width - 10, height - 50), 130);
        var startPoint = new Vector2(width / 2, 25);
        var endPoint = new Vector2(width / 2, height - 25);
        points.Add(startPoint);
        points.Add(endPoint);
        var delaunator = new Delaunator(DelaunatorExtensions.ToPoints(points));
        var graph = new Graph();
        delaunator.ForEachTriangleEdge(edge =>
        {
            graph.AddNewEdge(edge.P.ToVector2(), edge.Q.ToVector2());
        });
        registerLevel(startPoint, 0);
        registerLevel(endPoint, -1);
        for (int i = Random.Range(5, 12); i > 0; --i)
        {
            var path = DijkstraSearch.Search(graph, new Vertex(startPoint), new Vertex(endPoint));
            if (path == null) break;
            for(int j = 0; j <path.Length - 1; j++) {
                int currentID = registerLevel(path[j].Name, j);
                int next = registerLevel(path[j + 1].Name, j + 1);
                List<int> value;
                if (currentMap.edges.TryGetValue(currentID,out value)) {
                    value.Add(next);
                }
                else{
                    var list = new List<int>();
                    list.Add(next);
                    currentMap.edges.Add(currentID, list);
                }                
            }
            var temp = path[Random.Range(1, path.Length - 1)];
            graph.RemoveVertex(temp);


        }
    }

    private void CreateIcon(level level) {
        GameObject newIcon = Instantiate(IconPrefab);
        switch (level.levelType) {
            case LevelType.Boss:
                newIcon.GetComponent<Image>().sprite = DefenceImage;
                break;
            case LevelType.CapturePointBattleMode:
                newIcon.GetComponent<Image>().sprite = DefenceImage;
                break;
            case LevelType.DefensePointBattleMode:
                newIcon.GetComponent<Image>().sprite = DefenceImage;
                break;
            case LevelType.DungeonMode:
                newIcon.GetComponent<Image>().sprite = DefenceImage;
                break;
            case LevelType.Event:
                newIcon.GetComponent<Image>().sprite = DefenceImage;
                break;
            case LevelType.PushCarBattleMode:
                newIcon.GetComponent<Image>().sprite = DefenceImage;
                break;
            case LevelType.Shop:
                newIcon.GetComponent<Image>().sprite = ShopImage;
                break;
            default:
                newIcon.GetComponent<Image>().sprite = DefenceImage;
                break;
        }
        

        newIcon.name = "Level "+ level.id;
        newIcon.transform.parent = graphContainer;
        RectTransform rectTransform = newIcon.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = level.pos;
        var button = newIcon.GetComponent<Button>();
        button.onClick.AddListener(() => this.onIconClick(level.id));
    }
    private void CreateLink(Vector2 from, Vector2 to, int a, int b )
    {
        GameObject newIcon = Instantiate(LinkPrefab);
        newIcon.name = "Link "+ a + "" + b;
        newIcon.transform.parent = graphContainer;
        RectTransform rectTransform = newIcon.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector2(((from - to).magnitude - 50) / 7, 1.0F);
        rectTransform.anchoredPosition = (from + to)*0.5F;
        rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2((to.y - from.y), (to.x - from.x)) * Mathf.Rad2Deg);
    }
    private void CreateCircle(Vector2 from, int name)
    {
        GameObject newIcon = Instantiate(CirclePrefab);
        newIcon.name = "Circle " + name;
        newIcon.transform.parent = graphContainer;
        RectTransform rectTransform = newIcon.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = from;
    }

    public void onIconClick(int id) {
        List<int> edge;
        if (!endSceen) {
            if (currentMap.edges.TryGetValue(PlayerStatus.CurrentPlayer.currentLevel, out edge)) {
                if (edge.Contains(id))
                {
                    PlayerStatus.CurrentPlayer.currentLevel = id;
                    GameFlowCtrl.EnterMapNode(currentMap.levels[id].levelType);
                    Debug.Log("enter level" + currentMap.levels[id].levelType);
                }
            }
        }
    }

    private LevelType randomizeType(int sequence) {
        if (sequence == 1 || sequence == 0 || sequence == -1) return LevelType.DefensePointBattleMode;
        if (Random.Range(1, 3) == 1) return LevelType.Shop;
        return LevelType.DefensePointBattleMode;
    }

    private int registerLevel(Vector2 pos, int sequence) {
        int id = 0;
        if (currentMap.pos2Id.TryGetValue(pos, out id))
        {
            return id;
        }
        else
        {
            id = currentMap.levels.Count;
            var level = new level();
            level.id = id;
            level.levelType = randomizeType(sequence);
            level.pos = pos;
            level.sequence = sequence;
            currentMap.levels.Add(id, level);
            currentMap.pos2Id.Add(pos, id);
            return id;
        }
    }

    private void drawMap() {
        foreach (KeyValuePair<int, level> entry in currentMap.levels)
        {
            List<int> edge;
            if (currentMap.edges.TryGetValue(entry.Key, out edge)) foreach (int to in edge) CreateLink(entry.Value.pos, currentMap.levels[to].pos, entry.Key, to);
            CreateIcon(entry.Value);
            if (PlayerStatus.CurrentPlayer.currentLevel == entry.Key) CreateCircle(entry.Value.pos, entry.Key);
        }
    }
}


