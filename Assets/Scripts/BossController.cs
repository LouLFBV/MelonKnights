using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    private HealthSystem _healthSystem;
    private Animator _bossAnimator;
    private NavMeshAgent _agent;

    [SerializeField] private EnemySO enemySO;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform coreTransform;
    [SerializeField] private BossUI bossUI;

    [Header("Combat Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackPointRange = 2f;
    [SerializeField] private float attackRange = 2f;

    [Header("Animation Parameters")]
    [SerializeField] private string attackBoolName = "IsAttacking";
    [SerializeField] private string horizFloatName = "H";
    [SerializeField] private string vertFloatName = "V";
    [SerializeField] private string dieTriggerName = "Die";
    [SerializeField] private string speedFloatName = "Speed";

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;

    private float _lastAttackTime = -Mathf.Infinity;

    [Header("Other Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Piece coinDropOnDeath;

    private bool _isAttacking;
    private bool _isDead;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _bossAnimator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        if (_healthSystem != null)
            _healthSystem.OnDeath += OnBossDeath;
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
            _healthSystem.OnDeath -= OnBossDeath;
    }

    private void Start()
    {
        if (_agent == null)
        {
            Debug.LogError("NavMeshAgent manquant sur le Boss.", this);
            enabled = false;
            return;
        }

        if (enemySO == null)
            Debug.LogError("EnemySO non assigné.", this);

        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform non assigné, on le cherche.", this);
            playerTransform = CoreDefense.Instance.transform;
        }

        if (attackPoint == null)
            Debug.LogError("Attack Point non assigné.", this);

        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer non assigné.", this);

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        if (enemySO != null)
            _agent.speed = enemySO.speed;
    }

    private void Update()
    {
        if (_isDead || playerTransform == null)
            return;

        if (!_isAttacking)
            HandleMovement();

        UpdateAnimatorParameters();
    }

    private void HandleMovement()
    {
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= attackRange)
        {
            _agent.isStopped = true;

            if (!_isAttacking && Time.time >= _lastAttackTime + attackCooldown)
            {
                _lastAttackTime = Time.time;

                _isAttacking = true;
                _bossAnimator.SetBool(attackBoolName, true);
            }
        }
        else /*if (distance <= detectionRange)*/
        {
            _agent.isStopped = false;

            // Evite de recalculer le chemin à chaque frame
            if (Vector3.Distance(_agent.destination, playerTransform.position) > 0.2f)
            {
                _agent.SetDestination(playerTransform.position);
            }
        }
        //else
        //{
        //    _agent.isStopped = true;
        //}
    }

    private void UpdateAnimatorParameters()
    {
        if (_agent == null)
            return;

        Vector2 velocity = _agent.velocity;

        float speed = velocity.magnitude;

        if (speed > 0.01f)
        {
            velocity.Normalize();

            _bossAnimator.SetFloat(horizFloatName, velocity.x);
            _bossAnimator.SetFloat(vertFloatName, velocity.y);
        }

        _bossAnimator.SetFloat(speedFloatName, speed);
    }

    //==========================================================================
    // Animation Events
    //==========================================================================

    // Début de l'animation
    public void AE_StartAttack()
    {
        if (_agent != null)
            _agent.isStopped = true;
    }

    // Moment où le coup touche
    public void AE_Attack()
    {
        if (_isDead)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackPointRange
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") &&
                hit.TryGetComponent(out HealthSystem playerHealth))
            {
                playerHealth.TakeDamage(attackDamage);
            }
            if (hit.CompareTag("Tower") &&
                hit.TryGetComponent(out HealthSystem towerHealth))
            {
                towerHealth.TakeDamage(attackDamage);
            }
        }
    }

    // Fin de l'animation
    public void AE_EndAttack()
    {
        if (_isDead)
            return;

        _isAttacking = false;

        if (_agent != null)
            _agent.isStopped = false;

        _bossAnimator.SetBool(attackBoolName, false);
    }

    //==========================================================================
    // Boss Death
    //==========================================================================

    private void OnBossDeath()
    {
        if (_isDead)
            return;

        _isDead = true;
        _isAttacking = false;

        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.enabled = false;
        }

        _bossAnimator.SetBool(attackBoolName, false);
        _bossAnimator.SetTrigger(dieTriggerName);
    }

    public void AE_Despawn()
    {
        PlayerController.Instance.XpSystem.AddXP(enemySO.xpDrop);

        Piece piece = Instantiate(
            coinDropOnDeath,
            transform.position,
            Quaternion.identity);
        piece.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        piece.SetCoinAmount(enemySO.coinDrop);

        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackPointRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        //Gizmos.color = Color.cyan;
        //Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}