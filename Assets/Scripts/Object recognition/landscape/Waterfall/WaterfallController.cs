using CodeMonkey.HealthSystemCM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterfallController : LandscapeController
{
    public float healPerSec;
    public float MaxHealTime;
    private CombatHandler target;
    private float TimeEclipse;
    private bool isHealing;

    protected override void OnTriggerEnter(Collider other)
    {
        if (Active && other.CompareTag("Player"))
        {
            Active = false;
            remainCD = CD;
            ActiveSign.Stop();
            OnCollectStart();

            TimeEclipse = 0;
            isHealing = true;
            target = other.GetComponent<CombatHandler>();
            var healthSys = other.GetComponent<HealthSystemComponent>().GetHealthSystem();
            healthSys.OnDamaged += HealthSys_OnDamaged;
        }
    }

    private void HealthSys_OnDamaged(object sender, System.EventArgs e)
    {
        isHealing = false;
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
        isHealing = false;
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
        if (isHealing) {
            TimeEclipse += Time.deltaTime;
            if (TimeEclipse > MaxHealTime) { 
                isHealing = false;
                return;
            }
            target.ReceiveHeal(healPerSec * Time.deltaTime);
        }
    }
}
