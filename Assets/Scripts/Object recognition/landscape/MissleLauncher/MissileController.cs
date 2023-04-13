using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileController : MonoBehaviour
{
    public Transform target;
    public float damage;
    public CombatHandler combatHandler;
    public float speed;
    public float damageRange;
    public float MaximumLifeTime;
    public Collider Collider;
    public float EnableTime;

    private bool Activated;
    private float TimeElapsed;

    // Start is called before the first frame update
    void Start()
    {
        Activated = false;
        TimeElapsed = 0.0F;
        Collider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        TimeElapsed += Time.deltaTime;
        if (TimeElapsed > EnableTime && !Collider.enabled)
        {
            Collider.enabled = true;
        }

        if (target != null)
        {
            var dir = target.transform.position - transform.position;
            transform.position += speed * Time.deltaTime * dir.normalized;
            transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0,0);
        }
        else
        {
            Despawn();
        }
        
        if(TimeElapsed > MaximumLifeTime) Despawn();
    }


    void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        if (TimeElapsed < MaximumLifeTime && TimeElapsed > EnableTime && enemyList.Length != 0)
        {
            foreach (GameObject enemy in enemyList)
            {
                var dir = enemy.transform.position - transform.position;
                if (dir.magnitude < damageRange)
                {
                    combatHandler.DoDamage(enemy.GetComponent<CombatHandler>(), damage);
                }
            }
            Despawn();
        }
        
    }
}
