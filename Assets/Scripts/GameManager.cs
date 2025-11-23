using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Necesario para TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Game Over")]
    public GameObject panelGameOver;
    public GameObject panelNivelCompletado;
    public GameObject panelPausa; // Nuevo panel de pausa

    [Header("UI Timer")]
    public TextMeshProUGUI textoTimerHUD;      // El que se ve mientras juegas
    public TextMeshProUGUI textoTimerFinal;    // Para Game Over
    public TextMeshProUGUI textoTimerVictoria; // Para Victoria
    public TextMeshProUGUI textoTimerPausa;    // Para Pausa

    [Header("Audio")]
    public AudioClip musicaJuego;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoMuerte;
    
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private bool juegoTerminado = false;
    private bool juegoPausado = false; // Estado de pausa
    private float tiempoJuego = 0f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Configurar AudioSources
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

        // Reproducir música de fondo
        if (musicaJuego != null)
        {
            musicSource.clip = musicaJuego;
            musicSource.Play();
        }
    }

    void Update()
    {
        // Detectar tecla P para pausar
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        // Actualizar temporizador si el juego está activo
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
            // Formatear tiempo a mm:ss
            int minutos = Mathf.FloorToInt(tiempoJuego / 60F);
            int segundos = Mathf.FloorToInt(tiempoJuego % 60F);
            int milisegundos = Mathf.FloorToInt((tiempoJuego * 100F) % 100F);
            
            texto.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            // Si quieres milisegundos usa: string.Format("{0:00}:{1:00}:{2:00}", minutos, segundos, milisegundos);
        }
    }

    public void TogglePause()
    {
        if (juegoTerminado) return;

        juegoPausado = !juegoPausado;

        if (panelPausa != null)
        {
            panelPausa.SetActive(juegoPausado);
            // Mostrar tiempo en el menú de pausa
            if (juegoPausado) ActualizarTextoTiempo(textoTimerPausa);
        }

        if (juegoPausado)
        {
            Time.timeScale = 0f; // Pausar tiempo
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Opcional: Bajar volumen de música al pausar
            musicSource.volume = 0.5f;
        }
        else
        {
            Time.timeScale = 1f; // Reanudar tiempo
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Restaurar volumen
            musicSource.volume = 1f;
        }
    }

    public void SetVolume(float volume)
    {
        // Controlar volumen global (0.0 a 1.0)
        AudioListener.volume = volume;
    }

    public void GameOver()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Debug.Log("Game Over!");

        // Audio Muerte
        if (musicSource.isPlaying) musicSource.Stop();
        if (sonidoMuerte != null) sfxSource.PlayOneShot(sonidoMuerte);

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Activar panel de Game Over
        if (panelGameOver != null) 
        {
            panelGameOver.SetActive(true);
            ActualizarTextoTiempo(textoTimerFinal); // Mostrar tiempo final
        }
        if (panelNivelCompletado != null) panelNivelCompletado.SetActive(false);

        // Detener el tiempo
        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Debug.Log("¡Nivel Completado!");

        // Audio Victoria
        if (musicSource.isPlaying) musicSource.Stop();
        if (sonidoVictoria != null) sfxSource.PlayOneShot(sonidoVictoria);

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Activar panel de Victoria
        if (panelNivelCompletado != null) 
        {
            panelNivelCompletado.SetActive(true);
            ActualizarTextoTiempo(textoTimerVictoria); // Mostrar tiempo final
        }
        if (panelGameOver != null) panelGameOver.SetActive(false);

        // Detener el tiempo
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
        // Cargar escena del menú principal o salir
        // Asumiendo que la escena 0 es el menú, o usar Application.Quit() si es build
        // SceneManager.LoadScene(0); 
        Application.Quit();
        
        // Si estás en el editor de Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
