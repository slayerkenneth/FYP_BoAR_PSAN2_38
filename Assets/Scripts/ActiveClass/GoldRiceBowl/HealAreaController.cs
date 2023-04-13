using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAreaController : MonoBehaviour
{
    public float range;             //radius of the effect area
    public GameObject effect;
    public CapsuleCollider Collider;
    public float healPerSec;
    public float duration;

    private List<GameObject> onEffect = new List<GameObject>();  //list of gameobject that are currently effect by this landscape
    private float TimeEclipse = 0.0F;

    // Start is called before the first frame update
    void Start()
    {
        var scale = effect.transform.localScale;
        scale.x = range / 4.0F;
        scale.z = range / 4.0F;
        effect.transform.localScale = scale;
        Collider.radius = range;
    }

    // Update is called once per frame
    void Update()
    {
        var deleteList = new List<GameObject>();
        foreach (GameObject target in onEffect)
        {
            CombatHandler combat;
            if (target != null && target.TryGetComponent<CombatHandler>(out combat)) combat.ReceiveHeal(healPerSec * Time.deltaTime);
            else deleteList.Add(target);
        }

        foreach (GameObject target in deleteList) onEffect.Remove(target);

        TimeEclipse += Time.deltaTime;
        if (TimeEclipse > duration) Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onEffect.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onEffect.Remove(other.gameObject);
        }
    }
}
