using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShoot : MonoBehaviour {

    public Transform barrel;
    public GameObject bulletPre;
    public float bulletForce;

	public void Shoot()
    {
        GameObject bullet = Instantiate(bulletPre);
        bullet.transform.position = barrel.position;
        bullet.transform.rotation = barrel.rotation;
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * bulletForce);
    }
}
