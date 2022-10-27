using Niantic.ARDK.Extensions.Gameboard;

using UnityEngine;

public class CoinGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject CoinPrefab;
    
    [SerializeField]
    private float _coinSize = 0.5f;

    [SerializeField]
    private float _minDiscoveredArea = 5f;
    
    [SerializeField]
    private float _minDistanceSpawnToAgent = 1.5f;

    private float _searchRadius = 10f;
    private Transform _agent;
    private IGameboard _gameboard;
    private bool _shouldCreateNewCoin = false;
    
    void Start()
    {
        GameboardFactory.GameboardInitialized += args => _gameboard = args.Gameboard;
    }
    
     // 1. Is total discovered Gameboard area big enough?     
     // 2. Is proposed spawn far enough away from agents position?     
     // 3. Does the proposed spawn fit the coin?
     void Update()
    {
        if (_shouldCreateNewCoin)
        {
            // 1. Is discovered Gameboard area big enough?  
            if (_gameboard.Area > _minDiscoveredArea)
            {
                _gameboard.FindRandomPosition(_agent.position, _searchRadius, out Vector3 proposedSpawnPosition);
                float distanceSpawnToAgent = Vector3.Distance(_agent.position, proposedSpawnPosition);
        
                // 2. Is proposed spawn far enough away from agents position?
                if (distanceSpawnToAgent > _minDistanceSpawnToAgent)
                {
                    // 3. Does the proposed spawn fit the coin?
                    if (_gameboard.CheckFit(proposedSpawnPosition, _coinSize))
                    {
                        Instantiate(CoinPrefab, proposedSpawnPosition, Quaternion.identity);
                        _shouldCreateNewCoin = false;
                    }
                }
            }
        }
    }

    public void SetAgent(Transform agent)
    {
        _agent = agent;
    }

    public void CreateNewCoin()
    {
        _shouldCreateNewCoin = true;
    }
}
