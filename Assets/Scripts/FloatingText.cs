using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            // Hacer que el texto mire a la cámara
            // Esta técnica asegura que el texto siempre esté orientado correctamente hacia el jugador
            transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
        }
    }
}
