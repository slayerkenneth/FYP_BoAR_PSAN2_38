using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookController : MonoBehaviour
{
    public float Damage;
    // public float EnableTime;
    // public GameObject OriginWeanpon;
    // public float Speed;
    // public float Rotation;      //regular rotation around y axix. Num of rotation per second
    public CombatHandler combatHandler;
    // public Collider Collider;
    // public bool DespawnByBuilding;

    // private int HitCount;
    // private bool Activated;
    // private float TimeElapsed;
    // private string LastEnemy;

    private GameObject collideEnemy;

    // Start is called before the first frame update
    void Start()
    {
        // HitCount = 0;
        // Activated = false;
        // TimeElapsed = 0.0F;
        // LastEnemy = "";
    }

    // Update is called once per frame
    void Update()
    {
        // if (collideEnemy)
        // {

        // }

        // TimeElapsed += Time.deltaTime;

        // if (!Activated) {
        //     if (TimeElapsed >= EjectTime) {
        //         transform.position = OriginWeanpon.transform.position;
        //         transform.rotation = Quaternion.Euler(0.0F, OriginWeanpon.transform.rotation.eulerAngles.y, -90.0F) ;
        //         Activated = true;
        //         TimeElapsed -= EjectTime;
        //     }
        // }
        // if (Activated)
        // {
        //     if (TimeElapsed > EnableTime && !Collider.enabled) {
        //         Collider.enabled = true;
        //     }

        //     var enemyList = GameObject.FindGameObjectsWithTag("Enemy");

        //     if (TimeElapsed < MaximumLifeTime && enemyList.Length != 0)
        //     {
        //         var minDir = Vector3.zero;
        //         var minDistance = float.PositiveInfinity;
        //         foreach (GameObject enemy in  enemyList) {
        //             var dir = enemy.transform.position - transform.position;
        //             if (dir.magnitude < minDistance && LastEnemy != enemy.name)
        //             {
        //                 minDistance = dir.magnitude;
        //                 minDir = dir.normalized;
        //             }
        //         }
        //         transform.position += Speed * Time.deltaTime * minDir;
        //         transform.rotation *=  Quaternion.Euler(Rotation * Time.deltaTime * 360, 0 ,0);
        //     }
        //     else {
        //         Despawn();
        //     }
        // }
    }

    void Despawn() {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("Enemy"))
        {
            collideEnemy = other.gameObject;
        }
        if (collideEnemy)
        {
            combatHandler.DoDamage(collideEnemy.GetComponent<CombatHandler>(), Damage);
        }




        // if (Activated)
        // {
        //     if (other.CompareTag("Enemy"))
        //     {

        //         Debug.Log("backpack: "+HitCount);
        //         HitCount++;
        //         if (HitCount == MaxHitCount)
        //         {
        //             Despawn();
        //             return;
        //         }
        //         LastEnemy = other.gameObject.name;
        //         combatHandler.DoDamage(other.gameObject.GetComponent<CombatHandler>(), Damage);
        //     }
        //     else if(TimeElapsed > 2.0F && !other.CompareTag("Player") && DespawnByBuilding)
        //     {
        //         Despawn();
        //     }
        // }
    }
}
