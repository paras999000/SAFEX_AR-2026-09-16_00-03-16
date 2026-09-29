using UnityEngine;

/// <summary>
/// SAFEX Procedural Underground Mine Generator
/// Designed for mobile AR:
/// - Modular primitive-based geometry
/// - Controlled object count
/// - No external GLB dependency
/// - Industrial mine visual language
/// - Fire/Gas training-ready
/// </summary>
public class MineGenerator : MonoBehaviour
{
    // =========================================================
    // MINE DIMENSIONS
    // =========================================================

    [Header("MINE DIMENSIONS")]
    public float width = 16f;
    public float length = 28f;
    public float height = 4f;

    [Header("ROOM & PILLAR")]
    public float pillarSize = 1.5f;
    public float pillarSpacingX = 4f;
    public float pillarSpacingZ = 5f;

    // =========================================================
    // DETAILS
    // =========================================================

    [Header("DETAILS")]
    public bool generateLights = true;
    public bool generateSigns = true;
    public bool generateVentilation = true;
    public bool generateVehicle = true;
    public bool generatePipes = true;
    public bool generateRockDetails = true;

    [Header("ADVANCED")]
    public bool generateRails = true;
    public bool generateConveyor = true;
    public bool generateTrainingZones = true;
    public bool generateElectricalEquipment = true;

    [Header("PERFORMANCE")]
    [Range(0, 100)]
    public int rockDetailCount = 28;

    [Range(3, 20)]
    public int lightCount = 10;

    // =========================================================
    // MATERIALS
    // =========================================================

    private Material rockMat;
    private Material darkRockMat;
    private Material rockEdgeMat;

    private Material floorMat;
    private Material floorEdgeMat;

    private Material steelMat;
    private Material darkSteelMat;
    private Material rustySteelMat;

    private Material yellowMat;
    private Material yellowBlackMat;
    private Material whiteMat;

    private Material redMat;
    private Material emergencyMat;
    private Material greenMat;
    private Material blueMat;

    private Material pipeRedMat;
    private Material pipeYellowMat;
    private Material pipeBlueMat;

    private Material glassMat;

    // =========================================================
    // ROOTS
    // =========================================================

    private Transform environmentRoot;
    private Transform structureRoot;
    private Transform equipmentRoot;
    private Transform safetyRoot;
    private Transform lightingRoot;

    // =========================================================
    // LIFECYCLE
    // =========================================================

    void Start()
    {
        GenerateMine();
    }

    // =========================================================
    // CONTEXT MENU
    // =========================================================

    [ContextMenu("GENERATE SAFEX MINE")]
    public void GenerateMine()
    {
        ClearGeneratedMine();

        CreateMaterials();
        CreateRoots();

        // Environment
        CreateFloor();
        CreateRockWalls();
        CreateRockCeiling();

        // Structural
        CreatePillars();
        CreateTunnelFrames();

        // Transportation
        if (generateRails)
            CreateRailSystem();

        CreateMainHaulageRoad();
        CreateSideRoads();
        CreateRoadMarkings();

        // Industrial equipment
        if (generateConveyor)
            CreateConveyor();

        CreateBunker();

        if (generateVehicle)
            CreateLHD();

        // Infrastructure
        if (generateVentilation)
            CreateVentilationSystem();

        if (generatePipes)
            CreateIndustrialPipes();

        CreateCableInfrastructure();

        // Lighting
        if (generateLights)
            CreateIndustrialLights();

        // Safety
        if (generateSigns)
            CreateSafetySigns();

        CreateEscapeRoute();
        CreateEmergencyStation();
        CreateFireGate();
        CreateEntrance();

        // Training
        if (generateTrainingZones)
            CreateTrainingZones();

        if (generateElectricalEquipment)
        {
            CreateElectricalCabinet();
            CreateControlPanel();
        }

        // Environment detail
        if (generateRockDetails)
            CreateRockDetails();

        Debug.Log("SAFEX Mine generated successfully.");
    }

    // =========================================================
    // CLEAR
    // =========================================================

    void ClearGeneratedMine()
    {
        Transform[] children = GetComponentsInChildren<Transform>();

        for (int i = children.Length - 1; i >= 0; i--)
        {
            if (children[i] == transform)
                continue;

            DestroySafe(children[i].gameObject);
        }
    }

    void DestroySafe(GameObject obj)
    {
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    // =========================================================
    // ROOTS
    // =========================================================

    void CreateRoots()
    {
        environmentRoot = CreateEmpty("ENVIRONMENT");
        structureRoot = CreateEmpty("STRUCTURE");
        equipmentRoot = CreateEmpty("INDUSTRIAL EQUIPMENT");
        safetyRoot = CreateEmpty("SAFETY & TRAINING");
        lightingRoot = CreateEmpty("LIGHTING");
    }

    Transform CreateEmpty(string objectName)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        return obj.transform;
    }

    // =========================================================
    // MATERIALS
    // =========================================================

