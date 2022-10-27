using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Niantic.ARDK.AR;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.Utilities;
using UnityEngine.UI;

public class WorkshopWayspotAnchorController : MonoBehaviour
{
    /*
     * Global variables
     * These sets of variables are used either to display content on the screen,
     * or for accessing ARDK features across methods.
     */

    public Camera _camera; // the ARDK's AR camera instead of the default Unity camera
    public GameObject _objectPrefab; // the prefab we will be spawning on screen
    public Text _statusLog; // updates the status log for Wayspot Anchors on screen
    public Text _localizationStatus; // updates the localization status message on screen

    private string LocalSaveKey = "my_wayspots"; // key used to store anchors locally
    private IARSession _arSession; // the AR session started by ARDK
    private WayspotAnchorService _wayspotAnchorService; // controls VPS features used

    /* 
     * Unity Event Lifecycle Functions
     * Learn more: https://docs.unity3d.com/Manual/ExecutionOrder.html
     */

    // When our app is enabled, register the OnSessionInitiliazed method to
    // ARDK's SessionInialized event handler
    private void OnEnable()
    {
        ARSessionFactory.SessionInitialized += OnSessionInitialized;
    }

    // Listen for touch events only if the app has localized to a VPS Wayspot
    void Update()
    {
        if (_wayspotAnchorService == null ||
            _wayspotAnchorService.LocalizationState != LocalizationState.Localized) return;
        
        if (PlatformAgnosticInput.touchCount <= 0) return;
        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.IsTouchOverUIObject()) return;
        
        if (touch.phase == TouchPhase.Began)
        {
            OnTouchScreen(touch);
        }
    }

    // Deregister the SessionInitiliazed method to ensure AR Session is terminated between sessions
    private void OnDisable()
    {
        ARSessionFactory.SessionInitialized -= OnSessionInitialized;
    }

    /*
     * Wayspot Anchor methods
     * The following methods allow us to place anchors with our game objects attached to them
     */

    // Place the Wayspot Anchor into the scene based on touch
    private void PlaceAnchor(Matrix4x4 poseData)
    {
        var anchors = _wayspotAnchorService.CreateWayspotAnchors(poseData);
        if (anchors.Length == 0) return;
        // get data for game object
        var position = poseData.ToPosition();
        var rotation = poseData.ToRotation();
        CreateWayspotAnchorGameObject(anchors[0], position, rotation);
        _statusLog.text = "Anchor placed.";
    }

    // Create and attach the game object prefab to the wayspot anchor
    private void CreateWayspotAnchorGameObject(IWayspotAnchor anchor, Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(_objectPrefab, position, rotation);
        
        var tracker = go.GetComponent<WayspotAnchorTracker>();
        if (tracker == null)
        {
            tracker = go.AddComponent<WayspotAnchorTracker>();
        }
        
        tracker.AttachAnchor(anchor);
    }

    /* 
     * ARDK Event Handlers
     * 
     */

    // Initialize the AR Session
    void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
        _statusLog.text = "Session initialized";
        if (_arSession != null) return;
        _arSession = args.Session;
        _arSession.Ran += OnSessionRan;
    }

    // Once the session has run, we will need to create the wayspot anchor service
    void OnSessionRan(ARSessionRanArgs args)
    {
        _arSession.Ran -= OnSessionRan;
        var wayspotAnchorsConfiguration = WayspotAnchorsConfigurationFactory.Create();
        var locationService = LocationServiceFactory.Create(_arSession.RuntimeEnvironment);
        locationService.Start();
        _wayspotAnchorService = new WayspotAnchorService(_arSession, locationService, wayspotAnchorsConfiguration);
        _wayspotAnchorService.LocalizationStateUpdated += OnLocalizationStateUpdated;
        _statusLog.text = "Session running";
    }

    // Process the touch to see if it falls on a horizontal plane
    private void OnTouchScreen(Touch touch)
    {
        var currentFrame = _arSession.CurrentFrame;
        if (currentFrame == null) return;
        var hitTestResults = currentFrame.HitTest(_camera.pixelWidth, _camera.pixelHeight, touch.position,
            ARHitTestResultType.EstimatedHorizontalPlane);
        if (hitTestResults.Count <= 0) return;
        var position = hitTestResults[0].WorldTransform.ToPosition();
        var rotation = Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360)));
        Matrix4x4 poseData = Matrix4x4.TRS(position, rotation, _objectPrefab.transform.localScale);
        PlaceAnchor(poseData);
    }

    // We only want to interact with anchors when we are fully localized with VPS
    // For debugging, display the state as Text on screen
    private void OnLocalizationStateUpdated(LocalizationStateUpdatedArgs args)
    {
        _localizationStatus.text = args.State.ToString();
        if (args.State == LocalizationState.Failed)
        {
            _statusLog.text = args.FailureReason.ToString();
        }
    }

    /*
     * Button Handlers
     * The UI buttons on screen are hooked up to the following button handlers.
     */

    // Gather all of the wayspot anchors and save them on device
    public void SaveLocalWayspotAnchors()
    {
        IWayspotAnchor[] wayspotAnchors = _wayspotAnchorService.GetAllWayspotAnchors();
        MyStoredAnchorsData storedAnchorData = new MyStoredAnchorsData();
        storedAnchorData.Payloads = wayspotAnchors.Select(a => a.Payload.Serialize()).ToArray(); // lookup => notation
        string jsonData = JsonUtility.ToJson(storedAnchorData);
        PlayerPrefs.SetString(LocalSaveKey, jsonData);
        _statusLog.text = $"Saved {wayspotAnchors.Count()} anchors";
    }

    // Using the player key, check if there are stored wayspot anchors on device. Restore them if true.
    public void LoadLocalWayspotAnchors()
    {
        if (PlayerPrefs.HasKey(LocalSaveKey))
        {
            string json = PlayerPrefs.GetString(LocalSaveKey);
            MyStoredAnchorsData storedAnchorsData = JsonUtility.FromJson<MyStoredAnchorsData>(json);
        
            foreach (var wayspotAnchorPayload in storedAnchorsData.Payloads)
            {
                var payload = WayspotAnchorPayload.Deserialize(wayspotAnchorPayload);
        
                var anchors = _wayspotAnchorService.RestoreWayspotAnchors(payload);
                if (anchors.Length == 0)
                    return;
        
                var position = anchors[0].LastKnownPosition;
                var rotation = anchors[0].LastKnownRotation;
                CreateWayspotAnchorGameObject(anchors[0], position, rotation);
            }
        
            _statusLog.text = $"Restored {storedAnchorsData.Payloads.Count()} anchors";
        }
        else
        {
            _statusLog.text = "No key found";
        }
    }

    /*
     * Serializabe Data Class 
     * Used for storing Wayspot Anchor Payloads
     */

    [Serializable]
    private class MyStoredAnchorsData
    {
        public string[] Payloads = Array.Empty<string>();
    }
}