using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCanvas : MonoBehaviour {

    public Transform follow;
    private Vector3 vel;

    // Update is called once per frame
    void Update() {
        if (follow == null) return;
        if (Time.timeScale < .8f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, follow.position, ref vel, .08f);
        } else
        {
            transform.position = follow.position;
        }
        
	}
}
