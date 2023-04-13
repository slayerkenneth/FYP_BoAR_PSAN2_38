using Aglomera;
using Aglomera.Linkage;
using ExamplesUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;
using TMPro;

class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
{
    public int Compare(T x, T y)
    {
        return y.CompareTo(x);
    }
}

public class ObjectSpawningController : MonoBehaviour
{
    public int maxObjectNum;

    public TextAsset labelsFile;

    [SerializeField] public PhoneCamera phoneCamera;
    [SerializeField] public List<string> objectClass = new List<string>();
    [SerializeField] public List<GameObject> objectPrefabs = new List<GameObject>();
    public ARController ARCtrl;
    public CentralBattleController CentralBattleController;
    public float ClusteringDistance = 0.1F;

    private SortedDictionary<float, Tuple<string, Vector3>> objectList;

    /// A list of placed game objects to be destroyed in the OnDestroy method.
    private List<GameObject> _placedObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        objectList = new SortedDictionary<float, Tuple<string, Vector3>>(new DescendingComparer<float>());
        objectClass = new List<string>(Regex.Split(this.labelsFile.text, "\n|\r|\r\n").Where(s => !String.IsNullOrEmpty(s)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnSceneItems() {
        /*
        var num = new List<int>();
        num.Add(0);
        num.Add(2);
        num.Add(4);
        num.Add(5);
        num.Add(8);
        num.Add(12);
        num.Add(14);
        if (_placedObjects.Count != 0) return;
        Debug.Log("spawn: wrong");
        for (int i = 0; i < 7; i++)
        {
            var landscape = Instantiate(objectPrefabs[i], new Vector3(i *0.3F, -1.199F, 0), Quaternion.identity);

            _placedObjects.Add(landscape);  
        }
        */
        phoneCamera.StopInference();
        objectList.Clear();
        Debug.Log("spawn: A" + phoneCamera.SceneItemsLocations.Count);

        Dictionary<string, HashSet<DataPoint>> RayOfClass = TransformRays(phoneCamera.SceneItemsLocations);

        Debug.Log("spawn: B" +RayOfClass.Count);

        var clusteringAlg = new AgglomerativeClusteringAlgorithm<DataPoint>(new SingleLinkage<DataPoint>(new DataPoint()));

        foreach (KeyValuePair<string, HashSet<DataPoint>> entry in RayOfClass)
        {
            Clustering(entry, clusteringAlg);
        }
        Debug.Log("spawn: C" + objectList.Count);

        spawnFilterObject(maxObjectNum);
        Debug.Log("spawn: D" + _placedObjects.Count);
        
    }

    private Dictionary<string, HashSet<DataPoint>> TransformRays(List<Tuple<Ray, string, float>> Rays) {
        Dictionary<string, HashSet<DataPoint>> Result = new Dictionary<string, HashSet<DataPoint>>();

        foreach (Tuple<Ray, string, float> entry in Rays)
        {

            Vector3 pos;
            if (getGameBoardPos(entry.Item1, out pos))
            {
                if (Result.ContainsKey(entry.Item2))
                {

                    Result[entry.Item2].Add(new DataPoint(entry.Item3, new[] { pos.x, pos.y, pos.z }));
                }
                else
                {
                    Debug.Log("spawn: test");
                    Result[entry.Item2] = new HashSet<DataPoint>();
                    Result[entry.Item2].Add(new DataPoint(entry.Item3, new[] { pos.x, pos.y, pos.z }));
                }
            }
        }
        Debug.Log("spawn: result" + Result.Count);
        return Result;
    }

    private void Clustering(KeyValuePair<string, HashSet<DataPoint>> entry, AgglomerativeClusteringAlgorithm<DataPoint> algorithm) {
        //Debug.Log("spawn: hash" + entry.Value.Count);
        var clustering = algorithm.GetClustering(entry.Value);
        var clusterResult = clustering.GetEnumerator();
        //clustering.SaveToCsv(Application.dataPath + "/../SaveCSV/" + entry.Key+".csv");
        //Debug.Log("spawn: cluster" + clustering.Count);

        clusterResult.Reset();
        clusterResult.MoveNext();
        ClusterSet<DataPoint> before = clusterResult.Current;

        clusterResult.Reset();
        bool find = false;

        while (clusterResult.MoveNext())
        {
            if (clusterResult.Current.Dissimilarity > ClusteringDistance)
            {
                var enumerator = before.GetEnumerator();
                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    var center = DataPoint.GetCentroid(enumerator.Current);
                    objectList.Add(center.ID, new Tuple<string, Vector3>(entry.Key, new Vector3(center.Value[0], center.Value[1], center.Value[2])));
                }
                find = true;
                break;
            }
            else
            {
                before = clusterResult.Current;
            }
        }

        if (!find) {
            var enumerator = before.GetEnumerator();
            enumerator.Reset();
            while (enumerator.MoveNext())
            {
                var center = DataPoint.GetCentroid(enumerator.Current);
                objectList.Add(center.ID, new Tuple<string, Vector3>(entry.Key, new Vector3(center.Value[0], center.Value[1], center.Value[2])));
            }
        }

    }

    private void spawnFilterObject(int maxObjectNum) {
        Debug.Log("spawn: " + objectList.Count);

        IGameboard gameboard = ARCtrl.GetActiveGameboard();
        foreach (var x in objectList)
        {
            if (_placedObjects.Count >= maxObjectNum) break;

            if (/*!gameboard.CheckFit(center: x.Value.Item2, 0.01f)*/
        false) continue;
            
            int i = objectClass.FindIndex(a => a == x.Value.Item1);
            Debug.Log("spawn: "+i+" " +x.Value.Item1);

            if (i< 0) continue;


            Debug.Log("spawn: spawn");
            var landscape = Instantiate(objectPrefabs[i], x.Value.Item2, Quaternion.identity);

            if (objectPrefabs[i].name == "TextCube Variant") {
                var text = landscape.GetComponentInChildren<TextMeshPro>();
                text.text = x.Value.Item1;
                landscape.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            _placedObjects.Add(landscape);            
        }
    }

    public bool getGameBoardPos(Ray ray,out Vector3 pos) {
        var hitPoint = new Vector3();
        IGameboard gameboard = ARCtrl.GetActiveGameboard();
        var b = gameboard.RayCast(ray, out hitPoint);      // Null Object reference 
                                                           // Intersect the Gameboard with the ray
        if (b)
        {
            // Check whether the object can be fit in the resulting position
            if (gameboard.CheckFit(center: hitPoint, 0.01f))
            {
                pos = hitPoint;
                return true;
                //var landscape = Instantiate(PlacementObjectPf, hitPoint, Quaternion.identity);
                //landscape.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                //_placedObjects.Add(landscape);
            }
        }
        pos = Vector3.zero;
        return false;
    }
}
