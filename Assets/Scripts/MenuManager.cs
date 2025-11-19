using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject panelCreditos;
    public GameObject panelControles;
    public GameObject menuPrincipal;

    void Start()
    {
        MostrarMenuPrincipal();
    }

    public void IniciarJuego()
    {
        SceneManager.LoadScene("Juego");
    }

    public void MostrarCreditos()
    {
        if (menuPrincipal != null) menuPrincipal.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }
    public void MostrarControles()
    {
        if (menuPrincipal != null) menuPrincipal.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(true);
    }

    public void MostrarMenuPrincipal()
    {
        if (menuPrincipal != null) menuPrincipal.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelControles != null) panelControles.SetActive(false);
    }

    public void SalirJuego()
    {
        Application.Quit();
    }
}
