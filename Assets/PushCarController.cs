using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushCarController : MonoBehaviour
{
    public GameObject CarPrefab;
    // Start is called before the first frame update

    public void SpawnCar()
    {
        Instantiate(CarPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), transform);
    }
}
