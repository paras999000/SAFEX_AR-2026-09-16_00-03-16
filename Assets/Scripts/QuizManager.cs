using UnityEngine;

/// <summary>
/// Legacy QuizManager.
///
/// SAFEX no longer uses quiz/MCQ assessment.
/// Training performance is evaluated from the
/// actual emergency-response and evacuation flow.
///
/// This component intentionally creates NO UI.
/// </summary>
public class QuizManager : MonoBehaviour
{
    private void Start()
    {
        Debug.Log(
            "SAFEX: QuizManager disabled. " +
            "Assessment is based on scenario response and evacuation."
        );
    }

    public void StartQuiz()
    {
        Debug.Log(
            "SAFEX: Quiz request ignored. " +
            "SAFEX uses scenario-based training."
        );
    }
}