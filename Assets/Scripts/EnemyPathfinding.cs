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
    private Transform tower;
    public GameFlowController GameFlowCtrl;
    private bool towerSpawned;
    public Vector3 AtkTowerPosition;
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

    private float enemySpeed = 2.0f;
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
    
    //Jump
    private float jumpDistance = 1;
    private int jumpPenalty = 2;
    
    private PathFindingBehaviour pathFindingBehaviour = PathFindingBehaviour.InterSurfacePreferResults;

    public enum AgentNavigationState {Paused, Idle, HasPath}
    public AgentNavigationState State { get; set; } = AgentNavigationState.Idle;
    private Path _path = new Path(null, Path.Status.PathInvalid);
    private int _currentWaypoint = 0;
    private Vector3 _destination;

    private Coroutine _actorMoveCoroutine;
    private Coroutine _actorJumpCoroutine;

    private AgentConfiguration _agentConfig;

    private IGameboard _gameboard;

    private EnemyBehavior behaviourScript;

    void Start()
    {
        _agentConfig = new AgentConfiguration(jumpPenalty, jumpDistance, pathFindingBehaviour);
        GameboardFactory.GameboardInitialized += OnGameboardCreated;
        animator = gameObject.GetComponent<Animator>();
        behaviourScript = gameObject.GetComponent<EnemyBehavior>();
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

    private void OnGameboardCreated(GameboardCreatedArgs args)
    {
        _gameboard = args.Gameboard;
        _gameboard.GameboardUpdated += OnGameboardUpdated;
        _gameboard.GameboardDestroyed += OnGameboardDestroyed;
    }

    private void OnGameboardDestroyed(IArdkEventArgs args)
    {
        _gameboard = null;
        _path = new Path(null, Path.Status.PathInvalid);
        StopMoving();
    }

    private void OnGameboardUpdated(GameboardUpdatedArgs args)
    {
        if (State == AgentNavigationState.Idle || _path.PathStatus == Path.Status.PathInvalid)
            return;

        if (args.PruneOrClear)
        {
            SetDestination(_destination);
            return;
        }

        for (int i = _currentWaypoint; i < _path.Waypoints.Count; i++)
        {
            if (args.RemovedNodes.Contains(_path.Waypoints[i].Coordinates))
                SetDestination(_destination);
        }
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
                Debug.Log("Halo1");
            }
            else if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.CapturePointMode)
            {
                Debug.Log("Halo2");
            }

            else if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode)
            {
                Debug.Log("Halo3");
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
                Debug.Log("Halo4");
            }
            else if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.BossFight)
            {
                Debug.Log("Halo5");
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
        var playerList = GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform();
        //GameObject closest_player = FindClosestPlayer(playerList);
        float ColliderToPlayer = Vector3.Distance(hitCollider.gameObject.transform.position, playerList.transform.position);
        float ThisToPlayer = Vector3.Distance(transform.position, playerList.transform.position);
        //Debug.Log("Show Player Distance: " + ColliderToEnemy + " " + ThisToEnemy);
        if (ThisToPlayer > ColliderToPlayer)
        {
            newPosition = CalculatingEnemyNewPath(gameObject);
            transform.localPosition = Vector3.MoveTowards(transform.position, newPosition, 0.1f * Time.deltaTime);
        }
    }

    public Vector3 CalculatingEnemyNewPath(GameObject col)
    {
        Vector3 newPosition;
        float theta;
        if (transform.localEulerAngles.y > col.transform.localEulerAngles.y)
        {
            theta = transform.localEulerAngles.y - 120;
            if (theta < -180)
            {
                theta = 360 + theta;
            }
        }
        else
        {
            theta = transform.localEulerAngles.y + 120;
            if (theta > 180)
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
        towerSpawned = GameFlowCtrl.GetTowerSpawnLocationVector(out towerPosition);
        if (towerSpawned)
        {
            // transform.LookAt(GameFlowCtrl.GetCloneTower().transform);
            finalPosition = towerPosition + AtkTowerPosition;
            SetDestination(finalPosition); 
            //Debug.Log(gameObject.name + " " + transform.position);        
            if(Vector3.Distance(transform.position, finalPosition) <= 0.2f)
            {
                AttackTower();
            }  
        }
    }

    public void AttackTower()
    {
        //attack (need to change)
        Transform tempTower = GameFlowCtrl.GetCloneTower().transform;
        tempTower.position = new Vector3 (tempTower.position.x, transform.position.y, tempTower.position.z);
        transform.LookAt(tempTower);
        if (!isAttacking())
        {
            attackTime = gameObject.GetComponent<EnemyBehavior>().attackTime;
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

    public void StopMoving()
    {
        if (_actorMoveCoroutine != null)
            StopCoroutine(_actorMoveCoroutine);
    }

    private void OnDestroy()
    {
        GameboardFactory.GameboardInitialized -= OnGameboardCreated;
        if (_gameboard != null)
        {
            _gameboard.GameboardUpdated -= OnGameboardUpdated;
            _gameboard.GameboardDestroyed -= OnGameboardDestroyed;
        }
    }

    public void SetDestination(Vector3 destination)
    {
        StopMoving();
        
        if (_gameboard == null)
            return;
        
        _destination = destination;
        _currentWaypoint = 0;
        
        Vector3 startOnBoard;
        _gameboard.FindNearestFreePosition(transform.position, out startOnBoard);
        
        bool result = _gameboard.CalculatePath(startOnBoard, destination, _agentConfig, out _path);
        
        if (!result)
            State = AgentNavigationState.Idle;
        else
        {
            State = AgentNavigationState.HasPath;
            _actorMoveCoroutine = StartCoroutine(Move(this.transform, _path.Waypoints));
        }
    }

    private void StayOnGameboard()
    {
        if (_gameboard == null || _gameboard.Area == 0)
            return;

        if (_gameboard.IsOnGameboard(transform.position, 0.2f))
            return;

        List<Waypoint> pathToGameboard = new List<Waypoint>();
        Vector3 nearestPosition;
        _gameboard.FindNearestFreePosition(transform.position, out nearestPosition);

        _destination = nearestPosition;
        _currentWaypoint = 0;

        pathToGameboard.Add(new Waypoint
        (
            transform.position,
            Waypoint.MovementType.Walk,
            Utils.PositionToTile(transform.position, _gameboard.Settings.TileSize)
        ));

        pathToGameboard.Add(new Waypoint
        (
            nearestPosition,
            Waypoint.MovementType.SurfaceEntry,
            Utils.PositionToTile(nearestPosition, _gameboard.Settings.TileSize)
        ));

        _path = new Path(pathToGameboard, Path.Status.PathComplete);
        _actorMoveCoroutine = StartCoroutine(Move(this.transform, _path.Waypoints));
        State = AgentNavigationState.HasPath;
    }

    private IEnumerator Move(Transform actor, IList<Waypoint> path)
    {
        var startPosition = actor.position;
        var startRotation = actor.rotation;
        var interval = 0.0f;
        var destIdx = 0;

        while (destIdx < path.Count)
        {
            //do i need to jump or walk to the target point
            if (path[destIdx].Type == Waypoint.MovementType.SurfaceEntry)
            {
                yield return new WaitForSeconds(0.5f);

                _actorJumpCoroutine = StartCoroutine
                (
                    Jump(actor, actor.position, path[destIdx].WorldPosition)
                );

                yield return _actorJumpCoroutine;

                _actorJumpCoroutine = null;
                startPosition = actor.position;
                startRotation = actor.rotation;

            }
            else
            {
                //move on step towards target waypoint
                interval += Time.deltaTime * walkingSpeed;
                actor.position = Vector3.Lerp(startPosition, path[destIdx].WorldPosition, interval);
            }

            //face the direction we are moving
            Vector3 lookRotationTarget = (path[destIdx].WorldPosition - transform.position);

            //ignore up/down we dont want the creature leaning forward/backward.
            lookRotationTarget.y = 0.0f;
            lookRotationTarget = lookRotationTarget.normalized;

            //check for bad rotation
            if (lookRotationTarget != Vector3.zero)
                transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(lookRotationTarget), interval);

            //have we reached our target position, if so go to the next waypoint
            if (Vector3.Distance(actor.position, path[destIdx].WorldPosition) < 0.01f)
            {
                startPosition = actor.position;
                startRotation = actor.rotation;
                interval = 0;
                destIdx++;
            }

            yield return null;
        }

        _actorMoveCoroutine = null;
        State = AgentNavigationState.Idle;
    }

    private IEnumerator Jump(Transform actor, Vector3 from, Vector3 to, float speed = 2.0f)
    {
        var interval = 0.0f;
        Quaternion startRotation = actor.rotation;
        var height = Mathf.Max(0.1f, Mathf.Abs(to.y - from.y));
        while (interval < 1.0f)
        {
            interval += Time.deltaTime * speed;
            Vector3 rotation = to - from;
            rotation = Vector3.ProjectOnPlane(rotation, Vector3.up).normalized;
            if (rotation != Vector3.zero)
                transform.rotation = Quaternion.Lerp(startRotation, Quaternion.LookRotation(rotation), interval);
            var p = Vector3.Lerp(from, to, interval);
            actor.position = new Vector3
            (
                p.x,
                -4.0f * height * interval * interval +
                4.0f * height * interval +
                Mathf.Lerp(from.y, to.y, interval),
                p.z
            );

            yield return null;
        }

        actor.position = to;
    }

  
}
