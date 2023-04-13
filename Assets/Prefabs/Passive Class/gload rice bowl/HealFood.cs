using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealFood : MonoBehaviour
{
    public float healAmount;

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
        if (other.CompareTag("Player"))
        {
            var combatHandler = other.transform.GetComponent<CombatHandler>();
            Debug.Log("heal");
            combatHandler.ReceiveHeal(healAmount);
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //ValidHit = false;
        // shd be after animation end and depends on collider of the weapon
    }
}
