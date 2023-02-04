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
    [SerializeField] private CentralBattleController centralBattleCtrl;

    [Header("Spawner Setting")] 
    [SerializeField] public List<GameObject> EnemySpawnPrefabList;
    [SerializeField] public GameObject EnemyParentObj;
    [SerializeField] public int MaxEnemyCount;
    public List<Vector3> EnemySpawnLocationList;
    private bool EnemySpawnEnable = false;
    public static int currentEnemyCount;

    private Vector3 towerPosition;
    public LayerMask whatIsGround; //ARDK_Gameboard
    public LayerMask whatIsPlayer; //add a layer mask on player
    public GameObject towerPrefab;
    public GameObject playerPrefab;
    
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
        e.AddComponent<EnemyPathfinding>();
        e.GetComponent<EnemyPathfinding>().GameFlowCtrl = GameFlowCtrl;
        e.GetComponent<EnemyPathfinding>().whatIsGround = whatIsGround;
        e.GetComponent<EnemyPathfinding>().whatIsPlayer = whatIsPlayer;
        e.GetComponent<EnemyPathfinding>().towerPrefab = towerPrefab;
        e.GetComponent<EnemyPathfinding>().playerPrefab = playerPrefab;
        e.GetComponent<CombatHandler>().SetCentralCombatHandler(centralBattleCtrl);
        yield return new WaitForSeconds(time);
    }

    public void SetSpawner(bool sw)
    {
        EnemySpawnEnable = sw;
    }

    public Vector3 getTowerPosition()
    {
        return towerPosition;
    }
}
