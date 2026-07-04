using System;
using UnityEngine;
using UnityEngine.AI; // Indispensable pour le NavMeshAgent

public class BossController : MonoBehaviour
{
    private HealthSystem _healSystem;
    private Animator _bossAnimator;
    private NavMeshAgent _agent;

    [SerializeField] private EnemySO enemySO;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BossUI bossUI;

    [Header("Combat Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private int attackDamage = 1;       // Distance à laquelle le boss attaque
    [SerializeField] private float attackPointRange = 2f;       // Distance à laquelle le boss attaque
    [SerializeField] private float attackRange = 2f;       // Distance à laquelle le boss attaque
    [SerializeField] private float attackDuration = 3f;    // Combien de temps le bool reste vrai
    [SerializeField] private float attackCooldown = 2f;    // Temps de repos après l'attaque
    [SerializeField] private float detectionRange = 10f;    // Temps de repos après l'attaque

    [Header("Animation Parameters")]
    [SerializeField] private string attackBoolName = "IsAttacking";
    [SerializeField] private string speedFloatName = "Speed";
    [SerializeField] private string dieTriggerName = "Die";

    [Header("Other Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Piece coinDropOnDeath;

    private float _cooldownTimer = 0f;
    private float _attackTimer = 0f;
    private bool _isAttacking = false;
    private bool _isDead = false;

    void Awake()
    {
        _healSystem = GetComponent<HealthSystem>();
        _bossAnimator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        if (_agent != null)
        {
            _agent.updateRotation = false;
            _agent.updateUpAxis = false;
        }
    }

    void Update()
    {
        if (_isDead || playerTransform == null) return;

        // Gestion du miroir (Flip X) pour que le boss regarde toujours le joueur
        FlipTowardsPlayer();

        if (_isAttacking)
        {
            HandleAttackState();
        }
        else
        {
            HandleMovementAndCombat();
        }

        // Met à jour la vitesse dans l'animator pour déclencher l'anim de course/marche si tu en as une
        _bossAnimator.SetFloat(speedFloatName, _agent.velocity.sqrMagnitude);
    }

    private void HandleMovementAndCombat()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Gestion du cooldown de l'attaque
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }

        // Si le joueur est à portée et que le boss n'est pas en cooldown -> Attaque !
        if (distanceToPlayer <= attackRange && _cooldownTimer <= 0)
        {
            StartAttack();
        }
        else if (distanceToPlayer < detectionRange)
        {
            // Sinon, on poursuit le joueur
            _agent.isStopped = false;
            _agent.SetDestination(playerTransform.position);
        }
    }

    private void StartAttack()
    {
        _isAttacking = true;
        _attackTimer = attackDuration;
        _agent.isStopped = true; // Le boss s'arrête pour frapper
        _agent.velocity = Vector3.zero;

        _bossAnimator.SetBool(attackBoolName, true);
    }

    private void HandleAttackState()
    {
        _attackTimer -= Time.deltaTime;

        if (_attackTimer <= 0f)
        {
            // Fin de la tempête d'attaque
            _isAttacking = false;
            _cooldownTimer = attackCooldown;

            _bossAnimator.SetBool(attackBoolName, false);
            _agent.isStopped = false; // Il peut de nouveau bouger
        }
    }

    public void AE_AttackPlayer()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackPointRange);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Player"))
            {
                if (enemy.TryGetComponent<HealthSystem>(out var enemyHealth))
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }
        Debug.Log("Le Boss attaque le joueur !");
    }

    private void FlipTowardsPlayer()
    {
        // Ne pas flip pendant qu'il frappe (optionnel, retire cette ligne si tu veux qu'il se retourne même en attaquant)
        if (_isAttacking) return;

        if (spriteRenderer == null) return; // Sécurité

        if (playerTransform.position.x < transform.position.x)
        {
            spriteRenderer.flipX = false; // Regarde à gauche
        }
        else if (playerTransform.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true; // Regarde à droite
        }
    }

    private void OnBossDeath()
    {
        if (_isDead) return;

        _isDead = true;
        _agent.isStopped = true;
        _agent.enabled = false; // Coupe le pathfinding à la mort

        _bossAnimator.SetBool(attackBoolName, false);
        _bossAnimator.SetTrigger(dieTriggerName);
        Debug.Log("Le Boss est tombé au combat.");
    }

    private void OnEnable()
    {
        if (_healSystem != null)
            _healSystem.OnDeath += OnBossDeath;
    }

    private void OnDisable()
    {
        if (_healSystem != null)
            _healSystem.OnDeath -= OnBossDeath;
    }

    public void AE_Despawn()
    {
        PlayerController.Instance.XpSystem.AddXP(enemySO.xpDrop);
        Piece piece = Instantiate(coinDropOnDeath, transform.position, Quaternion.identity);
        if (piece != null)
        {
            piece.SetCoinAmount(enemySO.coinDrop);
            piece.playerPosition = PlayerController.Instance.transform;
            piece.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
        }
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(attackPoint.position, attackPointRange);

            Gizmos.color += Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackRange);


            Gizmos.color += Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

        }

    }
}