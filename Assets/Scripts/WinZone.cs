using UnityEngine;

public class WinZone : MonoBehaviour
{
    private bool ganadado = false;

    void OnCollisionEnter(Collision collision)
    {
        if (ganadado) return;

        // Verificar si es el jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            ganadado = true;
            Debug.Log("¡Nivel Completado!");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelComplete();
            }
        }
    }
}
