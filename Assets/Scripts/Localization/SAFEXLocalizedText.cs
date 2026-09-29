using UnityEngine;
using UnityEngine.UI;

public class SAFEXLocalizedText : MonoBehaviour
{
    private Text targetText;

    private string englishText;
    private string localizationKey;

    public void Initialize(
        Text textComponent,
        string key,
        string originalEnglish)
    {
        targetText = textComponent;

        localizationKey = key;
        englishText = originalEnglish;

        Refresh();
    }

    public void Initialize(
        Text textComponent,
        string originalEnglish)
    {
        targetText = textComponent;

        englishText = originalEnglish;

        if (SAFEXLocalizationManager.Instance != null)
        {
            localizationKey =
                SAFEXLocalizationManager.Instance
                    .GetKeyFromEnglish(originalEnglish);
        }

        Refresh();
    }

    private void OnEnable()
    {
        if (SAFEXLocalizationManager.Instance != null)
        {
            SAFEXLocalizationManager.Instance.LanguageChanged +=
                OnLanguageChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (SAFEXLocalizationManager.Instance != null)
        {
            SAFEXLocalizationManager.Instance.LanguageChanged -=
                OnLanguageChanged;
        }
    }

    private void OnLanguageChanged(
        SAFEXLanguage language)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (targetText == null)
        {
            targetText =
                GetComponent<Text>();
        }

        if (targetText == null)
            return;

        if (SAFEXLocalizationManager.Instance == null)
        {
            targetText.text =
                englishText;

            return;
        }

        if (string.IsNullOrEmpty(localizationKey))
        {
            localizationKey =
                SAFEXLocalizationManager.Instance
                    .GetKeyFromEnglish(englishText);
        }

        if (string.IsNullOrEmpty(localizationKey))
        {
            targetText.text =
                englishText;

            return;
        }

        targetText.text =
            SAFEXLocalizationManager.Instance
                .Get(localizationKey);
    }
}