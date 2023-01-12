using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.Extensions.Gameboard;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Controllers, Gameboard, Environment Reference")]
    [SerializeField] private ARController ARCtrl;
    [SerializeField] private GameFlowController GameFlowCtrl;
    private IGameboard _activeGameboard;

    [Header("Spawner Setting")] 
    [SerializeField] public List<GameObject> EnemySpawnPrefabList;
    [SerializeField] public GameObject EnemyParentObj;
    [SerializeField] public int MaxEnemyCount;
    public List<Vector3> EnemySpawnLocationList;
    private bool EnemySpawnEnable = false;
    private static int currentEnemyCount;
    
    void Start()
    {
        _activeGameboard = ARCtrl.GetActiveGameboard();
        EnemySpawnLocationList = GameFlowCtrl.GetEnemySpawnLocationVectorList();
    }
    
    void Update()
    {
        if (!EnemySpawnEnable || EnemySpawnPrefabList.Count == 0 || EnemySpawnLocationList.Count == 0 || currentEnemyCount >= MaxEnemyCount) return;
        for (int i=0; i < EnemySpawnLocationList.Count; i++)
        {
            StartCoroutine(SpawnEnemyAfterWaiting(1000, EnemySpawnPrefabList[i], EnemySpawnLocationList[i]));
            currentEnemyCount++;
        }
        
    }

    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 SpawnLocationVec)
    {
        return Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(), EnemyParentObj.transform);
    }
    
    public IEnumerator SpawnEnemyAfterWaiting(float time, GameObject enemyPrefab, Vector3 SpawnLocationVec)
    {
        var e = Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(0,0,0,0), EnemyParentObj.transform);
        e.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        e.AddComponent<EnemyBehavior>();
        yield return new WaitForSeconds(time);
    }

    public void SetSpawner(bool sw)
    {
        EnemySpawnEnable = sw;
    }
}
