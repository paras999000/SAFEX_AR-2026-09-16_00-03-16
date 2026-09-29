using UnityEngine;

/// <summary>
/// SAFEX Gas Leak Scenario
///
/// Creates the gas hazard and exposes a controlled ON/OFF state
/// for the spatial Gas & Confined Space training flow.
///
/// Training flow:
/// Gas detected
/// -> Raise alarm at gas detector
/// -> Move to isolation / ventilation control
/// -> Isolate contaminated area
/// -> Gas clears
/// -> Follow green evacuation route
/// -> Reach safe exit
/// -> Training complete
/// </summary>
public class GasLeakScenario : MonoBehaviour
{
    [Header("Gas Location")]
    public Vector3 gasPosition = new Vector3(5f, 0.8f, -4f);

    [Header("Gas Settings")]
    public bool gasActive = true;

    private GameObject gasObject;
    private GameObject gasLightObject;
    private GameObject hazardZone;

    private ParticleSystem gasParticles;
    private Light gasLight;

    private Material gasMaterial;
    private Material warningMaterial;

    public bool IsGasActive => gasActive;

    private void Start()
    {
        CreateMaterials();
        CreateGasZone();

        if (gasActive)
            StartGasLeak();
        else
            HideHazardVisuals();

        Debug.Log("SAFEX GAS LEAK SCENARIO READY");
    }

    // =========================================================
    // MATERIALS
    // =========================================================

    private void CreateMaterials()
    {
        gasMaterial = CreateMaterial(
            "Gas Warning",
            new Color(0.2f, 1f, 0.1f)
        );

        warningMaterial = CreateMaterial(
            "Gas Hazard",
            new Color(0.8f, 0.1f, 0.02f)
        );
    }

    private Material CreateMaterial(string materialName, Color color)
    {
        Shader shader =
            Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = materialName;
        mat.color = color;

        return mat;
    }

    // =========================================================
    // HAZARD ZONE
    // =========================================================

    private void CreateGasZone()
    {
        hazardZone =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder
            );

        hazardZone.name = "GAS HAZARD ZONE";
        hazardZone.transform.SetParent(transform);

        hazardZone.transform.localPosition =
            new Vector3(
                gasPosition.x,
                0.06f,
                gasPosition.z
            );

        hazardZone.transform.localScale =
            new Vector3(3f, 0.03f, 3f);

        Renderer renderer =
            hazardZone.GetComponent<Renderer>();

        if (renderer != null)
            renderer.material = warningMaterial;

        Collider collider =
            hazardZone.GetComponent<Collider>();

        if (collider != null)
            collider.enabled = false;
    }

    // =========================================================
    // START GAS
    // =========================================================

    public void StartGasLeak()
    {
        gasActive = true;

        CreateGasParticles();
        CreateGasLight();

        if (hazardZone != null)
            hazardZone.SetActive(true);

        Debug.Log("SAFEX: GAS LEAK ACTIVE");
    }

    // =========================================================
    // STOP GAS
    // =========================================================

    public void StopGasLeak()
    {
        if (!gasActive)
            return;

        gasActive = false;

        if (gasParticles != null)
        {
            gasParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        if (gasObject != null)
            gasObject.SetActive(false);

        if (gasLight != null)
            gasLight.enabled = false;

        if (gasLightObject != null)
            gasLightObject.SetActive(false);

        if (hazardZone != null)
            hazardZone.SetActive(false);

        Debug.Log("SAFEX: GAS LEAK ISOLATED");
    }

    // =========================================================
    // PARTICLES
    // =========================================================

    private void CreateGasParticles()
    {
        if (gasObject == null)
        {
            gasObject = new GameObject("TOXIC GAS");
            gasObject.transform.SetParent(transform);
            gasObject.transform.localPosition = gasPosition;

            gasParticles =
                gasObject.AddComponent<ParticleSystem>();

            var main = gasParticles.main;
            main.maxParticles = 100;
            main.startLifetime = 4f;
            main.startSpeed = 0.25f;

            main.startSize =
                new ParticleSystem.MinMaxCurve(
                    0.15f,
                    0.45f
                );

            main.startColor =
                new Color(
                    0.25f,
                    1f,
                    0.05f,
                    0.35f
                );

            var emission = gasParticles.emission;
            emission.rateOverTime = 18f;

            var shape = gasParticles.shape;
            shape.shapeType =
                ParticleSystemShapeType.Sphere;
            shape.radius = 0.8f;

            var velocity =
                gasParticles.velocityOverLifetime;

            velocity.enabled = true;
            velocity.y =
                new ParticleSystem.MinMaxCurve(
                    0.1f,
                    0.5f
                );
        }

        gasObject.SetActive(true);

        if (gasParticles != null &&
            !gasParticles.isPlaying)
        {
            gasParticles.Play();
        }
    }

    // =========================================================
    // WARNING LIGHT
    // =========================================================

    private void CreateGasLight()
    {
        if (gasLightObject == null)
        {
            gasLightObject =
                new GameObject(
                    "GAS WARNING LIGHT"
                );

            gasLightObject.transform.SetParent(
                transform
            );

            gasLightObject.transform.localPosition =
                gasPosition +
                new Vector3(0f, 1.5f, 0f);

            gasLight =
                gasLightObject.AddComponent<Light>();

            gasLight.type = LightType.Point;

            gasLight.color =
                new Color(0.2f, 1f, 0.05f);

            gasLight.range = 4f;
            gasLight.intensity = 2f;
            gasLight.shadows = LightShadows.None;
        }

        gasLightObject.SetActive(true);

        if (gasLight != null)
            gasLight.enabled = true;
    }

    private void HideHazardVisuals()
    {
        if (gasObject != null)
            gasObject.SetActive(false);

        if (gasLightObject != null)
            gasLightObject.SetActive(false);

        if (hazardZone != null)
            hazardZone.SetActive(false);
    }
}