    void CreateMaterials()
    {
        rockMat = CreateMaterial(
            "Rock - Charcoal",
            new Color(0.075f, 0.08f, 0.085f),
            0f,
            0.92f
        );

        darkRockMat = CreateMaterial(
            "Rock - Deep",
            new Color(0.035f, 0.038f, 0.042f),
            0f,
            0.96f
        );

        rockEdgeMat = CreateMaterial(
            "Rock - Edge",
            new Color(0.12f, 0.115f, 0.105f),
            0f,
            0.9f
        );

        floorMat = CreateMaterial(
            "Mine Floor",
            new Color(0.115f, 0.12f, 0.125f),
            0f,
            0.88f
        );

        floorEdgeMat = CreateMaterial(
            "Floor Edge",
            new Color(0.07f, 0.075f, 0.08f),
            0f,
            0.95f
        );

        steelMat = CreateMaterial(
            "Galvanized Steel",
            new Color(0.28f, 0.30f, 0.31f),
            0.75f,
            0.35f
        );

        darkSteelMat = CreateMaterial(
            "Dark Steel",
            new Color(0.055f, 0.065f, 0.07f),
            0.7f,
            0.38f
        );

        rustySteelMat = CreateMaterial(
            "Aged Steel",
            new Color(0.19f, 0.13f, 0.095f),
            0.55f,
            0.55f
        );

        yellowMat = CreateMaterial(
            "Safety Yellow",
            new Color(0.95f, 0.55f, 0.015f),
            0.05f,
            0.38f
        );

        yellowBlackMat = CreateMaterial(
            "Hazard Yellow",
            new Color(0.9f, 0.48f, 0.01f),
            0.05f,
            0.4f
        );

        whiteMat = CreateMaterial(
            "Safety White",
            new Color(0.82f, 0.84f, 0.85f),
            0.05f,
            0.42f
        );

        redMat = CreateMaterial(
            "Emergency Red",
            new Color(0.72f, 0.025f, 0.018f),
            0.05f,
            0.4f
        );

        emergencyMat = CreateEmissionMaterial(
            "Emergency Glow",
            new Color(0.9f, 0.015f, 0.01f),
            new Color(1f, 0.02f, 0.01f) * 2.5f
        );

        greenMat = CreateMaterial(
            "Safety Green",
            new Color(0.03f, 0.55f, 0.16f),
            0.05f,
            0.4f
        );

        blueMat = CreateMaterial(
            "Industrial Blue",
            new Color(0.025f, 0.22f, 0.55f),
            0.2f,
            0.4f
        );

        pipeRedMat = CreateMaterial(
            "Fire Pipe",
            new Color(0.55f, 0.035f, 0.02f),
            0.35f,
            0.4f
        );

        pipeYellowMat = CreateMaterial(
            "Gas Pipe",
            new Color(0.75f, 0.48f, 0.01f),
            0.25f,
            0.42f
        );

        pipeBlueMat = CreateMaterial(
            "Water Pipe",
            new Color(0.025f, 0.22f, 0.55f),
            0.35f,
            0.4f
        );

        glassMat = CreateMaterial(
            "Equipment Glass",
            new Color(0.04f, 0.12f, 0.14f),
            0.3f,
            0.18f
        );
    }

