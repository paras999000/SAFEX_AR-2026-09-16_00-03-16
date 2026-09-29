using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// SAFEX Cloud Sync - compatibility version.
///
/// Works offline by storing failed/unconfigured requests locally.
/// Includes both the newer Upload* API and compatibility aliases
/// used by existing SAFEX controllers.
/// </summary>
public class SAFEXCloudSync : MonoBehaviour
{
    public static SAFEXCloudSync Instance { get; private set; }

    [Header("Backend")]
    [SerializeField] private string apiBaseUrl = "";
    [SerializeField] private string apiKey = "";
    [SerializeField] private bool enableCloudSync = true;
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Retry")]
    [SerializeField] private float retryInterval = 15f;
    [SerializeField] private int maxRequestsPerSync = 10;

    [Serializable]
    public class SAFEXSession
    {
        public string sessionId;
        public string traineeId;
        public string traineeName;
        public string module;
        public string language;
        public string deviceId;
        public string appVersion;
        public string status;
        public string startedAt;
        public string completedAt;
        public int durationSeconds;
        public bool assessmentPassed;
        public string certificateId;
    }

    [Serializable]
    private class SyncItem
    {
        public string id;
        public string method;
        public string endpoint;
        public string json;
        public string createdAt;
        public int attempts;
    }

    [Serializable]
    private class SyncQueue
    {
        public List<SyncItem> items = new List<SyncItem>();
    }

    [Serializable]
    private class TrainingActionPayload
    {
        public string sessionId;
        public string action;
        public string timestamp;
    }

    [Serializable]
    private class DictionaryPayload
    {
        public Dictionary<string, string> values;
    }

    [Serializable]
    private class TrainingEventPayload
    {
        public string sessionId;
        public string eventType;
        public string eventData;
        public string timestamp;
    }

    [Serializable]
    private class AssessmentPayload
    {
        public string sessionId;
        public string module;
        public int completedActions;
        public int totalActions;
        public bool passed;
        public int durationSeconds;
        public string timestamp;
    }

    [Serializable]
    private class CertificatePayload
    {
        public string certificateId;
        public string sessionId;
        public string traineeId;
        public string module;
        public int score;
        public int total;
        public int percentage;
        public string status;
        public string issuedAt;
    }

    private SAFEXSession currentSession;
    private SyncQueue syncQueue = new SyncQueue();

