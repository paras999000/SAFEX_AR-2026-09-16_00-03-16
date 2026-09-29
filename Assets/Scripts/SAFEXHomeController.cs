using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// SAFEX FUTURISTIC UI CONTROLLER
///
/// Owns only the pre-training interface:
/// Home -> Module Selection -> Briefing.
///
/// AR Placement, Training, Training Complete and Certificate
/// are handled by their respective controllers.
///
/// Design direction:
/// - Industrial tactical HUD
/// - Dark glass panels
/// - Cyan / orange safety accents
/// - Minimal futuristic typography
/// - Soft transitions and button feedback
///
/// IMPORTANT:
/// This script does not change SAFEXFlowController logic.
/// It only changes the visual presentation of the existing flow.
/// </summary>
public class SAFEXHomeController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField]
    private SAFEXFlowController flowController;

    [Header("Visual")]
    [SerializeField]
    private bool enableAnimations = true;

    [SerializeField]
    private float transitionDuration = 0.28f;

    private Canvas canvas;
    private GameObject root;
    private CanvasGroup rootGroup;

    // SAFEX palette
    private readonly Color bg =
        new Color(0.008f, 0.014f, 0.021f, 1f);

    private readonly Color panel =
        new Color(0.018f, 0.030f, 0.042f, 0.96f);

    private readonly Color panelLight =
        new Color(0.028f, 0.045f, 0.060f, 0.98f);

    private readonly Color cyan =
        new Color(0.00f, 0.88f, 0.82f, 1f);

    private readonly Color cyanSoft =
        new Color(0.00f, 0.55f, 0.52f, 1f);

    private readonly Color orange =
        new Color(1.00f, 0.38f, 0.04f, 1f);

    private readonly Color white =
        new Color(0.94f, 0.97f, 0.98f, 1f);

    private readonly Color muted =
        new Color(0.48f, 0.56f, 0.61f, 1f);

    private readonly Color dim =
        new Color(0.25f, 0.31f, 0.35f, 1f);

    private Coroutine transitionRoutine;

    // =========================================================
    // UNITY
    // =========================================================

    private void Start()
    {
        if (flowController == null)
            flowController =
                FindFirstObjectByType<SAFEXFlowController>();

        if (flowController == null)
        {
            Debug.LogError(
                "SAFEXHomeController: SAFEXFlowController not found."
            );
            return;
        }

        flowController.StateChanged += OnStateChanged;
        flowController.ModuleChanged += OnModuleChanged;

        ShowHome();
    }

    private void OnDestroy()
    {
        if (flowController != null)
        {
            flowController.StateChanged -= OnStateChanged;
            flowController.ModuleChanged -= OnModuleChanged;
        }
    }

    // =========================================================
    // STATE MANAGEMENT
    // =========================================================

    private void OnStateChanged(
        SAFEXFlowController.SAFEXState state)
    {
        Debug.Log("SAFEX UI STATE -> " + state);

        switch (state)
        {
            case SAFEXFlowController.SAFEXState.Home:
                ShowHome();
                break;

            case SAFEXFlowController.SAFEXState.ModuleSelection:
                ShowModuleSelection();
                break;

            case SAFEXFlowController.SAFEXState.Briefing:
                ShowBriefing();
                break;

            case SAFEXFlowController.SAFEXState.ARPlacement:
            case SAFEXFlowController.SAFEXState.Training:
            case SAFEXFlowController.SAFEXState.TrainingComplete:
            case SAFEXFlowController.SAFEXState.Certificate:
                HideHomeUI();
                break;
        }
    }

    private void OnModuleChanged(
        SAFEXFlowController.TrainingModule module)
    {
        Debug.Log("SAFEX MODULE -> " + module);
    }

    private void HideHomeUI()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    // =========================================================
    // CANVAS
    // =========================================================

    private void CreateCanvas()
    {
        GameObject canvasObject =
            new GameObject("SAFEX UI");

        canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 50;

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

        root =
            new GameObject("SAFEX ROOT");

        root.transform.SetParent(
            canvas.transform,
            false
        );

        RectTransform rootRect =
            root.AddComponent<RectTransform>();

        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        rootGroup =
            root.AddComponent<CanvasGroup>();

        rootGroup.alpha = 0f;
    }

    private void RecreateRoot()
    {
        if (canvas != null)
        {
            Destroy(canvas.gameObject);

            canvas = null;
            root = null;
            rootGroup = null;
        }

        CreateCanvas();
    }

    // =========================================================
    // HOME
    // =========================================================

    private void ShowHome()
    {
        RecreateRoot();

        CreateBackground();
        CreateTopStatusBar();
        CreateBrand();
        CreateHero();
        CreateFeatureGrid();
        CreateStartButton();
        CreateBottomSystemBar();

        StartFadeIn();
    }

    private void CreateBackground()
    {
        GameObject background =
            CreateFullPanel(
                "FUTURISTIC BACKGROUND",
                bg
            );

        // Subtle horizontal technical lines.
        for (int i = 0; i < 9; i++)
        {
            float y = 620f - i * 155f;

            GameObject line =
                CreatePanel(
                    "GRID LINE",
                    new Vector2(0, y),
                    new Vector2(980, 1)
                );

            line.GetComponent<Image>().color =
                new Color(0.05f, 0.16f, 0.17f, 0.18f);
        }

        // Side accent rails.
        CreatePanel(
            "LEFT RAIL",
            new Vector2(-510, 0),
            new Vector2(3, 1500)
        ).GetComponent<Image>().color =
            new Color(cyan.r, cyan.g, cyan.b, 0.20f);

        CreatePanel(
            "RIGHT RAIL",
            new Vector2(510, 0),
            new Vector2(3, 1500)
        ).GetComponent<Image>().color =
            new Color(cyan.r, cyan.g, cyan.b, 0.20f);

        // Corner technical marks.
        CreateCornerMark(new Vector2(-455, 690), false);
        CreateCornerMark(new Vector2(455, 690), true);
        CreateCornerMark(new Vector2(-455, -690), false);
        CreateCornerMark(new Vector2(455, -690), true);
    }

    private void CreateCornerMark(Vector2 position, bool flip)
    {
        GameObject mark =
            new GameObject("CORNER MARK");

        mark.transform.SetParent(root.transform, false);

        RectTransform rect =
            mark.AddComponent<RectTransform>();

        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(70, 70);

        GameObject horizontal =
            CreatePanel(
                "CORNER H",
                new Vector2(flip ? -15 : 15, flip ? 15 : -15),
                new Vector2(50, 2),
                mark.transform
            );

        horizontal.GetComponent<Image>().color = cyanSoft;

        GameObject vertical =
            CreatePanel(
                "CORNER V",
                new Vector2(flip ? 15 : -15, flip ? -15 : 15),
                new Vector2(2, 50),
                mark.transform
            );

        vertical.GetComponent<Image>().color = cyanSoft;
    }

    private void CreateTopStatusBar()
    {
        GameObject bar =
            CreatePanel(
                "TOP STATUS BAR",
                new Vector2(0, 715),
                new Vector2(940, 52)
            );

        bar.GetComponent<Image>().color =
            new Color(0.01f, 0.025f, 0.032f, 0.96f);

        CreateText(
            bar.transform,
            "SAFEX // INDUSTRIAL SAFETY SYSTEM",
            new Vector2(-270, 0),
            new Vector2(500, 40),
            13,
            muted,
            TextAnchor.MiddleLeft
        );

        CreateText(
            bar.transform,
            "SYSTEM READY",
            new Vector2(275, 0),
            new Vector2(220, 40),
            13,
            cyan,
            TextAnchor.MiddleRight
        );

        CreateStatusDot(
            bar.transform,
            new Vector2(165, 0),
            cyan
        );

        // =========================================================
        // LANGUAGE SELECTOR
        // =========================================================

        GameObject languageObject =
            new GameObject("LANGUAGE SELECTOR CONTROLLER");

        languageObject.transform.SetParent(
            root.transform,
            false
        );

        SAFEXLanguageSelector languageSelector =
            languageObject.AddComponent<SAFEXLanguageSelector>();

        languageSelector.Build(
            root.transform,
            new Vector2(335, 650)
        );
    }

    private void CreateBrand()
    {
        GameObject brand =
            new GameObject("SAFEX BRAND");

        brand.transform.SetParent(root.transform, false);

        RectTransform rect =
            brand.AddComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(0, 575);
        rect.sizeDelta = new Vector2(900, 170);

        CreateText(
            brand.transform,
            "SAF",
            new Vector2(-78, 28),
            new Vector2(300, 95),
            78,
            white,
            TextAnchor.MiddleRight
        );

        CreateText(
            brand.transform,
            "EX",
            new Vector2(78, 28),
            new Vector2(300, 95),
            78,
            cyan,
            TextAnchor.MiddleLeft
        );

        CreateText(
            brand.transform,
            "MINE SAFETY / AR TRAINING PLATFORM",
            new Vector2(0, -42),
            new Vector2(760, 38),
            14,
            muted,
            TextAnchor.MiddleCenter
        );

        CreatePanel(
            "BRAND LINE",
            new Vector2(0, -70),
            new Vector2(250, 2)
        ).GetComponent<Image>().color = cyanSoft;
    }

    private void CreateHero()
    {
        CreateText(
            root.transform,
            "MINE SAFETY",
            new Vector2(0, 410),
            new Vector2(900, 72),
            39,
            white,
            TextAnchor.MiddleCenter
        );

        CreateText(
            root.transform,
            "TRAINING SYSTEM",
            new Vector2(0, 350),
            new Vector2(900, 65),
            28,
            cyan,
            TextAnchor.MiddleCenter
        );

        CreateText(
            root.transform,
            "AR-BASED INDUSTRIAL SAFETY SIMULATION",
            new Vector2(0, 292),
            new Vector2(850, 40),
            15,
            muted,
            TextAnchor.MiddleCenter
        );

        CreateText(
            root.transform,
            "TRAIN  /  RESPOND  /  EVACUATE  /  CERTIFY",
            new Vector2(0, 250),
            new Vector2(850, 35),
            12,
            dim,
            TextAnchor.MiddleCenter
        );
    }

    private void CreateFeatureGrid()
    {
        CreateFeatureCard(
            "01",
            "REAL SCENARIOS",
            "Industrial emergency response",
            new Vector2(-215, 100),
            orange
        );

        CreateFeatureCard(
            "02",
            "AR LEARNING",
            "Interactive mine environment",
            new Vector2(215, 100),
            cyan
        );

        CreateFeatureCard(
            "03",
            "LIVE RESPONSE",
            "Scenario-based evaluation",
            new Vector2(-215, -40),
            cyan
        );

        CreateFeatureCard(
            "04",
            "DIGITAL CREDENTIAL",
            "Verified completion certificate",
            new Vector2(215, -40),
            orange
        );
    }

    private void CreateFeatureCard(
        string number,
        string title,
        string description,
        Vector2 position,
        Color accent)
    {
        GameObject card =
            CreatePanel(
                "FEATURE " + number,
                position,
                new Vector2(410, 118)
            );

        card.GetComponent<Image>().color = panel;

        CreatePanel(
            "ACCENT",
            new Vector2(-197, 0),
            new Vector2(3, 84),
            card.transform
        ).GetComponent<Image>().color = accent;

        CreateText(
            card.transform,
            number,
            new Vector2(-160, 27),
            new Vector2(60, 30),
            12,
            accent,
            TextAnchor.MiddleLeft
        );

        CreateText(
            card.transform,
            title,
            new Vector2(20, 24),
            new Vector2(300, 34),
            16,
            white,
            TextAnchor.MiddleLeft
        );

        CreateText(
            card.transform,
            description,
            new Vector2(20, -22),
            new Vector2(315, 32),
            12,
            muted,
            TextAnchor.MiddleLeft
        );
    }

    private void CreateStartButton()
    {
        GameObject buttonObject =
            CreatePanel(
                "START TRAINING BUTTON",
                new Vector2(0, -205),
                new Vector2(780, 112)
            );

        Image image =
            buttonObject.GetComponent<Image>();

        image.color =
            new Color(
                cyan.r,
                cyan.g,
                cyan.b,
                0.12f
            );

        Button button =
            buttonObject.AddComponent<Button>();

        button.targetGraphic = image;

        ColorBlock colors = button.colors;

        colors.normalColor =
            new Color(cyan.r, cyan.g, cyan.b, 0.12f);

        colors.highlightedColor =
            new Color(cyan.r, cyan.g, cyan.b, 0.23f);

        colors.pressedColor =
            new Color(cyan.r, cyan.g, cyan.b, 0.32f);

        button.colors = colors;

        // Outer border.
        CreateBorder(
            buttonObject.transform,
            cyan
        );

        CreateText(
            buttonObject.transform,
            "INITIALIZE TRAINING",
            new Vector2(-60, 14),
            new Vector2(550, 38),
            22,
            white,
            TextAnchor.MiddleCenter
        );

        CreateText(
            buttonObject.transform,
            "SELECT SAFETY MODULE",
            new Vector2(-60, -22),
            new Vector2(550, 28),
            11,
            cyan,
            TextAnchor.MiddleCenter
        );

        CreateText(
            buttonObject.transform,
            "→",
            new Vector2(320, 0),
            new Vector2(60, 60),
            28,
            cyan,
            TextAnchor.MiddleCenter
        );

        button.onClick.AddListener(OpenModuleSelection);

        AddButtonPulse(
            buttonObject,
            cyan
        );
    }

    private void CreateBottomSystemBar()
    {
        GameObject bar =
            CreatePanel(
                "BOTTOM SYSTEM BAR",
                new Vector2(0, -675),
                new Vector2(940, 72)
            );

        bar.GetComponent<Image>().color =
            new Color(0.01f, 0.025f, 0.032f, 0.94f);

        CreateText(
            bar.transform,
            "SAFEX CORE",
            new Vector2(-370, 0),
            new Vector2(170, 35),
            12,
            cyan,
            TextAnchor.MiddleLeft
        );

        CreateText(
            bar.transform,
            "OFFLINE READY",
            new Vector2(-170, 0),
            new Vector2(190, 35),
            11,
            muted,
            TextAnchor.MiddleCenter
        );

        CreateText(
            bar.transform,
            "AR  /  RESPONSE  /  EVACUATION  /  CERTIFICATION",
            new Vector2(155, 0),
            new Vector2(500, 35),
            10,
            dim,
            TextAnchor.MiddleRight
        );
    }

    // =========================================================
    // MODULE SELECTION
    // =========================================================

    private void OpenModuleSelection()
    {
        if (flowController == null)
            return;

        flowController.ShowModuleSelection();
    }

    private void ShowModuleSelection()
    {
        RecreateRoot();

        CreateBackground();
        CreateSubpageHeader(
            "MODULE SELECTION",
            "SELECT INDUSTRIAL SAFETY PROTOCOL"
        );

        CreateModuleCard(
            "01",
            "FIRE",
            "& EXPLOSION",
            "EMERGENCY RESPONSE",
            "Recognize fire hazards, respond correctly and reach the safe zone.",
            new Vector2(0, 285),
            orange,
            SelectFire
        );

        CreateModuleCard(
            "02",
            "GAS LEAK",
            "& CONFINED SPACE",
            "HAZARD RESPONSE",
            "Identify gas hazards, avoid contaminated areas and evacuate safely.",
            new Vector2(0, -25),
            cyan,
            SelectGas
        );

        CreateBackButton(
            new Vector2(0, -350)
        );

        CreateText(
            root.transform,
            "02 ACTIVE TRAINING PROTOCOLS",
            new Vector2(0, -485),
            new Vector2(700, 35),
            11,
            dim,
            TextAnchor.MiddleCenter
        );

        StartFadeIn();
    }

    private void CreateSubpageHeader(
        string title,
        string subtitle)
    {
        CreateText(
            root.transform,
            "SAFEX / TRAINING",
            new Vector2(-330, 650),
            new Vector2(360, 35),
            12,
            cyan,
            TextAnchor.MiddleLeft
        );

        CreateText(
            root.transform,
            title,
            new Vector2(0, 545),
            new Vector2(900, 65),
            34,
            white,
            TextAnchor.MiddleCenter
        );

        CreateText(
            root.transform,
            subtitle,
            new Vector2(0, 485),
            new Vector2(850, 35),
            13,
            muted,
            TextAnchor.MiddleCenter
        );

        CreatePanel(
            "HEADER LINE",
            new Vector2(0, 440),
            new Vector2(700, 2)
        ).GetComponent<Image>().color =
            new Color(cyan.r, cyan.g, cyan.b, 0.45f);
    }

    private void CreateModuleCard(
        string number,
        string titleTop,
        string titleBottom,
        string category,
        string description,
        Vector2 position,
        Color accent,
        UnityAction action)
    {
        GameObject card =
            CreatePanel(
                "MODULE " + number,
                position,
                new Vector2(860, 245)
            );

        card.GetComponent<Image>().color = panel;

        CreateBorder(
            card.transform,
            new Color(accent.r, accent.g, accent.b, 0.48f)
        );

        CreatePanel(
            "MODULE ACCENT",
            new Vector2(-425, 0),
            new Vector2(4, 180),
            card.transform
        ).GetComponent<Image>().color = accent;

        CreateText(
            card.transform,
            number,
            new Vector2(-355, 78),
            new Vector2(80, 32),
            12,
            accent,
            TextAnchor.MiddleLeft
        );

        CreateText(
            card.transform,
            titleTop,
            new Vector2(-170, 55),
            new Vector2(420, 50),
            27,
            white,
            TextAnchor.MiddleLeft
        );

        CreateText(
            card.transform,
            titleBottom,
            new Vector2(-170, 5),
            new Vector2(420, 50),
            25,
            accent,
            TextAnchor.MiddleLeft
        );

        CreateText(
            card.transform,
            category,
            new Vector2(-170, -45),
            new Vector2(390, 30),
            11,
            muted,
            TextAnchor.MiddleLeft
        );

        CreateText(
            card.transform,
            description,
            new Vector2(180, 18),
            new Vector2(330, 82),
            13,
            muted,
            TextAnchor.MiddleLeft
        );

        GameObject selectButton =
            CreatePanel(
                "SELECT",
                new Vector2(325, -72),
                new Vector2(180, 48),
                card.transform
            );

        selectButton.GetComponent<Image>().color =
            new Color(accent.r, accent.g, accent.b, 0.12f);

        Button button =
            selectButton.AddComponent<Button>();

        button.targetGraphic =
            selectButton.GetComponent<Image>();

        ColorBlock colors = button.colors;

        colors.normalColor =
            new Color(accent.r, accent.g, accent.b, 0.12f);

        colors.highlightedColor =
            new Color(accent.r, accent.g, accent.b, 0.25f);

        colors.pressedColor =
            new Color(accent.r, accent.g, accent.b, 0.38f);

        button.colors = colors;

        CreateBorder(
            selectButton.transform,
            new Color(accent.r, accent.g, accent.b, 0.65f)
        );

        CreateText(
            selectButton.transform,
            "SELECT  →",
            Vector2.zero,
            new Vector2(170, 42),
            12,
            accent,
            TextAnchor.MiddleCenter
        );

        button.onClick.AddListener(action);

        AddButtonPulse(
            selectButton,
            accent
        );
    }

    private void CreateBackButton(Vector2 position)
    {
        GameObject buttonObject =
            CreatePanel(
                "BACK BUTTON",
                position,
                new Vector2(360, 64)
            );

        buttonObject.GetComponent<Image>().color =
            new Color(0.025f, 0.04f, 0.05f, 0.95f);

        CreateBorder(
            buttonObject.transform,
            new Color(0.25f, 0.32f, 0.35f, 0.7f)
        );

        Button button =
            buttonObject.AddComponent<Button>();

        button.targetGraphic =
            buttonObject.GetComponent<Image>();

        CreateText(
            buttonObject.transform,
            "←  RETURN TO HOME",
            Vector2.zero,
            new Vector2(330, 50),
            12,
            muted,
            TextAnchor.MiddleCenter
        );

        button.onClick.AddListener(() =>
        {
            if (flowController != null)
                flowController.ShowHome();
        });
    }

    // =========================================================
    // MODULE SELECTION
    // =========================================================

    private void SelectFire()
    {
        if (flowController == null)
            return;

        flowController.SelectFireModule();
    }

    private void SelectGas()
    {
        if (flowController == null)
            return;

        flowController.SelectGasModule();
    }

    // =========================================================
    // BRIEFING
    // =========================================================

    private void ShowBriefing()
    {
        RecreateRoot();

        CreateBackground();

        bool isFire =
            flowController != null &&
            flowController.SelectedModule ==
            SAFEXFlowController.TrainingModule.Fire;

        string moduleName =
            isFire
                ? "FIRE & EXPLOSION"
                : "GAS LEAK & CONFINED SPACE";

        string protocol =
            isFire
                ? "EMERGENCY RESPONSE PROTOCOL"
                : "HAZARD RESPONSE PROTOCOL";

        Color accent =
            isFire
                ? orange
                : cyan;

        CreateText(
            root.transform,
            "SAFEX / PROTOCOL BRIEFING",
            new Vector2(-320, 650),
            new Vector2(500, 35),
            12,
            accent,
            TextAnchor.MiddleLeft
        );

        CreateText(
            root.transform,
            moduleName,
            new Vector2(0, 545),
            new Vector2(900, 70),
            32,
            white,
            TextAnchor.MiddleCenter
        );

        CreateText(
            root.transform,
            protocol,
            new Vector2(0, 485),
            new Vector2(850, 35),
            13,
            accent,
            TextAnchor.MiddleCenter
        );

        CreateBriefingScenario(
            isFire,
            accent
        );

        CreateProtocolSteps(
            isFire,
            accent
        );

        GameObject scanButton =
            CreateActionButton(
                "SCAN MINE AREA",
                "INITIALIZE AR SURFACE SCAN",
                new Vector2(0, -375),
                new Vector2(760, 105),
                accent
            );

        scanButton
            .GetComponent<Button>()
            .onClick.AddListener(() =>
            {
                if (flowController != null)
                    flowController.ContinueFromBriefing();
            });

        CreateBackButton(
            new Vector2(0, -515)
        );

        StartFadeIn();
    }

    private void CreateBriefingScenario(
        bool isFire,
        Color accent)
    {
        GameObject card =
            CreatePanel(
                "SCENARIO CARD",
                new Vector2(0, 315),
                new Vector2(850, 210)
            );

        card.GetComponent<Image>().color = panel;

        CreateBorder(
            card.transform,
            new Color(accent.r, accent.g, accent.b, 0.38f)
        );

        CreateText(
            card.transform,
            "INCIDENT DETECTED",
            new Vector2(-285, 62),
            new Vector2(360, 30),
            11,
            accent,
            TextAnchor.MiddleLeft
        );

        string incident =
            isFire
                ? "FIRE EMERGENCY"
                : "HAZARDOUS GAS LEAK";

        CreateText(
            card.transform,
            incident,
            new Vector2(-285, 20),
            new Vector2(500, 42),
            22,
            white,
            TextAnchor.MiddleLeft
        );

        string body =
            isFire
                ? "A fire emergency has been detected inside the underground mine. Respond to the hazard and reach the designated safe zone."
                : "A hazardous gas leak has been detected inside the underground mine. Avoid contaminated areas and reach the designated safe zone.";

        CreateText(
            card.transform,
            body,
            new Vector2(150, 5),
            new Vector2(380, 105),
            13,
            muted,
            TextAnchor.MiddleLeft
        );

        CreateStatusDot(
            card.transform,
            new Vector2(-335, 20),
            accent
        );
    }

    private void CreateProtocolSteps(
        bool isFire,
        Color accent)
    {
        GameObject card =
            CreatePanel(
                "PROTOCOL CARD",
                new Vector2(0, 20),
                new Vector2(850, 230)
            );

        card.GetComponent<Image>().color =
            new Color(0.014f, 0.026f, 0.035f, 0.96f);

        CreateText(
            card.transform,
            "RESPONSE SEQUENCE",
            new Vector2(-310, 78),
            new Vector2(400, 30),
            11,
            accent,
            TextAnchor.MiddleLeft
        );

        string[] fireSteps =
        {
            "01   RAISE ALARM",
            "02   ISOLATE POWER",
            "03   CONTROL FIRE",
            "04   EVACUATE"
        };

        string[] gasSteps =
        {
            "01   DETECT GAS",
            "02   RAISE ALARM",
            "03   ISOLATE AREA",
            "04   EVACUATE"
        };

        string[] steps =
            isFire ? fireSteps : gasSteps;

        for (int i = 0; i < steps.Length; i++)
        {
            float x =
                -290 + (i % 2) * 320;

            float y =
                25 - (i / 2) * 70;

            CreateText(
                card.transform,
                steps[i],
                new Vector2(x, y),
                new Vector2(290, 45),
                12,
                i == 3 ? accent : white,
                TextAnchor.MiddleLeft
            );
        }

        CreateText(
            card.transform,
            "TRAINING RESULT IS BASED ON YOUR RESPONSE AND EVACUATION.",
            new Vector2(0, -88),
            new Vector2(760, 30),
            10,
            dim,
            TextAnchor.MiddleCenter
        );
    }

    // =========================================================
    // ACTION BUTTON
    // =========================================================

    private GameObject CreateActionButton(
        string title,
        string subtitle,
        Vector2 position,
        Vector2 size,
        Color accent)
    {
        GameObject buttonObject =
            CreatePanel(
                "ACTION BUTTON",
                position,
                size
            );

        buttonObject.GetComponent<Image>().color =
            new Color(
                accent.r,
                accent.g,
                accent.b,
                0.10f
            );

        CreateBorder(
            buttonObject.transform,
            new Color(
                accent.r,
                accent.g,
                accent.b,
                0.75f
            )
        );

        Button button =
            buttonObject.AddComponent<Button>();

        button.targetGraphic =
            buttonObject.GetComponent<Image>();

        ColorBlock colors = button.colors;

        colors.normalColor =
            new Color(accent.r, accent.g, accent.b, 0.10f);

        colors.highlightedColor =
            new Color(accent.r, accent.g, accent.b, 0.20f);

        colors.pressedColor =
            new Color(accent.r, accent.g, accent.b, 0.30f);

        button.colors = colors;

        CreateText(
            buttonObject.transform,
            title,
            new Vector2(-35, 15),
            new Vector2(size.x - 150, 35),
            19,
            white,
            TextAnchor.MiddleCenter
        );

        CreateText(
            buttonObject.transform,
            subtitle,
            new Vector2(-35, -20),
            new Vector2(size.x - 150, 28),
            10,
            accent,
            TextAnchor.MiddleCenter
        );

        CreateText(
            buttonObject.transform,
            "→",
            new Vector2(size.x / 2f - 55, 0),
            new Vector2(45, 50),
            25,
            accent,
            TextAnchor.MiddleCenter
        );

        AddButtonPulse(
            buttonObject,
            accent
        );

        return buttonObject;
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private GameObject CreateFullPanel(
        string objectName,
        Color color)
    {
        GameObject panel =
            new GameObject(objectName);

        panel.transform.SetParent(
            root.transform,
            false
        );

        RectTransform rect =
            panel.AddComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image =
            panel.AddComponent<Image>();

        image.color = color;

        return panel;
    }

    private GameObject CreatePanel(
        string objectName,
        Vector2 position,
        Vector2 size,
        Transform parent = null)
    {
        GameObject panel =
            new GameObject(objectName);

        if (parent == null)
            parent = root.transform;

        panel.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            panel.AddComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            position;

        rect.sizeDelta =
            size;

        Image image =
            panel.AddComponent<Image>();

        image.color = panelLight;

        return panel;
    }

    private void CreateBorder(
        Transform parent,
        Color color)
    {
        GameObject border =
            new GameObject("TECH BORDER");

        border.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            border.AddComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image =
            border.AddComponent<Image>();

        image.color = color;

        // Use a transparent center with a simple frame made from
        // four child images. This avoids requiring a sprite asset.
        image.color = Color.clear;
        image.raycastTarget = false;

        CreateBorderPart(
            border.transform,
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(0, -1),
            new Vector2(0, 1),
            color
        );

        CreateBorderPart(
            border.transform,
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(0, 1),
            color
        );

        CreateBorderPart(
            border.transform,
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 0),
            color
        );

        CreateBorderPart(
            border.transform,
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(-1, 0),
            new Vector2(1, 0),
            color
        );
    }

    private void CreateBorderPart(
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        Color color)
    {
        GameObject part =
            new GameObject("BORDER");

        part.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            part.AddComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;

        rect.offsetMin = offsetMin * 2f;
        rect.offsetMax = offsetMax * 2f;

        Image image =
            part.AddComponent<Image>();

        image.color = color;
        image.raycastTarget = false;
    }

    private Text CreateText(
        Transform parent,
        string text,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color,
        TextAnchor alignment)
    {
        GameObject obj =
            new GameObject("TEXT");

        obj.transform.SetParent(
            parent,
            false
        );

        Text txt =
            obj.AddComponent<Text>();

        txt.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        txt.text = text;

        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.color = color;

        txt.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        txt.verticalOverflow =
            VerticalWrapMode.Overflow;


        // =========================================================
        // SAFEX LOCALIZATION
        // =========================================================

        SAFEXLocalizedText localized =
            obj.AddComponent<SAFEXLocalizedText>();

        localized.Initialize(
            txt,
            text
        );

        RectTransform rect =
            txt.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            position;

        rect.sizeDelta =
            size;

        return txt;
    }

    private void CreateStatusDot(
        Transform parent,
        Vector2 position,
        Color color)
    {
        GameObject dot =
            new GameObject("STATUS DOT");

        dot.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            dot.AddComponent<RectTransform>();

        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(10, 10);

        Image image =
            dot.AddComponent<Image>();

        image.color = color;

        if (enableAnimations)
            StartCoroutine(PulseDot(dot, color));
    }

    private IEnumerator PulseDot(
        GameObject dot,
        Color color)
    {
        Image image =
            dot.GetComponent<Image>();

        if (image == null)
            yield break;

        while (dot != null)
        {
            float pulse =
                0.55f +
                Mathf.Sin(Time.unscaledTime * 3f) * 0.35f;

            image.color =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    pulse
                );

            yield return null;
        }
    }

    private void AddButtonPulse(
        GameObject buttonObject,
        Color accent)
    {
        if (!enableAnimations)
            return;

        StartCoroutine(ButtonGlowPulse(
            buttonObject,
            accent
        ));
    }

    private IEnumerator ButtonGlowPulse(
        GameObject buttonObject,
        Color accent)
    {
        Image image =
            buttonObject.GetComponent<Image>();

        if (image == null)
            yield break;

        Color baseColor =
            image.color;

        while (buttonObject != null)
        {
            float wave =
                (Mathf.Sin(Time.unscaledTime * 2.0f) + 1f)
                * 0.5f;

            image.color =
                new Color(
                    baseColor.r,
                    baseColor.g,
                    baseColor.b,
                    Mathf.Lerp(
                        baseColor.a,
                        Mathf.Min(
                            0.20f,
                            baseColor.a + 0.08f
                        ),
                        wave
                    )
                );

            yield return null;
        }
    }

    // =========================================================
    // TRANSITION
    // =========================================================

    private void StartFadeIn()
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        if (!enableAnimations)
        {
            if (rootGroup != null)
                rootGroup.alpha = 1f;

            return;
        }

        transitionRoutine =
            StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        if (rootGroup == null)
            yield break;

        rootGroup.alpha = 0f;

        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.unscaledDeltaTime;

            rootGroup.alpha =
                Mathf.Lerp(
                    0f,
                    1f,
                    time / transitionDuration
                );

            yield return null;
        }

        rootGroup.alpha = 1f;
    }
}
