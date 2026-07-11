using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    [Tooltip("Le nom du paramètre exposé dans l'Audio Mixer (ex: MasterVolume)")]
    [SerializeField] private string mixerParameter = "MasterVolume";

    private void Start()
    {
        // 1. Charger la valeur sauvegardée (ou mettre au max (1) par défaut)
        float savedVolume = PlayerPrefs.GetFloat(mixerParameter, 1f);

        // 2. Mettre le slider à la bonne valeur
        volumeSlider.value = savedVolume;

        // 3. Appliquer le volume au mixer
        SetVolume(savedVolume);

        // 4. Écouter les changements du slider en temps réel
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float sliderValue)
    {
        // La magie est ici : on convertit la valeur linéaire (0.0001 à 1) en décibels (-80dB à 0dB)
        float volumeInDb = Mathf.Log10(sliderValue) * 20f;

        // On applique au Mixer
        audioMixer.SetFloat(mixerParameter, volumeInDb);

        // On sauvegarde pour la prochaine fois qu'on lance le jeu
        PlayerPrefs.SetFloat(mixerParameter, sliderValue);
    }
}