using UnityEngine;

public class RotateCoin : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(Time.deltaTime * 30, 0, 0);
    }
}
