using System.Collections;
using System.Collections.Generic;
using Random = System.Random;

using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;

using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class EnemyPathfinding : MonoBehaviour
{

    
    public LayerMask whatIsGround; //ARDK_Gameboard
    public LayerMask whatIsPlayer; //add a layer mask on player
    public LayerMask whatIsEnemy;

    //tower
    private Vector3 towerPosition;
    //private GameObject towerPrefab;
    //public GameObject playerPrefab;
    //private bool attackTower;
    private Transform player;
    private bool hasSpawnPlayer = false;
    private GameObject tower;
    public GameFlowController GameFlowCtrl;
    private GameObject carPrefab;
    public List<Transform> towerPoint;
    private Vector3 finalPosition;

    private bool dealWithOverlap = false;
    private float towerToCollider;
    private float towerToThis;
    private float playerToCollider;
    private float playerToThis;
    private Vector3 playerPosition;
    private Collider[] hitColliders;
    private float theta;

    private Animator animator;
    public Collider attackOverlap;
    private bool attackPlayer = false;
    private bool calmDown = false;
    private float calmDownTime = 0.0f;

    private float enemySpeed;
    // private float jumpHeight = 2.0f;
    // private float gravityValue = -9.81f;
    // private Vector3 enemyVelocity;
    // private bool groundedPlayer;

    //State
    private bool playerInSightRange;
    private bool playerInAttackRange;

    // [Header("Agent Settings")]
    // [SerializeField]
    [SerializeField] public float walkingSpeed = 3.0f;

    private float sightRange;
    private float attackRange;
    private float colliderRange;
    private float attackTime;
    private bool exitTower = false;

    public int enemyID;
    private bool firstTimePatrol = true;

    private EnemyBehavior behaviourScript;

    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        behaviourScript = gameObject.GetComponent<EnemyBehavior>();
        enemySpeed = behaviourScript.speed;
        sightRange = behaviourScript.sightRange;
        attackRange = behaviourScript.attackRange;
        colliderRange = behaviourScript.colliderRange;
        attackTime = 0;
        playerInSightRange = false;
        playerInAttackRange = false;
        // get the player position (need to change)
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        //tower = GameObject.FindGameObjectWithTag("Tower(D)").transform;
        //Debug.Log(GameObject.FindGameObjectWithTag("Tower(D)"));
    }

    // Update is called once per frame
    void Update()
    { 
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        // if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningPlayer)
        //     hasSpawnPlayer = true;

        if(!dealWithOverlap)
        {
            if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.PushCarBattleMode)
            {
                if (!playerInSightRange && !playerInAttackRange)
                {
                    GoToCar();
                }
                if (playerInSightRange && !playerInAttackRange) 
                {
                    ChasePlayer();
                }
                if (playerInAttackRange && playerInSightRange) 
                {
                    AttackPlayer();
                }
            }
            else if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.CapturePointMode)
            {
                if (!playerInSightRange && !playerInAttackRange)
                {
                    GoToTower();
                }
                if (playerInSightRange && !playerInAttackRange) 
                {
                    ChasePlayer_CapturePoint();
                }
                if (playerInAttackRange && playerInSightRange) 
                {
                    AttackPlayer();
                }
            }

            else if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
            {
                if (!playerInSightRange && !playerInAttackRange)
                {
                    GoToTower();
                }
                if (playerInSightRange && !playerInAttackRange) 
                {
                    ChasePlayer();
                }
                if (playerInAttackRange && playerInSightRange) 
                {
                    AttackPlayer();
                }
            }
            else if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DungeonMode)
            {
                if (!playerInSightRange && !playerInAttackRange)
                {
                    Patrolling();
                }
                if (playerInSightRange && !playerInAttackRange) 
                {
                    ChasePlayer();
                }
                if (playerInAttackRange && playerInSightRange) 
                {
                    AttackPlayer();
                }
            }
            else if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.BossFight)
            {
                
                if (playerInSightRange && !playerInAttackRange) 
                {
                    ChasePlayer();
                }
                if (playerInAttackRange && playerInSightRange) 
                {
                    AttackPlayer();
                }
            }
        }  
             
    }

    // public void DealWithOverlap(Collider hitCollider)
    // {   
    //     Vector3 newPosition;
    //     if (playerInSightRange)
    //     {
    //         playerPosition = GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition();
    //         playerToCollider = Vector3.Distance(hitCollider.gameObject.transform.position, playerPosition);
    //         playerToThis = Vector3.Distance(this.transform.position, playerPosition);
    //         //Debug.Log("Show Player Distance: " + playerToCollider + " " + playerToThis);
    //         if (playerToThis >= playerToCollider)
    //         {
    //             GoToTower();
    //             calmDownTime = 5.0f;
    //             calmDown = true;
    //         }
    //     }
    // }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Tower(C)"))
        {
            exitTower = false;
        }
        
    }

    private void OnTriggerStay(Collider other) 
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            dealWithOverlap = true;
            DealWithOverlap(other);
        }
        
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            dealWithOverlap = false;
        }
        if (other.gameObject.CompareTag("Tower(C)"))
        {
            exitTower = true;
        }
    }

    // public GameObject FindClosestPlayer(GameObject[] playerList)
    // {
    //     float distance = float.PositiveInfinity;
    //     GameObject result_player = null;
    //     foreach (GameObject player in playerList) 
    //     {
    //         Debug.Log(player.transform.name);
    //         float temp_distance = Vector3.Distance(transform.localPosition, player.transform.localPosition);
    //         // Debug.Log(enemy.transform.name + " " + temp_distance);
    //         if (temp_distance < distance)
    //         {
    //             distance = temp_distance;
    //             result_player = player;
    //         }
    //     }
    //     return result_player;
    // }

    public void DealWithOverlap(Collider hitCollider)
    {   
        Vector3 newPosition;
        if (GameFlowCtrl.getPlayerMovementCtrl())
        {
            var playerList = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();
            //GameObject closest_player = FindClosestPlayer(playerList);
            float ColliderToPlayer = Vector3.Distance(hitCollider.gameObject.transform.position, playerList.transform.position);
            float ThisToPlayer = Vector3.Distance(transform.position, playerList.transform.position);
            //Debug.Log("Show Player Distance: " + ColliderToEnemy + " " + ThisToEnemy);
            if (ThisToPlayer > ColliderToPlayer)
            {
                newPosition = CalculatingEnemyNewPath(gameObject);
                transform.localPosition = Vector3.MoveTowards(transform.position, newPosition, 0.5f * Time.deltaTime);
            }
        }
        
    }

    public Vector3 CalculatingEnemyNewPath(GameObject col)
    {
        Vector3 newPosition;
        float theta;
        if (transform.localEulerAngles.y > col.transform.localEulerAngles.y)
        {
            theta = transform.localEulerAngles.y + 50;
            if (theta > 360)
            {
                theta = theta - 360;
            }
        }
        else
        {
            theta = transform.localEulerAngles.y - 50;
            if (theta < 0)
            {
                theta = 360 - theta;
            }
        }
        
        newPosition = new Vector3(transform.position.x + Mathf.Sin(theta) * 0.5f, transform.position.y, transform.position.z + Mathf.Cos(theta) * 0.5f);
        return newPosition;
    }

    public void GoToTower()
    {   
        // it will output error, says that cannot call 2 event in the same time
        if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.CapturePointMode)
        {
            tower = GameFlowCtrl.GetActiveCaptureTowerPrefab();
        }
        if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
        {
            tower = GameFlowCtrl.GetActiveDefenseTowerPrefab();
        }
        
        if (tower.GetComponentInChildren<Tower>())
        {
            // transform.LookAt(GameFlowCtrl.GetCloneTower().transform);
            towerPoint = tower.GetComponentInChildren<Tower>().GetTowerPoints();
            finalPosition = towerPoint[enemyID].position;
            SetDestination(finalPosition); 
            //Debug.Log(gameObject.name + " " + transform.position);        
            if(Vector3.Distance(transform.position, finalPosition) <= 0.2f)
            {
                if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
                {
                    AttackTower(tower.GetComponentInChildren<DefenceTarget>().GetSpawnedTower().transform);
                }   
            }  
        }
    }

    public void AttackTower(Transform towerTarget)
    {
        transform.LookAt(towerTarget);
        if (!isAttacking())
        {
            attackTime = gameObject.GetComponent<EnemyBehavior>().attackTime;
            gameObject.GetComponent<EnemyBehavior>().playerInAttackRange = playerInAttackRange;
            animator.SetTrigger("Attack");     
        }
    }

    public void ChasePlayer()
    {   
        Transform tempPlayer = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();
        tempPlayer.position = new Vector3 (tempPlayer.position.x, transform.position.y, tempPlayer.position.z);
        transform.LookAt(tempPlayer);
        SetDestination(GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition());
    }

    public void AttackPlayer()
    {
        //Make sure enemy doesn't move
        StopMoving();
        Transform tempPlayer = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();
        tempPlayer.position = new Vector3 (tempPlayer.position.x, transform.position.y, tempPlayer.position.z);
        transform.LookAt(tempPlayer);
        if (!isAttacking())
        {
            attackTime = gameObject.GetComponent<EnemyBehavior>().attackTime;
            gameObject.GetComponent<EnemyBehavior>().playerInAttackRange = playerInAttackRange;
            animator.SetTrigger("Attack");
        }
    }

    public bool isAttacking()
    {
        attackTime -= Time.deltaTime;
        if (attackTime > 0)
            return true;
        else
            return false;
    }

    public void SetDestination(Vector3 destination)
    {
        Vector3 relativePosition = destination - transform.position;
        transform.rotation = Quaternion.LookRotation(relativePosition);
        transform.localPosition = Vector3.MoveTowards(transform.position, destination, enemySpeed * Time.deltaTime);
    }

    public void StopMoving()
    {
        transform.localPosition = Vector3.MoveTowards(transform.position, transform.position, 0);
    }

    public void GoToCar()
    {   
        carPrefab = GameFlowCtrl.GetActivePushCarParent();
        if (carPrefab)
        {
            finalPosition = carPrefab.GetComponentInChildren<PushCarController>().ActiveCar.transform.position;
            if(Vector3.Distance(transform.position, finalPosition) <= 0.3f)
                StopMoving();
            else
                SetDestination(finalPosition); 
            //Debug.Log(gameObject.name + " " + transform.position);        
            
        }
        
    }

    public void Patrolling()
    {
        float RandomX = UnityEngine.Random.Range(-5, 5);
        float RandomZ = UnityEngine.Random.Range(-5, 5);
        Vector3 temp_position = new Vector3 (transform.position.x + RandomX, transform.position.y, transform.position.z + RandomZ);
        // Debug.Log(GameFlowCtrl.getARCtrl().GetActiveGameboard().CheckFit(center: temp_position, 0.1f));
        if (GameFlowCtrl.getARCtrl().GetActiveGameboard().CheckFit(center: temp_position, 0.1f))
        {
            if (firstTimePatrol)
            {
                finalPosition = temp_position;
            }
                

            if(isWalking(finalPosition, 0.01f, firstTimePatrol))
            {
                SetDestination(finalPosition);
            }
            else
            {
                finalPosition = temp_position;
            }
        }
        
    }

    public bool isWalking(Vector3 temp_position, float limit, bool firstTime)
    {
        if (Vector3.Distance(transform.position, temp_position) > limit || firstTime)
        {
            firstTimePatrol = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ChasePlayer_CapturePoint()
    {
        Transform tempPlayer = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();
        tempPlayer.position = new Vector3 (tempPlayer.position.x, transform.position.y, tempPlayer.position.z);
        transform.LookAt(tempPlayer);
        if (!exitTower)
        {
            SetDestination(GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition());
        } 
        else if (exitTower && playerInSightRange)
        {
            StopMoving();
        }
        else if (exitTower && !playerInSightRange)
        {
            tower =  GameFlowCtrl.GetActiveCaptureTowerPrefab();
            if (tower)
            {
                // transform.LookAt(GameFlowCtrl.GetCloneTower().transform);
                towerPoint = tower.GetComponentInChildren<Tower>().GetTowerPoints();
                finalPosition = towerPoint[enemyID].position;
                SetDestination(finalPosition); 
            }
        }
    }

  
}