    private string QueuePath =>
        Path.Combine(
            Application.persistentDataPath,
            "SAFEXCloudQueue.json"
        );

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadQueue();
    }

    private void Start()
    {
        StartCoroutine(CloudSyncLoop());
    }

    // =========================================================
    // SESSION API
    // =========================================================

    public string StartTrainingSession(
        string traineeId,
        string traineeName,
        string module,
        string language)
    {
        string sessionId = Guid.NewGuid().ToString();

        currentSession = new SAFEXSession
        {
            sessionId = sessionId,
            traineeId = traineeId,
            traineeName = traineeName,
            module = module,
            language = language,
            deviceId = SystemInfo.deviceUniqueIdentifier,
            appVersion = Application.version,
            status = "STARTED",
            startedAt = DateTime.UtcNow.ToString("o"),
            completedAt = "",
            durationSeconds = 0,
            assessmentPassed = false,
            certificateId = ""
        };

        SendOrQueue(
            "POST",
            "/sessions",
            JsonUtility.ToJson(currentSession)
        );

        return sessionId;
    }

    // Compatibility alias used by existing training controllers.
    public string StartSession(
        string traineeId,
        string traineeName,
        string module,
        string language)
    {
        return StartTrainingSession(
            traineeId,
            traineeName,
            module,
            language
        );
    }

    // Compatibility alias if an existing controller starts a session
    // without trainee details.
    public string StartSession(string module)
    {
        return StartTrainingSession(
            "",
            "",
            module,
            "English"
        );
    }

    public string StartSession()
    {
        return StartTrainingSession(
            "",
            "",
            "UNKNOWN",
            "English"
        );
    }

    public void SetCurrentSession(
        string sessionId,
        string traineeId,
        string traineeName,
        string module,
        string language)
    {
        currentSession = new SAFEXSession
        {
            sessionId = sessionId,
            traineeId = traineeId,
            traineeName = traineeName,
            module = module,
            language = language,
            deviceId = SystemInfo.deviceUniqueIdentifier,
            appVersion = Application.version,
            status = "STARTED",
            startedAt = DateTime.UtcNow.ToString("o")
        };
    }

    // =========================================================
    // ACTION / EVENT API
    // =========================================================

    public void RecordTrainingAction(string action)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("SAFEX CLOUD: No active session.");
            return;
        }

        RecordTrainingAction(
            currentSession.sessionId,
            action
        );
    }

    public void RecordTrainingAction(
        string sessionId,
        string action)
    {
        TrainingActionPayload payload =
            new TrainingActionPayload
            {
                sessionId = sessionId,
                action = action,
                timestamp = DateTime.UtcNow.ToString("o")
            };

        SendOrQueue(
            "POST",
            "/sessions/" + sessionId + "/actions",
            JsonUtility.ToJson(payload)
        );
    }

    public void SendTrainingEvent(string eventType)
    {
        SendTrainingEvent(
            GetCurrentSessionId(),
            eventType,
            "{}"
        );
    }

    // Existing ARPlacementManager uses this 2-argument form.
    public void SendTrainingEvent(
        string eventType,
        string eventDataJson)
    {
        SendTrainingEvent(
            GetCurrentSessionId(),
            eventType,
            eventDataJson
        );
    }

    // Compatibility overload used by ARPlacementManager.
    public void SendTrainingEvent(
        string eventType,
        Dictionary<string, string> eventData)
    {
        string json = "{}";

        if (eventData != null)
        {
            json = JsonUtility.ToJson(
                new DictionaryPayload
                {
                    values = eventData
                }
            );
        }

        SendTrainingEvent(
            GetCurrentSessionId(),
            eventType,
            json
        );
    }

    public void SendTrainingEvent(
        string sessionId,
        string eventType,
        string eventDataJson)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        TrainingEventPayload payload =
            new TrainingEventPayload
            {
                sessionId = sessionId,
                eventType = eventType,
                eventData = string.IsNullOrWhiteSpace(eventDataJson)
                    ? "{}"
                    : eventDataJson,
                timestamp = DateTime.UtcNow.ToString("o")
            };

        SendOrQueue(
            "POST",
            "/sessions/" + sessionId + "/events",
            JsonUtility.ToJson(payload)
        );
    }

    // =========================================================
    // ASSESSMENT API
    // =========================================================

    public void UploadAssessment(
        string module,
        int completedActions,
        int totalActions,
        bool passed,
        int durationSeconds)
    {
        string sessionId = GetCurrentSessionId();

        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        UploadAssessment(
            sessionId,
            module,
            completedActions,
            totalActions,
            passed,
            durationSeconds
        );
    }

    public void UploadAssessment(
        string sessionId,
        string module,
        int completedActions,
        int totalActions,
        bool passed,
        int durationSeconds)
    {
        AssessmentPayload payload =
            new AssessmentPayload
            {
                sessionId = sessionId,
                module = module,
                completedActions = completedActions,
                totalActions = totalActions,
                passed = passed,
                durationSeconds = durationSeconds,
                timestamp = DateTime.UtcNow.ToString("o")
            };

        SendOrQueue(
            "POST",
            "/assessments",
            JsonUtility.ToJson(payload)
        );
    }

    // Compatibility alias used by existing controllers.
    public void SendAssessment(
        string module,
        int completedActions,
        int totalActions,
        bool passed,
        int durationSeconds)
    {
        UploadAssessment(
            module,
            completedActions,
            totalActions,
            passed,
            durationSeconds
        );
    }

    public void SendAssessment(
        string sessionId,
        string module,
        int completedActions,
        int totalActions,
        bool passed,
        int durationSeconds)
    {
        UploadAssessment(
            sessionId,
            module,
            completedActions,
            totalActions,
            passed,
            durationSeconds
        );
    }

    public void SendAssessment()
    {
        if (currentSession == null)
            return;

        UploadAssessment(
            currentSession.module,
            0,
            0,
            currentSession.assessmentPassed,
            currentSession.durationSeconds
        );
    }

    // =========================================================
    // CERTIFICATE API
    // =========================================================

    public void UploadCertificate(
        string certificateId,
        string module,
        int score,
        int total)
    {
        UploadCertificate(
            GetCurrentSessionId(),
            currentSession != null
                ? currentSession.traineeId
                : "",
            certificateId,
            module,
            score,
            total
        );
    }

    public void UploadCertificate(
        string sessionId,
        string traineeId,
        string certificateId,
        string module,
        int score,
        int total)
    {
        int percentage = total > 0
            ? Mathf.RoundToInt(
                ((float)score / total) * 100f
            )
            : 0;

        CertificatePayload payload =
            new CertificatePayload
            {
                certificateId = certificateId,
                sessionId = sessionId,
                traineeId = traineeId,
                module = module,
                score = score,
                total = total,
                percentage = percentage,
                status = "PASSED",
                issuedAt = DateTime.UtcNow.ToString("o")
            };

        SendOrQueue(
            "POST",
            "/certificates",
            JsonUtility.ToJson(payload)
        );
    }

    // Compatibility aliases used by CertificateManager.
    // Compatibility signature used by CertificateManager:
    // (certificateId, score, percentage, module)
    public void SendCertificate(
        string certificateId,
        int score,
        float percentage,
        string module)
    {
        int total = 100;

        if (percentage > 0f && percentage <= 100f)
        {
            total = 100;
        }

        UploadCertificate(
            certificateId,
            module,
            score,
            total
        );
    }

    public void SendCertificate(
        string certificateId,
        string module,
        int score,
        int total)
    {
        UploadCertificate(
            certificateId,
            module,
            score,
            total
        );
    }

    public void SendCertificate(
        string sessionId,
        string traineeId,
        string certificateId,
        string module,
        int score,
        int total)
    {
        UploadCertificate(
            sessionId,
            traineeId,
            certificateId,
            module,
            score,
            total
        );
    }

    public void SendCertificate(
        string certificateId)
    {
        UploadCertificate(
            certificateId,
            currentSession != null
                ? currentSession.module
                : "UNKNOWN",
            0,
            0
        );
    }

    public void SendCertificate()
    {
        if (currentSession == null)
            return;

        UploadCertificate(
            currentSession.certificateId,
            currentSession.module,
            0,
            0
        );
    }

    // =========================================================
    // COMPLETE SESSION API
    // =========================================================

    public void CompleteCurrentSession(
        bool assessmentPassed,
        int durationSeconds,
        string certificateId)
    {
        if (currentSession == null)
            return;

        currentSession.status =
            assessmentPassed
                ? "COMPLETED"
                : "FAILED";

        currentSession.assessmentPassed =
            assessmentPassed;

        currentSession.durationSeconds =
            durationSeconds;

        currentSession.certificateId =
            certificateId;

        currentSession.completedAt =
            DateTime.UtcNow.ToString("o");

        SendOrQueue(
            "PATCH",
            "/sessions/" + currentSession.sessionId,
            JsonUtility.ToJson(currentSession)
        );
    }

    // Compatibility overload used by FireTrainingController.
    public void CompleteSession(
        string certificateId)
    {
        CompleteCurrentSession(
            true,
            0,
            certificateId
        );
    }

    // Compatibility overload used by FireTrainingController.
    public void CompleteSession(
        bool assessmentPassed)
    {
        CompleteCurrentSession(
            assessmentPassed,
            0,
            ""
        );
    }

    public void CompleteSession(
        bool assessmentPassed,
        string certificateId)
    {
        CompleteCurrentSession(
            assessmentPassed,
            0,
            certificateId
        );
    }

    // Compatibility alias.
    public void CompleteSession(
        bool assessmentPassed,
        int durationSeconds,
        string certificateId)
    {
        CompleteCurrentSession(
            assessmentPassed,
            durationSeconds,
            certificateId
        );
    }

    public void CompleteSession()
    {
        if (currentSession == null)
            return;

        CompleteCurrentSession(
            currentSession.assessmentPassed,
            currentSession.durationSeconds,
            currentSession.certificateId
        );
    }

    // =========================================================
    // NETWORK
    // =========================================================

    private void SendOrQueue(
        string method,
        string endpoint,
        string json)
    {
        if (!enableCloudSync)
            return;

        if (string.IsNullOrWhiteSpace(apiBaseUrl) ||
            Application.internetReachability ==
            NetworkReachability.NotReachable)
        {
            QueueRequest(method, endpoint, json);
            return;
        }

        StartCoroutine(
            SendRequest(
                method,
                endpoint,
                json,
                true
            )
        );
    }

    private IEnumerator SendRequest(
        string method,
        string endpoint,
        string json,
        bool queueOnFailure)
    {
        string url =
            apiBaseUrl.TrimEnd('/') + endpoint;

        using (UnityWebRequest request =
            new UnityWebRequest(url, method))
        {
            byte[] body =
                Encoding.UTF8.GetBytes(json);

            request.uploadHandler =
                new UploadHandlerRaw(body);

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            request.SetRequestHeader(
                "Accept",
                "application/json"
            );

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.SetRequestHeader(
                    "X-SAFEX-API-KEY",
                    apiKey
                );
            }

            yield return request.SendWebRequest();

            if (request.result !=
                UnityWebRequest.Result.Success)
            {
                if (queueOnFailure)
                    QueueRequest(
                        method,
                        endpoint,
                        json
                    );

                Log(
                    "SAFEX CLOUD FAILED: " +
                    endpoint +
                    " | " +
                    request.error
                );
            }
            else
            {
                Log(
                    "SAFEX CLOUD SUCCESS: " +
                    endpoint
                );
            }
        }
    }

    // =========================================================
    // OFFLINE QUEUE
    // =========================================================

    private void QueueRequest(
        string method,
        string endpoint,
        string json)
    {
        syncQueue.items.Add(
            new SyncItem
            {
                id = Guid.NewGuid().ToString(),
                method = method,
                endpoint = endpoint,
                json = json,
                createdAt = DateTime.UtcNow.ToString("o"),
                attempts = 0
            }
        );

        SaveQueue();
    }

    private IEnumerator CloudSyncLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                retryInterval
            );

            if (!enableCloudSync)
                continue;

            if (syncQueue.items.Count == 0)
                continue;

            if (string.IsNullOrWhiteSpace(apiBaseUrl))
                continue;

            if (Application.internetReachability ==
                NetworkReachability.NotReachable)
                continue;

            yield return StartCoroutine(
                ProcessQueue()
            );
        }
    }

    private IEnumerator ProcessQueue()
    {
        int processed = 0;

        for (int i = syncQueue.items.Count - 1;
             i >= 0;
             i--)
        {
            if (processed >= maxRequestsPerSync)
                break;

            SyncItem item = syncQueue.items[i];
            item.attempts++;

            yield return StartCoroutine(
                SendQueuedItem(item, i)
            );

            processed++;
        }

        SaveQueue();
    }

    private IEnumerator SendQueuedItem(
        SyncItem item,
        int index)
    {
        string url =
            apiBaseUrl.TrimEnd('/') +
            item.endpoint;

        using (UnityWebRequest request =
            new UnityWebRequest(url, item.method))
        {
            request.uploadHandler =
                new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(item.json)
                );

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            request.SetRequestHeader(
                "Accept",
                "application/json"
            );

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.SetRequestHeader(
                    "X-SAFEX-API-KEY",
                    apiKey
                );
            }

            yield return request.SendWebRequest();

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                if (index >= 0 &&
                    index < syncQueue.items.Count)
                {
                    syncQueue.items.RemoveAt(index);
                }

                Log(
                    "SAFEX CLOUD QUEUED REQUEST SYNCED: " +
                    item.endpoint
                );
            }
        }
    }

    private void SaveQueue()
    {
        try
        {
            File.WriteAllText(
                QueuePath,
                JsonUtility.ToJson(
                    syncQueue,
                    true
                )
            );
        }
        catch (Exception e)
        {
            Debug.LogWarning(
                "SAFEX CLOUD QUEUE SAVE ERROR: " +
                e.Message
            );
        }
    }

    private void LoadQueue()
    {
        try
        {
            if (!File.Exists(QueuePath))
            {
                syncQueue = new SyncQueue();
                return;
            }

            string json =
                File.ReadAllText(QueuePath);

            SyncQueue loaded =
                JsonUtility.FromJson<SyncQueue>(json);

            syncQueue =
                loaded ?? new SyncQueue();

            if (syncQueue.items == null)
                syncQueue.items =
                    new List<SyncItem>();
        }
        catch
        {
            syncQueue = new SyncQueue();
        }
    }

    // =========================================================
    // PUBLIC HELPERS
    // =========================================================

    public int GetPendingSyncCount()
    {
        return syncQueue.items.Count;
    }

    public string GetCurrentSessionId()
    {
        return currentSession != null
            ? currentSession.sessionId
            : "";
    }

    public SAFEXSession GetCurrentSession()
    {
        return currentSession;
    }

    public void ClearLocalQueue()
    {
        syncQueue.items.Clear();
        SaveQueue();
    }

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log(message);
    }
}
