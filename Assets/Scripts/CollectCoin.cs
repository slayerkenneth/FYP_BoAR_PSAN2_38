using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectCoin : MonoBehaviour
{
    private CoinGenerator coinGenerator;

    private void Start()
    {
        coinGenerator = FindObjectOfType<CoinGenerator>();
        coinGenerator.SetAgent(transform);
        coinGenerator.CreateNewCoin();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collected Coin");
        Destroy(other.transform.parent.gameObject);
        coinGenerator.CreateNewCoin();
    }
}
