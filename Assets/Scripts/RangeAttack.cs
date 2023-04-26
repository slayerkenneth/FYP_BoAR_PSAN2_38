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
    private float stayTime = 5f;
    private Vector3 player_tempPosition;
    private Vector3 enemy_tempPosition;
    private bool keepMoving = false;
    private float changeInX, changeInY;

    public CombatHandler CombatHandler;
    public GameFlowController GameFlowCtrl;

    [Header("Damage Parameters")]
    [SerializeField] private bool ValidHit = false;
    [SerializeField] private CombatHandler currentAttackingTarget;


    // Update is called once per frame
    void Update()
    {
        if (target && enemy)
        {
            if (stayTime == 5f)
            {
                player_tempPosition = target.localPosition;
                enemy_tempPosition = enemy.localPosition;
                transform.rotation = Quaternion.Euler(new Vector3(0, enemy.localEulerAngles.y, 0));
            }
            
            stayTime -= Time.deltaTime;

            if (!stopRangeAttack)
            { 
                //if collide with target, it will do damage  
                if (Vector3.Distance(transform.position, target.position) < 0.05f)
                {
                    CombatHandler.DoDamage(target.GetComponent<CombatHandler>(), DamageAmount);
                    stopRangeAttack = true;
                    Destroy(gameObject);
                }

                //if it cannot collide with target, move along the direction of target until it disappear
                else 
                {
                    if (Vector3.Distance(transform.position, player_tempPosition) < 0.05f || keepMoving)
                    {
                        var dir = Mathf.Atan2(player_tempPosition.x - enemy_tempPosition.x, player_tempPosition.z - enemy_tempPosition.z) * Mathf.Rad2Deg;
                        if (dir > 0)
                            changeInX = Mathf.Abs(Mathf.Sin(dir * Mathf.Deg2Rad));
                        else
                            changeInX = -Mathf.Abs(Mathf.Sin(dir * Mathf.Deg2Rad));

                        if (dir > -90 && dir < 90)
                            changeInY = Mathf.Abs(Mathf.Cos(dir * Mathf.Deg2Rad));
                        else
                            changeInY = -Mathf.Abs(Mathf.Cos(dir * Mathf.Deg2Rad));
                            
                        player_tempPosition = new Vector3(transform.position.x + changeInX, transform.position.y, 
                            transform.position.z + changeInY);
                        transform.position = Vector3.MoveTowards(transform.position, player_tempPosition, velocity * Time.deltaTime);
                        keepMoving = true;
                    }
                    else
                        transform.position = Vector3.MoveTowards(transform.position, player_tempPosition, velocity * Time.deltaTime);
                }
            }
            else
            {
                Destroy(gameObject);
            }

            //if the lifetime is zero, it will be disappear
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
