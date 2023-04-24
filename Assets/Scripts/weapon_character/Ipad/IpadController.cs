using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IpadController : MonoBehaviour
{

    public float Damage;
    public float EjectTime;
    public float MaximumLifeTime;
    public float EnableTime;
    public GameObject OriginWeanpon;
    public float Speed;
    public CombatHandler combatHandler;
    public Collider Collider;
    public bool DespawnByBuilding;
    public Vector3 direction;

    private bool Activated;
    private float TimeElapsed;
    private string LastEnemy;

    // Start is called before the first frame update
    void Start()
    {
        Activated = false;
        TimeElapsed = 0.0F;
        LastEnemy = "";
        Collider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        TimeElapsed += Time.deltaTime;

        if (!Activated)
        {
            if (TimeElapsed >= EjectTime)
            {
                transform.position = OriginWeanpon.transform.position;
                transform.rotation = Quaternion.LookRotation(direction);
                Activated = true;
                TimeElapsed -= EjectTime;
            }
        }
        if (Activated)
        {
            if (TimeElapsed > EnableTime && !Collider.enabled)
            {
                Collider.enabled = true;
            }


            if (TimeElapsed < MaximumLifeTime)
            {
                transform.position += Speed * Time.deltaTime * direction;
            }
            else
            {
                Despawn();
            }
        }
    }

    void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (Activated)
        {

            Debug.Log("IPAD: collide" + other.name);
            if (other.CompareTag("Enemy"))
            {
                combatHandler.DoDamage(other.gameObject.GetComponent<CombatHandler>(), Damage);
                Debug.Log("IPAD: damage");
                Despawn();

                return;
            }
            if (TimeElapsed > 2.0F && !other.CompareTag("Player") && DespawnByBuilding)
            {
                Despawn();
            }
        }

    }
}