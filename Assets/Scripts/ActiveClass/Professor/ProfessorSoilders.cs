using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfessorSoilders : MonoBehaviour
{
    public float Damage;
    public float speed;
    public Animator Animator;
    public SoliderAttack sa;
    public CombatHandler combatHandler;
    public GameObject parent;

    private float temp_speed;
    private Vector3 relative_position;
    private bool isAttack;
    private CombatHandler target;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        combatHandler.SetCentralCombatHandler(parent.GetComponent<CombatHandler>().GetCentralCombatHandler());
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        //Find the cloest enemy for target first
        GameObject closest_enemy = FindClosestEnemy(enemyList);
        if (closest_enemy)
        {
            //if fit the attack range of solider
            if (Vector3.Distance(transform.localPosition, closest_enemy.transform.localPosition) < 0.1f)
            {
                if (!isAttack)
                {
                    //turn the angle to fit the attack animation
                    var temp = transform.localEulerAngles.y - 90;
                    if (temp < -180)
                        temp = 360 + temp;
                    transform.localRotation = Quaternion.Euler(0, temp, 0);
                }
                isAttack = true;
                temp_speed = 0;

                //Attack enemy
                Attack();
            }
            else
            {
                //else set the speed and move towards to enemy
                transform.LookAt(closest_enemy.transform);
                isAttack = false;
                temp_speed = speed;
                Animator.Play("run");
            }

            transform.position = Vector3.MoveTowards(transform.localPosition, closest_enemy.transform.localPosition, 
                temp_speed * Time.deltaTime);
        }
        
    }

    //Animation Event
    public void AttackEnd()
    {
        target = sa.GetComponent<SoliderAttack>().currentAttackingTarget;
        if (target)
        {
            // if collide with enemy, do damage
            if (target.CompareTag("Enemy"))
            {
                combatHandler.DoDamage(target, Damage);
            } 
        }
        
    }

    public void Attack()
    {
        Animator.Play("Attack");
    }

    public GameObject FindClosestEnemy(GameObject[] enemyList)
    {
        float distance = float.PositiveInfinity;
        GameObject result_enemy = null;
        // find the smallest distance from each enemy
        foreach (GameObject enemy in enemyList) 
        {
            float temp_distance = Vector3.Distance(transform.localPosition, enemy.transform.localPosition);
            if (temp_distance < distance)
            {
                distance = temp_distance;
                result_enemy = enemy;
            }
        }
        return result_enemy;
    }

    //get the solider state whether they are overlap?
    private void OnTriggerStay(Collider other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log(other.gameObject.transform.name);
            DealWithOverlap(other);
        }
    }


    public void DealWithOverlap(Collider hitCollider)
    {   
        Vector3 newPosition;
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest_enemy = FindClosestEnemy(enemyList);
        //Find which one (overlap's solider and itself) is closer to the enemy
        float ColliderToEnemy = Vector3.Distance(hitCollider.gameObject.transform.position, closest_enemy.transform.position);
        float ThisToEnemy = Vector3.Distance(transform.position, closest_enemy.transform.position);
        //If this solider is closer, the overlap's solider will have a new path for preventing overlap
        if (ThisToEnemy > ColliderToEnemy)
        {
            newPosition = CalculatingEnemyNewPath(hitCollider.gameObject);
            transform.localPosition = Vector3.MoveTowards(transform.position, newPosition, 0.1f * Time.deltaTime);
        }
    }

    public Vector3 CalculatingEnemyNewPath(GameObject col)
    {
        Vector3 newPosition;
        float theta;
        //If the euler angle of this transform is larger than the overlap's solider euler angle, it will move anticlockwise
        //until there are no overlap, else it will move clockwise
        if (transform.localEulerAngles.y > col.transform.localEulerAngles.y)
        {
            theta = col.transform.localEulerAngles.y + 120;
            if (theta > 360)
            {
                theta = theta - 360;
            }
        }
        else
        {
            theta = col.transform.localEulerAngles.y - 120;
            if (theta < 0)
            {
                theta = 360 + theta;
            }
        }
        theta = theta * Mathf.Deg2Rad;
        
        newPosition = new Vector3(transform.position.x + Mathf.Sin(theta) * 0.5f, transform.position.y, 
            transform.position.z + Mathf.Cos(theta) * 0.5f);
        return newPosition;
    }
}
