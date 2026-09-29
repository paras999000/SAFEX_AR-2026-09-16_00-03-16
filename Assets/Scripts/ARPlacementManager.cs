using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    // =========================================================
    // REFERENCES
    // =========================================================

    [Header("AR References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject minePrefab;

    [Header("Flow")]
    [SerializeField] private SAFEXFlowController flowController;

    [Header("Home UI")]
    [SerializeField] private GameObject homeBackground;

    [Header("Placement")]
    [SerializeField] private float mineScale = 0.25f;

    // =========================================================
    // RUNTIME
    // =========================================================

    private GameObject spawnedMine;

    private Pose currentPose;

    private bool placementMode;
    private bool surfaceDetected;
    private bool minePlaced;

    private Canvas arCanvas;

    private Text statusText;
    private Text instructionText;
    private Button placeButton;

    private static readonly List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (raycastManager == null)
            raycastManager =
                GetComponent<ARRaycastManager>();

        if (planeManager == null)
            planeManager =
                GetComponent<ARPlaneManager>();

        if (flowController == null)
            flowController =
                FindFirstObjectByType<SAFEXFlowController>();
    }

    private void OnEnable()
    {
        if (flowController != null)
            flowController.StateChanged += OnStateChanged;
    }

    private void Start()
    {
        // AR placement must NOT start on application launch.
        DisableARPlacement();
    }

    private void OnDisable()
    {
        if (flowController != null)
            flowController.StateChanged -= OnStateChanged;
    }

    private void OnDestroy()
    {
        if (arCanvas != null)
            Destroy(arCanvas.gameObject);
    }

    private void Update()
    {
        if (!placementMode)
            return;

        if (minePlaced)
            return;

        DetectSurface();
    }

    // =========================================================
    // FLOW STATE
    // =========================================================

    private void OnStateChanged(
        SAFEXFlowController.SAFEXState state)
    {
        if (state ==
            SAFEXFlowController.SAFEXState.ARPlacement)
        {
            BeginPlacement();
        }
        else if (state ==
                 SAFEXFlowController.SAFEXState.Home)
        {
            DisableARPlacement();

            SetHomeBackground(true);
        }
        else
        {
            // Keep the cinematic Home background hidden
            // while moving through training/AR states.
            SetHomeBackground(false);
        }
    }

    // =========================================================
    // BEGIN AR PLACEMENT
    // =========================================================

    private void BeginPlacement()
    {
        placementMode = true;
        surfaceDetected = false;
        minePlaced = false;

        // Hide static Home background.
        SetHomeBackground(false);

        // Hide any runtime Home/Module UI.
        HideHomeRuntimeUI();

        // Enable AR systems.
        if (raycastManager != null)
            raycastManager.enabled = true;

        if (planeManager != null)
            planeManager.enabled = true;

        CreateARUI();

        SetStatus(
            "SCANNING",
            "Move your phone slowly to scan a flat surface."
        );

        if (placeButton != null)
            placeButton.gameObject.SetActive(false);

        Debug.Log(
            "SAFEX AR: Surface scanning started."
        );
    }

    // =========================================================
    // DISABLE AR PLACEMENT
    // =========================================================

    private void DisableARPlacement()
    {
        placementMode = false;
        surfaceDetected = false;

        if (raycastManager != null)
            raycastManager.enabled = false;

        if (planeManager != null)
            planeManager.enabled = false;

        if (arCanvas != null)
        {
            Destroy(arCanvas.gameObject);
            arCanvas = null;
        }
    }

    // =========================================================
    // SURFACE DETECTION
    // =========================================================

    private void DetectSurface()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch =
            Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        // Ignore touches on our UI buttons.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(
                touch.fingerId))
        {
            return;
        }

        if (raycastManager == null)
            return;

        bool hit =
            raycastManager.Raycast(
                touch.position,
                hits,
                TrackableType.PlaneWithinPolygon
            );

        if (!hit || hits.Count == 0)
            return;

        currentPose = hits[0].pose;

        if (!surfaceDetected)
        {
            surfaceDetected = true;

            SetStatus(
                "SURFACE DETECTED",
                "Suitable surface found. Place the training mine."
            );

            if (placeButton != null)
                placeButton.gameObject.SetActive(true);

            Debug.Log(
                "SAFEX AR: Suitable surface detected."
            );
        }
    }

    // =========================================================
    // PLACE MINE
    // =========================================================

    private void PlaceMine()
    {
        if (!placementMode)
            return;

        if (minePlaced)
            return;

        if (!surfaceDetected)
            return;

        if (minePrefab == null)
        {
            Debug.LogError(
                "SAFEX AR: Mine Prefab is not assigned."
            );

            return;
        }

        // Spawn mine only once.
        spawnedMine =
            Instantiate(
                minePrefab,
                currentPose.position,
                currentPose.rotation
            );

        spawnedMine.transform.localScale =
            Vector3.one * mineScale;

        // Slightly lower the mine into the detected surface
        spawnedMine.transform.position +=
            Vector3.down * 0.05f;

        minePlaced = true;
        placementMode = false;

        // Stop AR placement interaction.
        if (raycastManager != null)
            raycastManager.enabled = false;

        if (planeManager != null)
            planeManager.enabled = false;

        SetStatus(
            "TRAINING MINE PLACED",
            "Training environment ready."
        );

        if (placeButton != null)
            placeButton.gameObject.SetActive(false);

        Debug.Log(
            "SAFEX AR: Training mine successfully placed."
        );

        // Tell the global flow controller.
        if (flowController != null)
        {
            flowController.OnMinePlaced();
            // Cloud event: mine placed
            if (SAFEXCloudSync.Instance != null)
            {
                var data = new Dictionary<string, string>();
                data["position"] = spawnedMine.transform.position.x + "," + spawnedMine.transform.position.y + "," + spawnedMine.transform.position.z;
                SAFEXCloudSync.Instance.SendTrainingEvent("MINE_PLACED", data);
            }
        }
    }

    // =========================================================
    // HOME BACKGROUND
    // =========================================================

    private void SetHomeBackground(bool visible)
    {
        if (homeBackground != null)
            homeBackground.SetActive(visible);
    }

    // =========================================================
    // HIDE RUNTIME HOME UI
    // =========================================================

    private void HideHomeRuntimeUI()
    {
        GameObject homeUI =
            GameObject.Find("SAFEX UI");

        if (homeUI != null)
            homeUI.SetActive(false);
    }

    // =========================================================
    // AR UI
    // =========================================================

    private void CreateARUI()
    {
        if (arCanvas != null)
            Destroy(arCanvas.gameObject);

        GameObject canvasObject =
            new GameObject("SAFEX AR UI");

        arCanvas =
            canvasObject.AddComponent<Canvas>();

        arCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        arCanvas.sortingOrder = 100;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080, 1920);

        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        CreateStatusPanel();

        CreatePlacementButton();
    }

    // =========================================================
    // STATUS PANEL
    // =========================================================

    private void CreateStatusPanel()
    {
        GameObject panel =
            new GameObject("AR STATUS PANEL");

        panel.transform.SetParent(
            arCanvas.transform,
            false
        );

        Image image =
            panel.AddComponent<Image>();

        image.color =
            new Color(
                0.005f,
                0.008f,
                0.012f,
                0.82f
            );

        RectTransform rect =
            panel.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.05f, 0.80f);

        rect.anchorMax =
            new Vector2(0.95f, 0.96f);

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        statusText =
            CreateText(
                panel.transform,
                "SCANNING",
                new Vector2(0, 30),
                new Vector2(850, 45),
                24,
                Color.white
            );

        instructionText =
            CreateText(
                panel.transform,
                "Move your phone slowly to scan a flat surface.",
                new Vector2(0, -25),
                new Vector2(820, 60),
                15,
                new Color(
                    0.70f,
                    0.75f,
                    0.80f,
                    1f
                )
            );
    }

    // =========================================================
    // PLACE BUTTON
    // =========================================================

    private void CreatePlacementButton()
    {
        GameObject buttonObject =
            new GameObject(
                "PLACE TRAINING MINE"
            );

        buttonObject.transform.SetParent(
            arCanvas.transform,
            false
        );

        Image image =
            buttonObject.AddComponent<Image>();

        image.color =
            new Color(
                1f,
                0.42f,
                0.03f,
                1f
            );

        placeButton =
            buttonObject.AddComponent<Button>();

        placeButton.targetGraphic =
            image;

        ColorBlock colors =
            placeButton.colors;

        colors.normalColor =
            new Color(
                1f,
                0.42f,
                0.03f,
                1f
            );

        colors.highlightedColor =
            new Color(
                1f,
                0.55f,
                0.08f,
                1f
            );

        colors.pressedColor =
            new Color(
                0.75f,
                0.25f,
                0.01f,
                1f
            );

        placeButton.colors =
            colors;

        RectTransform rect =
            buttonObject.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.12f, 0.07f);

        rect.anchorMax =
            new Vector2(0.88f, 0.17f);

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CreateText(
            buttonObject.transform,
            "PLACE TRAINING MINE   →",
            Vector2.zero,
            new Vector2(750, 100),
            23,
            Color.white
        );

        placeButton.onClick.AddListener(
            PlaceMine
        );

        placeButton.gameObject.SetActive(false);
    }

    // =========================================================
    // STATUS UPDATE
    // =========================================================

    private void SetStatus(
        string status,
        string instruction)
    {
        if (statusText != null)
            statusText.text = status;

        if (instructionText != null)
            instructionText.text = instruction;
    }

    // =========================================================
    // TEXT
    // =========================================================

    private Text CreateText(
        Transform parent,
        string text,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color)
    {
        GameObject obj =
            new GameObject("Text");

        obj.transform.SetParent(
            parent,
            false
        );

        Text txt =
            obj.AddComponent<Text>();

        txt.text = text;

        txt.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        txt.fontSize =
            fontSize;

        txt.alignment =
            TextAnchor.MiddleCenter;

        txt.color =
            color;

        txt.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        txt.verticalOverflow =
            VerticalWrapMode.Overflow;

        RectTransform rect =
            txt.GetComponent<RectTransform>();

        rect.anchoredPosition =
            position;

        rect.sizeDelta =
            size;

        return txt;
    }
}