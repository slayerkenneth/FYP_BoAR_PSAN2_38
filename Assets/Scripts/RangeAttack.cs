using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeAttack : MonoBehaviour
{
    public float DamageAmount;
    public float velocity;
    public Transform enemy;
    public Transform target;
    private bool stopRangeAttack = false;
    private float stayTime = 7f;
    private Vector3 tempPosition;

    public CombatHandler CombatHandler;
    public GameFlowController GameFlowCtrl;

    [Header("Damage Parameters")]
    [SerializeField] private bool ValidHit = false;
    [SerializeField] private CombatHandler currentAttackingTarget;


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
                if (Vector3.Distance(transform.position, target.position) < 0.05f)
                {
                    Debug.Log("Halo");
                    CombatHandler.DoDamage(target.GetComponent<CombatHandler>(), DamageAmount);
                    stopRangeAttack = true;
                    Destroy(gameObject);
                }
                else if (Vector3.Distance(transform.position, tempPosition) < 0.05f)
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
            // Debug.Log(transform.name + " " + stayTime);
            if (stayTime <= 0)
            {
                stopRangeAttack = true;
                Destroy(gameObject);
            }
            
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
