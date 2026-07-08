using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int count = 5;
    public float spawnInterval = 1f; // délai entre chaque spawn de ce type d'ennemi
}

[System.Serializable]
public class Wave
{
    public string waveName = "Vague 1";
    public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
    public float delayBeforeNextWave = 5f;
}

public class WaveManager : MonoBehaviour
{
    [Header("Points de spawn (place 6 Transform vides autour de la map)")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Vagues")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private bool autoStart = true;
    [SerializeField] private float delayBeforeFirstWave = 3f;

    private int _currentWaveIndex = 0;
    private int _enemiesAliveInWave = 0;

    // Permet à d'autres scripts (UI, musique, etc.) de réagir au début d'une vague ou à la fin de toutes les vagues
    public event System.Action<int> OnWaveStarted;
    public event System.Action OnAllWavesCompleted;

    private void Start()
    {
        if (autoStart)
        {
            StartCoroutine(StartWavesRoutine());
        }
    }

    public void StartWaves()
    {
        StartCoroutine(StartWavesRoutine());
    }

    private IEnumerator StartWavesRoutine()
    {
        yield return new WaitForSeconds(delayBeforeFirstWave);

        while (_currentWaveIndex < waves.Count)
        {
            yield return StartCoroutine(SpawnWave(waves[_currentWaveIndex]));

            // Attend que tous les ennemis de la vague en cours soient morts avant de continuer
            yield return new WaitUntil(() => _enemiesAliveInWave <= 0);

            float delay = waves[_currentWaveIndex].delayBeforeNextWave;
            _currentWaveIndex++;

            if (_currentWaveIndex < waves.Count)
            {
                yield return new WaitForSeconds(delay);
            }
        }

        OnAllWavesCompleted?.Invoke();
        Debug.Log("Toutes les vagues sont terminées !");
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        OnWaveStarted?.Invoke(_currentWaveIndex + 1);
        Debug.Log($"Début de la vague : {wave.waveName}");

        foreach (EnemySpawnInfo info in wave.enemies)
        {
            for (int i = 0; i < info.count; i++)
            {
                SpawnEnemy(info.enemyPrefab);
                yield return new WaitForSeconds(info.spawnInterval);
            }
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || spawnPoints.Count == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        _enemiesAliveInWave++;

        // On écoute la mort de l'ennemi via son HealthSystem pour savoir quand la vague est terminée
        if (enemyObj.TryGetComponent<HealthSystem>(out var health))
        {
            health.OnDeath += OnEnemyDeath;
        }
        else
        {
            Debug.LogWarning($"{enemyPrefab.name} n'a pas de HealthSystem : la vague ne pourra pas détecter sa mort.");
            _enemiesAliveInWave--; // évite de bloquer la vague indéfiniment
        }
    }

    private void OnEnemyDeath()
    {
        _enemiesAliveInWave--;
    }

    private void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
                Gizmos.DrawLine(point.position, point.position + Vector3.up * 1.5f);
            }
        }
    }
}