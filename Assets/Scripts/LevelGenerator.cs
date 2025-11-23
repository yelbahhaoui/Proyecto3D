using UnityEngine;
using TMPro; // Necesario para TextMeshPro

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

    [Header("Plataformas Extendidas")]
    [Range(0f, 1f)]
    public float probabilidadExtendida = 0.15f;
    public GameObject escaleraPrefab;
    public float longitudExtendida = 3f; // Multiplicador de longitud
    public Vector3 offsetEscalera = new Vector3(0, 0.5f, 0.5f); // Ajuste de posición de la escalera relativo al final

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
            // Determinar si esta plataforma será un trampolín o extendida
            bool esTrampolin = false;
            bool esExtendida = false;

            // Solo si no es la última ni penúltima, y no venimos de un salto largo
            if (!siguienteEsSaltoLargo && i < cantidadPlataformas - 2)
            {
                float rnd = Random.value;
                if (rnd < probabilidadTrampolin)
                {
                    esTrampolin = true;
                    siguienteEsSaltoLargo = true;
                }
                else if (rnd < probabilidadTrampolin + probabilidadExtendida)
                {
                    esExtendida = true;
                    // Extendida no implica salto largo necesariamente, pero ajustaremos la posición
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

            // Instanciar la plataforma en la posición actual
            GameObject nuevaPlataforma = Instantiate(plataformaPrefab, posicionActual, Quaternion.identity);
            nuevaPlataforma.transform.parent = transform;

            // Configurar Trampolín
            if (esTrampolin)
            {
                Trampoline t = nuevaPlataforma.AddComponent<Trampoline>();
                t.fuerzaRebote = fuerzaRebote;

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
            // Configurar Extendida
            else if (esExtendida)
            {
                // Escalar la plataforma en Z
                Vector3 scale = nuevaPlataforma.transform.localScale;
                scale.z *= longitudExtendida;
                nuevaPlataforma.transform.localScale = scale;

                // Instanciar escalera al final
                if (escaleraPrefab != null)
                {
                    // Instanciar en el mundo primero
                    GameObject escalera = Instantiate(escaleraPrefab);
                    
                    // Calcular la posición del borde final en coordenadas mundiales
                    // Asumimos que el pivote está en el centro. 
                    // Borde Z = forward * (escalaZ * 0.5)
                    // Borde Y (arriba) = up * (escalaY * 0.5)
                    
                    float halfLength = nuevaPlataforma.transform.localScale.z * 0.5f;
                    float halfHeight = nuevaPlataforma.transform.localScale.y * 0.5f;

                    Vector3 posicionBorde = nuevaPlataforma.transform.position 
                                          + (nuevaPlataforma.transform.forward * halfLength) 
                                          + (nuevaPlataforma.transform.up * halfHeight);

                    // Colocar la escalera en ese borde
                    escalera.transform.position = posicionBorde;
                    escalera.transform.rotation = nuevaPlataforma.transform.rotation;

                    // Ahora emparentar
                    escalera.transform.SetParent(nuevaPlataforma.transform, true);

                    // Corregir la escala del hijo (al emparentar hereda la escala del padre, hay que invertirla)
                    Vector3 childScale = escalera.transform.localScale;
                    childScale.z /= longitudExtendida; 
                    escalera.transform.localScale = childScale;
                }
                else
                {
                    Debug.LogWarning($"⚠️ Plataforma {i} es extendida pero no hay 'Escalera Prefab' asignado.");
                }

                nuevaPlataforma.name = $"Plataforma_Extendida_{i}";
            }
            // Configurar Meta
            else if (i == cantidadPlataformas - 1) 
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

                // Añadir sistema de partículas
                GameObject particlesObj = new GameObject("WinParticles");
                particlesObj.transform.parent = nuevaPlataforma.transform;
                particlesObj.transform.localPosition = Vector3.up * 0.5f; 
                
                ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startColor = Color.yellow;
                main.startSize = 0.2f;
                main.startSpeed = 0.5f;
                main.startLifetime = 2f;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                
                var emission = ps.emission;
                emission.rateOverTime = 20f;
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 1f;

                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            }
            else
            {
                nuevaPlataforma.name = $"Plataforma_{i}";
            }

            // Rotación (solo si no es extendida para evitar problemas con la escalera)
            if (rotacionAleatoria && !esExtendida)
            {
                nuevaPlataforma.transform.Rotate(Vector3.up, Random.Range(-rangoRotacionY, rangoRotacionY));
            }

            // Calcular posición para la SIGUIENTE plataforma
            float multiplicador = esTrampolin ? multiplicadorDistanciaSalto : 1f;
            
            // Si es extendida, movemos el punto de origen hacia adelante para compensar la longitud extra
            if (esExtendida)
            {
                // Asumiendo que el pivote es central, avanzamos la mitad de la longitud extra
                // Longitud extra total = (longitudExtendida - 1) * escalaBase (asumimos 1)
                posicionActual += nuevaPlataforma.transform.forward * (longitudExtendida - 1) * 0.5f;
            }

            Vector3 offset = new Vector3(x, y * multiplicador, z * multiplicador);
            posicionActual += offset;
        }
        
        Debug.Log($"✅ Se generaron {cantidadPlataformas} plataformas.");
    }
}
