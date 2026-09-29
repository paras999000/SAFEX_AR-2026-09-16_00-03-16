using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class SyncQueueItem
{
    public string endpoint;
    public string method;
    public string payload;
    public string createdAt;
}

[Serializable]
public class SyncQueue
{
    public List<SyncQueueItem> items = new List<SyncQueueItem>();
}

[Serializable]
public class TraineePayload
{
    public string traineeId;
    public string name;
    public string language;
    public string deviceId;
}

[Serializable]
public class SessionPayload
{
    public string sessionId;
    public string traineeId;
    public string module;
    public string status;
    public string startedAt;
    public string completedAt;
    public int durationSeconds;
    public bool assessmentPassed;
    public string certificateId;
}

[Serializable]
public class SerializableEvent
{
    public string sessionId;
    public string eventType;
    public string module;
    public List<StringKeyValue> data = new List<StringKeyValue>();
    public string timestamp;
}

[Serializable]
public class StringKeyValue
{
    public string key;
    public string value;
}

[Serializable]
public class AssessmentPayload
{
    public string sessionId;
    public string module;
    public List<string> completedActions = new List<string>();
    public List<string> requiredActions = new List<string>();
    public int score;
    public bool passed;
    public int durationSeconds;
}

[Serializable]
public class CertificatePayload
{
    public string certificateId;
    public string sessionId;
    public string traineeId;
    public string module;
    public int score;
    public float percentage;
    public string status;
    public string issuedAt;
}

public class SAFEXCloudSyncManager : MonoBehaviour
{
    public static SAFEXCloudSyncManager Instance { get; private set; }

    [Header("API")]
    [SerializeField]
    private string apiBaseUrl = "";

    [SerializeField]
    private string apiKey = "";

    private string queuePath;
    private SyncQueue queue = new SyncQueue();

    private string traineeId;
    private string deviceId;

    private string currentSessionId;
    private string currentModule;

    private Coroutine syncRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        queuePath = Path.Combine(Application.persistentDataPath, "SAFEXSyncQueue.json");

        LoadQueue();

        // Ensure stable device/trainee id
        if (PlayerPrefs.HasKey("SAFEX_DEVICE_ID"))
        {
            deviceId = PlayerPrefs.GetString("SAFEX_DEVICE_ID");
        }
        else
        {
            deviceId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("SAFEX_DEVICE_ID", deviceId);
            PlayerPrefs.Save();
        }

        if (PlayerPrefs.HasKey("SAFEX_TRAINEE_ID"))
            traineeId = PlayerPrefs.GetString("SAFEX_TRAINEE_ID");

