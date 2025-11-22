using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject plataformaPrefab; // El prefab de la plataforma a generar
    public int cantidadPlataformas = 20;
    public Transform puntoInicio; // Dónde empieza a generar (opcional)
    public bool generarAlInicio = true; // Controla si se genera al dar Play

    [Header("Meta Final")]
    public Material materialFinal; // Material para la última plataforma
    public Color colorFinal = Color.green; // Color alternativo si no hay material

    [Header("Trampolines")]
    [Range(0f, 1f)]
    public float probabilidadTrampolin = 0.2f; // 20% de probabilidad
    public float fuerzaRebote = 25f;
    public Material materialTrampolin;
    public Color colorTrampolin = Color.red;
    public float multiplicadorDistanciaSalto = 2.5f; // Cuánto más lejos estará la siguiente plataforma

    [Header("Distancias entre Plataformas")]
    public float minX = -2f; // Variación lateral mínima
    public float maxX = 2f;  // Variación lateral máxima
    
    public float minY = 1.5f; // Altura mínima (para asegurar que suba)
    public float maxY = 3f;   // Altura máxima
    
    public float minZ = 4f;   // Distancia hacia adelante mínima
    public float maxZ = 7f;   // Distancia hacia adelante máxima

    [Header("Rotación")]
    public bool rotacionAleatoria = false;
    public float rangoRotacionY = 15f;

    void Start()
    {
        if (generarAlInicio)
        {
            GenerarNivel();
        }
    }

    [ContextMenu("Generar Nivel Ahora")]
    public void GenerarNivel()
    {
        if (plataformaPrefab == null)
        {
            Debug.LogError("⚠️ Debes asignar un Prefab de plataforma en el LevelGenerator");
            return;
        }

        // Limpiar plataformas anteriores si existen (solo hijos directos)
        // Nota: Destruir objetos en editor requiere DestroyImmediate
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        Vector3 posicionActual = puntoInicio != null ? puntoInicio.position : transform.position;
        bool siguienteEsSaltoLargo = false;

        for (int i = 0; i < cantidadPlataformas; i++)
        {
            // Determinar si esta plataforma será un trampolín (para la SIGUIENTE distancia)
            // No poner trampolín en la última ni en la penúltima (para asegurar llegada a meta)
            bool esTrampolin = false;
            if (!siguienteEsSaltoLargo && i < cantidadPlataformas - 2)
            {
                if (Random.value < probabilidadTrampolin)
                {
                    esTrampolin = true;
                    siguienteEsSaltoLargo = true;
                }
            }
            else
            {
                siguienteEsSaltoLargo = false;
            }

            // Calcular distancias base
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            float z = Random.Range(minZ, maxZ);

            // Si la ANTERIOR fue trampolín, esta distancia debe ser mayor
            // (En este loop, 'siguienteEsSaltoLargo' se refiere a la distancia DESDE esta plataforma HACIA la siguiente)
            // Espera, la lógica es:
            // 1. Creo plataforma i.
            // 2. Si i es trampolín, la distancia a i+1 debe ser grande.
            // 3. Si i-1 fue trampolín, la distancia a i ya fue calculada grande.
            
            // Vamos a reestructurar un poco:
            // Calculamos la posición de ESTA plataforma basándonos en si la ANTERIOR era trampolín.
            // Pero ya tenemos 'posicionActual' que es donde debe ir esta.
            // Lo que cambia es cómo calculamos la posición de la SIGUIENTE (al final del loop).
            
            // Instanciar la plataforma en la posición actual
            GameObject nuevaPlataforma = Instantiate(plataformaPrefab, posicionActual, Quaternion.identity);
            nuevaPlataforma.transform.parent = transform;

            // Configurar Trampolín si toca
            if (esTrampolin)
            {
                // Añadir componente Trampoline
                Trampoline t = nuevaPlataforma.AddComponent<Trampoline>();
                t.fuerzaRebote = fuerzaRebote;

                // Cambiar visual
                Renderer rend = nuevaPlataforma.GetComponent<Renderer>();
                if (rend != null)
                {
                    if (materialTrampolin != null)
                        rend.material = materialTrampolin;
                    else
                        rend.material.color = colorTrampolin;
                }
                
                nuevaPlataforma.name = $"Plataforma_Trampolin_{i}";
            }
            else if (i == cantidadPlataformas - 1) // Última plataforma (Meta)
            {
                Renderer rend = nuevaPlataforma.GetComponent<Renderer>();
                if (rend != null)
                {
                    if (materialFinal != null)
                        rend.material = materialFinal;
                    else
                        rend.material.color = colorFinal;
                }
                nuevaPlataforma.AddComponent<WinZone>();
                nuevaPlataforma.name = "Plataforma_Final";
            }
            else
            {
                nuevaPlataforma.name = $"Plataforma_{i}";
            }

            // Rotación
            if (rotacionAleatoria)
            {
                nuevaPlataforma.transform.Rotate(Vector3.up, Random.Range(-rangoRotacionY, rangoRotacionY));
            }

            // Calcular posición para la SIGUIENTE plataforma
            float multiplicador = esTrampolin ? multiplicadorDistanciaSalto : 1f;
            
            // Si es trampolín, aumentamos mucho la Y y la Z
            Vector3 offset = new Vector3(x, y * multiplicador, z * multiplicador);
            posicionActual += offset;
        }
        
        Debug.Log($"✅ Se generaron {cantidadPlataformas} plataformas.");
    }
}
