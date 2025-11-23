using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Game Over")]
    public GameObject panelGameOver;
    public GameObject panelNivelCompletado;
    public GameObject panelPausa;

    [Header("UI Timer")]
    public TextMeshProUGUI textoTimerHUD;
    public TextMeshProUGUI textoTimerFinal;
    public TextMeshProUGUI textoTimerVictoria;
    public TextMeshProUGUI textoTimerPausa;

    [Header("Audio")]
    public AudioClip musicaJuego;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoMuerte;
    
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private bool juegoTerminado = false;
    private bool juegoPausado = false;
    private float tiempoJuego = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelNivelCompletado != null) panelNivelCompletado.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(false);

        if (musicaJuego != null)
        {
            musicSource.clip = musicaJuego;
            musicSource.Play();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (!juegoTerminado && !juegoPausado)
        {
            tiempoJuego += Time.deltaTime;
            ActualizarTextoTiempo(textoTimerHUD);
        }
    }

    void ActualizarTextoTiempo(TextMeshProUGUI texto)
    {
        if (texto != null)
        {
            int minutos = Mathf.FloorToInt(tiempoJuego / 60F);
            int segundos = Mathf.FloorToInt(tiempoJuego % 60F);
            int milisegundos = Mathf.FloorToInt((tiempoJuego * 100F) % 100F);
            
            texto.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public void TogglePause()
    {
        if (juegoTerminado) return;

        juegoPausado = !juegoPausado;

        if (panelPausa != null)
        {
            panelPausa.SetActive(juegoPausado);
            if (juegoPausado) ActualizarTextoTiempo(textoTimerPausa);
        }

        if (juegoPausado)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            musicSource.volume = 0.5f;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            musicSource.volume = 1f;
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void GameOver()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;

        if (musicSource.isPlaying) musicSource.Stop();
        if (sonidoMuerte != null) sfxSource.PlayOneShot(sonidoMuerte);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelGameOver != null) 
        {
            panelGameOver.SetActive(true);
            ActualizarTextoTiempo(textoTimerFinal);
        }
        if (panelNivelCompletado != null) panelNivelCompletado.SetActive(false);

        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;

        if (musicSource.isPlaying) musicSource.Stop();
        if (sonidoVictoria != null) sfxSource.PlayOneShot(sonidoVictoria);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelNivelCompletado != null) 
        {
            panelNivelCompletado.SetActive(true);
            ActualizarTextoTiempo(textoTimerVictoria);
        }
        if (panelGameOver != null) panelGameOver.SetActive(false);

        Time.timeScale = 0f;
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Abandonar()
    {
        Time.timeScale = 1f;
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
