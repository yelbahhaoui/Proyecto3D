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
    public float gravedadCaida = 40f; // Gravedad extra al caer para que se sienta mejor
    public int saltosMaximos = 2; // Para doble salto
    
    [Header("Detección de Suelo")]
    public LayerMask capaSuelo;
    public LayerMask capaMuerte; // Capa que mata al jugador (el terreno de abajo)

    [Header("Rotación")]
    public float velocidadRotacion = 10f;

    [Header("Animaciones")]
    public Animator animator;
    public float suavizadoAnimacion = 0.1f;

    // Referencias
    private CharacterController controller;
    private Transform cameraTransform;
    
    // Variables de movimiento
    private Vector3 velocidad;
    private Vector3 velocidadMovimiento;
    private bool enSuelo;
    private int saltosRealizados = 0;
    
    // Variables de animación
    private float velocidadActualAnimacion;
    private float velocidadAnimacionTarget;
    private float tiempoCayendo = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Validar que existe el Character Controller
        if (controller == null)
        {
            Debug.LogError("❌ No se encontró Character Controller en " + gameObject.name);
            enabled = false;
            return;
        }

        // Obtener Animator automáticamente si no está asignado
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("⚠️ No se encontró Animator. Las animaciones no funcionarán.");
            }
        }

        // Obtener referencia a la cámara
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró Main Camera. El movimiento puede no funcionar correctamente.");
        }

        // Bloquear y ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("✅ PlayerController inicializado correctamente");
    }

    void Update()
    {
        // Validar que el controller esté listo
        if (controller == null || !controller.enabled) return;
        
        DetectarSuelo();
        Mover();
        Saltar();
        ActualizarAnimaciones();
    }

    void DetectarSuelo()
    {
        // Detectar si está en el suelo usando el Character Controller
        enSuelo = controller.isGrounded;
        
        // También verificar con un raycast adicional para mayor precisión
        if (!enSuelo)
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
            enSuelo = Physics.Raycast(ray, 0.2f, capaSuelo);
        }

        // Resetear saltos cuando toca el suelo
        if (enSuelo)
        {
            saltosRealizados = 0;

            // Verificar si estamos sobre un trampolín
            RaycastHit hit;
            // Lanzamos un rayo un poco más largo para asegurar detección
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 0.5f))
            {
                Trampoline trampolin = hit.collider.GetComponent<Trampoline>();
                if (trampolin != null)
                {
                    // Aplicar fuerza de rebote
                    velocidad.y = trampolin.fuerzaRebote;
                    
                    // Opcional: Trigger animación de salto
                    if (animator != null) animator.SetTrigger("Jump");
                    
                    // Salimos para no aplicar la fuerza hacia abajo
                    return;
                }
            }
            
            // Si está cayendo (y no es trampolín), resetear la velocidad vertical
            if (velocidad.y < 0)
            {
                velocidad.y = -2f; // Pequeña fuerza hacia abajo para mantenerlo pegado
            }
        }
    }

    void Mover()
    {
        // Validar que la cámara existe
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Obtener input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        // Determinar velocidad (correr o caminar)
        float velocidadObjetivo = Input.GetKey(KeyCode.LeftShift) ? velocidadCorrer : velocidadCaminar;

        // Si hay input, mover en la dirección de la cámara
        if (inputDir.magnitude >= 0.1f)
        {
            // Calcular dirección basada en la cámara (si existe)
            float anguloObjetivo;
            if (cameraTransform != null)
            {
                anguloObjetivo = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            }
            else
            {
                // Si no hay cámara, usar la dirección directa del input
                anguloObjetivo = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            }
            
            Vector3 direccionMovimiento = Quaternion.Euler(0f, anguloObjetivo, 0f) * Vector3.forward;

            // Rotar el personaje suavemente hacia la dirección del movimiento
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMovimiento);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, velocidadRotacion * Time.deltaTime);

            // Acelerar el movimiento
            velocidadMovimiento = Vector3.Lerp(velocidadMovimiento, direccionMovimiento * velocidadObjetivo, aceleracion * Time.deltaTime);
        }
        else
        {
            // Desacelerar cuando no hay input
            velocidadMovimiento = Vector3.Lerp(velocidadMovimiento, Vector3.zero, desaceleracion * Time.deltaTime);
        }

        // Aplicar gravedad
        if (!enSuelo)
        {
            // Usar gravedad más fuerte al caer para mejor sensación
            float gravedadActual = velocidad.y > 0 ? gravedad : gravedadCaida;
            velocidad.y -= gravedadActual * Time.deltaTime;
        }
        else
        {
            // Cuando está en el suelo, mantener una pequeña fuerza hacia abajo
            if (velocidad.y < 0)
            {
                velocidad.y = -2f;
            }
        }

        // Combinar movimiento horizontal y vertical en un solo Move
        Vector3 movimientoFinal = velocidadMovimiento + velocidad;
        controller.Move(movimientoFinal * Time.deltaTime);
    }

    void Saltar()
    {
        // Saltar o doble salto
        if (Input.GetButtonDown("Jump"))
        {
            if (enSuelo || saltosRealizados < saltosMaximos)
            {
                velocidad.y = fuerzaSalto;
                saltosRealizados++;
            }
        }

        // Salto variable (presionar más tiempo = saltar más alto)
        if (Input.GetButtonUp("Jump") && velocidad.y > 0)
        {
            velocidad.y *= 0.5f;
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        // Calcular la velocidad de movimiento (0 = idle, 0.5 = walk, 1 = run)
        float velocidadHorizontal = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        
        // Determinar velocidad objetivo para la animación
        if (velocidadHorizontal > 0.1f)
        {
            // Si está corriendo (Shift presionado)
            if (Input.GetKey(KeyCode.LeftShift))
            {
                velocidadAnimacionTarget = 1f; // Running
            }
            else
            {
                velocidadAnimacionTarget = 0.5f; // Walking
            }
        }
        else
        {
            velocidadAnimacionTarget = 0f; // Idle
        }

        // Suavizar la transición de velocidad
        velocidadActualAnimacion = Mathf.Lerp(velocidadActualAnimacion, velocidadAnimacionTarget, suavizadoAnimacion);

        // Enviar parámetros al Animator
        // Asegúrate de que estos parámetros existen en tu Animator Controller
        animator.SetFloat("Speed", velocidadActualAnimacion);
        animator.SetFloat("VelocityX", velocidadHorizontal / velocidadCorrer); // Normalizado 0-1
        animator.SetBool("isGrounded", enSuelo);
        animator.SetFloat("VelocityY", velocidad.y);

        // Detectar si está cayendo
        if (!enSuelo && velocidad.y < -2f)
        {
            tiempoCayendo += Time.deltaTime;
            if (tiempoCayendo > 1.0f)
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

        // Trigger de salto (si quieres usar un trigger en lugar de bool)
        if (Input.GetButtonDown("Jump") && (enSuelo || saltosRealizados < saltosMaximos))
        {
            animator.SetTrigger("Jump");
        }
    }

    // Visualizar información de depuración en el editor
    void OnDrawGizmosSelected()
    {
        // Mostrar raycast de detección de suelo
        Gizmos.color = controller != null && controller.isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.2f);
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }

    // Detectar colisiones con el Character Controller
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Verificar si el objeto con el que chocamos está en la capa de muerte
        // Usamos bit shifting para comprobar la máscara
        if ((capaMuerte.value & (1 << hit.gameObject.layer)) > 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Debug.LogError("GameManager no encontrado en la escena");
            }
        }
    }
}
