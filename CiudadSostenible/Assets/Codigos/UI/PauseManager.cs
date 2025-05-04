using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject pauseMenuCanvas;    
    public GameObject controlsCanvas;     
    public int MenuScene = 0;
    private bool isPaused = false;

    [Header("Audio")]
    public AudioMixer audioMixer; // Referencia al AudioMixer
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public string musicVolumeParameter = "MusicVol";
    public string sfxVolumeParameter = "SFXVol";

    private float previousMusicVolume;
    private float previousSFXVolume;

    void Start()
    {
        InitializeVolumeControls();
        pauseMenuCanvas.SetActive(false);
        controlsCanvas.SetActive(false);
    }

    void Update()
    {
        // Detectar tecla ESC para pausar/reanudar
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void InitializeVolumeControls()
    {
        // Configurar valores iniciales con defaults si no existen
        float musicVol = PlayerPrefs.GetFloat(musicVolumeParameter, 0.7f);
        float sfxVol = PlayerPrefs.GetFloat(sfxVolumeParameter, 0.7f);

        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        musicVolumeSlider.value = musicVol;
        sfxVolumeSlider.value = sfxVol;

        // Configurar listeners
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

        public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; 
        pauseMenuCanvas.SetActive(true); 
        //Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;

        // Guardar volúmenes actuales
        audioMixer.GetFloat(musicVolumeParameter, out previousMusicVolume);
        audioMixer.GetFloat(sfxVolumeParameter, out previousSFXVolume);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        pauseMenuCanvas.SetActive(false); 
        controlsCanvas.SetActive(false); 
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    public void ShowControls()
    {
        pauseMenuCanvas.SetActive(false);
        controlsCanvas.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        controlsCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(MenuScene);
    }

    public void SetMusicVolume(float volume)
    {
        float dBValue = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
        audioMixer.SetFloat(musicVolumeParameter, dBValue);
        PlayerPrefs.SetFloat(musicVolumeParameter, volume);
    }

    public void SetSFXVolume(float volume)
    {
        float dBValue = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
        audioMixer.SetFloat(sfxVolumeParameter, dBValue);
        PlayerPrefs.SetFloat(sfxVolumeParameter, volume);
    }

    public void ResetVolumes()
    {
        SetMusicVolume(previousMusicVolume);
        SetSFXVolume(previousSFXVolume);
        musicVolumeSlider.value = previousMusicVolume;
        sfxVolumeSlider.value = previousSFXVolume;
    }
}