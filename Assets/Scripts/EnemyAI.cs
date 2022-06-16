using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.IO;

using Random = UnityEngine.Random;

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
    float _timeStep = 0.01f;

    [Header("Patrolling")]
    [SerializeField] Vector3 walkPoint;
    [SerializeField] float distanceToPointDelta = 0.3f;
    bool _walkPointSet;

    [Header("Attacking")]
    [SerializeField] float timeBetweenAttacks = 0.2f;
    [SerializeField] float pushForce = 5f;
    //[SerializeField] GameObject projectile;
    bool _alreadyAttacked;

    [Header("States")]
    [SerializeField] float sightRange;
    [SerializeField] float attackRange;
    [SerializeField] bool playerInSightRange;
    [SerializeField] bool playerInAttackRange;

    [Header("Genetic Algorithm")]
    [SerializeField] List<Params> gaParamsList;
    [SerializeField] int defaultParam;
    [SerializeField] bool showWaypointsInGame = true;

    GeneticAlgorithm<float> _ga;
    Params _currConfig;
    float _generationTimer;
    float _commonTimer;

    List<Vector3> _waypointsPositions;
    string _gaResultsData;

    void Start()
    {
        _player = Player.Instance.gameObject.transform;
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();

        if (!gaParamsList.Any())
            Debug.LogError("Params list is empty");

        if (gaParamsList[defaultParam] == null)
            defaultParam = 0;

        _currConfig = gaParamsList[defaultParam];
        _ga = new GeneticAlgorithm<float>(_currConfig.PopulationSize, _currConfig.DnaLength, GetRandomFloat, FitnessFunction, _currConfig.Elitism, _currConfig.MutationRate);

        if (_agent == null)
            Debug.LogError("Agent wasn't found");

        if (Player.Instance.gameObject.transform == null)
            Debug.LogError("Player wasn't found");

        if (_currConfig.ExponentialBase <= 1)
            Debug.Log("Low efficiency. Better with base > 1");

        _waypointsPositions = new List<Vector3>();
        _waypointsPositions.Add(transform.position);

        _gaResultsData += "Generation:\tBestFitness\tTime:\n";

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
            _generationTimer += _timeStep;
            _commonTimer += _timeStep;
            yield return new WaitForSecondsRealtime(_timeStep);
        }
    }

    private float GetRandomFloat() => Random.Range(0.0f, _currConfig.SearchRange) * Mathf.Sign(Random.Range(-1,1));
    private float FitnessFunction(int index)
    {
        float score = 0f;
        DNA<float> dna = _ga.Population[index];

        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = new Vector3(
            currentPosition.x + dna.Genes[0],
            currentPosition.y,
            currentPosition.z + dna.Genes[1]);

        bool isThereGround = Physics.Raycast(nextPosition, -transform.up, _agent.height, whatIsGround);
        bool isThereObstacleOnWay = IsThereObstacle(nextPosition, out var hit, Vector3.Distance(transform.position, nextPosition));

        if (isThereObstacleOnWay && showWaypointsInGame)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red, 2f);
            Debug.DrawLine(transform.position, nextPosition, Color.grey, 2f);
        }

        if (isThereGround && !isThereObstacleOnWay)
        {
            var distanceToPlayer = Vector3.Distance(Player.Instance.GetPosition(), nextPosition);
            score = 1f / (distanceToPlayer - attackRange);

            if (Physics.CheckSphere(nextPosition, _currConfig.ObstacleDetectionRange, whatIsObstacle)) //are obstacles too close to us?
                score /= _currConfig.ObstacleDetectionPenalty;

            if (Physics.CheckSphere(nextPosition, sightRange, whatIsFriend)) //are friends around us?
                score *= _currConfig.FriendDetectionBonus;
        }

        float coefficient = _currConfig.ExponentialBase;
        if (coefficient > 1)
            score = (Mathf.Pow(coefficient, score) - 1) / (coefficient - 1);

        return score;
    }

    bool IsThereObstacle(Vector3 destination, out RaycastHit hit, float distance)
    {
        return Physics.SphereCast(transform.position, _agent.radius, destination, out hit, distance, whatIsObstacle);
    }

    void Patrolling()
    {
        if (!_walkPointSet || _generationTimer > _currConfig.GenerationLifeTime)
            SearchWalkPoint();

        if (_walkPointSet)
        {
            transform.LookAt(walkPoint);
            _agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Waypoint reached
        if (distanceToWalkPoint.magnitude < distanceToPointDelta)
            _walkPointSet = false;

    }

    void SearchWalkPoint()
    {
        _generationTimer = 0f;
        _ga.NewGeneration();
        _waypointsPositions.Add(transform.position);

        _gaResultsData += _ga.Generation + "\t" + _ga.BestFitness + "\t" + _commonTimer + "\n";

        if (_ga.BestFitness > float.Epsilon)
        {
            Vector3 pos = transform.position;
            walkPoint = new Vector3(pos.x + _ga.BestGenes[0], pos.y, pos.z + _ga.BestGenes[1]);

            _walkPointSet = true;
            Debug.DrawLine(pos, walkPoint, Color.yellow, 1f);
        }
        else
            _ga.ForceMutate(); //need to find new move direction
    }

    void ChasePlayer()
    {
        transform.LookAt(_player);
        //can we get to player with out hitting obstacles?
        if (!IsThereObstacle(_player.position, out var hit, Vector3.Distance(transform.position, _player.position)))
            _agent.SetDestination(_player.position);
        else
            Debug.DrawLine(transform.position, hit.point, Color.grey, 1f);
    }

    void AttackPlayer()
    {
        //make sure enemy doesn't move
        _agent.SetDestination(transform.position);
        transform.LookAt(_player);
        _waypointsPositions.Add(transform.position);

        if (!_alreadyAttacked)
        {
            _agent.enabled = false;

            _rb.velocity = Vector3.zero;
            _rb.AddForce(Player.Instance.GetMoveDirection() * pushForce, ForceMode.VelocityChange);
            _rb.AddForce(transform.up * pushForce, ForceMode.VelocityChange);

            _alreadyAttacked = true;

            WriteResultsInFile();
            gameObject.SetActive(false);
            
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
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        if (!showWaypointsInGame)
            DrawEnemyWay();

        if (_currConfig == null)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _currConfig.ObstacleDetectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _currConfig.SearchRange);
    }

    void OnDrawGizmos()
    {
        if(showWaypointsInGame && Application.isPlaying)
            DrawEnemyWay();
    }

    void DrawEnemyWay()
    {
        if (!_waypointsPositions.Any())
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < _waypointsPositions.Count - 1; i++)
        {
            Gizmos.DrawLine(_waypointsPositions[i], _waypointsPositions[i + 1]);
            Gizmos.DrawSphere(_waypointsPositions[i], 0.2f);
        }
    }

    float GetEnemyWayLength()
    {
        float result = 0f;
        for (int i = 0; i < _waypointsPositions.Count - 1; i++)
            result += Vector3.Distance(_waypointsPositions[i], _waypointsPositions[i + 1]);

        return result;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
            TakeDamage(other.gameObject.GetComponent<Bullet>().Damage);
    }

    public void WriteResultsInFile()
    {
        string summary = "Strategy:\tDistance:\tTime:\n";
        summary += _currConfig.StrategyName + "\t" + GetEnemyWayLength() + "\t" + _commonTimer;

        string sumPath = Application.dataPath + "/Results/Summary.txt";

        StreamWriter writer = new StreamWriter(sumPath, true);
        writer.WriteLine(summary);
        writer.Close();

        string path = Application.dataPath + "/Results/" + _currConfig.StrategyName + ".txt";
        
        writer = new StreamWriter(path, true);
        writer.WriteLine(_gaResultsData);
        writer.Close();
        Debug.Log(_gaResultsData);
    }
}
