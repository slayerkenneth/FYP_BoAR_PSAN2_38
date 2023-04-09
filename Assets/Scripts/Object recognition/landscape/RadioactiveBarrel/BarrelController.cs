using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelController : MonoBehaviour
{
    public float range;             //radius of the effect area
    public GameObject effect;
    public CapsuleCollider Collider;
    public float damagePerSec;
    public float slowdownRatio;
    public CombatHandler combatHandler;

    private List<GameObject> onEffect = new List<GameObject>();  //list of gameobject that are currently effect by this landscape

    // Start is called before the first frame update
    void Start()
    {
        var scale = effect.transform.localScale;
        scale.x = range / 4.0F;
        scale.z = range / 4.0F;
        effect.transform.localScale = scale;
        Collider.radius = range;
        combatHandler.SetCentralCombatHandler(GameObject.FindObjectsOfType<CentralBattleController>()[0]);
    }

    // Update is called once per frame
    void Update()
    {
        var deleteList = new List<GameObject>();
        foreach (GameObject target in onEffect)
        {
            CombatHandler combat;
            if (target != null && target.TryGetComponent<CombatHandler>(out combat)) combatHandler.DoDamage(combat, damagePerSec * Time.deltaTime);
            else deleteList.Add(target);
        }

        foreach (GameObject target in deleteList) onEffect.Remove(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemyPathfinding = other.GetComponent<EnemyPathfinding>();
            enemyPathfinding.setSpeed(slowdownRatio * enemyPathfinding.walkingSpeed);
            onEffect.Add(other.gameObject);
        }
        else if(other.CompareTag("Player"))
        {
            var playerMovementController = other.GetComponent<CharacterMovementController>();
            playerMovementController.setSpeed(slowdownRatio * playerMovementController.getSpeed());
            onEffect.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemyPathfinding = other.GetComponent<EnemyPathfinding>();
            enemyPathfinding.resetSpeed();
            onEffect.Remove(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            var playerMovementController = other.GetComponent<CharacterMovementController>();
            playerMovementController.resetSpeed();
            onEffect.Remove(other.gameObject);
        }
    }
}