        // Start retry loop
        syncRoutine = StartCoroutine(ProcessQueueRoutine());
    }

    private void OnDestroy()
    {
        if (syncRoutine != null)
            StopCoroutine(syncRoutine);
    }

    // =========================================================
    // PUBLIC CONFIG
    // =========================================================

    public void SetApiBaseUrl(string url)
    {
        apiBaseUrl = url;
    }

    public void SetApiKey(string key)
    {
        apiKey = key;
    }

    // =========================================================
    // TRAINEE
    // =========================================================

    public void RegisterTrainee(string name)
    {
        if (string.IsNullOrEmpty(traineeId))
        {
            traineeId = "SAFEX-TRAINEE-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpperInvariant();
            PlayerPrefs.SetString("SAFEX_TRAINEE_ID", traineeId);
            PlayerPrefs.Save();
        }

        TraineePayload payload = new TraineePayload
        {
            traineeId = traineeId,
            name = name ?? "",
            language = GetCurrentLanguageCode(),
            deviceId = deviceId
        };

        string json = JsonUtility.ToJson(payload);

        string endpoint = CombineApi("/trainees");

        StartCoroutine(SendOrQueueRequest(endpoint, "POST", json));

        Debug.Log("SAFEX CLOUD: Trainee register queued/sent.");
    }

    // =========================================================
    // SESSION
    // =========================================================

    public void StartSession(string module)
    {
        if (!string.IsNullOrEmpty(currentSessionId))
            return; // already started

        currentModule = module?.ToUpperInvariant();

        currentSessionId = GenerateSessionId();

        SessionPayload payload = new SessionPayload
        {
            sessionId = currentSessionId,
            traineeId = traineeId ?? deviceId,
            module = currentModule,
            status = "STARTED",
            startedAt = DateTime.UtcNow.ToString("o"),
            completedAt = null,
            durationSeconds = 0,
            assessmentPassed = false,
            certificateId = null
        };

        string json = JsonUtility.ToJson(payload);
        string endpoint = CombineApi("/sessions");

        StartCoroutine(SendOrQueueRequest(endpoint, "POST", json));

        Debug.Log("SAFEX CLOUD: Session started: " + currentSessionId);
    }

    public void CompleteSession(bool passed, string certificateId = null)
    {
        if (string.IsNullOrEmpty(currentSessionId))
            return;

        SessionPayload payload = new SessionPayload
        {
            sessionId = currentSessionId,
            traineeId = traineeId ?? deviceId,
            module = currentModule,
            status = "COMPLETED",
            startedAt = null,
            completedAt = DateTime.UtcNow.ToString("o"),
            durationSeconds = 0,
            assessmentPassed = passed,
            certificateId = certificateId
        };

        string json = JsonUtility.ToJson(payload);

        string endpoint = CombineApi($"/sessions/{currentSessionId}");

        // Use PATCH
        StartCoroutine(SendOrQueueRequest(endpoint, "PATCH", json));

        Debug.Log("SAFEX CLOUD: Session completed: " + currentSessionId);

        // Clear current session id to avoid duplicates
        currentSessionId = null;
        currentModule = null;
    }

    // =========================================================
    // EVENTS
    // =========================================================

    public void SendTrainingEvent(string eventType, Dictionary<string, string> eventData = null)
    {
        if (string.IsNullOrEmpty(currentSessionId))
        {
            Debug.Log("SAFEX CLOUD: No active session. Event not sent: " + eventType);
            return;
        }

        SerializableEvent se = new SerializableEvent();
        se.sessionId = currentSessionId;
        se.eventType = eventType;
        se.module = currentModule;
        se.timestamp = DateTime.UtcNow.ToString("o");

        if (eventData != null)
        {
            foreach (var kv in eventData)
                se.data.Add(new StringKeyValue { key = kv.Key, value = kv.Value });
        }

        string json = JsonUtility.ToJson(se);

        string endpoint = CombineApi($"/sessions/{currentSessionId}/events");

        StartCoroutine(SendOrQueueRequest(endpoint, "POST", json));

        Debug.Log("SAFEX CLOUD: Event queued/sent: " + eventType);
    }

    // =========================================================
    // ASSESSMENT
    // =========================================================

    public void SendAssessment()
    {
        if (SAFEXAssessmentManager.Instance == null)
            return;

        if (string.IsNullOrEmpty(currentSessionId))
        {
            Debug.Log("SAFEX CLOUD: No active session. Assessment not sent.");
            return;
        }

        AssessmentPayload payload = new AssessmentPayload
        {
            sessionId = currentSessionId,
            module = SAFEXAssessmentManager.Instance.CurrentModule,
            completedActions = new List<string>(),
            requiredActions = new List<string>(),
            score = 0,
            passed = SAFEXAssessmentManager.Instance.AssessmentPassed,
            durationSeconds = Mathf.RoundToInt(SAFEXAssessmentManager.Instance.TrainingDuration)
        };

        foreach (var a in SAFEXAssessmentManager.Instance.Actions)
            payload.completedActions.Add(a.action);

        int requiredCount = SAFEXAssessmentManager.Instance.GetRequiredActionCount();
        payload.score = SAFEXAssessmentManager.Instance.GetRequiredActionsCompletedCount();

        // required actions - try to fetch from manager's internal mapping via reflection is avoided; leave empty list
        // but we can attempt to populate from counts

        string json = JsonUtility.ToJson(payload);

        string endpoint = CombineApi("/assessments");

        StartCoroutine(SendOrQueueRequest(endpoint, "POST", json));

        Debug.Log("SAFEX CLOUD: Assessment queued/sent for session: " + currentSessionId);
    }

    // =========================================================
    // CERTIFICATE
    // =========================================================

    public void SendCertificate(string certificateId, int score, float percentage, string status)
    {
        if (string.IsNullOrEmpty(currentSessionId))
        {
            Debug.Log("SAFEX CLOUD: No active session. Certificate not sent.");
            return;
        }

        CertificatePayload payload = new CertificatePayload
        {
            certificateId = certificateId,
            sessionId = currentSessionId,
            traineeId = traineeId ?? deviceId,
            module = currentModule,
            score = score,
            percentage = percentage,
            status = status,
            issuedAt = DateTime.UtcNow.ToString("o")
        };

        string json = JsonUtility.ToJson(payload);

        string endpoint = CombineApi("/certificates");

        StartCoroutine(SendOrQueueRequest(endpoint, "POST", json));

        Debug.Log("SAFEX CLOUD: Certificate queued/sent: " + certificateId);
    }

    // =========================================================
    // QUEUE
    // =========================================================

    private void LoadQueue()
    {
        try
        {
            if (File.Exists(queuePath))
            {
                string raw = File.ReadAllText(queuePath);
                queue = JsonUtility.FromJson<SyncQueue>(raw) ?? new SyncQueue();
            }
            else
            {
                queue = new SyncQueue();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("SAFEX CLOUD: Failed to load queue: " + ex.Message);
            queue = new SyncQueue();
        }
    }

    private void SaveQueue()
    {
        try
        {
            string raw = JsonUtility.ToJson(queue);
            File.WriteAllText(queuePath, raw);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("SAFEX CLOUD: Failed to save queue: " + ex.Message);
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);

            if (Application.internetReachability == NetworkReachability.NotReachable)
                continue;

            if (queue.items.Count == 0)
                continue;

            // Attempt to send queued items one by one
            for (int i = queue.items.Count - 1; i >= 0; i--)
            {
                var item = queue.items[i];
                bool success = false;
                yield return SendRequest(item.endpoint, item.method, item.payload, (s) => success = s);

                if (success)
                {
                    queue.items.RemoveAt(i);
                    SaveQueue();
                    Debug.Log("SAFEX CLOUD: Sync successful for queued item: " + item.endpoint);
                }
                else
                {
                    Debug.Log("SAFEX CLOUD: Sync failed for queued item: " + item.endpoint);
                }

                // Wait a short delay between items
                yield return new WaitForSeconds(3f);
            }
        }
    }

    private IEnumerator SendOrQueueRequest(string endpoint, string method, string payload)
    {
        bool sent = false;

        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            bool success = false;
            yield return SendRequest(endpoint, method, payload, (s) => success = s);

            if (success)
                sent = true;
        }

        if (!sent)
        {
            SyncQueueItem item = new SyncQueueItem
            {
                endpoint = endpoint,
                method = method,
                payload = payload,
                createdAt = DateTime.UtcNow.ToString("o")
            };

            queue.items.Add(item);
            SaveQueue();

            Debug.Log("SAFEX CLOUD: Offline - queued");
        }
    }

    private IEnumerator SendRequest(string endpoint, string method, string payload, Action<bool> callback)
    {
        using (UnityWebRequest www = new UnityWebRequest(endpoint, method))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload ?? "");
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(apiKey))
                www.SetRequestHeader("X-SAFEX-API-KEY", apiKey);

            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool networkError = www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError;
#else
            bool networkError = www.isNetworkError || www.isHttpError;
#endif

            if (networkError)
            {
                Debug.LogWarning("SAFEX CLOUD: Request failed: " + endpoint + " | " + www.error);
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("SAFEX CLOUD: Request success: " + endpoint + " | Status: " + www.responseCode);
                callback?.Invoke(true);
            }
        }
    }

    private string CombineApi(string path)
    {
        if (string.IsNullOrEmpty(apiBaseUrl))
            return path; // allow local queueing

        string baseUrl = apiBaseUrl.TrimEnd('/');
        string p = path.TrimStart('/');
        return baseUrl + "/" + p;
    }

    private string GetCurrentLanguageCode()
    {
        if (SAFEXLocalizationManager.Instance == null)
            return "en";

        switch (SAFEXLocalizationManager.Instance.CurrentLanguage)
        {
            case SAFEXLanguage.Hindi:
                return "hi";
            case SAFEXLanguage.Santali:
                return "sat";
            default:
                return "en";
        }
    }

    private string GenerateSessionId()
    {
        return "SAFEX-SESSION-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpperInvariant();
    }
}
