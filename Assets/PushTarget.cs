using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PushTarget : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float nearByRadius;
    public bool pushing;
    public List<GameObject> checkPoints;
    
    // Update is called once per frame
    void Update()
    {
        var nearbyColliders = Physics.OverlapSphere(transform.position, nearByRadius);
        if (nearbyColliders.Any(i => i.CompareTag("Enemy") || !i.CompareTag("Player")))
        {
            pushing = false;
        }
        else
        {
            pushing = true;
        }

        if (pushing)
        {
            MoveTowardsCheckPoints();
        }
    }

    public void MoveTowardsCheckPoints()
    {
        
    }

    public void SetOrientation(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}
