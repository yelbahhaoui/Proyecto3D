using UnityEngine;
using TMPro;

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject plataformaPrefab;
    public int cantidadPlataformas = 20;
    public Transform puntoInicio;
    public bool generarAlInicio = true;

    [Header("Meta Final")]
    public Material materialFinal;
    public Color colorFinal = Color.green;

    [Header("Trampolines")]
    [Range(0f, 1f)]
    public float probabilidadTrampolin = 0.2f;
    public float fuerzaRebote = 25f;
    public Material materialTrampolin;
    public Color colorTrampolin = Color.red;
    public float multiplicadorDistanciaSalto = 2.5f;

    [Header("Plataformas Extendidas")]
    [Range(0f, 1f)]
    public float probabilidadExtendida = 0.15f;
    public GameObject escaleraPrefab;
    public float longitudExtendida = 3f;
    public Vector3 offsetEscalera = new Vector3(0, 0.5f, 0.5f);

    [Header("Distancias entre Plataformas")]
    public float minX = -2f;
    public float maxX = 2f;
    
    public float minY = 1.5f;
    public float maxY = 3f;
    
    public float minZ = 4f;
    public float maxZ = 7f;

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
            return;
        }

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
            bool esTrampolin = false;
            bool esExtendida = false;

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
                }
            }
            else
            {
                siguienteEsSaltoLargo = false;
            }

            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            float z = Random.Range(minZ, maxZ);

            GameObject nuevaPlataforma = Instantiate(plataformaPrefab, posicionActual, Quaternion.identity);
            nuevaPlataforma.transform.parent = transform;

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
            else if (esExtendida)
            {
                Vector3 scale = nuevaPlataforma.transform.localScale;
                scale.z *= longitudExtendida;
                nuevaPlataforma.transform.localScale = scale;

                if (escaleraPrefab != null)
                {
                    GameObject escalera = Instantiate(escaleraPrefab);
                    
                    float halfLength = nuevaPlataforma.transform.localScale.z * 0.5f;
                    float halfHeight = nuevaPlataforma.transform.localScale.y * 0.5f;

                    Vector3 posicionBorde = nuevaPlataforma.transform.position 
                                          + (nuevaPlataforma.transform.forward * halfLength) 
                                          + (nuevaPlataforma.transform.up * halfHeight);

                    escalera.transform.position = posicionBorde;
                    escalera.transform.rotation = nuevaPlataforma.transform.rotation;

                    escalera.transform.SetParent(nuevaPlataforma.transform, true);

                    Vector3 childScale = escalera.transform.localScale;
                    childScale.z /= longitudExtendida; 
                    escalera.transform.localScale = childScale;
                }

                nuevaPlataforma.name = $"Plataforma_Extendida_{i}";
            }
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

            if (rotacionAleatoria && !esExtendida)
            {
                nuevaPlataforma.transform.Rotate(Vector3.up, Random.Range(-rangoRotacionY, rangoRotacionY));
            }

            float multiplicador = esTrampolin ? multiplicadorDistanciaSalto : 1f;
            
            if (esExtendida)
            {
                posicionActual += nuevaPlataforma.transform.forward * (longitudExtendida - 1) * 0.5f;
            }

            Vector3 offset = new Vector3(x, y * multiplicador, z * multiplicador);
            posicionActual += offset;
        }
    }
}
