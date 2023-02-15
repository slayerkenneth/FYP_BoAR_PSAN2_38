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
    DefencePointBattleMode,
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
}

public struct Map {
    public Dictionary<int, level> levels;
    public Dictionary<Vector2, int> pos2Id; 
    public Dictionary<int, List<int>> edges; //key is the level id and value is the list connected levels id that can be proceed
}

public class MapController : MonoBehaviour
{

    public RectTransform graphContainer;
    public GameObject DefencePrefab;
    public GameObject LinkPrefab;
    public GameObject CirclePrefab;
    public static Map currentMap;

    public void Start()
    {
        if (currentMap.levels == null) GenerateMap();
        drawMap();
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

        for (int i = Random.Range(5, 7); i > 0; --i)
        {
            var path = DijkstraSearch.Search(graph, new Vertex(startPoint), new Vertex(endPoint));
            if (path == null) break;
            for(int j = 0; j <path.Length - 1; j++) {
                int currentID = registerLevel(path[j].Name);
                int next = registerLevel(path[j + 1].Name);
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
        GameObject newIcon = Instantiate(DefencePrefab);
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
        if (currentMap.edges.TryGetValue(PlayerStatus.CurrentPlayer.currentLevel, out edge)) {
            if (edge.Contains(id))
            {
                PlayerStatus.CurrentPlayer.currentLevel = id;
                SceneManager.LoadScene(currentMap.levels[id].levelType.ToString());
                // Testing (Shop UI Scene)
                // SceneManager.LoadScene(3);
            }
        }
    }

    private LevelType randomizeType() {
        return LevelType.DefencePointBattleMode;
    }

    private int registerLevel(Vector2 pos) {
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
            level.levelType = randomizeType();
            level.pos = pos;
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


