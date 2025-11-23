using UnityEngine;
using UnityEngine.SceneManagement;

public class WinZone : MonoBehaviour
{
    private bool ganadado = false;

    public void JugadorTocoPlataforma()
    {
        if (ganadado) return;

        ganadado = true;
        
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            JugadorTocoPlataforma();
        }
    }
}
