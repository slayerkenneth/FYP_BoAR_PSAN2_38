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
    [SerializeField] private float walkingSpeed = 3.0f;

    private float sightRange;
    private float attackRange;
    
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

    void Start()
    {
        _agentConfig = new AgentConfiguration(jumpPenalty, jumpDistance, pathFindingBehaviour);
        GameboardFactory.GameboardInitialized += OnGameboardCreated;
        animator = gameObject.GetComponent<Animator>();
        sightRange = gameObject.GetComponent<EnemyBehavior>().sightRange;
        attackRange = gameObject.GetComponent<EnemyBehavior>().attackRange;
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
        calmDownTime = calmDownTime - Time.deltaTime;
        if (calmDownTime <= 0)
        {
            calmDown = false;
        }
        
        //using check capsules
        hitColliders = Physics.OverlapSphere(this.transform.position, 0.1f, whatIsEnemy);
        //hide For Performance
        if(hitColliders.Length <= 1)
        {
            dealWithOverlap = false;
        }
        else
        {
            dealWithOverlap = true;
            for (int i = 0; i < hitColliders.Length; ++i)
            {
                if (hitColliders[i].gameObject != this.gameObject)
                {
                    //Debug.Log(this.name + " " + hitColliders[i].gameObject.name);
                    DealWithOverlap(hitColliders[i]);
                }
            } 
        }     

        if(!dealWithOverlap)
        {
            if (calmDown || (!playerInSightRange && !playerInAttackRange))
            {
                if (GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningPlayer)
                    GoToTower();
            }
                
            if (!calmDown && playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (!calmDown && playerInAttackRange && playerInSightRange) AttackPlayer();
        }

        
    }

    public void DealWithOverlap(Collider hitCollider)
    {   
        Vector3 newPosition;
        if (playerInSightRange)
        {
            playerPosition = GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition();
            playerToCollider = Vector3.Distance(hitCollider.gameObject.transform.position, playerPosition);
            playerToThis = Vector3.Distance(this.transform.position, playerPosition);
            //Debug.Log("Show Player Distance: " + playerToCollider + " " + playerToThis);
            if (playerToThis > playerToCollider)
            {
                GoToTower();
                calmDownTime = 10.0f;
                calmDown = true;
            }
        }
    }


    public Vector3 CalculatingEnemyNewPath(GameObject col)
    {
        Vector3 newPosition;
        theta = transform.rotation.y;
        var rnd = new Random();
        float index  = (float)(rnd.Next(1, 11) / 20.0);
        newPosition = new Vector3(transform.position.x - Mathf.Sin(theta) * index, transform.position.y, transform.position.z - Mathf.Cos(theta) * index);
        return newPosition;
    }

    public void GoToTower()
    {   
        //GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode 
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
        transform.LookAt(GameFlowCtrl.GetCloneTower().transform);
        animator.SetTrigger("Attack");
    }

    public void ChasePlayer()
    {   
        transform.LookAt(GameFlowCtrl.GetPlayerMovementCtrl().transform);
        SetDestination(GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition());
    }

    public void AttackPlayer()
    {
        //Make sure enemy doesn't move
        StopMoving();
        transform.LookAt(GameFlowCtrl.GetPlayerMovementCtrl().transform);
        // animator.enabled = true;
        animator.SetTrigger("Attack");
    }


    public void StopMoving()
    {
        if (_actorMoveCoroutine != null)
            StopCoroutine(_actorMoveCoroutine);
    }

    // void onTriggerStay(Collider other)
    // {
    //     if (other.gameObject.tag == "Enemy")
    //     {
    //         Debug.Log("Collide");
    //     }
    // }

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
