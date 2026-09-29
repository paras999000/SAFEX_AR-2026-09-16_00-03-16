using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SAFEX Gas Training Controller - Simple Mobile Flow.
///
/// No green route.
/// No spatial checkpoints.
/// PPE and Buddy System remain as real training steps.
///
/// FLOW:
/// Gas detected
/// -> Reach Gas Detector
/// -> Raise Alarm
/// -> Select Correct PPE
/// -> Buddy Confirmed
/// -> Isolate Contaminated Area
/// -> Complete Evacuation
/// -> Training Complete
/// -> Certificate + QR
/// </summary>
public class GasTrainingController : MonoBehaviour
{
    [Header("Flow")]
    [SerializeField] private SAFEXFlowController flowController;

    [Header("Scenario")]
    [SerializeField] private GasLeakScenario gasScenario;

    [Header("UI")]
    [SerializeField]
    private Color gasAccent =
        new Color(0.05f, 0.75f, 0.95f, 1f);

    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Text topText;
    private Text messageText;
    private Text statusText;
    private Button actionButton;
    private Text actionButtonText;

    private bool trainingActive;
    private bool detectorReached;
    private bool alarmRaised;
    private bool ppeCompleted;
    private bool buddyConfirmed;
    private bool areaIsolated;
    private bool trainingCompleted;

    private void Awake()
    {
        if (flowController == null)
            flowController =
                FindFirstObjectByType<SAFEXFlowController>();

        if (gasScenario == null)
            gasScenario =
                FindFirstObjectByType<GasLeakScenario>();
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
            SAFEXFlowController.TrainingModule.Gas)
        {
            StartGasTraining();
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

    private void StartGasTraining()
    {
        if (trainingActive)
            return;

        if (gasScenario == null)
            gasScenario =
                FindFirstObjectByType<GasLeakScenario>();

        trainingActive = true;

        detectorReached = false;
        alarmRaised = false;
        ppeCompleted = false;
        buddyConfirmed = false;
        areaIsolated = false;
        trainingCompleted = false;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.StartAssessment("GAS");
        }
        else
        {
            Debug.LogWarning(
                "SAFEX GAS: Assessment Manager not found."
            );
        }

        CreateTrainingHUD();

        SetTop(
            "SAFEX // GAS RESPONSE",
            "● LIVE"
        );

        SetMessage(
            "GAS HAZARD DETECTED",
            "Start the emergency response sequence. The gas detector is ready."
        );

        SetStatus(
            "ACTION 1 OF 6"
        );

        ShowActionButton(
            "REACH GAS DETECTOR"
        );

        Debug.Log(
            "SAFEX GAS: Simple training started."
        );

        if (SAFEXCloudSyncManager.Instance != null)
        {
            SAFEXCloudSyncManager.Instance.StartSession("GAS");
            SAFEXCloudSyncManager.Instance.SendTrainingEvent("TRAINING_STARTED");
        }
    }

    // =========================================================
    // MAIN ACTION FLOW
    // =========================================================

    private void OnGasActionButtonPressed()
    {
        if (!trainingActive || trainingCompleted)
            return;

        if (!detectorReached)
        {
            CompleteDetectorReach();
            return;
        }

        if (!alarmRaised)
        {
            RaiseAlarm();
            return;
        }

        if (!ppeCompleted)
        {
            ShowPPESelection();
            return;
        }

        if (!buddyConfirmed)
        {
            ShowBuddyCheck();
            return;
        }

        if (!areaIsolated)
        {
            IsolateArea();
            return;
        }

        CompleteEvacuation();
    }

    private void CompleteDetectorReach()
    {
        detectorReached = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "REACH_GAS_DETECTOR"
            );

