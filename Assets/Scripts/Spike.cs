using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("Configuración")]
    public float fuerzaEmpuje = 15f;

    // Usamos OnControllerColliderHit en el PlayerController para detectar colisiones físicas,
    // pero si el pincho es un Trigger, usamos esto:
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EmpujarJugador(other.gameObject);
        }
    }

    // Esta función será llamada desde el PlayerController si chocamos físicamente
    public void OnPlayerHit(GameObject player)
    {
        EmpujarJugador(player);
    }

    void EmpujarJugador(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            // Calcular dirección: Desde el pincho HACIA el jugador
            Vector3 direccionEmpuje = player.transform.position - transform.position;
            
            // Ignorar altura para el cálculo de dirección horizontal
            direccionEmpuje.y = 0;

            controller.AplicarEmpuje(direccionEmpuje, fuerzaEmpuje);
            
            Debug.Log("¡Auch! Pinchazo.");
        }
    }
}
