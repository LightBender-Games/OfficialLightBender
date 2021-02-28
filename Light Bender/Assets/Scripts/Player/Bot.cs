using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
/*
    public NavMeshAgent agent;

    public Transform player;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;
    
    [SerializeField]  Item[] items;
    int itemIndex = 0;

    public int Team;
    
    //Patrouille
    public Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange;
    
    //Attaque
    public float timeBetweenAttacks;
    private bool alreadyAttacked;
    
    //Status
    public float sightRange;
    public bool playerInSight, playerInAttackRange;
    
    Rigidbody rb;

    PhotonView Phv;

    private void Awake()
    {
        player = GameObject.Find("PlayerObj").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        if (Phv.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }

    private void Update()
    {
        //verifie si il y a des joueurs en vue et si ils sont à distance de tir
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        switch (playerInSightRange, playerInAttackRange)
        {
            case (false, false):
                Patrol();
                break;
            case (true, false):
                Chase();
                break;
            case (true, true):
                Attack();
                break;
        }
    }
            
    private void Patrol()
    {
        if (walkPointSet)
            agent.SetDestination(walkPoint);
        else
        {
            SearchWalkPoint();
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        
        //Point atteint
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        //Calcule un point random à distance de vue
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(
            transform.position.x + randomX, 
            transform.position.y, 
            transform.position.z + randomZ
            );

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }
    
    private void Chase()
    {
        agent.SetDestination(player.position);
    }
    private void Attack()
    {
        //S'assurer que l'ennemi n'a pas bougé
        agent.SetDestination(transform.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            items[itemIndex].Use();
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0f)
            Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }
*/
}