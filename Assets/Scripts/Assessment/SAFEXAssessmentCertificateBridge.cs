using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// SAFEX ASSESSMENT -> CERTIFICATE BRIDGE
///
/// IMPORTANT:
/// The training controllers finish in TrainingComplete.
/// Therefore this bridge listens to BOTH:
/// - TrainingComplete
/// - Certificate
///
/// This makes certificate generation independent of whether the
/// FlowController exposes a separate Certificate transition.
///
/// Existing CertificateManager and OrbitalNine.QRCode are preserved.
/// </summary>
public class SAFEXAssessmentCertificateBridge : MonoBehaviour
{
    private SAFEXFlowController flowController;
    private bool certificateShown;

    private void Awake()
    {
        flowController =
            FindFirstObjectByType<SAFEXFlowController>();

        if (flowController == null)
        {
            Debug.LogError(
                "SAFEX CERTIFICATE BRIDGE: " +
                "SAFEXFlowController not found."
            );
        }
    }

    private void OnEnable()
    {
        if (flowController == null)
        {
            flowController =
                FindFirstObjectByType<SAFEXFlowController>();
        }

        if (flowController != null)
        {
            flowController.StateChanged +=
                OnStateChanged;
        }
    }

    private void OnDisable()
    {
        if (flowController != null)
        {
            flowController.StateChanged -=
                OnStateChanged;
        }
    }

    private void OnStateChanged(
        SAFEXFlowController.SAFEXState state)
    {
        if (certificateShown)
            return;

        if (state ==
            SAFEXFlowController.SAFEXState.TrainingComplete ||
            state ==
            SAFEXFlowController.SAFEXState.Certificate)
        {
            // Give the assessment completion call from the
            // training controller a frame to finish first.
            Invoke(
                nameof(ShowCertificateIfPassed),
                0.15f
            );
        }
    }

    private void ShowCertificateIfPassed()
    {
        if (certificateShown)
            return;

        SAFEXAssessmentManager assessment =
            SAFEXAssessmentManager.Instance;

        if (assessment == null)
        {
            Debug.LogError(
                "SAFEX CERTIFICATE: Assessment Manager not found."
            );
            return;
        }

        // Safety check:
        // if all required actions were completed but the controller
        // did not call CompleteAssessment(), finalize it here.
        if (!assessment.AssessmentCompleted)
        {
            assessment.CompleteAssessment();
        }

        if (!assessment.HasPassed())
        {
            Debug.LogWarning(
                "SAFEX CERTIFICATE BLOCKED: " +
                "Assessment did not pass. Missing: " +
                assessment.GetMissingActionsText()
            );
            return;
        }

        CertificateManager certificate =
            FindFirstObjectByType<CertificateManager>();

        if (certificate == null)
        {
            GameObject certificateObject =
                new GameObject(
                    "CERTIFICATE MANAGER"
                );

            certificate =
                certificateObject.AddComponent<
                    CertificateManager
                >();
        }

        int score =
            assessment.GetRequiredActionsCompletedCount();

        int total =
            assessment.GetRequiredActionCount();

        string moduleName =
            GetModuleDisplayName(
                assessment.CurrentModule
            );

        bool opened =
            InvokeShowCertificate(
                certificate,
                score,
                total,
                moduleName
            );

        if (!opened)
        {
            Debug.LogError(
                "SAFEX CERTIFICATE: Could not find " +
                "a compatible ShowCertificate method."
            );
            return;
        }

        certificateShown = true;

        Debug.Log(
            "SAFEX CERTIFICATE: GENERATED SUCCESSFULLY | " +
            "Score " + score + "/" + total +
            " | " + moduleName
        );
    }

    private bool InvokeShowCertificate(
        CertificateManager certificate,
        int score,
        int total,
        string moduleName
    )
    {
        MethodInfo[] methods =
            typeof(CertificateManager).GetMethods(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        foreach (MethodInfo method in methods)
        {
            if (method.Name != "ShowCertificate")
                continue;

            ParameterInfo[] parameters =
                method.GetParameters();

            // Normal SAFEX CertificateManager:
            // ShowCertificate(int, int, string)
            if (parameters.Length == 3 &&
                parameters[0].ParameterType ==
                    typeof(int) &&
                parameters[1].ParameterType ==
                    typeof(int) &&
                parameters[2].ParameterType ==
                    typeof(string))
            {
                method.Invoke(
                    certificate,
                    new object[]
                    {
                        score,
                        total,
                        moduleName
                    }
                );

                return true;
            }

            // Fallback:
            // ShowCertificate()
            if (parameters.Length == 0)
            {
                method.Invoke(
                    certificate,
                    null
                );

                return true;
            }
        }

        return false;
    }

    private string GetModuleDisplayName(
        string module
    )
    {
        if (module == "FIRE")
            return "Fire & Explosion Safety";

        if (module == "GAS")
            return "Gas & Confined Space Safety";

        return "Industrial Safety Training";
    }
}
