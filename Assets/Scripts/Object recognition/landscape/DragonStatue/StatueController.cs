using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatueController : LandscapeController
{
    public float extraDamage;
    public float increaseSpeedRatio;
    public float duration;
    private GameObject player;
    private bool onEffect;
    private float TimeEclipse;


    // Start is called before the first frame update
    void Start()
    {
        
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
        if (onEffect) {
            TimeEclipse += Time.deltaTime;
            if (TimeEclipse > duration & player!= null) {
                onEffect = false;
                var combat = player.GetComponent<CombatHandler>();
                combat.resetExtraDamage();
                var movement = player.GetComponent<CharacterMovementController>();
                movement.resetSpeed();
                TimeEclipse = 0;
                player = null;
            }
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (Active && other.CompareTag("Player"))
        {
            Active = false;
            remainCD = CD;
            ActiveSign.Stop();
            var combat = other.GetComponent<CombatHandler>();
            combat.setExtraDamage(extraDamage);
            var movement = other.GetComponent<CharacterMovementController>();
            movement.setSpeed(movement.getSpeed() * increaseSpeedRatio);

            player = other.gameObject;
            TimeEclipse = 0;
            onEffect = true;
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        
    }
}
