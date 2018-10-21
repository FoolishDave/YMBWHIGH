using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImageFollow : MonoBehaviour
{

    public Transform follow;
    public float followTime;
    private Vector3 vel;

    // Update is called once per frame
    void Update()
    {
        if (follow == null) return;
        transform.position = Vector3.SmoothDamp(transform.position, follow.position, ref vel, followTime);
    }
}
