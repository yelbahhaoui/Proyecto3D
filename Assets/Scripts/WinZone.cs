using UnityEngine;
using UnityEngine.SceneManagement;

public class WinZone : MonoBehaviour
{
    private bool ganadado = false;

    public void JugadorTocoPlataforma()
    {
        if (ganadado) return;

        ganadado = true;
        Debug.Log("¡Nivel Completado!");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            JugadorTocoPlataforma();
        }
    }

    // Añadir también OnTriggerEnter por si acaso el collider es Trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            JugadorTocoPlataforma();
        }
    }
}
