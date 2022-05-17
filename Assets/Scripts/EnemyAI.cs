using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    Transform player;
    NavMeshAgent agent;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] LayerMask whatIsPlayer;

    [SerializeField] float health = 100f;

    [Header("Patroling")]
    [SerializeField] Vector3 walkPoint;
    [SerializeField] float walkPointRange;
    bool walkPointSet;

    [Header("Attacking")]
    [SerializeField] float timeBetweenAttacks = 0.2f;
    [SerializeField] float pushForce = 5f;
    //[SerializeField] GameObject projectile;
    bool alreadyAttacked;

    [Header("States")]
    [SerializeField] float sightRange;
    [SerializeField] float attackRange;
    [SerializeField] bool playerInSightRange;
    [SerializeField] bool playerInAttackRange;

    void Start()
    {
        player = Player.Instance.gameObject.transform;
        agent = GetComponent<NavMeshAgent>();

        if (GetComponent<NavMeshAgent>() == null)
            Debug.LogError("Agent wasn't found");

        if (Player.Instance.gameObject.transform == null)
            Debug.LogError("Player wasn't found");
    }

    void Update()
    {
        if (alreadyAttacked)
            return;

        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    void Patroling()
    {
        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        Vector3 pos = transform.position;
        walkPoint = new Vector3(pos.x + randomX, pos.y, pos.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            ///Attack code here
            Rigidbody rb = GetComponent<Rigidbody>();
            //rb.AddForce(transform.forward * -pushForce, ForceMode.Impulse);
            rb.AddForce(transform.up * pushForce * rb.mass * 50f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void ResetAttack()
    {
        alreadyAttacked = false;
    }

    void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
            Invoke(nameof(DestroyEnemy), 0.5f);
    }

    void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
            TakeDamage(other.gameObject.GetComponent<Bullet>().Damage);
    }
}
