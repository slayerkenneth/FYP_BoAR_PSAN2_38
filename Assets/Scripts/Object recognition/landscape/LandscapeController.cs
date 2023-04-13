using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LandscapeController : MonoBehaviour
{
    public float CD;
    public float remainCD;
    [SerializeField]public ParticleSystem ActiveSign;
    public event EventHandler OnCollect;
    public bool Active;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected abstract void OnTriggerEnter(Collider other);

    protected abstract void OnTriggerExit(Collider other);

    protected void OnCollectStart() {
        OnCollect?.Invoke(this, EventArgs.Empty);
    }
}
