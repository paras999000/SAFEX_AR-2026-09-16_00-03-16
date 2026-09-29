using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SAFEX Fire Training Controller - Simple Mobile Flow.
///
/// No green route.
/// No spatial checkpoints.
/// No quiz.
///
/// FLOW:
/// Fire detected
/// -> Reach Power Control
/// -> Isolate Power
/// -> Use Fire Extinguisher
/// -> Complete Evacuation
/// -> Training Complete
/// -> Certificate + QR
///
/// The action button is visible immediately and changes to the
/// next required action after every successful step.
/// </summary>
public class FireTrainingController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private SAFEXFlowController flowController;

    [Header("AR Camera")]
    [SerializeField] private Camera arCamera;

    [Header("Scenario")]
    [SerializeField] private FireScenario fireScenario;

    [Header("UI")]
    [SerializeField]
    private Color fireAccent =
        new Color(1f, 0.25f, 0.04f, 1f);

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Text topText;
    private Text messageText;
    private Text statusText;
    private Button actionButton;
    private Text actionButtonText;

    private bool trainingActive;
    private bool reachPowerCompleted;
    private bool powerIsolated;
    private bool extinguisherActionCompleted;
    private bool trainingCompleted;

    private void Awake()
    {
        if (flowController == null)
            flowController =
                FindFirstObjectByType<SAFEXFlowController>();

        if (arCamera == null)
            arCamera = Camera.main;

        if (fireScenario == null)
            fireScenario =
                FindFirstObjectByType<FireScenario>();
    }

    private void OnEnable()
    {
        if (flowController != null)
            flowController.StateChanged += OnStateChanged;
    }

    private void Start()
    {
        HideTrainingUI();
    }

    private void OnDisable()
    {
        if (flowController != null)
            flowController.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(
        SAFEXFlowController.SAFEXState state)
    {
        if (state ==
            SAFEXFlowController.SAFEXState.Training &&
            flowController != null &&
            flowController.SelectedModule ==
            SAFEXFlowController.TrainingModule.Fire)
        {
            StartFireTraining();
            return;
        }

        if (state !=
            SAFEXFlowController.SAFEXState.Training)
        {
            StopTraining();
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void StartFireTraining()
    {
        if (trainingActive)
            return;

        if (fireScenario == null)
            fireScenario =
                FindFirstObjectByType<FireScenario>();

        if (arCamera == null)
            arCamera = Camera.main;

        trainingActive = true;
        reachPowerCompleted = false;
        powerIsolated = false;
        extinguisherActionCompleted = false;
        trainingCompleted = false;
        // Cloud session
        if (SAFEXCloudSync.Instance != null)
        {
            SAFEXCloudSync.Instance.StartSession("FIRE");
            SAFEXCloudSync.Instance.SendTrainingEvent("TRAINING_STARTED");
        }

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.StartAssessment("FIRE");
        }
        else
        {
            Debug.LogWarning(
                "SAFEX FIRE: Assessment Manager not found."
            );
        }

        CreateTrainingHUD();

        SetTop(
            "SAFEX // FIRE RESPONSE",
            "● LIVE"
        );

        SetMessage(
            "FIRE EMERGENCY DETECTED",
            "Follow the emergency response sequence. Start with the power control."
        );

        SetStatus(
            "ACTION 1 OF 4"
        );

        ShowActionButton(
            "REACH POWER CONTROL"
        );

        Debug.Log(
            "SAFEX FIRE: Simple training started."
        );
    }

    // =========================================================
    // ACTION FLOW
    // =========================================================

    private void OnFireActionButtonPressed()
    {
        if (!trainingActive || trainingCompleted)
            return;

        if (!reachPowerCompleted)
        {
            CompleteReachPower();
            return;
        }

        if (!powerIsolated)
        {
            PerformIsolatePower();
            return;
        }

        if (!extinguisherActionCompleted)
        {
            PerformFireExtinguisher();
            return;
        }

        CompleteEvacuation();
    }

    private void CompleteReachPower()
    {
        reachPowerCompleted = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "REACH_POWER_CONTROL"
            );
        }

        SetTop(
            "SAFEX // FIRE RESPONSE",
            "● POWER CONTROL"
        );

        SetMessage(
            "POWER CONTROL REACHED",
            "Power control located. Isolate the electrical supply before using the extinguisher."
        );

        SetStatus(
            "ACTION 2 OF 4"
        );

        ShowActionButton(
            "ISOLATE POWER"
        );

        Debug.Log(
            "SAFEX FIRE: Power control reached."
        );
    }

    private void PerformIsolatePower()
    {
        if (!reachPowerCompleted || powerIsolated)
            return;

        powerIsolated = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "ISOLATE_POWER"
            );

            if (SAFEXCloudSync.Instance != null)
            {
                SAFEXCloudSync.Instance.SendTrainingEvent("ISOLATE_POWER");
            }
        }

        SetTop(
            "SAFEX // FIRE RESPONSE",
            "● POWER ISOLATED"
        );

        SetMessage(
            "POWER ISOLATED",
            "Electrical supply isolated. Use the appropriate fire extinguisher."
        );

        SetStatus(
            "ACTION 3 OF 4"
        );

        ShowActionButton(
            "USE FIRE EXTINGUISHER"
        );

        Debug.Log(
            "SAFEX FIRE: Power isolated."
        );
    }

    private void PerformFireExtinguisher()
    {
        if (!powerIsolated || extinguisherActionCompleted)
            return;

        extinguisherActionCompleted = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "USE_FIRE_EXTINGUISHER"
            );

            if (SAFEXCloudSync.Instance != null)
            {
                SAFEXCloudSync.Instance.SendTrainingEvent("USE_FIRE_EXTINGUISHER");
            }
        }

        if (fireScenario != null)
            fireScenario.StopFire();

        SetTop(
            "SAFEX // FIRE RESPONSE",
            "● FIRE CONTROLLED"
        );

        SetMessage(
            "FIRE EXTINGUISHED",
            "Fire controlled successfully. Complete the evacuation step."
        );

        SetStatus(
            "ACTION 4 OF 4"
        );

        ShowActionButton(
            "COMPLETE EVACUATION"
        );

        Debug.Log(
            "SAFEX FIRE: Extinguisher used. Fire stopped."
        );
    }

    private void CompleteEvacuation()
    {
        if (!extinguisherActionCompleted || trainingCompleted)
            return;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "EVACUATE_SAFE_EXIT"
            );

            // Keep this action for projects whose assessment manager
            // tracks completion as a separate action.
            SAFEXAssessmentManager.Instance.RecordAction(
                "COMPLETE_FIRE_TRAINING"
            );

            bool passed =
                SAFEXAssessmentManager.Instance.CompleteAssessment();

            Debug.Log(
                "SAFEX FIRE ASSESSMENT RESULT: " +
                (passed ? "PASSED" : "INCOMPLETE")
            );

            if (SAFEXCloudSync.Instance != null)
            {
                SAFEXCloudSync.Instance.SendAssessment();
                SAFEXCloudSync.Instance.CompleteSession(passed);
            }
        }

        trainingCompleted = true;
        trainingActive = false;

        if (actionButton != null)
            actionButton.gameObject.SetActive(false);

        SetTop(
            "SAFEX // TRAINING COMPLETE",
            "● SUCCESS"
        );

        SetMessage(
            "FIRE TRAINING COMPLETED",
            "Emergency response sequence completed successfully."
        );

        SetStatus(
            "ASSESSMENT COMPLETE"
        );

        StartCoroutine(FinishTrainingAfterDelay());
    }

    private IEnumerator FinishTrainingAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);

        HideTrainingUI();

        if (flowController != null)
            flowController.CompleteTraining();
    }

    // =========================================================
    // HUD
    // =========================================================

    private void CreateTrainingHUD()
    {
        if (canvas != null)
            Destroy(canvas.gameObject);

        GameObject canvasObject =
            new GameObject("SAFEX FIRE SIMPLE HUD");

        canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 100;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080, 1920);

        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        canvasGroup =
            canvasObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;

        CreateTopHUD();
        CreateMessagePanel();
        CreateActionButton();
        CreateBottomStatus();
    }

    private void CreateTopHUD()
    {
        GameObject panel =
            CreatePanel(
                "TOP HUD",
                new Vector2(0, 770),
                new Vector2(940, 65),
                new Color(0.005f, 0.012f, 0.016f, 0.78f)
            );

        topText =
            CreateText(
                panel.transform,
                "SAFEX // FIRE RESPONSE",
                new Vector2(-210, 0),
                new Vector2(500, 50),
                14,
                Color.white,
                TextAnchor.MiddleLeft
            );

        CreateText(
            panel.transform,
            "● LIVE",
            new Vector2(325, 0),
            new Vector2(160, 50),
            12,
            fireAccent,
            TextAnchor.MiddleRight
        );
    }

    private void CreateMessagePanel()
    {
        GameObject panel =
            CreatePanel(
                "MESSAGE PANEL",
                new Vector2(0, 570),
                new Vector2(800, 150),
                new Color(0.008f, 0.018f, 0.023f, 0.82f)
            );

        CreateBorder(
            panel.transform,
            new Color(
                fireAccent.r,
                fireAccent.g,
                fireAccent.b,
                0.55f
            )
        );

        messageText =
            CreateText(
                panel.transform,
                "",
                new Vector2(0, 5),
                new Vector2(730, 120),
                17,
                Color.white,
                TextAnchor.MiddleCenter
            );
    }

    private void CreateActionButton()
    {
        GameObject buttonObject =
            CreatePanel(
                "ACTION REQUIRED",
                new Vector2(0, -500),
                new Vector2(760, 120),
                new Color(
                    fireAccent.r,
                    fireAccent.g,
                    fireAccent.b,
                    0.22f
                )
            );

        CreateBorder(
            buttonObject.transform,
            new Color(
                fireAccent.r,
                fireAccent.g,
                fireAccent.b,
                0.90f
            )
        );

        actionButton =
            buttonObject.AddComponent<Button>();

        actionButton.targetGraphic =
            buttonObject.GetComponent<Image>();

        ColorBlock colors =
            actionButton.colors;

        colors.normalColor =
            new Color(
                fireAccent.r,
                fireAccent.g,
                fireAccent.b,
                0.22f
            );

        colors.highlightedColor =
            new Color(
                fireAccent.r,
                fireAccent.g,
                fireAccent.b,
                0.34f
            );

        colors.pressedColor =
            new Color(
                fireAccent.r,
                fireAccent.g,
                fireAccent.b,
                0.50f
            );

        actionButton.colors = colors;

        actionButtonText =
            CreateText(
                buttonObject.transform,
                "ACTION",
                Vector2.zero,
                new Vector2(710, 100),
                20,
                Color.white,
                TextAnchor.MiddleCenter
            );

        actionButton.onClick.AddListener(
            OnFireActionButtonPressed
        );

        actionButton.gameObject.SetActive(false);
    }

    private void ShowActionButton(string label)
    {
        if (actionButton == null)
            return;

        if (actionButtonText != null)
            actionButtonText.text = label;

        actionButton.gameObject.SetActive(true);
    }

    private void CreateBottomStatus()
    {
        GameObject panel =
            CreatePanel(
                "BOTTOM STATUS",
                new Vector2(0, -760),
                new Vector2(940, 58),
                new Color(0.005f, 0.012f, 0.016f, 0.72f)
            );

        statusText =
            CreateText(
                panel.transform,
                "ACTION 1 OF 4",
                Vector2.zero,
                new Vector2(850, 45),
                11,
                fireAccent,
                TextAnchor.MiddleCenter
            );
    }

    private void SetTop(
        string left,
        string right)
    {
        if (topText != null)
            topText.text =
                left + "                         " + right;
    }

    private void SetMessage(
        string title,
        string body)
    {
        if (messageText != null)
        {
            messageText.text =
                title + "\n" +
                body;
        }
    }

    private void SetStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }

    // =========================================================
    // STOP / HIDE
    // =========================================================

    private void StopTraining()
    {
        trainingActive = false;

        if (actionButton != null)
            actionButton.gameObject.SetActive(false);

        HideTrainingUI();
    }

    private void HideTrainingUI()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private GameObject CreatePanel(
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject panel =
            new GameObject(objectName);

        panel.transform.SetParent(
            canvas.transform,
            false
        );

        Image image =
            panel.AddComponent<Image>();

        image.color = color;

        RectTransform rect =
            panel.GetComponent<RectTransform>();

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

        return panel;
    }

    private void CreateBorder(
        Transform parent,
        Color color)
    {
        GameObject border =
            new GameObject("BORDER");

        border.transform.SetParent(
            parent,
            false
        );

        Image image =
            border.AddComponent<Image>();

        image.color = color;
        image.raycastTarget = false;

        RectTransform rect =
            border.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        image.color =
            new Color(
                color.r,
                color.g,
                color.b,
                0.12f
            );
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

        txt.text = text;

        txt.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.color = color;

        txt.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        txt.verticalOverflow =
            VerticalWrapMode.Overflow;

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
}
