using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// SAFEX Employee Registration - DIRECT BACKEND VERSION
///
/// This version does NOT depend on RegisterTrainee() existing inside
/// SAFEXCloudSyncManager.
///
/// It directly sends:
/// POST https://safex-arnx.onrender.com/api/trainees
///
/// Header:
/// X-SAFEX-API-KEY
///
/// This is intentionally separate from SAFEXCloudSyncManager so it
/// cannot fail because of a RegisterTrainee method/signature mismatch.
/// </summary>
public class SAFEXEmployeeRegistration : MonoBehaviour
{
    public static SAFEXEmployeeRegistration Instance { get; private set; }

    [Header("Backend")]
    [SerializeField]
    private string apiBaseUrl =
        "https://safex-arnx.onrender.com/api";

    [SerializeField]
    private string apiKey =
        "SAFEX-AR-SAFETY-KEY-2026";

    [Header("Behaviour")]
    [SerializeField] private bool rememberEmployee = true;
    [SerializeField] private bool showOnModuleSelection = true;
    [SerializeField] private bool continueOfflineIfBackendFails = true;

    private const string ID_KEY = "SAFEX_TRAINEE_ID";
    private const string NAME_KEY = "SAFEX_TRAINEE_NAME";
    private const string LANG_KEY = "SAFEX_TRAINEE_LANGUAGE";

    public static string CurrentTraineeId { get; private set; } = "";
    public static string CurrentTraineeName { get; private set; } = "";
    public static string CurrentLanguage { get; private set; } = "en";

    private Canvas canvas;
    private GameObject panel;
    private InputField nameInput;
    private InputField idInput;
    private Dropdown languageDropdown;
    private Text statusText;
    private Button continueButton;

    private SAFEXFlowController flow;

    [Serializable]
    private class TraineePayload
    {
        public string traineeId;
        public string name;
        public string language;
        public string deviceId;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (rememberEmployee)
        {
            CurrentTraineeId = PlayerPrefs.GetString(ID_KEY, "");
            CurrentTraineeName = PlayerPrefs.GetString(NAME_KEY, "");
            CurrentLanguage = PlayerPrefs.GetString(LANG_KEY, "en");
        }
    }

    private void Start()
    {
        flow = SAFEXFlowController.Instance;

        BuildUI();

        if (flow != null)
        {
            flow.StateChanged += OnStateChanged;

            if (showOnModuleSelection &&
                flow.CurrentState == SAFEXFlowController.SAFEXState.ModuleSelection)
            {
                ShowOrSkip();
            }
        }
    }

