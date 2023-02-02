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
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.External;
using Assets.Scripts;
using System.IO;

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

    
    [EnumFlagAttribute]
    public ARHitTestResultType HitTestType = ARHitTestResultType.ExistingPlane;

    /// A list of placed game objects to be destroyed in the OnDestroy method.
    private List<GameObject> _placedObjects = new List<GameObject>();

    

    /*
     * Global variable for handling semantic texture and shaders
     */
    [SerializeField] [Tooltip("Semantic texture and custom shader rendering")]
    public Material _shaderMaterial;
    Texture2D _semanticTexture;
    public ARSemanticSegmentationManager _semanticManager;


    [SerializeField] public GameFlowController GameFlowController;
    [SerializeField] public Text DebugText;
    [SerializeField] public Text AreaText;

    [Header("object reconition")]
    public GameObject PlacementObjectPf;
    public GameObject boxContainer;
    public GameObject boxPrefab;
    public Yolov5Detector yolov5Detector;

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
        if (DebugText) DebugText.text = "Debug: AR Session Disabled";
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
        GameFlowController._activeGameboard = _gameboard;
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

        if (gameboardArea < 100)
        {
            _gameboard.Scan(origin, 5);
        }
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
        var tempPos = new Vector3();
        if (Physics.Raycast(ray, out hit))
        {
            DebugText.text = "Debug: Hit" + touchCount.ToString();
            touchCount++;
            _gameboard.FindNearestFreePosition(hit.point, out tempPos);
            // if agent does not exist, spawn it!
            
            if (_agentGameObject == null)
            {
                var node = new GridNode();
                if (GameFlowController.SpatialTree.GetElement(Utils.PositionToTile(tempPos, _gameboard.Settings.TileSize), out node))
                    SpawnAgent(new Vector3(node.Coordinates.x, 0.5f, node.Coordinates.y));
                GameFlowController.battleSceneState = GameFlowController.PVEBattleSceneState.SpawningPlayer;
            }
            else
            {
                // Test
                var node = new GridNode();
                if (GameFlowController.SpatialTree.GetElement(Utils.PositionToTile(tempPos, _gameboard.Settings.TileSize), out node))
                    DebugText.text = DebugText.text + " Raycast hit nearest board pos " + tempPos.ToString() + " GN node Coord" + node.Coordinates;
            }
        }
        
        // RenderTexture rt = new RenderTexture(_arCamera.pixelWidth, _arCamera.pixelHeight, 24);
        // _arCamera.targetTexture = rt;
        // Texture2D screenShot = new Texture2D(_arCamera.pixelWidth, _arCamera.pixelHeight, TextureFormat.RGB24, false);
        // _arCamera.Render();
        // RenderTexture.active = rt;
        // screenShot.ReadPixels(new Rect(0, 0, _arCamera.pixelWidth, _arCamera.pixelHeight), 0, 0);
        // _arCamera.targetTexture = null;
        // RenderTexture.active = null; // JC: added to avoid errors
        // Destroy(rt);
        //
        //
        //
        //
        // //byte[] bytes = screenShot.EncodeToPNG();
        // //var dirPath = Application.dataPath + "/../SaveImages/";
        // //if (!Directory.Exists(dirPath))
        // //{
        // //    Directory.CreateDirectory(dirPath);
        // //}
        // //File.WriteAllBytes(dirPath + "Image1" + ".png", bytes);
        //
        // Rect texR = new Rect(0, 0, 416, 416);
        // _gpu_scale(screenShot, 416, 416, FilterMode.Trilinear);
        //
        // //Get rendered data back to a new texture
        // Texture2D result = new Texture2D(416, 416, TextureFormat.RGB24, true);
        // result.Reinitialize(416, 416);
        // result.ReadPixels(texR, 0, 0, true);
        //
        // var dirPath = Application.dataPath + "/../SaveImages/";
        // byte[] bytes = result.EncodeToPNG();
        // if (!Directory.Exists(dirPath))
        // {
        //     Directory.CreateDirectory(dirPath);
        // }
        // File.WriteAllBytes(dirPath + "Image2" + ".png", bytes);
        //
        // DebugText.text = "start model inference";
        // StartCoroutine(yolov5Detector.Detect(result.GetPixels32(), 416, boxes =>
        // {
        //     Resources.UnloadUnusedAssets();
        //
        //     foreach (Transform child in boxContainer.transform)
        //     {
        //         Destroy(child.gameObject);
        //     }
        //     Debug.Log("3");
        //     DebugText.text = "finish model inference: " + boxes.Count;
        //     for (int i = 0; i < boxes.Count; i++)
        //     {
        //         GameObject newBox = Instantiate(boxPrefab);
        //         newBox.name = boxes[i].Label + " " + boxes[i].Confidence;
        //         newBox.GetComponent<Image>().color = new Color(0.3843137f, 0, 0.9333333f, 1.0f);
        //         newBox.transform.parent = boxContainer.transform;
        //
        //         var pred_x = boxes[i].Rect.x;
        //         var pred_y = boxes[i].Rect.y;
        //
        //
        //
        //         var org_h = boxContainer.GetComponent<RectTransform>().rect.height;
        //         var org_w = boxContainer.GetComponent<RectTransform>().rect.width;
        //
        //         Debug.Log(boxes[i].Label + "  x: " + boxes[i].Rect.x + "y: " + boxes[i].Rect.y + "w: " + boxes[i].Rect.width + "h: " + boxes[i].Rect.height
        //             + new Vector3(boxes[i].Rect.x * org_w / 416, org_h - (boxes[i].Rect.y * org_h / 416) - 0.5f * org_h)
        //             + new Vector2(boxes[i].Rect.width * org_w / 100 / 416, boxes[i].Rect.height * org_h / 100 / 416));
        //
        //         newBox.transform.localPosition = new Vector3(boxes[i].Rect.x * org_w / 416, org_h - (boxes[i].Rect.y * org_h / 416) - 0.5f * org_h);
        //         newBox.transform.localScale = new Vector2(boxes[i].Rect.width * org_w / 100 / 416, boxes[i].Rect.height * org_h / 100 / 416);
        //
        //         SpawnLandscape(new Vector2(boxes[i].Rect.x * Screen.width / 416, Screen.height - boxes[i].Rect.y * Screen.height / 416));
        //     }
        //
        // }));
    }

    private void SpawnAgent(Vector3 hitPoint)
    {
        if (_agentGameObject != null || GameFlowController.PlayerSpawnActive == false) return;
        // Instantiate the agent with the predefined prefab
        _agentGameObject = Instantiate(_agentPrefab);
        
        // Set the position of the agent as the raycast hit result
        _agentGameObject.transform.position = hitPoint;
        
        // Have the prefab face towards camera
        var rotation = Vector3.ProjectOnPlane(_arCamera.transform.forward, Vector3.up).normalized;
        _agentGameObject.transform.rotation = Quaternion.LookRotation(-rotation);
        
        // Set agent's state for navigation
        // _agent = _agentGameObject.GetComponent<WorkshopGameboardAgent>(); // TODO: A rework on gameboard agent is needed
        // _agent.State = WorkshopGameboardAgent.AgentNavigationState.Idle;
        var CharMoveCtrl = _agentGameObject.GetComponent<CharacterMovementController>();
        CharMoveCtrl.ARController = this;
        CharMoveCtrl.GameFlowController = GameFlowController;
        DebugText.text = "Debug: Spawned stuff";
    }

    public IGameboard GetActiveGameboard()
    {
        return _gameboard;
    }

    public CharacterMovementController GetActivePlayerMovementCtrl()
    {
        return _agentGameObject.GetComponent<CharacterMovementController>();
    }

    public void SpawnLandscape(Vector2 position) {
        var ray = _arCamera.ScreenPointToRay(position); ;

        // Intersect the Gameboard with the ray
        if (_gameboard.RayCast(ray, out Vector3 hitPoint))
        {
            // Check whether the object can be fit in the resulting position
            if (_gameboard.CheckFit(center: hitPoint, 0.1f))
            {
                var landscape = Instantiate(PlacementObjectPf, hitPoint, Quaternion.identity);
                landscape.transform.localScale = new Vector3(3, 3, 3);
                _placedObjects.Add(landscape);
            }
        }
    }

    void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
    {
        //We need the source texture in VRAM because we render with it
        src.filterMode = fmode;
        src.Apply(true);

        //Using RTT for best quality and performance. Thanks, Unity 5
        RenderTexture rtt = new RenderTexture(width, height, 32);

        //Set the RTT in order to render to it
        Graphics.SetRenderTarget(rtt);

        //Setup 2D matrix in range 0..1, so nobody needs to care about sized
        GL.LoadPixelMatrix(0, 1, 1, 0);

        //Then clear & draw the texture to fill the entire RTT.
        GL.Clear(true, true, new Color(0, 0, 0, 0));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
    }

    public IGameboard GetGameboard() {
        return _gameboard;
    }
}
