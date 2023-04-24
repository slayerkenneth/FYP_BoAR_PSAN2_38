using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassSelectionNotice : MonoBehaviour
{
    public GameObject classNotice;

    public void DestroyUI()
    {
        Destroy(gameObject);
    }
}
