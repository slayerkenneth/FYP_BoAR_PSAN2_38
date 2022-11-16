using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.Extensions.Gameboard;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Semantics;

public class ARController : MonoBehaviour
{
    /*
    * Global variables for handling AR meshing, refer to gameboard example
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
    private int touchCount = 0;
    private float gameboardArea = 0;
    
    
    /*
     * Global variable for handling semantic texture and shaders
     */
    [SerializeField] [Tooltip("Semantic texture and custom shader rendering")]
    public Material _shaderMaterial;
    Texture2D _semanticTexture;
    public ARSemanticSegmentationManager _semanticManager;


    [SerializeField] public Text DebugText;
    [SerializeField] public Text AreaText;
    
    private void OnEnable()
    {
        //add a callback for catching the updated semantic buffer
        _semanticManager.SemanticBufferUpdated += OnSemanticsBufferUpdated;
        //on initialisation
        ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }
    
    private void OnDisable()
    {
        ARSessionFactory.SessionInitialized -= OnSessionInitialized;
        Destroy(_agentGameObject);
        _agentGameObject = null;
        DebugText.text = "Debug: AR Session Disabled";
    }
    
    /*
     *  Implementing the AR session Init function
     */
    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
        if (_arSession != null && _arSession == args.Session) return;
        // Semantic texture part
        Resolution resolution = new Resolution();
        resolution.width = Screen.width;
        resolution.height = Screen.height;
        // ARSessionFactory.SessionInitialized -= OnSessionInitialized;
        
        //Gameboard init section
        _arSession = args.Session;
        GameboardFactory.GameboardInitialized += OnGameboardCreated;
        // ARSessionFactory.SessionInitialized -= OnSessionInitialized;
    }
    
    private void OnGameboardCreated(GameboardCreatedArgs args)
    {
        _gameboard = args.Gameboard;
        _gameboard.GameboardDestroyed += OnGameboardDestroyed;
        DebugText.text = "Debug: GB created";
    }

    private void OnGameboardDestroyed(IArdkEventArgs args)
    {
        _gameboard = null;
    }
    
    
    /*
     * From the Awareness buffer to retrieve the channel index for specific type of semantics
     */
    private void OnSemanticsBufferUpdated(ContextAwarenessStreamUpdatedArgs<ISemanticBuffer> args)
    {
        //get the current buffer
        ISemanticBuffer semanticBuffer = args.Sender.AwarenessBuffer;
        //get the index for sky
        int channel = semanticBuffer.GetChannelIndex("ground");

        semanticBuffer.CreateOrUpdateTextureARGB32(
            ref _semanticTexture, channel, FilterMode.Trilinear
        );
        
        // //alt
        // _semanticManager.SemanticBufferProcessor.CopyToAlignedTextureARGB32(
        //     texture: ref _semanticTexture,
        //     channel: channel,
        //     orientation: Screen.orientation
        //     );

        //not good performance
        // _overlayImage.texture = _semanticTexture;
    }
    
    /*
     * Passing the material with semantics texture and matrix of the buffered transfrom to shader of Materials
     */
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //pass in our texture
        //Our Depth Buffer
        _shaderMaterial.SetTexture("_SemanticTex", _semanticTexture);

        //pass in our transform
        _shaderMaterial.SetMatrix("_semanticTransform", _semanticManager.SemanticBufferProcessor.SamplerTransform);

        //blit everything with our shader
        Graphics.Blit(source, destination, _shaderMaterial);
    }
    
    private void Update()
    {
        // If the gameboard isn't running, do not process touches
        if (_gameboard == null)
        {
            DebugText.text = "Debug: GM == null";
            return;
        }
        var cameraTransform = _arCamera.transform;
        var playerPosition = cameraTransform.position;
        var playerForward = cameraTransform.forward;

        // The origin of the scan should be in front of the player
        var origin = playerPosition + Vector3.ProjectOnPlane(playerForward, Vector3.up).normalized;

        _gameboard.Scan(origin, 5);
        gameboardArea = _gameboard.Area;
        AreaText.text = "Gameboard A: " + gameboardArea.ToString();
        
        // if there is no touch, return
        if (PlatformAgnosticInput.touchCount <= 0) return;
        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            OnTouchScreen(touch);
            touchCount++;
        }
    }
    
    // Process the touch to see if it landed on a gameboard tile
    private void OnTouchScreen(Touch touch)
    {
        // do not process if the AR session is not active
        if (_arSession == null || _arCamera == null)
        {
            DebugText.text = "Debug: No AR Session";
            if (_arCamera == null)
            {
                DebugText.text = "Debug: No AR Camera";
            }
            return;
        }

        //as we are using meshing we can use a standard ray cast
        Ray ray = _arCamera.ScreenPointToRay(touch.position);
        
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            DebugText.text = "Debug: Hit" + touchCount.ToString();
            touchCount++;
            // if agent does not exist, spawn it!
            if (_agent == null)
            {
                SpawnAgent(hit.point);
            }
            else
            {
                // if agent does exist, set destination
                _agent.SetDestination(hit.point);
                DebugText.text = "Debug: SD Count" + touchCount.ToString();
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
        _agent = _agentGameObject.GetComponent<WorkshopGameboardAgent>(); // TODO: A rework on gameboard agent is needed
        _agent.State = WorkshopGameboardAgent.AgentNavigationState.Idle;
        DebugText.text = "Debug: Spawned stuff";
    }
}
