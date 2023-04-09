using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    public float Damage;
    public float EjectTime;
    public GameObject character;
    public GameObject OriginWeanpon;
    public float MaximumLifeTime;
    public float EnableTime;
    public float Speed;
    public float gravity;
    public float damageDistance;
    public CombatHandler combatHandler;
    public Collider Collider;
    public bool DespawnByBuilding;

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

        if (!Activated) {
            if (TimeElapsed >= EjectTime) {
                transform.position = OriginWeanpon.transform.position;
                transform.rotation = Quaternion.identity;

                var dir = character.transform.forward * Speed + character.transform.right * Speed * 0.2F;
                GetComponent<Rigidbody>().AddForce(dir, ForceMode.VelocityChange);
               
                Activated = true;
                TimeElapsed -= EjectTime;
            }
        }
        if (Activated)
        {
            if (TimeElapsed > EnableTime && !Collider.enabled) {
                Collider.enabled = true;
            }

            if(TimeElapsed > MaximumLifeTime)
            {
                Despawn();
            }
            var pos = this.transform.position;
            pos.y -= gravity * Time.deltaTime;
            transform.position = pos;
        }
    }

    void Despawn() {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (Activated)
        {

            var enemyList = GameObject.FindGameObjectsWithTag("Enemy");

            if (TimeElapsed < MaximumLifeTime && enemyList.Length != 0)
            {
                foreach (GameObject enemy in enemyList)
                {
                    var dir = enemy.transform.position - transform.position;
                    if (dir.magnitude < damageDistance)
                    {
                        combatHandler.DoDamage(enemy.GetComponent<CombatHandler>(), Damage);
                    }
                }
            }

            Despawn();
        }
    }
}
