using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PushTarget : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float nearByRadiusForCheckpoints;
    public float nearByRadiusForBattleDetect;
    public bool pushing;
    public List<GameObject> checkPoints;
    public List<Collider> nearbyCollidersRange1;
    public List<Collider> nearbyCollidersRange2;
    public Vector3 targetPosition;

    // Update is called once per frame
    void Update()
    {
        nearbyCollidersRange1 = Physics.OverlapSphere(transform.position, nearByRadiusForCheckpoints).ToList();
        PushCarCheckPoint checkPoint = new PushCarCheckPoint();
        if (nearbyCollidersRange1.Exists(c => c.TryGetComponent<PushCarCheckPoint>(out checkPoint)))
        {
            if (checkPoint.nextCheckPoint && !checkPoint.CarPassed) SetOrientation(checkPoint.nextCheckPoint.transform);
            checkPoint.CarPassed = true;
        }
        
        nearbyCollidersRange2 = Physics.OverlapSphere(transform.position, nearByRadiusForBattleDetect).ToList();
        if (nearbyCollidersRange2.Exists( c => c.CompareTag("Enemy")) || !nearbyCollidersRange2.Exists( c => c.CompareTag("Player")))
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
