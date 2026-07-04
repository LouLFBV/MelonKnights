using UnityEngine;
using UnityEngine.UI;
public class BossUI : MonoBehaviour
{
    private HealthSystem _healSystem;

    [SerializeField] private GameObject healthBarCanvas;
    [SerializeField] private CanvasGroup healthBarUI;
    [SerializeField] private Image healthAmount;
    void Awake()
    {
        _healSystem = GetComponent<HealthSystem>();
    }


    private void OnEnable()
    {
        _healSystem.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        _healSystem.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI()
    {
        float currentHealth = _healSystem.currentHealth;
        float maxHealth = _healSystem.maxHealth;

        if (currentHealth > 0)
        {
            healthAmount.fillAmount = currentHealth / maxHealth;
        }
        else
            DespawnHealthBar();
    }
    public void FlipHealthBar(bool right)
    {
        if (!right)
            healthBarCanvas.transform.localScale = new Vector3(1f, 1f, 1f);
        else
            healthBarCanvas.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void DespawnHealthBar()
    {
        healthBarUI.alpha = 0f;
    }
}
