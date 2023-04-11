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
    [SerializeField] public List<Vector3> EnemyAtkTowerPositionList;
    [SerializeField] public GameObject EnemyParentObj;
    [SerializeField] public int MaxEnemyCount;
    public List<Vector3> EnemySpawnLocationList;
    private bool EnemySpawnEnable = false;
    public static int currentEnemyCount;

    private Vector3 towerPosition;
    public LayerMask whatIsGround; //ARDK_Gameboard
    public LayerMask whatIsPlayer; //add a layer mask on player
    public LayerMask whatIsEnemy;
    public GameObject towerPrefab;
    public GameObject playerPrefab;
    
    
    void Start()
    {
        _activeGameboard = ARCtrl.GetActiveGameboard();
        currentEnemyCount = 0;
    }
    
    void Update()
    {
        for (int i=0; i < EnemySpawnLocationList.Count; i++)
        {
            EnemyAtkTowerPositionList.Add(getTowerPosition());
            if (!EnemySpawnEnable || EnemySpawnPrefabList.Count == 0 || EnemySpawnLocationList.Count == 0 || currentEnemyCount >= MaxEnemyCount) return;
            StartCoroutine(SpawnEnemyAfterWaiting(1000, EnemySpawnPrefabList[i], EnemySpawnLocationList[i], EnemyAtkTowerPositionList[i]));
            currentEnemyCount++;
        }
        
    }

    public GameObject SpawnEnemy(GameObject enemyPrefab, Vector3 SpawnLocationVec)
    {
        return Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(), EnemyParentObj.transform);
    }
    
    public IEnumerator SpawnEnemyAfterWaiting(float time, GameObject enemyPrefab, Vector3 SpawnLocationVec, Vector3 AtkTowerPos)
    {
        var e = Instantiate(enemyPrefab, SpawnLocationVec, new Quaternion(0,0,0,0), EnemyParentObj.transform);
        e.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        e.AddComponent<EnemyPathfinding>();
        e.GetComponent<EnemyBehavior>().GameFlowCtrl = GameFlowCtrl;
        e.GetComponent<EnemyPathfinding>().GameFlowCtrl = GameFlowCtrl;
        e.GetComponent<EnemyPathfinding>().AtkTowerPosition = AtkTowerPos;
        e.GetComponent<EnemyPathfinding>().whatIsGround = whatIsGround;
        e.GetComponent<EnemyPathfinding>().whatIsPlayer = whatIsPlayer;
        e.GetComponent<CombatHandler>().SetCentralCombatHandler(centralBattleCtrl);
        e.GetComponent<EnemyPathfinding>().whatIsEnemy = whatIsEnemy;
        yield return new WaitForSeconds(time);
    }

    public void SetSpawner(bool sw)
    {
        EnemySpawnEnable = sw;
        if (!sw)
        {
            GameFlowCtrl.ResetEnemySpawnLocationList();
            ResetEnemySpawnLocationList();
            return;
        }
        GameFlowCtrl.SetRandomEnemySpawnLocationVectors(MaxEnemyCount);
        EnemySpawnLocationList = GameFlowCtrl.GetEnemySpawnLocationVectorList();
    }

    public Vector3 getTowerPosition()
    {
        return towerPosition;
    }

    private void ResetEnemySpawnLocationList()
    {
        EnemySpawnLocationList.Clear();
    }
}
