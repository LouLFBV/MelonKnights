using UnityEngine;

public class TriggerScene : MonoBehaviour
{
    [SerializeField] private string sceneName; // Nom de la scène à charger
    [SerializeField] private Animator panelTransitionAnimator; // Nom de la scène à charger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (panelTransitionAnimator != null)
            {
                panelTransitionAnimator.SetTrigger("StartTransition");
            }
            else
            {
                Debug.LogWarning("L'Animator pour la transition n'est pas assigné !");
                AE_ChangeScene(); // Charger la scène immédiatement si l'Animator n'est pas assigné
            }
        }
    }

    public void AE_ChangeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
