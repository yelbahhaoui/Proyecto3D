using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Configuración")]
    public float fuerzaRebote = 20f;

    void OnCollisionEnter(Collision collision)
    {
        // Nota: CharacterController no siempre dispara OnCollisionEnter de forma fiable
        // Es mejor detectarlo desde el PlayerController
    }
}
