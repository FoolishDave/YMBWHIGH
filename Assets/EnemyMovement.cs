using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    public enum EnemyState { MovingIn, InRange, Fleeing }
    public enum JukeDirection { Left, Right, Still }

    private GameObject playerTarget;
    private Rigidbody body;
    private EnemyShoot gun;
    private float jukeTimer;
    private float shootTimer;
    private static List<Transform> enemies = new List<Transform>();

    public float shootingRadius;
    public float fleeRadius;
    public float moveSpeed;
    public float jukeTime;
    public float shootTime;

    public bool avoidOtherEnemies;
    public bool moveAndShoot;

    public EnemyState state;
    public JukeDirection jukeDir;

    private void Awake()
    {
        //Get the player
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        body = GetComponent<Rigidbody>();
        TimePower.playerAndEnemies.Add(body);
        gun = GetComponent<EnemyShoot>();
        if (jukeTime == 0)
        {
            jukeTime = Random.Range(0f, 10f);
        }
        enemies.Add(transform);
    }

    // Update is called once per frame
    void FixedUpdate () {
        //Todo: Add logic to only look at player if enemy has LOS
        bool visionOfPlayer = true;
        Vector3 moveVector = new Vector3();
        if (visionOfPlayer)
        {
            transform.LookAt(playerTarget.transform);
            float distToPlayer = Vector3.Distance(playerTarget.transform.position, transform.position);

            if (distToPlayer > shootingRadius)
            {
                state = EnemyState.MovingIn;
                moveVector += transform.forward;
            }
            else if (distToPlayer < fleeRadius)
            {
                state = EnemyState.Fleeing;
                moveVector -= transform.forward;
                moveVector = Strafe(moveVector);
            }
            else
            {
                state = EnemyState.InRange;
                moveVector = Strafe(moveVector);
            }
        }

        if (shootTimer < 0)
        {
            gun.Shoot();
            shootTimer = shootTime;
        }

        shootTimer -= Time.deltaTime;
        body.DOMove(transform.position + moveVector.normalized * moveSpeed, 0.15f);
	}

    private Vector3 Strafe(Vector3 currentMovement)
    {
        if (jukeTimer > jukeTime)
        {
            float ran = Random.Range(0f, 1f);
            if (ran < 0.3333f)
            {
                jukeDir = JukeDirection.Right;
                currentMovement += transform.right;
            } else if (ran < 0.6666f)
            {
                jukeDir = JukeDirection.Left;
                currentMovement -= transform.right;
            } else
            {
                jukeDir = JukeDirection.Still;
            }
            jukeTimer = 0;
        } else
        {
            if (jukeDir == JukeDirection.Right)
            {
                currentMovement += transform.right;
            } else if (jukeDir == JukeDirection.Left)
            {
                currentMovement -= transform.right;
            }
        }

        jukeTimer += Time.deltaTime;
        return currentMovement;
    }
}
