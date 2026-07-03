using UnityEngine;

public class ScaleMoving : MonoBehaviour
{
    [Header("Réglages de l'effet")]
    [Tooltip("Vitesse à laquelle le GameObject grossit et rétrécit.")]
    public float speed = 2.0f;

    [Tooltip("Amplitude du changement de taille (0.2 signifie qu'il va varier de +/- 20%).")]
    public float amplitude = 0.2f;

    // Permet de stocker la taille d'origine du GameObject
    private Vector3 initialScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // On sauvegarde la taille initiale pour éviter que le scale ne dérive à l'infini
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // Mathf.Sin oscille naturellement entre -1 et 1 au cours du temps.
        // En multipliant par 'amplitude', l'oscillation passe de -amplitude à +amplitude.
        float scaleOffset = Mathf.Sin(Time.time * speed) * amplitude;

        // On applique le changement de taille de manière uniforme sur les 3 axes (X, Y, Z)
        // en se basant toujours sur la taille initiale.
        transform.localScale = initialScale * (1.0f + scaleOffset);
    }
}