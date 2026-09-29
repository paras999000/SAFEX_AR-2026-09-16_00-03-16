using UnityEngine;

public class MineAtmosphere : MonoBehaviour
{
    [Header("Atmosphere")]
    public bool enableFog = true;
    public bool enableDust = true;

    [Header("Dust")]
    public int dustParticles = 180;
    public float dustArea = 14f;
    public float dustHeight = 3.5f;

    [Header("Emergency Lights")]
    public bool createEmergencyLights = true;

    private Material dustMaterial;

    void Start()
    {
        SetupAtmosphere();

        if (enableDust)
            CreateDust();

        if (createEmergencyLights)
            CreateEmergencyLights();
    }

    // =========================================================
    // ATMOSPHERE
    // =========================================================

    void SetupAtmosphere()
    {
        if (!enableFog)
            return;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;

        RenderSettings.fogColor =
            new Color(0.035f, 0.04f, 0.045f);

        RenderSettings.fogDensity = 0.025f;
    }

    // =========================================================
    // DUST PARTICLES
    // =========================================================

    void CreateDust()
    {
        GameObject dust =
            new GameObject("UNDERGROUND DUST");

        dust.transform.SetParent(transform);

        dust.transform.localPosition =
            new Vector3(0, 2f, 0);

        ParticleSystem ps =
            dust.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main =
            ps.main;

        main.maxParticles = dustParticles;
        main.startLifetime = 8f;
        main.startSpeed = 0.08f;
        main.startSize =
            new ParticleSystem.MinMaxCurve(0.015f, 0.06f);

        main.startColor =
            new Color(0.55f, 0.5f, 0.42f, 0.22f);

        main.simulationSpace =
            ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission =
            ps.emission;

        emission.rateOverTime = 12f;

        ParticleSystem.ShapeModule shape =
            ps.shape;

        shape.shapeType =
            ParticleSystemShapeType.Box;

        shape.scale =
            new Vector3(
                dustArea,
                dustHeight,
                26f
            );

        ParticleSystem.VelocityOverLifetimeModule velocity =
            ps.velocityOverLifetime;

        velocity.enabled = true;

        velocity.x =
            new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);

        velocity.y =
            new ParticleSystem.MinMaxCurve(-0.01f, 0.025f);

        velocity.z =
            new ParticleSystem.MinMaxCurve(-0.03f, 0.03f);

        ps.Play();
    }

    // =========================================================
    // EMERGENCY LIGHTS
    // =========================================================

    void CreateEmergencyLights()
    {
        for (int i = -1; i <= 1; i++)
        {
            GameObject lightObject =
                new GameObject("EMERGENCY LIGHT " + i);

            lightObject.transform.SetParent(transform);

            lightObject.transform.localPosition =
                new Vector3(
                    0,
                    2.8f,
                    -8f + (i * 6f)
                );

            Light light =
                lightObject.AddComponent<Light>();

            light.type =
                LightType.Point;

            light.color =
                new Color(1f, 0.04f, 0.015f);

            light.range = 3.5f;
            light.intensity = 0.8f;

            EmergencyLightFlicker flicker =
                lightObject.AddComponent<EmergencyLightFlicker>();

            flicker.targetLight = light;
        }
    }
}