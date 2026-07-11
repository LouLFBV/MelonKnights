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
    [SerializeField] private float detectionRange = 10f;

    [Header("Animation Parameters")]
    [SerializeField] private string attackBoolName = "IsAttacking";
    [SerializeField] private string horizFloatName = "H";
    [SerializeField] private string vertFloatName = "V";
    [SerializeField] private string dieTriggerName = "Die";
    [SerializeField] private string speedFloatName = "Speed";

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 3f;

    private float _lastAttackTime = -Mathf.Infinity;

    [Header("Archer Settings")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowSpawnPointLeft, arrowSpawnPointRight, arrowSpawnPointForward, arrowSpawnPointBack;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip attackAudioClip;
    [SerializeField] private AudioClip deathAudioClip;

    [Header("Other Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Piece coinDropOnDeath;

    [SerializeField] private float targetRefreshRate = 0.2f;

    private float _nextTargetSearch;
    private bool _isAttacking;
    private bool _isDead;
    private bool _isStunned;
    private Transform _currentTarget;
    private int _lastAttackFrame = -1;


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
            playerTransform = PlayerController.Instance.transform;


        if (coreTransform == null)
            coreTransform = CoreDefense.Instance.transform;

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
        if (_isDead || playerTransform == null || _isStunned)
            return;

        if (!_isAttacking)
            HandleMovement();

        UpdateAnimatorParameters();
    }

    private void HandleMovement()
    {
        if (Time.time >= _nextTargetSearch)
        {
            _nextTargetSearch = Time.time + targetRefreshRate;
            UpdateTarget();
        }

        if (_currentTarget == null)
            return;

        float distance = Vector2.Distance(transform.position, _currentTarget.position);

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
        else
        {
            _agent.isStopped = false;

            if (Vector3.Distance(_agent.destination, _currentTarget.position) > 0.2f)
            {
                _agent.SetDestination(_currentTarget.position);
            }
        }
    }

    private void UpdateTarget()
    {
        // ===========================
        // 1. Cherche les tours
        // ===========================

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRange);

        Transform closestTower = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Tower"))
                continue;

            if (!hit.TryGetComponent(out HealthSystem health))
                continue;

            if (health.currentHealth <= 0)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTower = hit.transform;
            }
        }

        if (closestTower != null)
        {
            _currentTarget = closestTower;
            return;
        }

        // ===========================
        // 2. Cherche le joueur
        // ===========================

        if (playerTransform != null)
        {
            float playerDistance = Vector2.Distance(
                transform.position,
                playerTransform.position);

            if (playerDistance <= detectionRange)
            {
                _currentTarget = playerTransform;
                return;
            }
        }

        // ===========================
        // 3. Sinon Core
        // ===========================

        if (coreTransform != null)
        {
            _currentTarget = coreTransform;
        }
        else
        {
            _currentTarget = null;
        }
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


    public void Stun(float duration)
    {
        Debug.Log($"Boss stunned for {duration} seconds.");
        if (_isDead || _isStunned) return;

        StartCoroutine(StunCoroutine(duration));
    }

    private System.Collections.IEnumerator StunCoroutine(float duration)
    {
        _isStunned = true;
        _agent.isStopped = true;
        _bossAnimator.SetBool(attackBoolName, false); // Annule l'attaque en cours si besoin

        yield return new WaitForSeconds(duration);

        _isStunned = false;
        // On ne remet _agent.isStopped = false que si on n'est pas en train d'attaquer
        if (!_isAttacking)
        {
            _agent.isStopped = false;
        }
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
        if (_isDead || _currentTarget == null)
            return;

        if (Time.frameCount == _lastAttackFrame)
        {
            Debug.Log($"<color=orange>[AE_Attack Bloqué]</color> Évité sur la même frame pour {gameObject.name}");
            return;
        }
        _lastAttackFrame = Time.frameCount;

        if (enemySO.enemyType != EnemyType.Ranged)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackPointRange);

            // Création d'une liste pour mémoriser les cibles déjà touchées par CE coup d'épée
            System.Collections.Generic.HashSet<HealthSystem> damagedTargets = new System.Collections.Generic.HashSet<HealthSystem>();

            foreach (Collider2D hit in hits)
            {
                // Si on trouve un HealthSystem et que ce n'est pas un Boss
                if (hit.TryGetComponent(out HealthSystem health) && !hit.CompareTag("Boss"))
                {
                    // On vérifie si on n'a pas déjà infligé des dégâts à cette cible précise
                    if (!damagedTargets.Contains(health))
                    {
                        health.TakeDamage(attackDamage);
                        damagedTargets.Add(health); // On l'ajoute à la liste pour ne pas la retaper

                        // NOTE : Le mot-clé "break;" a été supprimé ici ! 
                        // Le boss va donc continuer la boucle et blesser les autres cibles.
                    }
                }
            }
        }
        else
        {
            if (arrowPrefab == null)
                return;

            Vector2 dir = (_currentTarget.position - transform.position).normalized;
            Transform spawnPoint = GetArrowSpawnPoint(dir);

            Debug.Log($"<color=green>[Arrow Spawned]</color> Flèche créée par {gameObject.name}");

            GameObject arrow = Instantiate(
                arrowPrefab,
                spawnPoint.position,
                Quaternion.identity);

            arrow.GetComponent<EnemyArrow>().Initialize(dir, attackDamage);
        }

        if (TryGetComponent(out AudioSource audioSource) && attackAudioClip != null)
        {
            audioSource.PlayOneShot(attackAudioClip);
        }
    }

    private Transform GetArrowSpawnPoint(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0
                ? arrowSpawnPointRight
                : arrowSpawnPointLeft;
        }

        return dir.y > 0
            ? arrowSpawnPointBack
            : arrowSpawnPointForward;
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

        if(TryGetComponent(out AudioSource audioSource) && deathAudioClip != null)
        {
            audioSource.PlayOneShot(deathAudioClip);
        }
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
        Destroy(gameObject, 0.1f);
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
#endif
}