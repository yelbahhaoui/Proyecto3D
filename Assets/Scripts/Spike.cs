using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("Configuración")]
    public float fuerzaEmpuje = 15f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EmpujarJugador(other.gameObject);
        }
    }

    public void OnPlayerHit(GameObject player)
    {
        EmpujarJugador(player);
    }

    void EmpujarJugador(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            Vector3 direccionEmpuje = player.transform.position - transform.position;
            
            direccionEmpuje.y = 0;

            controller.AplicarEmpuje(direccionEmpuje, fuerzaEmpuje);
        }
    }
}
