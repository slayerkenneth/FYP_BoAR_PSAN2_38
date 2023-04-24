using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTextureReplacer : MonoBehaviour
{
    [SerializeField] public MeshRenderer GroundMeshRenderer;
    [SerializeField] public List<Material> groundMatList;

    public MeshRenderer GroundMeshRenderer1
    {
        get => GroundMeshRenderer;
        set => GroundMeshRenderer = value;
    }

    public void SetGroundMeshMat(Material newMat)
    {
        GroundMeshRenderer.material = newMat;
    }
}
