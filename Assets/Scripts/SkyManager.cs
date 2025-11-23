using UnityEngine;

public class SkyManager : MonoBehaviour
{
    [Header("Configuración del Cielo")]
    public Color colorCielo = new Color(0.05f, 0.05f, 0.1f);
    public Color colorHorizonte = new Color(0.1f, 0.1f, 0.2f);
    public Color colorSuelo = Color.black;
    
    [Header("Estrellas")]
    public int cantidadEstrellas = 1000;
    public float distanciaEstrellas = 100f;
    public float tamañoEstrellas = 0.2f;
    [Range(0, 1)]
    public float porcentajeConLuz = 0.1f;

    private GameObject starSystemObj;

    void Start()
    {
        ConfigurarIluminacion();
        CrearEstrellas();
    }

    void Update()
    {
        if (starSystemObj != null && Camera.main != null)
        {
            starSystemObj.transform.position = Camera.main.transform.position;
        }
    }

    void ConfigurarIluminacion()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = colorCielo;
        RenderSettings.ambientEquatorColor = colorHorizonte;
        RenderSettings.ambientGroundColor = colorSuelo;

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
        shape.radiusThickness = 0f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        Material starMat = new Material(Shader.Find("Particles/Standard Unlit"));
        starMat.color = new Color(1f, 1f, 1f, 1f);
        renderer.material = starMat;

        var lights = ps.lights;
        lights.enabled = true;
        lights.ratio = porcentajeConLuz;
        lights.maxLights = 50;

        GameObject lightTemplate = new GameObject("StarLightTemplate");
        lightTemplate.transform.parent = starSystemObj.transform;
        Light lightComp = lightTemplate.AddComponent<Light>();
        lightComp.type = LightType.Point;
        lightComp.range = 20f;
        lightComp.intensity = 2f;
        lightComp.color = new Color(1f, 1f, 0.8f);
        lightTemplate.SetActive(false);

        lights.light = lightComp;
    }
}
