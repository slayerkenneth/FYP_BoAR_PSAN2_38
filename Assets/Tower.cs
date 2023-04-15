using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public List<Transform> TowerPointsTransforms;

    public List<Transform> GetTowerPoints()
    {
        return TowerPointsTransforms;
    }
}
