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
    public List<Collider> nearbyColliders;
    public Vector3 targetPosition;

    // Update is called once per frame
    void Update()
    {
        nearbyColliders = Physics.OverlapSphere(transform.position, nearByRadius).ToList();
        PushCarCheckPoint checkPoint = new PushCarCheckPoint();
        if (nearbyColliders.Exists(c => c.TryGetComponent<PushCarCheckPoint>(out checkPoint)))
        {
            if (checkPoint.nextCheckPoint && !checkPoint.CarPassed) SetOrientation(checkPoint.nextCheckPoint.transform);
            checkPoint.CarPassed = true;
        }
        
        if (nearbyColliders.Exists( c => c.CompareTag("Enemy")) || !nearbyColliders.Exists( c => c.CompareTag("Player")))
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
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    public void SetOrientation(Transform target)
    {
        targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
        Vector3 direction = (targetPosition - transform.position).normalized;                        
        Quaternion lookRotation = Quaternion.LookRotation(direction);                                 
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 1);
    }
}
