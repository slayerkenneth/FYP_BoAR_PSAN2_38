using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GroundTextureReplacer : MonoBehaviour
{
    [SerializeField] public MeshRenderer GroundMeshRenderer;
    [SerializeField] public List<Material> groundMatList;

    public MeshRenderer GroundMeshRenderer1
    {
        get => GroundMeshRenderer;
        set => GroundMeshRenderer = value;
    }

    private void Start()
    {
        GroundMeshRenderer.material = groundMatList[Random.Range(0, groundMatList.Count)];
    }

    public void SetGroundMeshMat(Material newMat)
    {
        GroundMeshRenderer.material = newMat;
    }
}
