using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Game Over")]
    public GameObject panelGameOver;
    public GameObject panelNivelCompletado;

    private bool juegoTerminado = false;

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
    }

    void Start()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Debug.Log("Game Over!");

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Activar panel de Game Over
        if (panelGameOver != null) panelGameOver.SetActive(true);
        if (panelNivelCompletado != null) panelNivelCompletado.SetActive(false);

        // Detener el tiempo
        Time.timeScale = 0f;
    }

    public void LevelComplete()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;
        Debug.Log("¡Nivel Completado!");

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Activar panel de Victoria
        if (panelNivelCompletado != null) panelNivelCompletado.SetActive(true);
        if (panelGameOver != null) panelGameOver.SetActive(false);

        // Detener el tiempo
        Time.timeScale = 0f;
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuInicial");
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
