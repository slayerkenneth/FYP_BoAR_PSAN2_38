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
                //transform.rotation = OriginWeanpon.transform.rotation;
                transform.rotation = Quaternion.identity;
                //GetComponent<Rigidbody>().velocity = character.transform.forward * Speed;
                Debug.Log("velocity: " + character.transform.forward * Speed);
                //Debug.Log("velocity: " + OriginWeanpon.transform.rotation * Vector3.forward * Speed);
                //Debug.Log("velocity: " + OriginWeanpon.transform.rotation * character.transform.forward * Speed);
                //GetComponent<Rigidbody>().velocity = Quaternion.Euler(0, OriginWeanpon.transform.rotation.y, 0) * Vector3.forward * Speed;

                var dir = character.transform.forward * Speed + character.transform.right * Speed * 0.2F;
                GetComponent<Rigidbody>().AddForce(dir, ForceMode.VelocityChange);
                //GetComponent<Rigidbody>().AddForce(character.transform.up * Speed, ForceMode.VelocityChange);
               
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
            //if (other.CompareTag("Enemy"))
            //{
            //
            //    Debug.Log("backpack: "+HitCount);
            //    HitCount++;
            //    if (HitCount == MaxHitCount)
            //    {
            //        Despawn();
            //        return;
            //    }
            //    LastEnemy = other.gameObject.name;
            //    combatHandler.DoDamage(other.gameObject.GetComponent<CombatHandler>(), Damage);
            //}
            //else if(TimeElapsed > 2.0F && !other.CompareTag("Player") && DespawnByBuilding)
            //{
            //    Despawn();
            //}

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
