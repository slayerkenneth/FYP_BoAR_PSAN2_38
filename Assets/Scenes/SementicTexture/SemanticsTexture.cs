using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Awareness.Semantics;
using UnityEngine.UI;


public class SemanticsTexture : MonoBehaviour
{
    public Material _shaderMaterial;
    Texture2D _semanticTexture;
    public ARSemanticSegmentationManager _semanticManager;
    public GameObject refGO;

    //Not good performance type of overlay:
    // public RawImage _overlayImage;

    void Start()
    {
        //add a callback for catching the updated semantic buffer
        _semanticManager.SemanticBufferUpdated += OnSemanticsBufferUpdated;
        //on initialisation
        ARSessionFactory.SessionInitialized += OnSessionInitialized;
        
        Matrix4x4 WorldToUVMatrix = Matrix4x4.TRS(refGO.transform.position, refGO.transform.rotation, Vector3.one);
        _shaderMaterial.SetMatrix("_WorldToUVMatrix", WorldToUVMatrix);
    }

    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
        Resolution resolution = new Resolution();
        resolution.width = Screen.width;
        resolution.height = Screen.height;
        ARSessionFactory.SessionInitialized -= OnSessionInitialized;
    }

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
}