    private void OnDestroy()
    {
        if (flow != null)
        {
            flow.StateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged(SAFEXFlowController.SAFEXState state)
    {
        if (!showOnModuleSelection)
            return;

        if (state == SAFEXFlowController.SAFEXState.ModuleSelection)
        {
            ShowOrSkip();
        }
        else
        {
            Hide();
        }
    }

    private void ShowOrSkip()
    {
        if (HasEmployee())
        {
            Hide();
            return;
        }

        Show();
    }

    public void Show()
    {
        if (panel == null)
            BuildUI();

        panel.SetActive(true);

        if (nameInput != null)
            nameInput.text = CurrentTraineeName;

        if (idInput != null)
            idInput.text = CurrentTraineeId;

        if (statusText != null)
            statusText.text = "";
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void BuildUI()
    {
        if (canvas != null)
            return;

        GameObject c = new GameObject(
            "SAFEX Employee Registration Canvas"
        );

        c.transform.SetParent(transform, false);

        canvas = c.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = c.AddComponent<CanvasScaler>();
        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        c.AddComponent<GraphicRaycaster>();

        panel = new GameObject(
            "Employee Registration Panel"
        );

        panel.transform.SetParent(c.transform, false);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(
            0.035f,
            0.045f,
            0.055f,
            0.98f
        );

        RectTransform pr =
            panel.GetComponent<RectTransform>();

        pr.anchorMin = new Vector2(0.05f, 0.10f);
        pr.anchorMax = new Vector2(0.95f, 0.90f);
        pr.offsetMin = Vector2.zero;
        pr.offsetMax = Vector2.zero;

        AddText(
            panel.transform,
            "SAFEX",
            48,
            0.78f,
            0.90f,
            TextAnchor.MiddleCenter
        );

        AddText(
            panel.transform,
            "EMPLOYEE DETAILS",
            26,
            0.71f,
            0.78f,
            TextAnchor.MiddleCenter
        );

        AddText(
            panel.transform,
            "Employee Name",
            22,
            0.62f,
            0.67f,
            TextAnchor.MiddleLeft
        );

        nameInput = AddInput(
            panel.transform,
            "Enter employee name",
            0.53f,
            0.60f
        );

        AddText(
            panel.transform,
            "Employee ID",
            22,
            0.43f,
            0.48f,
            TextAnchor.MiddleLeft
        );

        idInput = AddInput(
            panel.transform,
            "Enter employee ID",
            0.34f,
            0.41f
        );

        AddText(
            panel.transform,
            "Language",
            22,
            0.24f,
            0.29f,
            TextAnchor.MiddleLeft
        );

        languageDropdown = AddDropdown(
            panel.transform,
            0.15f,
            0.22f
        );

        continueButton = AddButton(
            panel.transform,
            "CONTINUE TO TRAINING",
            0.045f,
            0.125f,
            RegisterEmployee
        );

        statusText = AddText(
            panel.transform,
            "",
            18,
            0.005f,
            0.04f,
            TextAnchor.MiddleCenter
        );

        if (!string.IsNullOrEmpty(CurrentTraineeName))
            nameInput.text = CurrentTraineeName;

        if (!string.IsNullOrEmpty(CurrentTraineeId))
            idInput.text = CurrentTraineeId;

        SetLanguage(CurrentLanguage);

        panel.SetActive(false);
    }

    private Text AddText(
        Transform parent,
        string value,
        int size,
        float minY,
        float maxY,
        TextAnchor alignment)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);

        Text t = go.AddComponent<Text>();

        t.text = value;
        t.fontSize = size;
        t.alignment = alignment;
        t.color = Color.white;
        t.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        t.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        t.verticalOverflow =
            VerticalWrapMode.Overflow;

        RectTransform r =
            t.GetComponent<RectTransform>();

        r.anchorMin =
            new Vector2(0.10f, minY);

        r.anchorMax =
            new Vector2(0.90f, maxY);

        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        return t;
    }

    private InputField AddInput(
        Transform parent,
        string placeholder,
        float minY,
        float maxY)
    {
        GameObject go = new GameObject(
            "InputField"
        );

        go.transform.SetParent(parent, false);

        Image bg = go.AddComponent<Image>();
        bg.color =
            new Color(0.10f, 0.12f, 0.14f, 1f);

        InputField input =
            go.AddComponent<InputField>();

        Text text =
            AddChildText(go.transform, "", 21);

        Text hint =
            AddChildText(
                go.transform,
                placeholder,
                21
            );

        hint.color =
            new Color(0.55f, 0.58f, 0.62f);

        input.textComponent = text;
        input.placeholder = hint;

        RectTransform r =
            go.GetComponent<RectTransform>();

        r.anchorMin =
            new Vector2(0.10f, minY);

        r.anchorMax =
            new Vector2(0.90f, maxY);

        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        return input;
    }

    private Text AddChildText(
        Transform parent,
        string value,
        int size)
    {
        GameObject go =
            new GameObject("Text");

        go.transform.SetParent(
            parent,
            false
        );

        Text t =
            go.AddComponent<Text>();

        t.text = value;
        t.fontSize = size;
        t.alignment =
            TextAnchor.MiddleLeft;

        t.color = Color.white;

        t.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        RectTransform r =
            t.GetComponent<RectTransform>();

        r.anchorMin =
            new Vector2(0.04f, 0.05f);

        r.anchorMax =
            new Vector2(0.96f, 0.95f);

        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        return t;
    }

    private Dropdown AddDropdown(
        Transform parent,
        float minY,
        float maxY)
    {
        GameObject go =
            new GameObject(
                "Language Dropdown"
            );

        go.transform.SetParent(
            parent,
            false
        );

        Image bg =
            go.AddComponent<Image>();

        bg.color =
            new Color(0.10f, 0.12f, 0.14f, 1f);

        Dropdown d =
            go.AddComponent<Dropdown>();

        Text caption =
            AddChildText(
                go.transform,
                "English",
                21
            );

        Text item =
            AddChildText(
                go.transform,
                "English",
                21
            );

        d.captionText = caption;
        d.itemText = item;

        d.options.Clear();

        d.options.Add(
            new Dropdown.OptionData("English")
        );

        d.options.Add(
            new Dropdown.OptionData("Hindi")
        );

        d.options.Add(
            new Dropdown.OptionData("Santali")
        );

        d.onValueChanged.AddListener(
            LanguageChanged
        );

        RectTransform r =
            go.GetComponent<RectTransform>();

        r.anchorMin =
            new Vector2(0.10f, minY);

        r.anchorMax =
            new Vector2(0.90f, maxY);

        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        return d;
    }

    private void LanguageChanged(int index)
    {
        if (index == 1)
        {
            CurrentLanguage = "hi";
        }
        else if (index == 2)
        {
            CurrentLanguage = "sat";
        }
        else
        {
            CurrentLanguage = "en";
        }
    }

    private void SetLanguage(string language)
    {
        if (languageDropdown == null)
            return;

        int index = 0;

        if (language == "hi")
            index = 1;
        else if (language == "sat")
            index = 2;

        languageDropdown.SetValueWithoutNotify(
            index
        );
    }

    private Button AddButton(
        Transform parent,
        string value,
        float minY,
        float maxY,
        UnityEngine.Events.UnityAction action)
    {
        GameObject go =
            new GameObject(
                "Continue Button"
            );

        go.transform.SetParent(
            parent,
            false
        );

        Image bg =
            go.AddComponent<Image>();

        bg.color =
            new Color(0.85f, 0.65f, 0.15f, 1f);

        Button b =
            go.AddComponent<Button>();

        b.onClick.AddListener(action);

        Text t =
            AddChildText(
                go.transform,
                value,
                22
            );

        t.alignment =
            TextAnchor.MiddleCenter;

        t.color = Color.black;

        RectTransform r =
            go.GetComponent<RectTransform>();

        r.anchorMin =
            new Vector2(0.10f, minY);

        r.anchorMax =
            new Vector2(0.90f, maxY);

        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        return b;
    }

    private void RegisterEmployee()
    {
        string employeeName =
            nameInput != null
                ? nameInput.text.Trim()
                : "";

        string employeeId =
            idInput != null
                ? idInput.text.Trim()
                : "";

        if (string.IsNullOrEmpty(employeeName))
        {
            SetStatus(
                "Please enter employee name."
            );

            return;
        }

        if (string.IsNullOrEmpty(employeeId))
        {
            SetStatus(
                "Please enter employee ID."
            );

            return;
        }

        CurrentTraineeName = employeeName;
        CurrentTraineeId = employeeId;

        if (rememberEmployee)
        {
            PlayerPrefs.SetString(
                NAME_KEY,
                CurrentTraineeName
            );

            PlayerPrefs.SetString(
                ID_KEY,
                CurrentTraineeId
            );

            PlayerPrefs.SetString(
                LANG_KEY,
                CurrentLanguage
            );

            PlayerPrefs.Save();
        }

        if (continueButton != null)
            continueButton.interactable = false;

        SetStatus(
            "Registering employee..."
        );

        StartCoroutine(
            RegisterEmployeeRequest()
        );
    }

    private IEnumerator RegisterEmployeeRequest()
    {
        TraineePayload payload =
            new TraineePayload
            {
                traineeId =
                    CurrentTraineeId,

                name =
                    CurrentTraineeName,

                language =
                    CurrentLanguage,

                deviceId =
                    SystemInfo.deviceUniqueIdentifier
            };

        string json =
            JsonUtility.ToJson(payload);

        string baseUrl =
            string.IsNullOrWhiteSpace(apiBaseUrl)
                ? "https://safex-arnx.onrender.com/api"
                : apiBaseUrl.TrimEnd('/');

        string url =
            baseUrl + "/trainees";

        using (UnityWebRequest request =
            new UnityWebRequest(
                url,
                UnityWebRequest.kHttpVerbPOST))
        {
            byte[] body =
                System.Text.Encoding.UTF8.GetBytes(
                    json
                );

            request.uploadHandler =
                new UploadHandlerRaw(body);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.SetRequestHeader(
                    "X-SAFEX-API-KEY",
                    apiKey
                );
            }

            request.timeout = 15;

            Debug.Log(
                "[SAFEX Employee] POST " +
                url
            );

            Debug.Log(
                "[SAFEX Employee] Payload: " +
                json
            );

            yield return request.SendWebRequest();

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                Debug.Log(
                    "[SAFEX Employee] Backend registration SUCCESS. " +
                    request.responseCode +
                    " " +
                    request.downloadHandler.text
                );

                SetStatus(
                    "Employee registered successfully."
                );

                Hide();
                yield break;
            }

            string error =
                request.error;

            string response =
                request.downloadHandler != null
                    ? request.downloadHandler.text
                    : "";

            Debug.LogError(
                "[SAFEX Employee] Backend registration FAILED. " +
                "HTTP " +
                request.responseCode +
                " Error: " +
                error +
                " Response: " +
                response
            );

            if (continueOfflineIfBackendFails)
            {
                SetStatus(
                    "Saved locally. Cloud registration will be retried later."
                );

                Hide();
            }
            else
            {
                if (continueButton != null)
                    continueButton.interactable = true;

                SetStatus(
                    "Registration failed: " +
                    error
                );
            }
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log(
            "[SAFEX Employee] " +
            message
        );
    }

    public void ClearEmployee()
    {
        CurrentTraineeId = "";
        CurrentTraineeName = "";
        CurrentLanguage = "en";

        PlayerPrefs.DeleteKey(ID_KEY);
        PlayerPrefs.DeleteKey(NAME_KEY);
        PlayerPrefs.DeleteKey(LANG_KEY);

        PlayerPrefs.Save();

        Show();
    }

    public static bool HasEmployee()
    {
        return
            !string.IsNullOrEmpty(
                CurrentTraineeId
            )
            &&
            !string.IsNullOrEmpty(
                CurrentTraineeName
            );
    }
}
