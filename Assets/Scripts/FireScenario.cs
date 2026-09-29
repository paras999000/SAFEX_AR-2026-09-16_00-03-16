using UnityEngine;

/// <summary>
/// SAFEX Fire Scenario
///
/// Creates the visual fire hazard and exposes a controlled
/// StopFire() method for the spatial training flow.
///
/// Training logic is handled by FireTrainingController.
/// </summary>
public class FireScenario : MonoBehaviour
{
    [Header("Fire Location")]
    public Vector3 firePosition = new Vector3(-5f, 0.8f, -4f);

    [Header("Fire Settings")]
    public bool fireActive = true;

    private GameObject fireObject;
    private GameObject smokeObject;
    private GameObject fireLightObject;
    private GameObject hazardZone;

    private Material fireMat;
    private Material smokeMat;
    private Material warningMat;

    private void Start()
    {
        CreateMaterials();
        CreateFireZone();

        if (fireActive)
            StartFire();
    }

    private void CreateMaterials()
    {
        fireMat = CreateMaterial(
            "Fire Glow",
            new Color(1f, 0.12f, 0.01f)
        );

        smokeMat = CreateMaterial(
            "Smoke",
            new Color(0.08f, 0.08f, 0.08f)
        );

        warningMat = CreateMaterial(
            "Fire Hazard",
            new Color(0.9f, 0.03f, 0.01f)
        );
    }

    private Material CreateMaterial(string materialName, Color color)
    {
        Material mat = new Material(
            Shader.Find("Universal Render Pipeline/Lit")
        );

        mat.name = materialName;
        mat.color = color;

        return mat;
    }

    // =========================================================
    // FIRE ZONE
    // =========================================================

    private void CreateFireZone()
    {
        hazardZone =
            GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        hazardZone.name = "FIRE HAZARD ZONE";
        hazardZone.transform.SetParent(transform);

        hazardZone.transform.localPosition =
            new Vector3(
                firePosition.x,
                0.08f,
                firePosition.z
            );

        hazardZone.transform.localScale =
            new Vector3(2.8f, 0.04f, 2.8f);

        Renderer renderer =
            hazardZone.GetComponent<Renderer>();

        renderer.material = warningMat;
    }

    // =========================================================
    // START FIRE
    // =========================================================

    private void StartFire()
    {
        CreateFire();
        CreateSmoke();
        CreateFireLight();

        Debug.Log("SAFEX FIRE SCENARIO STARTED");
    }

    // =========================================================
    // FIRE
    // =========================================================

    private void CreateFire()
    {
        fireObject =
            new GameObject("ACTIVE FIRE");

        fireObject.transform.SetParent(transform);
        fireObject.transform.localPosition = firePosition;

        ParticleSystem fire =
            fireObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = fire.main;

        main.maxParticles = 80;
        main.startLifetime = 0.6f;
        main.startSpeed = 1.2f;

        main.startSize =
            new ParticleSystem.MinMaxCurve(0.15f, 0.45f);

        main.startColor =
            new Color(1f, 0.18f, 0.01f, 1f);

        ParticleSystem.EmissionModule emission =
            fire.emission;

        emission.rateOverTime = 45f;

        ParticleSystem.ShapeModule shape =
            fire.shape;

        shape.shapeType =
            ParticleSystemShapeType.Cone;

        shape.angle = 20f;
        shape.radius = 0.5f;

        ParticleSystem.VelocityOverLifetimeModule velocity =
            fire.velocityOverLifetime;

        velocity.enabled = true;

        velocity.y =
            new ParticleSystem.MinMaxCurve(0.5f, 1.5f);

        fire.Play();
    }

    // =========================================================
    // SMOKE
    // =========================================================

    private void CreateSmoke()
    {
        smokeObject =
            new GameObject("FIRE SMOKE");

        smokeObject.transform.SetParent(transform);

        smokeObject.transform.localPosition =
            firePosition + new Vector3(0, 0.7f, 0);

        ParticleSystem smoke =
            smokeObject.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = smoke.main;

        main.maxParticles = 50;
        main.startLifetime = 3f;
        main.startSpeed = 0.4f;

        main.startSize =
            new ParticleSystem.MinMaxCurve(0.3f, 0.8f);

        main.startColor =
            new Color(0.08f, 0.08f, 0.08f, 0.55f);

        ParticleSystem.EmissionModule emission =
            smoke.emission;

        emission.rateOverTime = 10f;

        ParticleSystem.ShapeModule shape =
            smoke.shape;

        shape.shapeType =
            ParticleSystemShapeType.Sphere;

        shape.radius = 0.6f;

        ParticleSystem.VelocityOverLifetimeModule velocity =
            smoke.velocityOverLifetime;

        velocity.enabled = true;

        velocity.y =
            new ParticleSystem.MinMaxCurve(0.2f, 0.6f);

        smoke.Play();
    }

    // =========================================================
    // FIRE LIGHT
    // =========================================================

    private void CreateFireLight()
    {
        fireLightObject =
            new GameObject("FIRE LIGHT");

        fireLightObject.transform.SetParent(transform);

        fireLightObject.transform.localPosition =
            firePosition + new Vector3(0, 1f, 0);

        Light fireLight =
            fireLightObject.AddComponent<Light>();

        fireLight.type = LightType.Point;

        fireLight.color =
            new Color(1f, 0.08f, 0.01f);

        fireLight.range = 5f;
        fireLight.intensity = 3f;

        FireLightFlicker flicker =
            fireLightObject.AddComponent<FireLightFlicker>();

        flicker.targetLight = fireLight;
    }

    // =========================================================
    // TRAINING CONTROL
    // =========================================================

    /// <summary>
    /// Turns off the fire hazard after the trainee performs
    /// the required response.
    /// </summary>
    public void StopFire()
    {
        fireActive = false;

        if (fireObject != null)
            Destroy(fireObject);

        if (smokeObject != null)
            Destroy(smokeObject);

        if (fireLightObject != null)
            Destroy(fireLightObject);

        if (hazardZone != null)
            hazardZone.SetActive(false);

        Debug.Log("SAFEX FIRE SCENARIO: FIRE EXTINGUISHED");
    }

    public Vector3 GetFireWorldPosition()
    {
        return transform.TransformPoint(firePosition);
    }
}
