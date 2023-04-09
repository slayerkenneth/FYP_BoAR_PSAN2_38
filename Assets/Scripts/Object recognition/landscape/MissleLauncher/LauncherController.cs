using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherController : LandscapeController
{
    public GameObject missle;
    public int MissleNum;
    public float damagePerMissile;
    public float damageRange;
    public CombatHandler combatHandler;

    protected override void OnTriggerEnter(Collider other)
    {
        if (Active && other.CompareTag("Player")) {
            Active = false;
            remainCD = CD;
            ActiveSign.Stop();

            var enemyList = GameObject.FindGameObjectsWithTag("Enemy");

            for (int i = 0; i < MissleNum; i++) {
                var missleObject = Instantiate(missle);
                missleObject.transform.position = transform.position + new Vector3(0, 0.5F, 0);
                var controller = missleObject.GetComponent<MissileController>();
                controller.damage = damagePerMissile;
                controller.damageRange = damageRange;
                controller.target = enemyList[Random.Range(0, enemyList.Length)].transform;
                controller.combatHandler = combatHandler;
            }
            
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        Active = true;
        remainCD = 0;
        ActiveSign.Play();
        combatHandler.SetCentralCombatHandler(GameObject.FindObjectsOfType<CentralBattleController>()[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active)
        {
            remainCD -= Time.deltaTime;
            if (remainCD < 0)
            {
                Active = true;
                remainCD = 0;
                ActiveSign.Play();
            }
        }
    }
}
