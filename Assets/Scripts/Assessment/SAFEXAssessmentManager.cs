using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SAFEX offline assessment engine.
///
/// This is NOT a quiz and does NOT require internet.
/// The assessment is based on completion of the required
/// procedural safety actions performed during training.
///
/// Fire required actions:
/// - REACH_POWER_CONTROL
/// - ISOLATE_POWER
/// - EVACUATE_SAFE_EXIT
///
/// Gas required actions:
/// - REACH_GAS_DETECTOR
/// - RAISE_ALARM
/// - SELECT_CORRECT_GAS_PPE
/// - BUDDY_CONFIRMED
/// - ISOLATE_CONTAMINATED_AREA
/// - EVACUATE_SAFE_EXIT
///
/// A certificate is only valid when every required action
/// for the selected module has been completed.
/// </summary>
public class SAFEXAssessmentManager : MonoBehaviour
{
    public static SAFEXAssessmentManager Instance { get; private set; }

    [Serializable]
    public class AssessmentAction
    {
        public string action;
        public bool completed;
        public float timestamp;
    }

    public string CurrentModule { get; private set; }
    public bool AssessmentCompleted { get; private set; }
    public bool AssessmentPassed { get; private set; }

    public List<AssessmentAction> Actions { get; private set; } =
        new List<AssessmentAction>();

    private float trainingStartTime;

    private readonly Dictionary<string, string[]> requiredActions =
        new Dictionary<string, string[]>
        {
           {
    "FIRE",
    new[]
    {
        "REACH_POWER_CONTROL",
        "ISOLATE_POWER",
        "USE_FIRE_EXTINGUISHER",
        "EVACUATE_SAFE_EXIT"
    }
},
            {
                "GAS",
                new[]
                {
                    "REACH_GAS_DETECTOR",
                    "RAISE_ALARM",
                    "SELECT_CORRECT_GAS_PPE",
                    "BUDDY_CONFIRMED",
                    "ISOLATE_CONTAMINATED_AREA",
                    "EVACUATE_SAFE_EXIT"
                }
            }
        };

    public float TrainingDuration =>
        trainingStartTime > 0f
            ? Time.time - trainingStartTime
            : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartAssessment(string module)
    {
        CurrentModule =
            string.IsNullOrEmpty(module)
                ? null
                : module.Trim().ToUpperInvariant();

        AssessmentCompleted = false;
        AssessmentPassed = false;
        Actions.Clear();

        trainingStartTime = Time.time;

        Debug.Log(
            "SAFEX ASSESSMENT STARTED: " +
            CurrentModule
        );

        if (!HasRequiredActions(CurrentModule))
        {
            Debug.LogWarning(
                "SAFEX ASSESSMENT: No required-action definition for module '" +
                CurrentModule +
                "'."
            );
        }
    }

    public void RecordAction(string action)
    {
        if (AssessmentCompleted)
            return;

        if (string.IsNullOrWhiteSpace(action))
            return;

        string normalized =
            action.Trim().ToUpperInvariant();

        AssessmentAction existing =
            Actions.Find(
                x => x.action == normalized
            );

        if (existing != null)
        {
            existing.completed = true;
            return;
        }

        Actions.Add(
            new AssessmentAction
            {
                action = normalized,
                completed = true,
                timestamp = Time.time
            }
        );

        Debug.Log(
            "SAFEX ASSESSMENT ACTION: " +
            normalized
        );
    }

    public bool CompleteAssessment()
    {
        if (AssessmentCompleted)
            return AssessmentPassed;

        AssessmentCompleted = true;

        AssessmentPassed =
            HasRequiredActions(CurrentModule) &&
            AllRequiredActionsCompleted();

        Debug.Log(
            "SAFEX ASSESSMENT RESULT: " +
            (AssessmentPassed
                ? "PASSED"
                : "INCOMPLETE")
        );

        return AssessmentPassed;
    }

    public bool HasPassed()
    {
        return AssessmentCompleted &&
               AssessmentPassed;
    }

    public int GetCompletedActionCount()
    {
        int count = 0;

        foreach (AssessmentAction action in Actions)
        {
            if (action.completed)
                count++;
        }

        return count;
    }

    public int GetRequiredActionCount()
    {
        if (!HasRequiredActions(CurrentModule))
            return 0;

        return requiredActions[CurrentModule].Length;
    }

    public int GetRequiredActionsCompletedCount()
    {
        if (!HasRequiredActions(CurrentModule))
            return 0;

        int count = 0;

        foreach (string required in requiredActions[CurrentModule])
        {
            if (IsActionCompleted(required))
                count++;
        }

        return count;
    }

    public float GetPercentage()
    {
        int total = GetRequiredActionCount();

        if (total <= 0)
            return 0f;

        return
            (GetRequiredActionsCompletedCount() /
             (float)total) *
            100f;
    }

    public string GetResult()
    {
        if (!AssessmentCompleted)
            return "INCOMPLETE";

        return AssessmentPassed
            ? "PASSED"
            : "INCOMPLETE";
    }

    public bool IsActionCompleted(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            return false;

        string normalized =
            action.Trim().ToUpperInvariant();

        AssessmentAction existing =
            Actions.Find(
                x => x.action == normalized
            );

        return existing != null &&
               existing.completed;
    }

    public string GetMissingActionsText()
    {
        if (!HasRequiredActions(CurrentModule))
            return "No assessment definition available.";

        List<string> missing =
            new List<string>();

        foreach (string required in requiredActions[CurrentModule])
        {
            if (!IsActionCompleted(required))
                missing.Add(required);
        }

        if (missing.Count == 0)
            return "NONE";

        return string.Join(", ", missing);
    }

    public void ResetAssessment()
    {
        CurrentModule = null;
        AssessmentCompleted = false;
        AssessmentPassed = false;
        Actions.Clear();
        trainingStartTime = 0f;
    }

    private bool HasRequiredActions(string module)
    {
        return
            !string.IsNullOrEmpty(module) &&
            requiredActions.ContainsKey(module);
    }

    private bool AllRequiredActionsCompleted()
    {
        if (!HasRequiredActions(CurrentModule))
            return false;

        foreach (string required in requiredActions[CurrentModule])
        {
            if (!IsActionCompleted(required))
                return false;
        }

        return true;
    }
}