            if (SAFEXCloudSyncManager.Instance != null)
                SAFEXCloudSyncManager.Instance.SendTrainingEvent("REACH_GAS_DETECTOR");
        }

        SetTop(
            "SAFEX // GAS RESPONSE",
            "● DETECTOR"
        );

        SetMessage(
            "GAS DETECTOR REACHED",
            "Confirm the gas alarm before moving to PPE selection."
        );

        SetStatus(
            "ACTION 2 OF 6"
        );

        ShowActionButton(
            "RAISE ALARM"
        );
    }

    private void RaiseAlarm()
    {
        if (!detectorReached || alarmRaised)
            return;

        alarmRaised = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "RAISE_ALARM"
            );

            if (SAFEXCloudSyncManager.Instance != null)
                SAFEXCloudSyncManager.Instance.SendTrainingEvent("RAISE_ALARM");
        }

        SetTop(
            "SAFEX // GAS RESPONSE",
            "● ALARM RAISED"
        );

        SetMessage(
            "GAS ALARM RAISED",
            "Select the required respiratory protection and safety helmet."
        );

        SetStatus(
            "ACTION 3 OF 6"
        );

        ShowActionButton(
            "SELECT PPE"
        );
    }

    // =========================================================
    // PPE
    // =========================================================

    private void ShowPPESelection()
    {
        if (!alarmRaised || ppeCompleted)
            return;

        if (actionButton != null)
            actionButton.gameObject.SetActive(false);

        SAFEXGasPPESelector selector =
            GetComponent<SAFEXGasPPESelector>();

        if (selector == null)
            selector =
                gameObject.AddComponent<SAFEXGasPPESelector>();

        selector.Show(
            OnPPESelectionCompleted
        );
    }

    private void OnPPESelectionCompleted()
    {
        if (!trainingActive || ppeCompleted)
            return;

        ppeCompleted = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "SELECT_CORRECT_GAS_PPE"
            );

            if (SAFEXCloudSyncManager.Instance != null)
                SAFEXCloudSyncManager.Instance.SendTrainingEvent("SELECT_CORRECT_GAS_PPE");
        }

        SetTop(
            "SAFEX // GAS RESPONSE",
            "● PPE READY"
        );

        SetMessage(
            "PPE CONFIRMED",
            "Confirm that your buddy is with you and ready for the emergency response."
        );

        SetStatus(
            "ACTION 4 OF 6"
        );

        ShowActionButton(
            "BUDDY SYSTEM CHECK"
        );

        Debug.Log(
            "SAFEX GAS: Correct PPE selected."
        );
    }

    // =========================================================
    // BUDDY SYSTEM
    // =========================================================

    private void ShowBuddyCheck()
    {
        if (!ppeCompleted || buddyConfirmed)
            return;

        if (actionButton != null)
            actionButton.gameObject.SetActive(false);

        SAFEXGasBuddyCheck buddyCheck =
            GetComponent<SAFEXGasBuddyCheck>();

        if (buddyCheck == null)
            buddyCheck =
                gameObject.AddComponent<SAFEXGasBuddyCheck>();

        buddyCheck.Show(
            OnBuddyCheckCompleted
        );
    }

    private void OnBuddyCheckCompleted()
    {
        if (!trainingActive || buddyConfirmed)
            return;

        buddyConfirmed = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "BUDDY_CONFIRMED"
            );

            if (SAFEXCloudSyncManager.Instance != null)
                SAFEXCloudSyncManager.Instance.SendTrainingEvent("BUDDY_CONFIRMED");
        }

        SetTop(
            "SAFEX // GAS RESPONSE",
            "● BUDDY CONFIRMED"
        );

        SetMessage(
            "BUDDY SYSTEM CONFIRMED",
            "Now isolate the contaminated area before evacuation."
        );

        SetStatus(
            "ACTION 5 OF 6"
        );

        ShowActionButton(
            "ISOLATE CONTAMINATED AREA"
        );

        Debug.Log(
            "SAFEX GAS: Buddy confirmed."
        );
    }

    // =========================================================
    // ISOLATION
    // =========================================================

    private void IsolateArea()
    {
        if (!buddyConfirmed || areaIsolated)
            return;

        areaIsolated = true;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "ISOLATE_CONTAMINATED_AREA"
            );

            if (SAFEXCloudSyncManager.Instance != null)
                SAFEXCloudSyncManager.Instance.SendTrainingEvent("ISOLATE_CONTAMINATED_AREA");
        }

        if (gasScenario != null)
            gasScenario.StopGasLeak();

        SetTop(
            "SAFEX // GAS RESPONSE",
            "● AREA ISOLATED"
        );

        SetMessage(
            "CONTAMINATED AREA ISOLATED",
            "Gas leak controlled. Complete the evacuation step."
        );

        SetStatus(
            "ACTION 6 OF 6"
        );

        ShowActionButton(
            "COMPLETE EVACUATION"
        );

        Debug.Log(
            "SAFEX GAS: Contaminated area isolated."
        );
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteEvacuation()
    {
        if (!areaIsolated || trainingCompleted)
            return;

        if (SAFEXAssessmentManager.Instance != null)
        {
            SAFEXAssessmentManager.Instance.RecordAction(
                "EVACUATE_SAFE_EXIT"
            );

            bool passed =
                SAFEXAssessmentManager.Instance.CompleteAssessment();

            Debug.Log(
                "SAFEX GAS ASSESSMENT RESULT: " +
                (passed ? "PASSED" : "INCOMPLETE")
            );

            if (SAFEXCloudSyncManager.Instance != null)
            {
                SAFEXCloudSyncManager.Instance.SendAssessment();
                SAFEXCloudSyncManager.Instance.CompleteSession(SAFEXAssessmentManager.Instance.AssessmentPassed);
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
            "GAS TRAINING COMPLETED",
            "Emergency response sequence completed successfully."
        );

        SetStatus(
            "ASSESSMENT COMPLETE"
        );

        StartCoroutine(
            FinishTrainingAfterDelay()
        );
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
            new GameObject("SAFEX GAS SIMPLE HUD");

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
                "SAFEX // GAS RESPONSE",
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
            gasAccent,
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
                gasAccent.r,
                gasAccent.g,
                gasAccent.b,
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
                    gasAccent.r,
                    gasAccent.g,
                    gasAccent.b,
                    0.22f
                )
            );

        CreateBorder(
            buttonObject.transform,
            new Color(
                gasAccent.r,
                gasAccent.g,
                gasAccent.b,
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
                gasAccent.r,
                gasAccent.g,
                gasAccent.b,
                0.22f
            );

        colors.highlightedColor =
            new Color(
                gasAccent.r,
                gasAccent.g,
                gasAccent.b,
                0.34f
            );

        colors.pressedColor =
            new Color(
                gasAccent.r,
                gasAccent.g,
                gasAccent.b,
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
            OnGasActionButtonPressed
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
                "ACTION 1 OF 6",
                Vector2.zero,
                new Vector2(850, 45),
                11,
                gasAccent,
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
