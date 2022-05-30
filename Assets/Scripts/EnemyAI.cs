using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Common Props")]
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] LayerMask whatIsPlayer;
    [SerializeField] LayerMask whatIsObstacle;
    [SerializeField] LayerMask whatIsFriend;
    [SerializeField] float health = 100f;
    Transform _player;
    NavMeshAgent _agent;
    Rigidbody _rb;

    [Header("Patrolling")]
    [SerializeField] Vector3 walkPoint;
    [SerializeField] float walkPointRange;
    bool _walkPointSet;

    [Header("Attacking")]
    [SerializeField] float timeBetweenAttacks = 0.2f;
    [SerializeField] float pushForce = 5f;
    //[SerializeField] GameObject projectile;
    bool _alreadyAttacked;

    [Header("States")]
    [SerializeField] float sightRange;
    [SerializeField] float attackRange;
    [SerializeField] float obstacleDetectionRange;
    [SerializeField] bool playerInSightRange;
    [SerializeField] bool playerInAttackRange;

    [Header("Genetic Algorithm")]
    [SerializeField] int populationSize = 200;
    [SerializeField] int elitism = 10;
    [SerializeField] float mutationRate = 0.05f;
    [SerializeField] int exponentialCoefficientA = 5;
    [Range(1f, 1.5f)]
    [SerializeField] float obstacleDetectionPenalty = 1.03f;
    [Range(1f, 1.5f)]
    [SerializeField] float friendDetectionBonus = 1.025f;
    [SerializeField] float generationLifeTime = 10f; 
    GeneticAlgorithm<float> ga;
    readonly int dnaLength = 2;
    float _generationTimer = 10f;

    void Start()
    {
        _player = Player.Instance.gameObject.transform;
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        ga = new GeneticAlgorithm<float>(populationSize, dnaLength, GetRandomFloat, FitnessFunction, elitism, mutationRate);

        if (GetComponent<NavMeshAgent>() == null)
            Debug.LogError("Agent wasn't found");

        if (Player.Instance.gameObject.transform == null)
            Debug.LogError("Player wasn't found");

        StartCoroutine(Timer());
    }

    void Update()
    {
        if (_alreadyAttacked)
            return;

        //Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patrolling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    IEnumerator Timer()
    {
        while(true)
        {
            _generationTimer += 1f;
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private float GetRandomFloat() => UnityEngine.Random.Range(-walkPointRange, walkPointRange);
    private float FitnessFunction(int index)
    {
        float score = 0f;
        DNA<float> dna = ga.Population[index];

        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = new Vector3(currentPosition.x + dna.Genes[0], currentPosition.y, currentPosition.z + dna.Genes[1]);
        if (Physics.Raycast(nextPosition, -transform.up, 1f, whatIsGround))
        {
            var distanceToPlayer = Vector3.Distance(Player.Instance.GetPosition(), nextPosition);
            score = 1f / (distanceToPlayer - attackRange);

            if (Physics.CheckSphere(nextPosition, obstacleDetectionRange, whatIsObstacle))
                score /= obstacleDetectionPenalty;

            if (Physics.CheckSphere(nextPosition, sightRange, whatIsFriend))
                score *= friendDetectionBonus;

        }

        if (exponentialCoefficientA > 1)
            score = (Mathf.Pow(exponentialCoefficientA, score) - 1) / (exponentialCoefficientA - 1);
        else
            Debug.Log("Low efficiency. Better with A > 1");

        return score;
    }

    void Patrolling()
    {
        if (!_walkPointSet || _generationTimer > generationLifeTime)
            SearchWalkPoint();

        if (_walkPointSet)
            _agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Waypoint reached
        if (distanceToWalkPoint.magnitude < 0.5f)
            _walkPointSet = false;
    }

    void SearchWalkPoint()
    {
        _generationTimer = 0f;
        ga.NewGeneration();

        Vector3 pos = transform.position;
        walkPoint = new Vector3(pos.x + ga.BestGenes[0], pos.y, pos.z + ga.BestGenes[1]);

        _walkPointSet = true;
    }

    void ChasePlayer()
    {
        _agent.SetDestination(_player.position);
    }

    void AttackPlayer()
    {
        //Make sure enemy doesn't move
        _agent.SetDestination(transform.position);
        transform.LookAt(_player);

        if (!_alreadyAttacked)
        {
            _agent.enabled = false;

            _rb.velocity = Vector3.zero;
            _rb.AddForce(Player.Instance.GetMoveDirection() * pushForce, ForceMode.VelocityChange);
            _rb.AddForce(transform.up * pushForce, ForceMode.VelocityChange);

            _alreadyAttacked = true;
            
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    void ResetAttack()
    {
        _alreadyAttacked = false;
        _agent.enabled = true;
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, obstacleDetectionRange);
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
            TakeDamage(other.gameObject.GetComponent<Bullet>().Damage);
    }
}
