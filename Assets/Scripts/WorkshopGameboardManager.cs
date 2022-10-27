using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using UnityEngine;


public class WorkshopGameboardManager : MonoBehaviour
{
    /*
    * Global variables
    * These sets of variables are used either to display content on the screen,
    * or for accessing ARDK features across methods.
    */
    [SerializeField] [Tooltip("The scene's ARCamera")]
    private Camera _arCamera; // the ARDK's AR Camera instead of the default Unity camera

    [SerializeField] [Tooltip("GameObject to instantiate as the agent")]
    private GameObject _agentPrefab; // the prefab we will be spawning as our agent

    private IGameboard _gameboard; // ARDK's Gameboard object to handle smart placement and navigation
    private GameObject _agentGameObject; // the game object of the spawned agent 
    private WorkshopGameboardAgent _agent; // the agent that can navigate the gameboard
    private IARSession _arSession; // the AR session started by ARDK


    // When our app is enabled, register the OnSessionInitiliazed method to
    // ARDK's SessionInialized event handler
    private void OnEnable()
    {
        ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    private void OnDisable()
    {
        ARSessionFactory.SessionInitialized -= OnSessionInitialized;
        Destroy(_agentGameObject);
        _agentGameObject = null;
    }

    void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
        if (_arSession != null) return;
        _arSession = args.Session;
        GameboardFactory.GameboardInitialized += OnGameboardCreated;
    }

    private void OnGameboardCreated(GameboardCreatedArgs args)
    {
        _gameboard = args.Gameboard;
        _gameboard.GameboardDestroyed += OnGameboardDestroyed;
    }

    private void OnGameboardDestroyed(IArdkEventArgs args)
    {
        _gameboard = null;
    }

    private void Update()
    {
        // If the gameboard isn't running, do not process touches
        if (_gameboard == null) return;
        
        // if there is no touch, return
        if (PlatformAgnosticInput.touchCount <= 0) return;
        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            OnTouchScreen(touch);
        }
    }

    // Process the touch to see if it landed on a gameboard tile
    private void OnTouchScreen(Touch touch)
    {
        // do not process if the AR session is not active
        if (_arSession == null || _arCamera == null)
            return;
        
        //as we are using meshing we can use a standard ray cast
        Ray ray = _arCamera.ScreenPointToRay(touch.position);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // if agent does not exist, spawn it!
            if (_agent == null)
            {
                SpawnAgent(hit.point);
            }
            else
            {
                // if agent does exist, set destination
                _agent.SetDestination(hit.point);
            }
        }
    }

    private void SpawnAgent(Vector3 hitPoint)
    {
        // Instantiate the agent with the predefined prefab
        _agentGameObject = Instantiate(_agentPrefab);
        
        // Set the position of the agent as the raycast hit result
        _agentGameObject.transform.position = hitPoint;
        
        // Have the prefab face towards camera
        var rotation = Vector3.ProjectOnPlane(_arCamera.transform.forward, Vector3.up).normalized;
        _agentGameObject.transform.rotation = Quaternion.LookRotation(-rotation);
        
        // Set agent's state for navigation
        _agent = _agentGameObject.GetComponent<WorkshopGameboardAgent>();
        _agent.State = WorkshopGameboardAgent.AgentNavigationState.Idle;
    }
}