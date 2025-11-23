using UnityEngine;

public class SkyManager : MonoBehaviour
{
    [Header("Configuración del Cielo")]
    public Color colorCielo = new Color(0.05f, 0.05f, 0.1f); // Azul muy oscuro
    public Color colorHorizonte = new Color(0.1f, 0.1f, 0.2f);
    public Color colorSuelo = Color.black;
    
    [Header("Estrellas")]
    public int cantidadEstrellas = 1000;
    public float distanciaEstrellas = 100f;
    public float tamañoEstrellas = 0.2f; // Un poco más grandes para que se vean mejor
    [Range(0, 1)]
    public float porcentajeConLuz = 0.1f; // Porcentaje de estrellas que tienen luz real

    private GameObject starSystemObj;

    void Start()
    {
        ConfigurarIluminacion();
        CrearEstrellas();
    }

    void Update()
    {
        // Hacer que las estrellas sigan a la cámara para que siempre parezcan lejanas
        if (starSystemObj != null && Camera.main != null)
        {
            starSystemObj.transform.position = Camera.main.transform.position;
        }
    }

    void ConfigurarIluminacion()
    {
        // Configurar luz ambiental
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = colorCielo;
        RenderSettings.ambientEquatorColor = colorHorizonte;
        RenderSettings.ambientGroundColor = colorSuelo;

        // Configurar fondo de cámara
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = colorCielo;
        }
    }

    void CrearEstrellas()
    {
        if (starSystemObj != null) Destroy(starSystemObj);

        starSystemObj = new GameObject("SistemaEstrellas");
        starSystemObj.transform.parent = transform;
        
        ParticleSystem ps = starSystemObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = Mathf.Infinity;
        main.startSpeed = 0;
        main.startSize = tamañoEstrellas;
        main.maxParticles = cantidadEstrellas;
        main.startColor = Color.white;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.burstCount = 1;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, cantidadEstrellas) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = distanciaEstrellas;
        shape.radiusThickness = 0f; // Solo en la superficie de la esfera

        // Configurar Material Brillante
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        Material starMat = new Material(Shader.Find("Particles/Standard Unlit"));
        starMat.color = new Color(1f, 1f, 1f, 1f); // Blanco puro
        // Si tienes post-processing con Bloom, puedes aumentar la intensidad aquí:
        // starMat.SetColor("_Color", new Color(2f, 2f, 2f, 1f)); 
        renderer.material = starMat;

        // Configurar Luces (Lights Module)
        var lights = ps.lights;
        lights.enabled = true;
        lights.ratio = porcentajeConLuz; // No todas las estrellas necesitan luz real
        lights.maxLights = 50; // Límite para rendimiento

        // Crear plantilla de luz
        GameObject lightTemplate = new GameObject("StarLightTemplate");
        lightTemplate.transform.parent = starSystemObj.transform;
        Light lightComp = lightTemplate.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.range = 20f;
        lightComp.intensity = 2f;
        lightComp.color = new Color(1f, 1f, 0.8f); // Ligeramente amarillento
        lightTemplate.SetActive(false); // Ocultar plantilla

        lights.light = lightComp;
    }
}
