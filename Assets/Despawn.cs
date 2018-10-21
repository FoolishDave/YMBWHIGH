using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despawn : MonoBehaviour {

    public float despawnIn = 5f;

    public void Update()
    {
        if (despawnIn < 0)
        {
            Destroy(gameObject);
        }
        despawnIn -= Time.deltaTime;
    }
}
