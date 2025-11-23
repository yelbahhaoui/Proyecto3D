using UnityEngine;

public class CamaraTerceraPersona : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetivo;
    public Transform pivoteCamara;

    [Header("Posición de la Cámara")]
    public float distancia = 5f;
    public float altura = 2f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Header("Rotación")]
    public float sensibilidadRaton = 3f;
    public float limiteVerticalInferior = -30f;
    public float limiteVerticalSuperior = 60f;
    public float suavizadoRotacion = 10f;

    [Header("Colisión")]
    public bool evitarColisiones = true;
    public float radioColision = 0.3f;
    public LayerMask capasColision;

    [Header("Zoom (Opcional)")]
    public bool permitirZoom = true;
    public float distanciaMinima = 2f;
    public float distanciaMaxima = 10f;
    public float velocidadZoom = 2f;

    private float rotacionX = 0f;
    private float rotacionY = 0f;
    private float distanciaActual;
    private Vector3 posicionDeseada;

    void Start()
    {
        distanciaActual = distancia;

        if (pivoteCamara == null && objetivo != null)
        {
            GameObject pivoteObj = new GameObject("CameraPivot");
            pivoteCamara = pivoteObj.transform;
            pivoteCamara.SetParent(objetivo);
            pivoteCamara.localPosition = offset;
        }

        Vector3 angulos = transform.eulerAngles;
        rotacionX = angulos.y;
        rotacionY = angulos.x;
    }

    void LateUpdate()
    {
        if (objetivo == null || pivoteCamara == null) return;

        RotarCamara();
        CalcularPosicion();
        AplicarColisiones();
        AplicarZoom();
        
        transform.position = posicionDeseada;
        transform.LookAt(pivoteCamara.position);
    }

    void RotarCamara()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton;

        rotacionX += mouseX;

        rotacionY -= mouseY;
        rotacionY = Mathf.Clamp(rotacionY, limiteVerticalInferior, limiteVerticalSuperior);
    }

    void CalcularPosicion()
    {
        Quaternion rotacion = Quaternion.Euler(rotacionY, rotacionX, 0);
        Vector3 direccion = rotacion * Vector3.back;

        posicionDeseada = pivoteCamara.position + direccion * distanciaActual;
    }

    void AplicarColisiones()
    {
        if (!evitarColisiones) return;

        Vector3 direccionCamara = posicionDeseada - pivoteCamara.position;
        RaycastHit hit;

        if (Physics.SphereCast(pivoteCamara.position, radioColision, direccionCamara.normalized, 
            out hit, distanciaActual, capasColision))
        {
            posicionDeseada = pivoteCamara.position + direccionCamara.normalized * (hit.distance - radioColision);
        }
    }

    void AplicarZoom()
    {
        if (!permitirZoom) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (scroll != 0f)
        {
            distanciaActual -= scroll * velocidadZoom;
            distanciaActual = Mathf.Clamp(distanciaActual, distanciaMinima, distanciaMaxima);
        }
    }

    public void EstablecerRotacion(float horizontal, float vertical)
    {
        rotacionX = horizontal;
        rotacionY = Mathf.Clamp(vertical, limiteVerticalInferior, limiteVerticalSuperior);
    }

    public void ResetearDetrasJugador()
    {
        if (objetivo != null)
        {
            rotacionX = objetivo.eulerAngles.y;
            rotacionY = 15f;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (pivoteCamara != null && evitarColisiones)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pivoteCamara.position, radioColision);
            Gizmos.DrawLine(pivoteCamara.position, transform.position);
        }
    }
}
