using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoliderAttack : MonoBehaviour
{
    public CombatHandler currentAttackingTarget;
    

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other) 
    {
        if (other.CompareTag("Enemy"))
        {
            currentAttackingTarget = other.transform.GetComponent<CombatHandler>();
        }
        // else{
        //     currentAttackingTarget = null;
        // }
    }
}
