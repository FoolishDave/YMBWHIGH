using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {

    private Rigidbody body;
    public float moveSpeed;
    public Camera gameCamera;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        TimePower.playerAndEnemies.Add(body);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 vel = new Vector3();
        vel.x = moveSpeed * Input.GetAxis("Horizontal");
        vel.z = moveSpeed * Input.GetAxis("Vertical");
        body.velocity = vel;
        LookAtMouse();
	}

    private void LookAtMouse()
    {
        Ray cameraRay = gameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit))
        {
            Vector3 lookPoint = hit.point;
            lookPoint.y = transform.position.y;
            transform.LookAt(lookPoint);
        }
    }
}
