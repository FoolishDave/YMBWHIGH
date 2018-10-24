using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour {

    public Transform playerTarget;
    public float fireRate;
    public float range;

    private float fireTimer;

	// Update is called once per frame
	void FixedUpdate () {
        if (CanSeePlayer())
        {
            transform.LookAt(playerTarget);
            if (fireTimer <= 0.0f)
            {
                Debug.Log("Shooting");
                GetComponent<EnemyShoot>().Shoot();
                fireTimer = fireRate;
            }

            fireTimer -= Time.fixedDeltaTime;
        }
	}

    private bool CanSeePlayer()
    {
        RaycastHit hit;
        bool sees = false;
        if (Physics.Raycast(transform.position, playerTarget.position - transform.position, out hit, range))
        {
            sees = hit.transform.tag == "Player";
        }
        return sees;
    }
}
