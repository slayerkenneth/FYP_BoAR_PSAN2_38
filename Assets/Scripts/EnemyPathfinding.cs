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
    public Vector3 AtkPosition;
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
    }

    // Update is called once per frame
    void Update()
    { 
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

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
                    Debug.Log(transform.name + " Attacking Mode");
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
                    Debug.Log(transform.name + " Attacking Mode");
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
                    Debug.Log(transform.name + " Attacking Mode");
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
                    Debug.Log(transform.name + " Attacking Mode");
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
        //If the euler angle of this transform is larger than the overlap's solider euler angle, it will move anticlockwise
        //until there are no overlap, else it will move clockwise
        if (transform.localEulerAngles.y > col.transform.localEulerAngles.y)
        {
            theta = col.transform.localEulerAngles.y + 120;
            if (theta > 360)
            {
                theta = theta - 360;
            }
        }
        else
        {
            theta = col.transform.localEulerAngles.y - 120;
            if (theta < 0)
            {
                theta = 360 + theta;
            }
        }
        theta = theta * Mathf.Deg2Rad;
        
        newPosition = new Vector3(transform.position.x + Mathf.Sin(theta) * 0.5f, transform.position.y, transform.position.z + Mathf.Cos(theta) * 0.5f);
        return newPosition;
    }

    public void GoToTower()
    {   
        // If capture point mode, call the capture point tower
        if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.CapturePointMode)
        {
            tower = GameFlowCtrl.GetActiveCaptureTowerPrefab();
        }
        // If defense point mode, call the defense point tower
        if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
        {
            tower = GameFlowCtrl.GetActiveDefenseTowerPrefab();
        }
        
        if (tower.GetComponentInChildren<Tower>())
        {
            //towerPoint means the specfic location of each enemy
            finalPosition = tower.transform.position + AtkPosition;
            //set the location
            SetDestination(finalPosition);        
            if(Vector3.Distance(transform.position, finalPosition) <= 0.2f)
            {
                //if it is defense point mode, it will also do damage on the tower
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
        //set the location
        SetDestination(GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition());
    }

    public void AttackPlayer()
    {
        //Make sure enemy doesn't move
        StopMoving();
        Transform tempPlayer = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();
        tempPlayer.position = new Vector3 (tempPlayer.position.x, transform.position.y, tempPlayer.position.z);
        transform.LookAt(tempPlayer);
        //each enemy will have different time between two normal attack
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
            //set location to the car
            finalPosition = carPrefab.GetComponentInChildren<PushCarController>().ActiveCar.transform.position;
            if(Vector3.Distance(transform.position, finalPosition) <= 0.3f)
                StopMoving();
            else
                SetDestination(finalPosition);  
        }
        
    }

    public void Patrolling()
    {
        float RandomX = UnityEngine.Random.Range(-5, 5);
        float RandomZ = UnityEngine.Random.Range(-5, 5);
        Vector3 temp_position = new Vector3 (transform.position.x + RandomX, transform.position.y, transform.position.z + RandomZ);
        //make sure the random positio is fit in the gameboard
        if (GameFlowCtrl.getARCtrl().GetActiveGameboard().CheckFit(center: temp_position, 0.1f))
        {
            if (firstTimePatrol)
            {
                finalPosition = temp_position;
            }
                
            //set the location to that random point
            //if the enemy is walking, it will not assign any random point for enemy to walk unitil it stop moving
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
        //if the enemy not exit the tower, set the location to player
        if (!exitTower)
        {
            SetDestination(GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition());
        } 
        //if the enemy exit the tower but in player sight range, it just stop moving
        else if (exitTower && playerInSightRange)
        {
            StopMoving();
        }
        //if the enemy exit the tower but not in player sight range, it set the location to tower
        else if (exitTower && !playerInSightRange)
        {
            tower =  GameFlowCtrl.GetActiveCaptureTowerPrefab();
            if (tower)
            {
                towerPoint = tower.GetComponentInChildren<Tower>().GetTowerPoints();
                finalPosition = towerPoint[enemyID].position;
                SetDestination(finalPosition); 
            }
        }
    }

    public void setSpeed(float speed)
    {
        enemySpeed = speed;
    }

    public void resetSpeed()
    {
        enemySpeed = behaviourScript.speed;
    }

    public float getSpeed()
    {
        return enemySpeed;
    }

  
}
