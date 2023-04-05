using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttack : MonoBehaviour
{
    public float damage;
    public float velocity;
    public Transform target;
    public Transform enemy;
    private bool stopRangeAttack = false;
    private float stayTime = 7f;
    private Vector3 tempPosition;
    private bool enter = true;
    //public Rigidbody rb;
    // private bool playerInAttackRange;

    public CombatHandler CombatHandler;
    public GameFlowController GameFlowCtrl;


    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            if (stayTime == 7f)
            {
                tempPosition = target.position;
            }
            transform.rotation = Quaternion.Euler(new Vector3(0, enemy.rotation.y * 180, 0));
            transform.position = Vector3.MoveTowards(transform.position, tempPosition, velocity * Time.deltaTime);
            stayTime -= Time.deltaTime;

            if (!stopRangeAttack)
            {
                if(Vector3.Distance(transform.position, target.position) < 0.05f)
                {
                    if (target.CompareTag("Player"))
                    {
                        CombatHandler.DoDamage(GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform().GetComponent<CombatHandler>(), damage);
                        stopRangeAttack = true;
                        Destroy(gameObject);
                    }
                }
                else if (Vector3.Distance(transform.position, tempPosition) < 0.05f && enter)
                {
                    Destroy(gameObject);
                }
            // if (stayTime == 7f)
            // {
            //     tempPosition = target.position;
            // }
            // Debug.Log(enemy.rotation.z);
            // transform.rotation = Quaternion.Euler(new Vector3(-90f, 0, enemy.rotation.y * 180));
            // rb.AddForce(transform.forward * velocity, ForceMode.Impulse);
            // //transform.position = Vector3.MoveTowards(transform.position, tempPosition, velocity * Time.deltaTime);
            // stayTime -= Time.deltaTime;

            // if (!stopRangeAttack)
            // {
            //     if(Vector3.Distance(transform.position, target.position) < 0.05f)
            //     {
            //         if (target.CompareTag("Player"))
            //         {
            //             CombatHandler.DoDamage(GameFlowCtrl.getPlayerMovementCtrl().getCharacterTransform().GetComponent<CombatHandler>(), damage);
            //             stopRangeAttack = true;
            //             Destroy(gameObject);
            //         }
            //     }
            //     if (stayTime <= 0)
            //     {
            //         stopRangeAttack = true;
            //         Destroy(gameObject);
            //     }
            // }
            }
            else
            {
                Destroy(gameObject);
            }
            if (stayTime <= 0)
            {
                stopRangeAttack = true;
                Destroy(gameObject);
            }
            
        }
    }
}
