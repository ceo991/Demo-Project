using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    void Start()
    {
        //Destroys this object within 2 seconds
        Destroy(gameObject, 2f);
    }
}