    Material CreateMaterial(
        string materialName,
        Color color,
        float metallic,
        float smoothness
    )
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);

        mat.name = materialName;
        mat.color = color;

        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);

        return mat;
    }

    Material CreateEmissionMaterial(
        string materialName,
        Color color,
        Color emission
    )
    {
        Material mat = CreateMaterial(
            materialName,
            color,
            0f,
            0.3f
        );

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
        }

        return mat;
    }

    // =========================================================
    // FLOOR
    // =========================================================

    void CreateFloor()
    {
        CreateCube(
            "MINE FLOOR",
            new Vector3(0f, -0.15f, 0f),
            new Vector3(width, 0.3f, length),
            floorMat,
            environmentRoot
        );

        // Raised side floor shoulders
        CreateCube(
            "LEFT FLOOR SHOULDER",
            new Vector3(-width / 2f + 0.9f, 0.02f, 0f),
            new Vector3(1.5f, 0.12f, length),
            floorEdgeMat,
            environmentRoot
        );

        CreateCube(
            "RIGHT FLOOR SHOULDER",
            new Vector3(width / 2f - 0.9f, 0.02f, 0f),
            new Vector3(1.5f, 0.12f, length),
            floorEdgeMat,
            environmentRoot
        );

        // Drainage channels
        CreateCube(
            "LEFT DRAINAGE CHANNEL",
            new Vector3(-width / 2f + 2.0f, 0.06f, 0f),
            new Vector3(0.22f, 0.08f, length - 1f),
            darkSteelMat,
            environmentRoot
        );

        CreateCube(
            "RIGHT DRAINAGE CHANNEL",
            new Vector3(width / 2f - 2.0f, 0.06f, 0f),
            new Vector3(0.22f, 0.08f, length - 1f),
            darkSteelMat,
            environmentRoot
        );
    }

    // =========================================================
    // ROCK WALLS
    // =========================================================

    void CreateRockWalls()
    {
        float wallX = width / 2f - 0.18f;

        CreateCube(
            "LEFT ROCK WALL",
            new Vector3(-wallX, height / 2f - 0.05f, 0f),
            new Vector3(0.36f, height - 0.2f, length),
            rockMat,
            environmentRoot
        );

        CreateCube(
            "RIGHT ROCK WALL",
            new Vector3(wallX, height / 2f - 0.05f, 0f),
            new Vector3(0.36f, height - 0.2f, length),
            rockMat,
            environmentRoot
        );

        // Rock strata
        for (float z = -length / 2f + 1.5f;
             z <= length / 2f - 1.5f;
             z += 2.5f)
        {
            float leftOffset = Mathf.Sin(z * 1.7f) * 0.07f;
            float rightOffset = Mathf.Cos(z * 1.35f) * 0.07f;

            CreateCube(
                "LEFT ROCK STRATA",
                new Vector3(-wallX + 0.025f, 0.8f + leftOffset, z),
                new Vector3(0.05f, 0.035f, 1.5f),
                rockEdgeMat,
                environmentRoot
            );

            CreateCube(
                "RIGHT ROCK STRATA",
                new Vector3(wallX - 0.025f, 1.35f + rightOffset, z),
                new Vector3(0.05f, 0.035f, 1.2f),
                rockEdgeMat,
                environmentRoot
            );
        }

        // Side rock ribs
        for (float z = -length / 2f + 1.5f;
             z <= length / 2f - 1.5f;
             z += 4f)
        {
            CreateCube(
                "LEFT ROCK RIB",
                new Vector3(-wallX + 0.08f, 1.9f, z),
                new Vector3(0.16f, 3.0f, 0.55f),
                darkRockMat,
                environmentRoot
            );

            CreateCube(
                "RIGHT ROCK RIB",
                new Vector3(wallX - 0.08f, 1.9f, z),
                new Vector3(0.16f, 3.0f, 0.55f),
                darkRockMat,
                environmentRoot
            );
        }
    }

    // =========================================================
    // ROCK CEILING
    // =========================================================

    void CreateRockCeiling()
    {
        CreateCube(
            "ROCK CEILING",
            new Vector3(0f, height + 0.05f, 0f),
            new Vector3(width, 0.35f, length),
            darkRockMat,
            environmentRoot
        );

        // Ceiling rock strips
        for (float z = -length / 2f + 1.5f;
             z <= length / 2f - 1.5f;
             z += 3.2f)
        {
            float x = Mathf.Sin(z * 0.8f) * 2.2f;

            CreateCube(
                "CEILING ROCK STRIP",
                new Vector3(x, height - 0.12f, z),
                new Vector3(4.2f, 0.18f, 0.35f),
                rockEdgeMat,
                environmentRoot
            );
        }
    }

    // =========================================================
    // PILLARS
    // =========================================================

    void CreatePillars()
    {
        for (
            float x = -width / 2f + 2.5f;
            x <= width / 2f - 2.5f;
            x += pillarSpacingX
        )
        {
            for (
                float z = -length / 2f + 4f;
                z <= length / 2f - 4f;
                z += pillarSpacingZ
            )
            {
                if (Mathf.Abs(x) < 2.4f)
                    continue;

                CreateCube(
                    "ROCK SUPPORT PILLAR",
                    new Vector3(x, height / 2f, z),
                    new Vector3(
                        pillarSize,
                        height,
                        pillarSize
                    ),
                    rockMat,
                    structureRoot
                );

                // Steel collar
                CreateCube(
                    "PILLAR STEEL COLLAR",
                    new Vector3(
                        x,
                        height - 0.35f,
                        z
                    ),
                    new Vector3(
                        pillarSize + 0.18f,
                        0.22f,
                        pillarSize + 0.18f
                    ),
                    steelMat,
                    structureRoot
                );

                // Hazard stripe
                CreateHazardBand(
                    new Vector3(
                        x,
                        0.55f,
                        z
                    ),
                    new Vector3(
                        pillarSize + 0.04f,
                        0.14f,
                        pillarSize + 0.04f
                    ),
                    structureRoot
                );
            }
        }
    }

    // =========================================================
    // ARCH SUPPORTS
    // =========================================================

    void CreateTunnelFrames()
    {
        float spacing = 2.6f;

        for (
            float z = -length / 2f + 1.4f;
            z <= length / 2f - 1.4f;
            z += spacing
        )
        {
            float halfSpan = 2.25f;

            // Left vertical
            CreateCube(
                "STEEL ARCH LEFT",
                new Vector3(-halfSpan, 1.72f, z),
                new Vector3(0.20f, 3.35f, 0.20f),
                darkSteelMat,
                structureRoot
            );

            // Right vertical
            CreateCube(
                "STEEL ARCH RIGHT",
                new Vector3(halfSpan, 1.72f, z),
                new Vector3(0.20f, 3.35f, 0.20f),
                darkSteelMat,
                structureRoot
            );

            // Angled shoulders
            CreateBeam(
                "ARCH LEFT SHOULDER",
                new Vector3(-1.85f, 3.18f, z),
                new Vector3(-0.75f, 3.72f, z),
                0.20f,
                darkSteelMat,
                structureRoot
            );

            CreateBeam(
                "ARCH RIGHT SHOULDER",
                new Vector3(1.85f, 3.18f, z),
                new Vector3(0.75f, 3.72f, z),
                0.20f,
                darkSteelMat,
                structureRoot
            );

            // Top beam
            CreateCube(
                "ARCH TOP",
                new Vector3(0f, 3.72f, z),
                new Vector3(1.55f, 0.20f, 0.20f),
                darkSteelMat,
                structureRoot
            );

            // Yellow inspection marker
            CreateCube(
                "ARCH SAFETY MARKER",
                new Vector3(-2.02f, 2.65f, z),
                new Vector3(0.24f, 0.55f, 0.24f),
                yellowMat,
                structureRoot
            );
        }
    }

    // =========================================================
    // MAIN ROAD
    // =========================================================

    void CreateMainHaulageRoad()
    {
        CreateCube(
            "MAIN HAULAGE ROAD",
            new Vector3(0f, 0.035f, 0f),
            new Vector3(3.8f, 0.07f, length),
            floorMat,
            environmentRoot
        );

        // Center reflectors
        for (
            float z = -length / 2f + 1f;
            z <= length / 2f - 1f;
            z += 2f
        )
        {
            CreateCube(
                "ROAD REFLECTOR",
                new Vector3(0f, 0.09f, z),
                new Vector3(0.08f, 0.035f, 0.42f),
                yellowMat,
                safetyRoot
            );
        }
    }

    // =========================================================
    // SIDE ROADS
    // =========================================================

    void CreateSideRoads()
    {
        CreateCube(
            "LEFT PRODUCTION ROAD",
            new Vector3(-5f, 0.035f, 5.5f),
            new Vector3(6.2f, 0.07f, 2.5f),
            floorMat,
            environmentRoot
        );

        CreateCube(
            "RIGHT PRODUCTION ROAD",
            new Vector3(5f, 0.035f, -5.5f),
            new Vector3(6.2f, 0.07f, 2.5f),
            floorMat,
            environmentRoot
        );
    }

    // =========================================================
    // ROAD MARKINGS
    // =========================================================

    void CreateRoadMarkings()
    {
        for (
            float z = -length / 2f + 1f;
            z <= length / 2f - 1f;
            z += 1.8f
        )
        {
            CreateCube(
                "LEFT ROAD MARKING",
                new Vector3(-1.55f, 0.1f, z),
                new Vector3(0.10f, 0.035f, 0.75f),
                yellowMat,
                safetyRoot
            );

            CreateCube(
                "RIGHT ROAD MARKING",
                new Vector3(1.55f, 0.1f, z),
                new Vector3(0.10f, 0.035f, 0.75f),
                yellowMat,
                safetyRoot
            );
        }
    }

    // =========================================================
    // RAIL SYSTEM
    // =========================================================

    void CreateRailSystem()
    {
        float railX = 0.62f;

        CreateCube(
            "LEFT RAIL",
            new Vector3(-railX, 0.16f, 0f),
            new Vector3(0.10f, 0.12f, length - 1f),
            steelMat,
            equipmentRoot
        );

        CreateCube(
            "RIGHT RAIL",
            new Vector3(railX, 0.16f, 0f),
            new Vector3(0.10f, 0.12f, length - 1f),
            steelMat,
            equipmentRoot
        );

        // Sleepers
        for (
            float z = -length / 2f + 1f;
            z <= length / 2f - 1f;
            z += 1.2f
        )
        {
            CreateCube(
                "RAIL SLEEPER",
                new Vector3(0f, 0.08f, z),
                new Vector3(1.75f, 0.10f, 0.18f),
                rustySteelMat,
                equipmentRoot
            );
        }
    }

    // =========================================================
    // CONVEYOR
    // =========================================================

    void CreateConveyor()
    {
        float x = -5f;

        CreateCube(
            "CONVEYOR BELT",
            new Vector3(x, 0.82f, 3f),
            new Vector3(1.35f, 0.20f, 13f),
            darkSteelMat,
            equipmentRoot
        );

        for (int i = -6; i <= 6; i++)
        {
            float z = 3f + i;

            CreateCylinder(
                "CONVEYOR ROLLER",
                new Vector3(x, 0.94f, z),
                new Vector3(0.30f, 0.30f, 1.48f),
                steelMat,
                90f,
                0f,
                0f,
                equipmentRoot
            );

            CreateCube(
                "CONVEYOR LEG",
                new Vector3(x, 0.38f, z),
                new Vector3(1.4f, 0.10f, 0.10f),
                steelMat,
                equipmentRoot
            );
        }

        CreateSign(
            "CONVEYOR",
            new Vector3(x, 1.75f, 9.5f),
            yellowMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // BUNKER
    // =========================================================

    void CreateBunker()
    {
        CreateCube(
            "COAL BUNKER BODY",
            new Vector3(5.0f, 1.25f, 7f),
            new Vector3(3f, 2.5f, 3.5f),
            darkRockMat,
            equipmentRoot
        );

        CreateCube(
            "BUNKER FRAME",
            new Vector3(5.0f, 2.45f, 5.25f),
            new Vector3(2.2f, 0.18f, 0.18f),
            steelMat,
            equipmentRoot
        );

        CreateCube(
            "BUNKER OPENING",
            new Vector3(5.0f, 0.85f, 5.20f),
            new Vector3(1.35f, 1.25f, 0.18f),
            darkSteelMat,
            equipmentRoot
        );

        CreateSign(
            "COAL BUNKER",
            new Vector3(5f, 2.9f, 5.15f),
            yellowMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // FIRE GATE
    // =========================================================

    void CreateFireGate()
    {
        float z = -length / 2f + 2.5f;

        CreateCube(
            "FIRE DOOR LEFT",
            new Vector3(-1.65f, 1.7f, z),
            new Vector3(0.20f, 3.2f, 0.28f),
            redMat,
            safetyRoot
        );

        CreateCube(
            "FIRE DOOR RIGHT",
            new Vector3(1.65f, 1.7f, z),
            new Vector3(0.20f, 3.2f, 0.28f),
            redMat,
            safetyRoot
        );

        CreateCube(
            "FIRE DOOR TOP",
            new Vector3(0f, 3.3f, z),
            new Vector3(3.5f, 0.20f, 0.28f),
            redMat,
            safetyRoot
        );

        CreateSign(
            "FIRE DOOR",
            new Vector3(0f, 2.75f, z - 0.18f),
            redMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // ENTRANCE
    // =========================================================

    void CreateEntrance()
    {
        float z = -length / 2f;

        CreateCube(
            "ENTRANCE LEFT SUPPORT",
            new Vector3(-3f, 1.8f, z),
            new Vector3(0.35f, 3.6f, 0.5f),
            steelMat,
            structureRoot
        );

        CreateCube(
            "ENTRANCE RIGHT SUPPORT",
            new Vector3(3f, 1.8f, z),
            new Vector3(0.35f, 3.6f, 0.5f),
            steelMat,
            structureRoot
        );

        CreateCube(
            "ENTRANCE TOP BEAM",
            new Vector3(0f, 3.65f, z),
            new Vector3(6.4f, 0.35f, 0.5f),
            yellowMat,
            structureRoot
        );

        CreateSign(
            "UNDERGROUND MINE",
            new Vector3(0f, 3.55f, z - 0.3f),
            darkSteelMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // VENTILATION
    // =========================================================

    void CreateVentilationSystem()
    {
        float x = 5.8f;

        // Main duct
        CreateCylinder(
            "MAIN VENTILATION DUCT",
            new Vector3(x, 3.15f, 0f),
            new Vector3(0.58f, 0.58f, length / 2f - 1f),
            darkSteelMat,
            90f,
            0f,
            0f,
            equipmentRoot
        );

        for (
            float z = -length / 2f + 2f;
            z <= length / 2f - 2f;
            z += 4f
        )
        {
            CreateCube(
                "VENT DUCT SUPPORT",
                new Vector3(x, 2.55f, z),
                new Vector3(1.15f, 0.10f, 0.12f),
                steelMat,
                equipmentRoot
            );

            CreateArrow(
                new Vector3(x - 0.05f, 3.15f, z),
                equipmentRoot
            );
        }

        CreateSign(
            "VENTILATION",
            new Vector3(x - 0.1f, 2.35f, -2f),
            greenMat,
            safetyRoot,
            90f
        );
    }

    // =========================================================
    // LIGHTING
    // =========================================================

    void CreateIndustrialLights()
    {
        int count = Mathf.Clamp(
            lightCount,
            3,
            20
        );

        for (int i = 0; i < count; i++)
        {
            float t = count == 1
                ? 0.5f
                : (float)i / (count - 1);

            float z =
                Mathf.Lerp(
                    -length / 2f + 2f,
                    length / 2f - 2f,
                    t
                );

            // Fixture
            CreateCube(
                "INDUSTRIAL LIGHT FIXTURE",
                new Vector3(0f, 3.72f, z),
                new Vector3(0.65f, 0.08f, 0.18f),
                darkSteelMat,
                lightingRoot
            );

            CreateCube(
                "LIGHT EMITTER",
                new Vector3(0f, 3.61f, z),
                new Vector3(0.40f, 0.05f, 0.10f),
                whiteMat,
                lightingRoot
            );

            GameObject lightObj =
                new GameObject("MINE LIGHT");

            lightObj.transform.SetParent(
                lightingRoot
            );

            lightObj.transform.localPosition =
                new Vector3(0f, 3.45f, z);

            Light light =
                lightObj.AddComponent<Light>();

            light.type = LightType.Point;
            light.range = 4.5f;
            light.intensity = 1.0f;
            light.color =
                new Color(
                    1f,
                    0.76f,
                    0.55f
                );

            light.shadows = LightShadows.None;
        }

        // Emergency lamps near training zones
        CreateEmergencyLight(
            new Vector3(-5f, 2.6f, -4f)
        );

        CreateEmergencyLight(
            new Vector3(5f, 2.6f, -1f)
        );
    }

    void CreateEmergencyLight(Vector3 position)
    {
        CreateCube(
            "EMERGENCY LIGHT",
            position,
            new Vector3(0.22f, 0.22f, 0.22f),
            emergencyMat,
            lightingRoot
        );

        GameObject lightObj =
            new GameObject("EMERGENCY LIGHT SOURCE");

        lightObj.transform.SetParent(
            lightingRoot
        );

        lightObj.transform.localPosition =
            position;

        Light light =
            lightObj.AddComponent<Light>();

        light.type = LightType.Point;
        light.range = 2.5f;
        light.intensity = 0.8f;
        light.color = Color.red;
        light.shadows = LightShadows.None;
    }

    // =========================================================
    // SAFETY SIGNS
    // =========================================================

    void CreateSafetySigns()
    {
        CreateSign(
            "NO SMOKING",
            new Vector3(-2.4f, 2.35f, 5f),
            redMat,
            safetyRoot,
            0f
        );

        CreateSign(
            "WEAR PPE",
            new Vector3(2.4f, 2.35f, 1f),
            yellowMat,
            safetyRoot,
            180f
        );

        CreateSign(
            "EMERGENCY EXIT",
            new Vector3(-2.4f, 2.35f, -4f),
            greenMat,
            safetyRoot,
            0f
        );

        CreateSign(
            "AUTHORIZED",
            new Vector3(2.4f, 2.35f, 7f),
            redMat,
            safetyRoot,
            180f
        );

        CreateSign(
            "FIRST AID",
            new Vector3(-6.5f, 1.9f, -6f),
            greenMat,
            safetyRoot,
            90f
        );
    }

    // =========================================================
    // ESCAPE ROUTE
    // =========================================================

    void CreateEscapeRoute()
    {
        for (int i = -9; i <= 9; i++)
        {
            float z = i * 1.3f;

            CreateCube(
                "ESCAPE ROUTE MARKER",
                new Vector3(
                    1.15f,
                    0.12f,
                    z
                ),
                new Vector3(
                    0.45f,
                    0.05f,
                    0.65f
                ),
                greenMat,
                safetyRoot
            );
        }

        CreateSign(
            "ESCAPE ROUTE",
            new Vector3(1.15f, 2.25f, 9f),
            greenMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // EMERGENCY STATION
    // =========================================================

    void CreateEmergencyStation()
    {
        Vector3 basePos =
            new Vector3(
                -2.8f,
                0f,
                -5.5f
            );

        CreateCube(
            "EMERGENCY CABINET",
            basePos + new Vector3(0f, 1.15f, 0f),
            new Vector3(1.15f, 2.3f, 0.45f),
            redMat,
            equipmentRoot
        );

        CreateCube(
            "CABINET GLASS",
            basePos + new Vector3(
                0f,
                1.35f,
                -0.24f
            ),
            new Vector3(
                0.70f,
                1.15f,
                0.04f
            ),
            glassMat,
            equipmentRoot
        );

        // Extinguisher
        CreateCylinder(
            "FIRE EXTINGUISHER",
            basePos + new Vector3(
                0f,
                1.15f,
                -0.35f
            ),
            new Vector3(
                0.18f,
                0.65f,
                0.18f
            ),
            redMat,
            0f,
            0f,
            0f,
            equipmentRoot
        );

        CreateSign(
            "EMERGENCY",
            basePos + new Vector3(
                0f,
                2.55f,
                -0.30f
            ),
            redMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // LHD
    // =========================================================

    void CreateLHD()
    {
        Vector3 p =
            new Vector3(
                0f,
                0f,
                7f
            );

        // Main chassis
        CreateCube(
            "LHD CHASSIS",
            p + new Vector3(0f, 0.62f, 0f),
            new Vector3(1.7f, 0.45f, 2.8f),
            yellowMat,
            equipmentRoot
        );

        // Rear engine block
        CreateCube(
            "LHD ENGINE HOUSING",
            p + new Vector3(0f, 1.0f, 0.65f),
            new Vector3(1.45f, 0.65f, 0.9f),
            yellowMat,
            equipmentRoot
        );

        // Cabin
        CreateCube(
            "LHD CABIN",
            p + new Vector3(
                0f,
                1.45f,
                0.55f
            ),
            new Vector3(
                1.15f,
                0.85f,
                1.0f
            ),
            darkSteelMat,
            equipmentRoot
        );

        // Cabin glass
        CreateCube(
            "LHD FRONT GLASS",
            p + new Vector3(
                0f,
                1.55f,
                0.02f
            ),
            new Vector3(
                0.82f,
                0.48f,
                0.04f
            ),
            glassMat,
            equipmentRoot
        );

        // Bucket
        CreateCube(
            "LHD FRONT BUCKET",
            p + new Vector3(
                0f,
                0.55f,
                -1.65f
            ),
            new Vector3(
                2.15f,
                0.72f,
                0.85f
            ),
            rustySteelMat,
            equipmentRoot
        );

        // Bucket arms
        CreateBeam(
            "LHD LEFT BUCKET ARM",
            p + new Vector3(
                -0.72f,
                0.72f,
                -0.55f
            ),
            p + new Vector3(
                -0.72f,
                0.62f,
                -1.65f
            ),
            0.16f,
            darkSteelMat,
            equipmentRoot
        );

        CreateBeam(
            "LHD RIGHT BUCKET ARM",
            p + new Vector3(
                0.72f,
                0.72f,
                -0.55f
            ),
            p + new Vector3(
                0.72f,
                0.62f,
                -1.65f
            ),
            0.16f,
            darkSteelMat,
            equipmentRoot
        );

        // Wheels
        CreateWheel(
            p + new Vector3(-0.95f, 0.45f, 0.75f)
        );

        CreateWheel(
            p + new Vector3(0.95f, 0.45f, 0.75f)
        );

        CreateWheel(
            p + new Vector3(-0.95f, 0.45f, -0.75f)
        );

        CreateWheel(
            p + new Vector3(0.95f, 0.45f, -0.75f)
        );

        CreateSign(
            "LHD",
            p + new Vector3(
                0f,
                2.1f,
                0.5f
            ),
            yellowMat,
            safetyRoot,
            0f
        );
    }

    void CreateWheel(Vector3 position)
    {
        CreateCylinder(
            "LHD WHEEL",
            position,
            new Vector3(
                0.65f,
                0.32f,
                0.65f
            ),
            darkSteelMat,
            90f,
            0f,
            0f,
            equipmentRoot
        );

        CreateCylinder(
            "LHD WHEEL HUB",
            position,
            new Vector3(
                0.25f,
                0.34f,
                0.25f
            ),
            steelMat,
            90f,
            0f,
            0f,
            equipmentRoot
        );
    }

    // =========================================================
    // INDUSTRIAL PIPES
    // =========================================================

    void CreateIndustrialPipes()
    {
        float pipeX = -6.5f;

        // Water
        CreateCylinder(
            "BLUE WATER PIPE",
            new Vector3(
                pipeX,
                2.65f,
                0f
            ),
            new Vector3(
                0.16f,
                0.16f,
                length / 2f - 1f
            ),
            pipeBlueMat,
            90f,
            0f,
            0f,
            equipmentRoot
        );

        // Fire suppression
        CreateCylinder(
            "RED FIRE PIPE",
            new Vector3(
                pipeX + 0.45f,
                2.35f,
                0f
            ),
            new Vector3(
                0.13f,
                0.13f,
                length / 2f - 1f
            ),
            pipeRedMat,
            90f,
            0f,
            0f,
            equipmentRoot
        );

        // Gas
        CreateCylinder(
            "YELLOW GAS PIPE",
            new Vector3(
                -pipeX,
                2.55f,
                0f
            ),
            new Vector3(
                0.14f,
                0.14f,
                length / 2f - 1f
            ),
            pipeYellowMat,
            90f,
            0f,
            0f,
            equipmentRoot
        );

        // Pipe supports
        for (
            float z = -length / 2f + 2f;
            z <= length / 2f - 2f;
            z += 3f
        )
        {
            CreateCube(
                "PIPE SUPPORT LEFT",
                new Vector3(
                    pipeX,
                    2.15f,
                    z
                ),
                new Vector3(
                    1.0f,
                    0.10f,
                    0.10f
                ),
                steelMat,
                equipmentRoot
            );

            CreateCube(
                "PIPE SUPPORT RIGHT",
                new Vector3(
                    -pipeX,
                    2.15f,
                    z
                ),
                new Vector3(
                    1.0f,
                    0.10f,
                    0.10f
                ),
                steelMat,
                equipmentRoot
            );
        }
    }

    // =========================================================
    // CABLE TRAY
    // =========================================================

    void CreateCableInfrastructure()
    {
        float x = 6.5f;

        CreateCube(
            "MAIN CABLE TRAY",
            new Vector3(
                x,
                2.25f,
                0f
            ),
            new Vector3(
                0.45f,
                0.22f,
                length - 2f
            ),
            darkSteelMat,
            equipmentRoot
        );

        for (
            float z = -length / 2f + 2f;
            z <= length / 2f - 2f;
            z += 2.5f
        )
        {
            CreateCube(
                "CABLE TRAY SUPPORT",
                new Vector3(
                    x,
                    1.95f,
                    z
                ),
                new Vector3(
                    0.75f,
                    0.10f,
                    0.10f
                ),
                steelMat,
                equipmentRoot
            );
        }

        // Cable lines
        for (int i = 0; i < 3; i++)
        {
            CreateCylinder(
                "ELECTRICAL CABLE",
                new Vector3(
                    x + (i - 1) * 0.11f,
                    2.40f,
                    0f
                ),
                new Vector3(
                    0.045f,
                    0.045f,
                    length / 2f - 1f
                ),
                darkSteelMat,
                90f,
                0f,
                0f,
                equipmentRoot
            );
        }
    }

    // =========================================================
    // SAFETY TRAINING ZONES
    // =========================================================

    void CreateTrainingZones()
    {
        CreateFireTrainingZone();
        CreateGasTrainingZone();
    }

    void CreateFireTrainingZone()
    {
        Vector3 center =
            new Vector3(
                -5f,
                0f,
                -4f
            );

        // Subtle boundary
        CreateCube(
            "FIRE ZONE BORDER",
            center + new Vector3(
                0f,
                0.075f,
                -1.6f
            ),
            new Vector3(
                3.2f,
                0.06f,
                0.06f
            ),
            redMat,
            safetyRoot
        );

        CreateCube(
            "FIRE ZONE BORDER",
            center + new Vector3(
                0f,
                0.075f,
                1.6f
            ),
            new Vector3(
                3.2f,
                0.06f,
                0.06f
            ),
            redMat,
            safetyRoot
        );

        // Electrical panel
        CreateCube(
            "FIRE ELECTRICAL PANEL",
            center + new Vector3(
                0f,
                1.25f,
                1.35f
            ),
            new Vector3(
                1.2f,
                2.3f,
                0.45f
            ),
            darkSteelMat,
            equipmentRoot
        );

        // Warning light
        CreateCube(
            "FIRE WARNING LIGHT",
            center + new Vector3(
                0f,
                2.55f,
                1.05f
            ),
            new Vector3(
                0.18f,
                0.18f,
                0.18f
            ),
            emergencyMat,
            safetyRoot
        );

        CreateSign(
            "FIRE TRAINING",
            center + new Vector3(
                0f,
                2.35f,
                1.0f
            ),
            redMat,
            safetyRoot,
            0f
        );
    }

    void CreateGasTrainingZone()
    {
        Vector3 center =
            new Vector3(
                5f,
                0f,
                -1f
            );

        // Subtle yellow boundary
        CreateCube(
            "GAS ZONE BORDER",
            center + new Vector3(
                0f,
                0.075f,
                -1.6f
            ),
            new Vector3(
                3.2f,
                0.06f,
                0.06f
            ),
            yellowMat,
            safetyRoot
        );

        CreateCube(
            "GAS ZONE BORDER",
            center + new Vector3(
                0f,
                0.075f,
                1.6f
            ),
            new Vector3(
                3.2f,
                0.06f,
                0.06f
            ),
            yellowMat,
            safetyRoot
        );

        // Gas detector
        CreateCube(
            "GAS DETECTOR",
            center + new Vector3(
                0f,
                1.0f,
                0f
            ),
            new Vector3(
                0.45f,
                0.8f,
                0.28f
            ),
            darkSteelMat,
            equipmentRoot
        );

        CreateCube(
            "GAS DETECTOR SCREEN",
            center + new Vector3(
                0f,
                1.15f,
                -0.15f
            ),
            new Vector3(
                0.25f,
                0.18f,
                0.04f
            ),
            blueMat,
            equipmentRoot
        );

        CreateSign(
            "GAS HAZARD",
            center + new Vector3(
                0f,
                2.3f,
                0.2f
            ),
            yellowMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // ELECTRICAL CABINET
    // =========================================================

    void CreateElectricalCabinet()
    {
        Vector3 p =
            new Vector3(
                -6.7f,
                1.2f,
                2f
            );

        CreateCube(
            "ELECTRICAL CABINET",
            p,
            new Vector3(
                1.1f,
                2.4f,
                0.55f
            ),
            darkSteelMat,
            equipmentRoot
        );

        CreateCube(
            "CABINET FRONT",
            p + new Vector3(
                0f,
                0f,
                -0.29f
            ),
            new Vector3(
                0.78f,
                1.2f,
                0.04f
            ),
            glassMat,
            equipmentRoot
        );

        CreateSign(
            "HIGH VOLTAGE",
            p + new Vector3(
                0f,
                1.45f,
                -0.34f
            ),
            yellowMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // CONTROL PANEL
    // =========================================================

    void CreateControlPanel()
    {
        Vector3 p =
            new Vector3(
                6.4f,
                1.1f,
                -7f
            );

        CreateCube(
            "VENT CONTROL PANEL",
            p,
            new Vector3(
                1.4f,
                2.2f,
                0.6f
            ),
            darkSteelMat,
            equipmentRoot
        );

        CreateCube(
            "CONTROL SCREEN",
            p + new Vector3(
                0f,
                0.30f,
                -0.34f
            ),
            new Vector3(
                0.75f,
                0.50f,
                0.05f
            ),
            blueMat,
            equipmentRoot
        );

        // Buttons
        CreateSphere(
            "GREEN CONTROL BUTTON",
            p + new Vector3(
                -0.25f,
                -0.45f,
                -0.35f
            ),
            new Vector3(
                0.12f,
                0.12f,
                0.12f
            ),
            greenMat,
            equipmentRoot
        );

        CreateSphere(
            "RED CONTROL BUTTON",
            p + new Vector3(
                0.25f,
                -0.45f,
                -0.35f
            ),
            new Vector3(
                0.12f,
                0.12f,
                0.12f
            ),
            redMat,
            equipmentRoot
        );

        CreateSign(
            "VENT CONTROL",
            p + new Vector3(
                0f,
                1.35f,
                -0.35f
            ),
            greenMat,
            safetyRoot,
            0f
        );
    }

    // =========================================================
    // ROCK DETAILS
    // =========================================================

    void CreateRockDetails()
    {
        Random.InitState(2026);

        int count =
            Mathf.Clamp(
                rockDetailCount,
                0,
                100
            );

        for (int i = 0; i < count; i++)
        {
            bool leftSide =
                Random.value > 0.5f;

            float x =
                leftSide
                    ? Random.Range(
                        -width / 2f + 0.6f,
                        -2.8f
                    )
                    : Random.Range(
                        2.8f,
                        width / 2f - 0.6f
                    );

            float z =
                Random.Range(
                    -length / 2f + 1f,
                    length / 2f - 1f
                );

            float y =
                Random.Range(
                    0.12f,
                    0.8f
                );

            float size =
                Random.Range(
                    0.12f,
                    0.38f
                );

            GameObject rock =
                CreateSphere(
                    "ROCK DEBRIS",
                    new Vector3(
                        x,
                        y,
                        z
                    ),
                    new Vector3(
                        size * 1.3f,
                        size,
                        size * 0.85f
                    ),
                    darkRockMat,
                    environmentRoot
                );

            rock.transform.localEulerAngles =
                new Vector3(
                    Random.Range(0f, 40f),
                    Random.Range(0f, 180f),
                    Random.Range(0f, 40f)
                );
        }
    }

    // =========================================================
    // HAZARD BAND
    // =========================================================

    void CreateHazardBand(
        Vector3 position,
        Vector3 scale,
        Transform parent
    )
    {
        CreateCube(
            "YELLOW BLACK HAZARD BAND",
            position,
            scale,
            yellowBlackMat,
            parent
        );

        // Dark stripe
        CreateCube(
            "HAZARD DARK STRIPE",
            position + new Vector3(
                0f,
                0.02f,
                0f
            ),
            new Vector3(
                scale.x * 0.75f,
                scale.y * 0.5f,
                scale.z * 0.75f
            ),
            darkSteelMat,
            parent
        );
    }

    // =========================================================
    // SIGN SYSTEM
    // =========================================================

    void CreateSign(
        string text,
        Vector3 position,
        Material boardMaterial,
        Transform parent,
        float rotationY
    )
    {
        GameObject board =
            CreateCube(
                "SIGN - " + text,
                position,
                new Vector3(
                    1.9f,
                    0.65f,
                    0.10f
                ),
                boardMaterial,
                parent
            );

        board.transform.localEulerAngles =
            new Vector3(
                0f,
                rotationY,
                0f
            );

        // Border
        CreateCube(
            "SIGN TOP BORDER",
            position + Quaternion.Euler(
                0f,
                rotationY,
                0f
            ) * new Vector3(
                0f,
                0.30f,
                -0.06f
            ),
            new Vector3(
                1.95f,
                0.05f,
                0.04f
            ),
            whiteMat,
            parent
        );

        // Text
        GameObject textObj =
            new GameObject(
                "SIGN TEXT - " + text
            );

        textObj.transform.SetParent(
            board.transform
        );

        textObj.transform.localPosition =
            new Vector3(
                0f,
                -0.02f,
                -0.065f
            );

        textObj.transform.localRotation =
            Quaternion.Euler(
                0f,
                180f,
                0f
            );

        TextMesh tm =
            textObj.AddComponent<TextMesh>();

        tm.text = text;
        tm.fontSize = 34;
        tm.characterSize = 0.035f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Color.white;
    }

    // =========================================================
    // AIRFLOW ARROW
    // =========================================================

    void CreateArrow(
        Vector3 position,
        Transform parent
    )
    {
        CreateCube(
            "AIRFLOW ARROW",
            position,
            new Vector3(
                0.40f,
                0.07f,
                0.70f
            ),
            greenMat,
            parent
        );

        CreateCube(
            "AIRFLOW ARROW HEAD",
            position + new Vector3(
                0f,
                0f,
                0.40f
            ),
            new Vector3(
                0.65f,
                0.07f,
                0.18f
            ),
            greenMat,
            parent
        );
    }

    // =========================================================
    // BEAM BETWEEN TWO POINTS
    // =========================================================

    void CreateBeam(
        string objectName,
        Vector3 start,
        Vector3 end,
        float thickness,
        Material material,
        Transform parent
    )
    {
        Vector3 direction =
            end - start;

        float length =
            direction.magnitude;

        Vector3 midpoint =
            (start + end) * 0.5f;

        GameObject beam =
            CreateCube(
                objectName,
                midpoint,
                new Vector3(
                    thickness,
                    length,
                    thickness
                ),
                material,
                parent
            );

        beam.transform.up =
            direction.normalized;
    }

    // =========================================================
    // CREATE CUBE
    // =========================================================

    GameObject CreateCube(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent
    )
    {
        GameObject obj =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        obj.name =
            objectName;

        obj.transform.SetParent(
            parent
        );

        obj.transform.localPosition =
            position;

        obj.transform.localScale =
            scale;

        Renderer renderer =
            obj.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial =
                material;

        return obj;
    }

    // =========================================================
    // CREATE SPHERE
    // =========================================================

    GameObject CreateSphere(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material,
        Transform parent
    )
    {
        GameObject obj =
            GameObject.CreatePrimitive(
                PrimitiveType.Sphere
            );

        obj.name =
            objectName;

        obj.transform.SetParent(
            parent
        );

        obj.transform.localPosition =
            position;

        obj.transform.localScale =
            scale;

        Renderer renderer =
            obj.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial =
                material;

        return obj;
    }

    // =========================================================
    // CREATE CYLINDER
    // =========================================================

    GameObject CreateCylinder(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material,
        float rotationX,
        float rotationY,
        float rotationZ,
        Transform parent
    )
    {
        GameObject obj =
            GameObject.CreatePrimitive(
                PrimitiveType.Cylinder
            );

        obj.name =
            objectName;

        obj.transform.SetParent(
            parent
        );

        obj.transform.localPosition =
            position;

        obj.transform.localScale =
            scale;

        obj.transform.localEulerAngles =
            new Vector3(
                rotationX,
                rotationY,
                rotationZ
            );

        Renderer renderer =
            obj.GetComponent<Renderer>();

        if (renderer != null)
            renderer.sharedMaterial =
                material;

        return obj;
    }
}