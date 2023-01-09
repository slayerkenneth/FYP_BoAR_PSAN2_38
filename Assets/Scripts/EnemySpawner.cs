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
    public List<Vector3> EnemySpawnLocationList;
    private bool EnemySpawnEnable = false;

    void Start()
    {
        _activeGameboard = ARCtrl.GetActiveGameboard();
        EnemySpawnLocationList = GameFlowCtrl.GetEnemySpawnLocationVectorList();
    }
    
    void Update()
    {
        if (!EnemySpawnEnable || EnemySpawnPrefabList.Count == 0 || EnemySpawnLocationList.Count == 0) return;
        
    }

    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 SpawnLocationVec)
    {
        return Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(), EnemyParentObj.transform);
    }
    
    public IEnumerator SpawnEnemyAfterWaiting(float time, GameObject enemyPrefab, Vector3 SpawnLocationVec)
    {
        yield return new WaitForSeconds(time);
        var e = Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(), EnemyParentObj.transform);
    }

    public void SetSpawner(bool sw)
    {
        EnemySpawnEnable = sw;
    }
}
