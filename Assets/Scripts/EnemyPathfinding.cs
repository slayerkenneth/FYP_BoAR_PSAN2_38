using System.Collections;
using System.Collections.Generic;

using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;

using UnityEngine;
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
    private bool attackTower;
    private Transform player;
    private Transform tower;
    public GameFlowController GameFlowCtrl;
    private bool towerSpawned;

    private float enemySpeed = 2.0f;
    // private float jumpHeight = 2.0f;
    // private float gravityValue = -9.81f;
    // private Vector3 enemyVelocity;
    // private bool groundedPlayer;


    //State
    [SerializeField] private float sightRange = 1.0f;
    [SerializeField] private float attackRange = 0.2f;
    [SerializeField] private bool playerInSightRange;
    [SerializeField] private bool playerInAttackRange;
    [SerializeField] private bool towerInAttackRange;

    // [Header("Agent Settings")]
    // [SerializeField]
    [SerializeField] private float walkingSpeed = 3.0f;
    
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
        attackTower = false;
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
        
        //using check capsules
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, 0.3f, whatIsEnemy);
        // hide For Performance
        // for (int i = 0; i < hitColliders.Length; ++i)
        // {
        //     if (hitColliders[i].gameObject != this.gameObject)
        //     {
        //         Debug.Log(this.name + " " + hitColliders[i].gameObject.name);
        //         DealWithOverlap(hitColliders[i]);
        //     }
        // }

        if (!playerInSightRange && !playerInAttackRange)
        {
            if (!attackTower && GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.SpawningPlayer)
                GoToTower();
            else
                AttackTower();
        }
            
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    public void DealWithOverlap(Collider hitCollider)
    {   
        Vector3 newPosition;
        if (!playerInSightRange)
        {
            float towerToCollider = Vector3.Distance(hitCollider.gameObject.transform.position, towerPosition);
            float towerToThis = Vector3.Distance(this.transform.position, towerPosition);
            //Debug.Log("Show Tower Distance: " + towerToCollider + " " + towerToThis);
            if (towerToThis > towerToCollider)
            {
                newPosition = CalculatingEnemyNewPath(hitCollider.gameObject);
                SetDestination(newPosition);
            }      
        }
        else
        {
            Vector3 playerPosition = GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition();
            float playerToCollider = Vector3.Distance(hitCollider.gameObject.transform.position, playerPosition);
            float playerToThis = Vector3.Distance(this.transform.position, playerPosition);
            //Debug.Log("Show Player Distance: " + playerToCollider + " " + playerToThis);
            if (playerToThis > playerToCollider)
            {
                newPosition = CalculatingEnemyNewPath(hitCollider.gameObject);
                SetDestination(newPosition);
            }
        }
    }

    public Vector3 CalculatingEnemyNewPath(GameObject col)
    {
        Vector3 newPosition;
        float theta;
        if (transform.rotation.y > col.transform.rotation.y)
        {
            theta = transform.rotation.y - 120;
            if (theta < -180)
            {
                theta = 360 + theta;
            }
        }
        else
        {
            theta = transform.rotation.y + 120;
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
        //GameFlowCtrl.battleSceneState == GameFlowController.PVEBattleSceneState.DefencePointMode 
        // it will output error, says that cannot call 2 event in the same time
        towerSpawned = GameFlowCtrl.GetTowerSpawnLocationVector(out towerPosition);
        if (towerSpawned)
        {
            transform.LookAt(GameFlowCtrl.GetCloneTower().transform);
            SetDestination(towerPosition);         
            if(Vector3.Distance(transform.position, towerPosition) <= attackRange)
            {
                StopMoving();
                attackTower = true;
            }  
        }
    }

    public void AttackTower()
    {
        //attack (need to change)
        transform.LookAt(GameFlowCtrl.GetCloneTower().transform);
        // Debug.Log(this.name + " Attacking Tower");
    }

    public void ChasePlayer()
    {   
        transform.LookAt(GameFlowCtrl.getARCtrl().getClonePlayer().transform);
        SetDestination(GameFlowCtrl.getPlayerMovementCtrl().getPlayerPosition());
        attackTower = false;
    }

    public void AttackPlayer()
    {
        //Make sure enemy doesn't move
        StopMoving();
        transform.LookAt(GameFlowCtrl.getARCtrl().getClonePlayer().transform);
 
        //attack (need to change)
        Debug.Log(this.name + " Attacking Player");
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
