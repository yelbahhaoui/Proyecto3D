using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 6f;
    public float velocidadCorrer = 10f;
    public float aceleracion = 10f;
    public float desaceleracion = 10f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public float gravedad = 30f;
    public float gravedadCaida = 40f;
    public int saltosMaximos = 2;
    
    [Header("Detección de Suelo")]
    public LayerMask capaSuelo;
    public LayerMask capaMuerte;

    [Header("Rotación")]
    public float velocidadRotacion = 10f;

    [Header("Animaciones")]
    public Animator animator;
    public float suavizadoAnimacion = 0.1f;

    [Header("Iluminación")]
    public bool usarLuzPropia = true;
    public Color colorLuz = new Color(1f, 0.95f, 0.8f);
    public float rangoLuz = 15f;
    public float intensidadLuz = 1.5f;

    [Header("Audio")]
    public AudioClip sonidoPincho;
    public AudioClip sonidoTrampolin;
    private AudioSource audioSource;

    private CharacterController controller;
    private Transform cameraTransform;
    
    private Vector3 velocidad;
    private Vector3 velocidadMovimiento;
    private Vector3 velocidadEmpuje;
    private bool enSuelo;
    private int saltosRealizados = 0;
    
    private float velocidadActualAnimacion;
    private float velocidadAnimacionTarget;
    private float tiempoCayendo = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (controller == null)
        {
            enabled = false;
            return;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (usarLuzPropia)
        {
            if (GetComponentInChildren<Light>() == null)
            {
                GameObject lightObj = new GameObject("LuzJugador");
                lightObj.transform.parent = transform;
                lightObj.transform.localPosition = Vector3.up * 10f;
                
                Light lightComp = lightObj.AddComponent<Light>();
                lightComp.type = LightType.Point;
                lightComp.range = rangoLuz;
                lightComp.intensity = intensidadLuz;
                lightComp.color = colorLuz;
                lightComp.shadows = LightShadows.Soft;
                lightComp.renderMode = LightRenderMode.ForcePixel;
            }
        }
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (controller == null || !controller.enabled) return;
        
        DetectarSuelo();
        Mover();
        Saltar();
        ActualizarAnimaciones();
    }

    void DetectarSuelo()
    {
        enSuelo = controller.isGrounded;
        
        if (!enSuelo)
        {
            float radio = controller.radius * 0.9f;
            Vector3 origen = transform.position + Vector3.up * controller.radius;
            float distancia = controller.radius + 0.1f;

            if (Physics.SphereCast(origen, radio, Vector3.down, out RaycastHit hit, distancia, capaSuelo))
            {
                enSuelo = true;
            }
        }

        if (enSuelo)
        {
            saltosRealizados = 0;

            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.5f))
            {
                Trampoline trampolin = hit.collider.GetComponent<Trampoline>();
                if (trampolin != null)
                {
                    velocidad.y = trampolin.fuerzaRebote;
                    
                    if (sonidoTrampolin != null) audioSource.PlayOneShot(sonidoTrampolin);

                    if (animator != null) animator.SetTrigger("Jump");
                    
                    return;
                }
            }
            
            if (velocidad.y < 0)
            {
                velocidad.y = -2f;
            }
        }
    }

    void Mover()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        float velocidadObjetivo = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        if (inputDir.magnitude >= 0.1f)
        {
            float anguloObjetivo;
            if (cameraTransform != null)
            {
                anguloObjetivo = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            }
            else
            {
                anguloObjetivo = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            }
            
            Vector3 direccionMovimiento = Quaternion.Euler(0f, anguloObjetivo, 0f) * Vector3.forward;

            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMovimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);

            velocidadMovimiento = Vector3.Lerp(velocidadMovimiento, direccionMovimiento * velocidadObjetivo, aceleracion * Time.deltaTime);
        }
        else
        {
            velocidadMovimiento = Vector3.Lerp(velocidadMovimiento, Vector3.zero, desaceleracion * Time.deltaTime);
        }

        if (!enSuelo)
        {
            float gravedadActual = velocidad.y > 0 ? gravedad : gravedadCaida;
            velocidad.y -= gravedadActual * Time.deltaTime;

            velocidad.y = Mathf.Max(velocidad.y, -40f);
        }
        else
        {
            if (velocidad.y < 0)
            {
                velocidad.y = -5f;
            }
        }

        Vector3 movimientoFinal = velocidadMovimiento + velocidad + velocidadEmpuje;
        controller.Move(movimientoFinal * Time.deltaTime);

        if (velocidadEmpuje.magnitude > 0.1f)
        {
            velocidadEmpuje = Vector3.Lerp(velocidadEmpuje, Vector3.zero, 5f * Time.deltaTime);
        }
        else
        {
            velocidadEmpuje = Vector3.zero;
        }

        if ((controller.collisionFlags & CollisionFlags.Above) != 0)
        {
            if (velocidad.y > 0)
            {
                velocidad.y = -2f;
            }
        }
    }

    void Saltar()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (enSuelo || saltosRealizados < saltosMaximos)
            {
                velocidad.y = fuerzaSalto;
                saltosRealizados++;
            }
        }

        if (Input.GetButtonUp("Jump") && velocidad.y > 0)
        {
            velocidad.y *= 0.5f;
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        float velocidadHorizontal = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        
        if (velocidadHorizontal > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                velocidadAnimacionTarget = 1f;
            }
            else
            {
                velocidadAnimacionTarget = 0.5f;
            }
        }
        else
        {
            velocidadAnimacionTarget = 0f;
        }

        velocidadActualAnimacion = Mathf.Lerp(velocidadActualAnimacion, velocidadAnimacionTarget, suavizadoAnimacion);

        animator.SetFloat("Speed", velocidadActualAnimacion);
        animator.SetFloat("VelocityX", velocidadHorizontal / velocidadCorrer);
        animator.SetBool("isGrounded", enSuelo);
        animator.SetFloat("VelocityY", velocidad.y);

        if (!enSuelo && velocidad.y < -5f)
        {
            tiempoCayendo += Time.deltaTime;
            if (tiempoCayendo > 0.8f)
            {
                animator.SetBool("isFalling", true);
            }
            else
            {
                animator.SetBool("isFalling", false);
            }
        }
        else
        {
            tiempoCayendo = 0f;
            animator.SetBool("isFalling", false);
        }

        if (Input.GetButtonDown("Jump") && (enSuelo || saltosRealizados < saltosMaximos))
        {
            animator.SetTrigger("Jump");
        }
    }

    public void AplicarEmpuje(Vector3 direccion, float fuerza)
    {
        direccion.Normalize();
        
        direccion.y = 0.5f; 
        
        velocidadEmpuje = direccion * fuerza;
        
        velocidad.y = 0;
        
        if (sonidoPincho != null) audioSource.PlayOneShot(sonidoPincho);

        if (animator != null) animator.SetTrigger("Jump");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = controller != null && controller.isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.2f);
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if ((capaMuerte.value & (1 << hit.gameObject.layer)) > 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }

        WinZone winZone = hit.gameObject.GetComponent<WinZone>();
        if (winZone != null)
        {
            winZone.JugadorTocoPlataforma();
        }

        Spike spike = hit.gameObject.GetComponent<Spike>();
        if (spike == null) 
        {
            spike = hit.gameObject.GetComponentInParent<Spike>();
        }

        if (spike != null)
        {
            spike.OnPlayerHit(gameObject);
        }
    }
}
