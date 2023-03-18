using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    public CombatHandler combatHandler;
    public float damagePerSec;


    private List<CombatHandler> InContact = new List<CombatHandler>();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float damage = Time.deltaTime * damagePerSec;
        Debug.Log("laser: " + InContact.Count);
        foreach (CombatHandler enemy in InContact) {
            combatHandler.DoDamage(enemy, damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("laser: collid");
        if (other.CompareTag("Enemy"))
        {
            var combatHandler = other.GetComponent<CombatHandler>();
            if (!InContact.Contains(combatHandler)) InContact.Add(combatHandler);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var combatHandler = other.GetComponent<CombatHandler>();
            if (InContact.Contains(combatHandler)) InContact.Remove(combatHandler);
        }
    }
}
