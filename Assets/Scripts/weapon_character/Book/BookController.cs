using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookController : MonoBehaviour
{
    public float Damage;
    public CombatHandler combatHandler;
    public bool isAttack;

    private GameObject collideEnemy;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            collideEnemy = other.gameObject;
        }
        if (collideEnemy && isAttack)
        {
            combatHandler.DoDamage(collideEnemy.GetComponent<CombatHandler>(), Damage);
        }
    }
}
