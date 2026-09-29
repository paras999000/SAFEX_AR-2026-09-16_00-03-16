using System;
using UnityEngine;
using UnityEngine.UI;
using OrbitalNine.QRCode;

/// <summary>
/// SAFEX Certificate Manager
///
/// Creates the final SAFEX completion certificate.
/// No quiz, score or MCQ assessment is used.
///
/// Certificate contains:
/// - SAFEX branding
/// - Completed training module
/// - Completion status
/// - Unique certificate ID
/// - Issue date
/// - QR verification code
/// </summary>
public class CertificateManager : MonoBehaviour
{
    [Header("SAFEX Flow")]
    [SerializeField]
    private SAFEXFlowController flowController;

    [Header("Scene QR")]
    [SerializeField]
    private GameObject certificateQR;

    private Canvas certificateCanvas;
    private QRCodeGenerator qrGenerator;

    private string certificateID;
    private string issueDate;

    private void Awake()
    {
        if (flowController == null)
            flowController = FindFirstObjectByType<SAFEXFlowController>();

        HideSceneQR();

        if (flowController != null)
            flowController.StateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        if (flowController != null)
            flowController.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(SAFEXFlowController.SAFEXState state)
    {
        if (state == SAFEXFlowController.SAFEXState.TrainingComplete)
        {
            Debug.Log("SAFEX: Training completed. Opening certificate.");

            if (flowController != null)
                flowController.ShowCertificate();
        }
        else if (state == SAFEXFlowController.SAFEXState.Certificate)
        {
            ShowCertificate();
        }
        else
        {
            HideCertificate();
        }
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public void ShowCertificate()
    {
        string moduleName = "SAFETY TRAINING";

        if (flowController != null)
        {
            switch (flowController.SelectedModule)
            {
                case SAFEXFlowController.TrainingModule.Fire:
                    moduleName = "FIRE & EXPLOSION RESPONSE";
                    break;

                case SAFEXFlowController.TrainingModule.Gas:
                    moduleName = "GAS LEAK & CONFINED SPACE";
                    break;
            }
        }

        CreateCertificateUI(moduleName);
    }

    // Optional public method if you ever want to open it manually.
    public void ShowCertificate(string moduleName)
    {
        CreateCertificateUI(moduleName);
    }

    public void HideCertificate()
    {
        if (certificateCanvas != null)
            certificateCanvas.gameObject.SetActive(false);
    }

    // =========================================================
    // CERTIFICATE CREATION
    // =========================================================

    private void CreateCertificateUI(string moduleName)
    {
        if (certificateCanvas != null)
            Destroy(certificateCanvas.gameObject);

        certificateID = GenerateCertificateID();
        issueDate = DateTime.Now.ToString("dd MMMM yyyy");

        string employeeName = GetEmployeeName();
        string employeeID = GetEmployeeID();

        GameObject canvasObject = new GameObject("SAFEX CERTIFICATE");

        certificateCanvas = canvasObject.AddComponent<Canvas>();
        certificateCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        certificateCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        // =====================================================
        // PREMIUM BACKGROUND
        // =====================================================

        GameObject background = CreatePanel(
            "Certificate Background",
            canvasObject.transform,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(1080, 1920)
        );

        background.GetComponent<Image>().color =
            new Color(0.025f, 0.045f, 0.055f, 1f);

        GameObject outerFrame = CreatePanel(
            "Certificate Outer Frame",
            canvasObject.transform,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(1000, 1760)
        );

        outerFrame.GetComponent<Image>().color =
            new Color(0.78f, 0.62f, 0.22f, 1f);

        GameObject certificate = CreatePanel(
            "Certificate Card",
            canvasObject.transform,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            new Vector2(970, 1730)
        );

        certificate.GetComponent<Image>().color =
            new Color(0.975f, 0.985f, 0.98f, 1f);

        // =====================================================
        // HEADER
        // =====================================================

        CreateText(
            certificate.transform,
            "SAFEX",
            58,
            FontStyle.Bold,
            new Color(0.035f, 0.18f, 0.22f),
            new Vector2(0, 745),
            new Vector2(820, 80),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "AR-BASED INDUSTRIAL SAFETY TRAINING",
            18,
            FontStyle.Bold,
            new Color(0.38f, 0.43f, 0.44f),
            new Vector2(0, 690),
            new Vector2(820, 40),
            TextAnchor.MiddleCenter
        );

        CreateAccentLine(
            certificate.transform,
            new Vector2(0, 642),
            new Vector2(650, 5)
        );

        CreateText(
            certificate.transform,
            "CERTIFICATE",
            21,
            FontStyle.Bold,
            new Color(0.55f, 0.43f, 0.16f),
            new Vector2(0, 585),
            new Vector2(800, 40),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "OF COMPLETION",
            39,
            FontStyle.Bold,
            new Color(0.035f, 0.16f, 0.20f),
            new Vector2(0, 525),
            new Vector2(850, 70),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "This certificate confirms that",
            20,
            FontStyle.Normal,
            new Color(0.38f, 0.40f, 0.41f),
            new Vector2(0, 455),
            new Vector2(800, 40),
            TextAnchor.MiddleCenter
        );

        // =====================================================
        // EMPLOYEE NAME
        // =====================================================

        CreateText(
            certificate.transform,
            employeeName,
            38,
            FontStyle.Bold,
            new Color(0.035f, 0.18f, 0.22f),
            new Vector2(0, 390),
            new Vector2(820, 70),
            TextAnchor.MiddleCenter
        );

        CreateAccentLine(
            certificate.transform,
            new Vector2(0, 350),
            new Vector2(520, 3)
        );

        CreateText(
            certificate.transform,
            "Employee ID: " + employeeID,
            17,
            FontStyle.Normal,
            new Color(0.40f, 0.43f, 0.44f),
            new Vector2(0, 315),
            new Vector2(700, 38),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "has successfully completed the following safety training",
            18,
            FontStyle.Normal,
            new Color(0.38f, 0.40f, 0.41f),
            new Vector2(0, 270),
            new Vector2(820, 45),
            TextAnchor.MiddleCenter
        );

        // =====================================================
        // MODULE CARD
        // =====================================================

        GameObject moduleCard = CreatePanel(
            "Training Module Card",
            certificate.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(-5, 165),
            new Vector2(850, 145)
        );

        moduleCard.GetComponent<Image>().color =
            new Color(0.935f, 0.955f, 0.95f, 1f);

        CreateText(
            moduleCard.transform,
            "TRAINING MODULE",
            15,
            FontStyle.Bold,
            new Color(0.42f, 0.45f, 0.45f),
            new Vector2(0, 42),
            new Vector2(750, 32),
            TextAnchor.MiddleCenter
        );

        CreateText(
            moduleCard.transform,
            moduleName,
            24,
            FontStyle.Bold,
            new Color(0.035f, 0.18f, 0.22f),
            new Vector2(0, -10),
            new Vector2(780, 58),
            TextAnchor.MiddleCenter
        );

        CreateText(
            moduleCard.transform,
            "SAFEX INDUSTRIAL SAFETY SIMULATION",
            14,
            FontStyle.Normal,
            new Color(0.43f, 0.46f, 0.46f),
            new Vector2(0, -48),
            new Vector2(700, 28),
            TextAnchor.MiddleCenter
        );

        // =====================================================
        // VERIFIED STATUS
        // =====================================================

        GameObject statusBadge = CreatePanel(
            "Verified Status",
            certificate.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(-250, -15),
            new Vector2(390, 100)
        );

        statusBadge.GetComponent<Image>().color =
            new Color(0.90f, 0.96f, 0.93f, 1f);

        CreateText(
            statusBadge.transform,
            "VERIFIED",
            23,
            FontStyle.Bold,
            new Color(0.04f, 0.40f, 0.25f),
            new Vector2(0, 12),
            new Vector2(350, 38),
            TextAnchor.MiddleCenter
        );

        CreateText(
            statusBadge.transform,
            "TRAINING COMPLETED",
            14,
            FontStyle.Bold,
            new Color(0.30f, 0.40f, 0.35f),
            new Vector2(0, -22),
            new Vector2(350, 30),
            TextAnchor.MiddleCenter
        );

        // =====================================================
        // QR VERIFICATION
        // =====================================================

        GameObject qrCard = CreatePanel(
            "QR Verification Card",
            certificate.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(250, -15),
            new Vector2(390, 270)
        );

        qrCard.GetComponent<Image>().color =
            new Color(0.975f, 0.98f, 0.975f, 1f);

        CreateText(
            qrCard.transform,
            "SCAN TO VERIFY",
            16,
            FontStyle.Bold,
            new Color(0.30f, 0.34f, 0.35f),
            new Vector2(0, 102),
            new Vector2(330, 32),
            TextAnchor.MiddleCenter
        );

        CreateText(
            qrCard.transform,
            "Credential authenticity",
            13,
            FontStyle.Normal,
            new Color(0.48f, 0.50f, 0.50f),
            new Vector2(0, 78),
            new Vector2(330, 25),
            TextAnchor.MiddleCenter
        );

        CreateCertificateQR(
            certificate.transform,
            certificateID,
            moduleName,
            issueDate
        );

        // =====================================================
        // DETAILS
        // =====================================================

        CreateText(
            certificate.transform,
            "CERTIFICATE ID",
            14,
            FontStyle.Bold,
            new Color(0.42f, 0.44f, 0.45f),
            new Vector2(-250, -155),
            new Vector2(380, 30),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            certificateID,
            18,
            FontStyle.Bold,
            new Color(0.035f, 0.16f, 0.20f),
            new Vector2(-250, -190),
            new Vector2(430, 42),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "ISSUE DATE",
            14,
            FontStyle.Bold,
            new Color(0.42f, 0.44f, 0.45f),
            new Vector2(250, -155),
            new Vector2(380, 30),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            issueDate,
            18,
            FontStyle.Normal,
            new Color(0.035f, 0.16f, 0.20f),
            new Vector2(250, -190),
            new Vector2(430, 42),
            TextAnchor.MiddleCenter
        );

        CreateAccentLine(
            certificate.transform,
            new Vector2(0, -245),
            new Vector2(760, 2)
        );

        // =====================================================
        // AUTHORITY
        // =====================================================

        CreateText(
            certificate.transform,
            "SAFEX TRAINING SYSTEM",
            17,
            FontStyle.Bold,
            new Color(0.05f, 0.19f, 0.22f),
            new Vector2(-250, -300),
            new Vector2(390, 35),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "Authorized Digital Certificate",
            13,
            FontStyle.Normal,
            new Color(0.46f, 0.48f, 0.48f),
            new Vector2(-250, -330),
            new Vector2(390, 28),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "DIGITALLY VERIFIED",
            15,
            FontStyle.Bold,
            new Color(0.55f, 0.43f, 0.16f),
            new Vector2(250, -300),
            new Vector2(390, 35),
            TextAnchor.MiddleCenter
        );

        CreateText(
            certificate.transform,
            "QR credential validation enabled",
            13,
            FontStyle.Normal,
            new Color(0.46f, 0.48f, 0.48f),
            new Vector2(250, -330),
            new Vector2(390, 28),
            TextAnchor.MiddleCenter
        );

        // =====================================================
        // FOOTER
        // =====================================================

        CreateText(
            certificate.transform,
            "SAFEX  •  AR-BASED INDUSTRIAL SAFETY TRAINING",
            14,
            FontStyle.Normal,
            new Color(0.43f, 0.46f, 0.47f),
            new Vector2(0, -765),
            new Vector2(820, 35),
            TextAnchor.MiddleCenter
        );

        Debug.Log(
            "SAFEX: Professional certificate generated. " +
            "Employee = " + employeeName +
            " | ID = " + employeeID +
            " | Certificate = " + certificateID
        );

        if (SAFEXCloudSync.Instance != null)
        {
            float percentage = 0f;
            int score = 0;

            if (SAFEXAssessmentManager.Instance != null)
            {
                percentage = SAFEXAssessmentManager.Instance.GetPercentage();
                score = SAFEXAssessmentManager.Instance.GetRequiredActionsCompletedCount();
            }

            SAFEXCloudSync.Instance.SendCertificate(
                certificateID,
                score,
                percentage,
                "ISSUED"
            );
        }
    }

    private string GetEmployeeName()
    {
        string name = "";

        if (SAFEXEmployeeRegistration.Instance != null)
            name = SAFEXEmployeeRegistration.CurrentTraineeName;

        if (string.IsNullOrWhiteSpace(name))
            name = PlayerPrefs.GetString("SAFEX_TRAINEE_NAME", "");

        if (string.IsNullOrWhiteSpace(name))
            name = "TRAINING EMPLOYEE";

        return name.Trim().ToUpperInvariant();
    }

    private string GetEmployeeID()
    {
        string id = "";

        if (SAFEXEmployeeRegistration.Instance != null)
            id = SAFEXEmployeeRegistration.CurrentTraineeId;

        if (string.IsNullOrWhiteSpace(id))
            id = PlayerPrefs.GetString("SAFEX_TRAINEE_ID", "");

        if (string.IsNullOrWhiteSpace(id))
            id = "NOT PROVIDED";

        return id.Trim().ToUpperInvariant();
    }

    private void CreateAccentLine(
        Transform parent,
        Vector2 position,
        Vector2 size
    )
    {
        GameObject line = CreatePanel(
            "Accent Line",
            parent,
            new Vector2(0.5f, 0.5f),
            position,
            size
        );

        line.GetComponent<Image>().color =
            new Color(0.70f, 0.52f, 0.18f, 1f);
    }

    // =========================================================
    // QR CODE
    // =========================================================

    private void CreateCertificateQR(
        Transform certificateTransform,
        string id,
        string moduleName,
        string date
    )
    {
        if (certificateQR == null)
        {
            GameObject qrObject = GameObject.Find("CertificateQR");

            if (qrObject != null)
                certificateQR = qrObject;
        }

        if (certificateQR == null)
        {
            Debug.LogError(
                "SAFEX: CertificateQR GameObject not found in scene."
            );

            return;
        }

        qrGenerator = certificateQR.GetComponent<QRCodeGenerator>();

        if (qrGenerator == null)
        {
            Debug.LogError(
                "SAFEX: QRCodeGenerator component not found on CertificateQR."
            );

            return;
        }

        // Move QR into certificate canvas.
        certificateQR.transform.SetParent(
            certificateTransform,
            false
        );

        certificateQR.SetActive(true);

        RectTransform qrRect =
            certificateQR.GetComponent<RectTransform>();

        if (qrRect != null)
        {
            qrRect.anchorMin = new Vector2(0.5f, 0.5f);
            qrRect.anchorMax = new Vector2(0.5f, 0.5f);
            qrRect.pivot = new Vector2(0.5f, 0.5f);

            qrRect.anchoredPosition =
                new Vector2(260, -40);

            qrRect.sizeDelta =
                new Vector2(230, 230);
        }

        string qrData =
            "SAFEX CERTIFICATE\n" +
            "ID: " + id + "\n" +
            "MODULE: " + moduleName + "\n" +
            "STATUS: TRAINING COMPLETED\n" +
            "ISSUED: " + date;

        qrGenerator.GenerateQRCode(qrData);

        Debug.Log(
            "SAFEX: Certificate QR generated.\n" +
            qrData
        );
    }

    // =========================================================
    // HIDE ORIGINAL SCENE QR
    // =========================================================

    private void HideSceneQR()
    {
        if (certificateQR == null)
        {
            GameObject qrObject = GameObject.Find("CertificateQR");

            if (qrObject != null)
                certificateQR = qrObject;
        }

        if (certificateQR != null)
        {
            qrGenerator =
                certificateQR.GetComponent<QRCodeGenerator>();

            certificateQR.SetActive(false);
        }
    }

    // =========================================================
    // CERTIFICATE ID
    // =========================================================

    private string GenerateCertificateID()
    {
        string randomPart =
            UnityEngine.Random.Range(1000, 9999).ToString();

        return
            "SAFEX-" +
            DateTime.Now.ToString("yyyyMMdd") +
            "-" +
            randomPart;
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private GameObject CreatePanel(
        string objectName,
        Transform parent,
        Vector2 anchor,
        Vector2 position,
        Vector2 size
    )
    {
        GameObject panel = new GameObject(objectName);

        panel.transform.SetParent(parent, false);

        RectTransform rect =
            panel.AddComponent<RectTransform>();

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);

        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = panel.AddComponent<Image>();

        return panel;
    }

    private GameObject CreateText(
        Transform parent,
        string text,
        int fontSize,
        FontStyle style,
        Color color,
        Vector2 position,
        Vector2 size,
        TextAnchor alignment
    )
    {
        GameObject textObject =
            new GameObject("Text");

        textObject.transform.SetParent(
            parent,
            false
        );

        RectTransform rect =
            textObject.AddComponent<RectTransform>();

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

        Text uiText =
            textObject.AddComponent<Text>();

        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.fontStyle = style;
        uiText.color = color;
        uiText.alignment = alignment;
        uiText.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        uiText.verticalOverflow =
            VerticalWrapMode.Overflow;

        uiText.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        return textObject;
    }
}