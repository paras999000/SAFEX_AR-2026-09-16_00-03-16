using System;
using System.Collections.Generic;
using UnityEngine;

public enum SAFEXLanguage
{
    English,
    Hindi,
    Santali
}

public class SAFEXLocalizationManager : MonoBehaviour
{
    public static SAFEXLocalizationManager Instance { get; private set; }

    public SAFEXLanguage CurrentLanguage { get; private set; }

    public event Action<SAFEXLanguage> LanguageChanged;

    private const string LanguageKey = "SAFEX_LANGUAGE";

    private readonly Dictionary<string, string> english =
        new Dictionary<string, string>();

    private readonly Dictionary<string, string> hindi =
        new Dictionary<string, string>();

    private readonly Dictionary<string, string> santali =
        new Dictionary<string, string>();

    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        BuildDictionary();

        LoadLanguage();
    }

    // =========================================================
    // LANGUAGE
    // =========================================================

    private void LoadLanguage()
    {
        string savedLanguage =
            PlayerPrefs.GetString(
                LanguageKey,
                SAFEXLanguage.English.ToString()
            );

        if (Enum.TryParse(
            savedLanguage,
            out SAFEXLanguage language))
        {
            CurrentLanguage = language;
        }
        else
        {
            CurrentLanguage =
                SAFEXLanguage.English;
        }
    }

    public void SetLanguage(
        SAFEXLanguage language)
    {
        if (CurrentLanguage == language)
        {
            RefreshAllTexts();

            LanguageChanged?.Invoke(language);

            return;
        }

        CurrentLanguage = language;

        PlayerPrefs.SetString(
            LanguageKey,
            language.ToString()
        );

        PlayerPrefs.Save();

        RefreshAllTexts();

        LanguageChanged?.Invoke(language);
    }

    // =========================================================
    // GET LOCALIZED TEXT
    // =========================================================

    public string Get(string key)
    {
        Dictionary<string, string> table =
            english;

        switch (CurrentLanguage)
        {
            case SAFEXLanguage.Hindi:

                table = hindi;

                break;

            case SAFEXLanguage.Santali:

                table = santali;

                break;
        }

        if (table.TryGetValue(
            key,
            out string localizedValue))
        {
            return localizedValue;
        }

        // English fallback
        if (english.TryGetValue(
            key,
            out string fallbackValue))
        {
            return fallbackValue;
        }

        // If key doesn't exist,
        // return the key itself.
        return key;
    }

    // =========================================================
    // IMPORTANT:
    // USED BY SAFEXHomeController
    // =========================================================

    public string GetKeyFromEnglish(
        string englishText)
    {
        if (string.IsNullOrEmpty(englishText))
            return null;

        foreach (var pair in english)
        {
            if (pair.Value == englishText)
            {
                return pair.Key;
            }
        }

        return null;
    }

    // =========================================================
    // REFRESH ALL LOCALIZED UI
    // =========================================================

    private void RefreshAllTexts()
    {
        SAFEXLocalizedText[] texts =
            FindObjectsByType<SAFEXLocalizedText>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            SAFEXLocalizedText text
            in texts)
        {
            if (text != null)
            {
                text.Refresh();
            }
        }
    }

    // =========================================================
    // DICTIONARY HELPER
    // =========================================================

    private void Add(
        string key,
        string en,
        string hi,
        string sat)
    {
        english[key] = en;
        hindi[key] = hi;
        santali[key] = sat;
    }

    // =========================================================
    // TRANSLATION DATABASE
    // =========================================================

    private void BuildDictionary()
    {
        // =====================================================
        // SYSTEM
        // =====================================================

        Add(
            "SYSTEM_READY",
            "SYSTEM READY",
            "सिस्टम तैयार",
            "सिस्टम तियार"
        );

        Add(
            "INITIALIZE_TRAINING",
            "INITIALIZE TRAINING",
            "प्रशिक्षण शुरू करें",
            "सिखना एहोब"
        );

        Add(
            "SELECT_SAFETY_MODULE",
            "SELECT SAFETY MODULE",
            "सुरक्षा मॉड्यूल चुनें",
            "सुरक्षा मॉड्यूल बाछाव"
        );

        Add(
            "MINE_SAFETY",
            "MINE SAFETY",
            "खदान सुरक्षा",
            "खान सुरक्षा"
        );

        Add(
            "TRAINING_SYSTEM",
            "TRAINING SYSTEM",
            "प्रशिक्षण प्रणाली",
            "सिखना सिस्टम"
        );

        Add(
            "AR_BASED_INDUSTRIAL_SAFETY",
            "AR-BASED INDUSTRIAL SAFETY SIMULATION",
            "AR आधारित औद्योगिक सुरक्षा प्रशिक्षण",
            "AR आधार औद्योगिक सुरक्षा सिखना"
        );

        Add(
            "TRAIN_RESPOND_EVACUATE_CERTIFY",
            "TRAIN  /  RESPOND  /  EVACUATE  /  CERTIFY",
            "प्रशिक्षण / प्रतिक्रिया / निकासी / प्रमाणन",
            "सिखना / जवाब / बाहरे आकान / प्रमाणित"
        );

        // =====================================================
        // FEATURES
        // =====================================================

        Add(
            "REAL_SCENARIOS",
            "REAL SCENARIOS",
            "वास्तविक परिदृश्य",
            "असली घटना"
        );

        Add(
            "INDUSTRIAL_EMERGENCY_RESPONSE",
            "Industrial emergency response",
            "औद्योगिक आपातकालीन प्रतिक्रिया",
            "औद्योगिक आपातकालीन जवाब"
        );

        Add(
            "AR_LEARNING",
            "AR LEARNING",
            "AR प्रशिक्षण",
            "AR सिखना"
        );

        Add(
            "INTERACTIVE_MINE_ENVIRONMENT",
            "Interactive mine environment",
            "इंटरैक्टिव खदान वातावरण",
            "जुड़ाव खान माहोल"
        );

        Add(
            "LIVE_RESPONSE",
            "LIVE RESPONSE",
            "तत्काल प्रतिक्रिया",
            "तुरंत जवाब"
        );

        Add(
            "SCENARIO_BASED_EVALUATION",
            "Scenario-based evaluation",
            "परिदृश्य आधारित मूल्यांकन",
            "घटना आधार जांच"
        );

        Add(
            "DIGITAL_CREDENTIAL",
            "DIGITAL CREDENTIAL",
            "डिजिटल प्रमाण-पत्र",
            "डिजिटल प्रमाण"
        );

        Add(
            "VERIFIED_COMPLETION_CERTIFICATE",
            "Verified completion certificate",
            "सत्यापित प्रशिक्षण प्रमाण-पत्र",
            "जांच होयाक सिखना प्रमाण-पत्र"
        );

        Add(
            "OFFLINE_READY",
            "OFFLINE READY",
            "ऑफलाइन तैयार",
            "ऑफलाइन तियार"
        );

        Add(
            "SAFEX_CORE",
            "SAFEX CORE",
            "SAFEX कोर",
            "SAFEX मूल"
        );

        // =====================================================
        // MODULE SELECTION
        // =====================================================

        Add(
            "MODULE_SELECTION",
            "MODULE SELECTION",
            "मॉड्यूल चयन",
            "मॉड्यूल बाछाव"
        );

        Add(
            "SELECT_INDUSTRIAL_SAFETY_PROTOCOL",
            "SELECT INDUSTRIAL SAFETY PROTOCOL",
            "औद्योगिक सुरक्षा प्रोटोकॉल चुनें",
            "औद्योगिक सुरक्षा तरीका बाछाव"
        );

        Add(
            "FIRE",
            "FIRE",
            "आग",
            "होलों"
        );

        Add(
            "EXPLOSION",
            "& EXPLOSION",
            "और विस्फोट",
            "आर विस्फोट"
        );

        Add(
            "EMERGENCY_RESPONSE",
            "EMERGENCY RESPONSE",
            "आपातकालीन प्रतिक्रिया",
            "आपातकालीन जवाब"
        );

        Add(
            "FIRE_DESCRIPTION",
            "Recognize fire hazards, respond correctly and reach the safe zone.",
            "आग के खतरे को पहचानें, सही प्रतिक्रिया दें और सुरक्षित क्षेत्र तक पहुँचें।",
            "होलों के खतरा चिन्हाव, सही जवाब दो आर सुरक्षित जगह रे जाहान।"
        );

        Add(
            "GAS_LEAK",
            "GAS LEAK",
            "गैस रिसाव",
            "गैस रिसाव"
        );

        Add(
            "CONFINED_SPACE",
            "& CONFINED SPACE",
            "और सीमित स्थान",
            "आर बंद जगह"
        );

        Add(
            "HAZARD_RESPONSE",
            "HAZARD RESPONSE",
            "खतरे की प्रतिक्रिया",
            "खतरा जवाब"
        );

        Add(
            "GAS_DESCRIPTION",
            "Identify gas hazards, avoid contaminated areas and evacuate safely.",
            "गैस के खतरे को पहचानें, दूषित क्षेत्र से बचें और सुरक्षित रूप से बाहर निकलें।",
            "गैस के खतरा चिन्हाव, दूषित जगह एड़ाव आर सुरक्षित रूपे बाहरे जाहान।"
        );

        Add(
            "SELECT",
            "SELECT  →",
            "चुनें  →",
            "बाछाव  →"
        );

        Add(
            "RETURN_HOME",
            "←  RETURN TO HOME",
            "←  होम पर वापस जाएँ",
            "← होम रे रुवाड़ जाहान"
        );

        // =====================================================
        // BRIEFING
        // =====================================================

        Add(
            "PROTOCOL_BRIEFING",
            "SAFEX / PROTOCOL BRIEFING",
            "SAFEX / प्रोटोकॉल जानकारी",
            "SAFEX / तरीका जानकारी"
        );

        Add(
            "FIRE_EXPLOSION",
            "FIRE & EXPLOSION",
            "आग और विस्फोट",
            "होलों आर विस्फोट"
        );

        Add(
            "GAS_CONFINED",
            "GAS LEAK & CONFINED SPACE",
            "गैस रिसाव और सीमित स्थान",
            "गैस रिसाव आर बंद जगह"
        );

        Add(
            "EMERGENCY_RESPONSE_PROTOCOL",
            "EMERGENCY RESPONSE PROTOCOL",
            "आपातकालीन प्रतिक्रिया प्रोटोकॉल",
            "आपातकालीन जवाब तरीका"
        );

        Add(
            "HAZARD_RESPONSE_PROTOCOL",
            "HAZARD RESPONSE PROTOCOL",
            "खतरा प्रतिक्रिया प्रोटोकॉल",
            "खतरा जवाब तरीका"
        );

        Add(
            "INCIDENT_DETECTED",
            "INCIDENT DETECTED",
            "घटना का पता चला",
            "घटना चिन्हा गेल"
        );

        Add(
            "FIRE_EMERGENCY",
            "FIRE EMERGENCY",
            "आग की आपात स्थिति",
            "होलों आपातकाल"
        );

        Add(
            "HAZARDOUS_GAS_LEAK",
            "HAZARDOUS GAS LEAK",
            "खतरनाक गैस रिसाव",
            "खतरनाक गैस रिसाव"
        );

        Add(
            "RESPONSE_SEQUENCE",
            "RESPONSE SEQUENCE",
            "प्रतिक्रिया क्रम",
            "जवाब के क्रम"
        );

        Add(
            "RAISE_ALARM",
            "RAISE ALARM",
            "अलार्म सक्रिय करें",
            "अलार्म चालू दो"
        );

        Add(
            "ISOLATE_POWER",
            "ISOLATE POWER",
            "बिजली की आपूर्ति बंद करें",
            "बिजली बंद दो"
        );

        Add(
            "CONTROL_FIRE",
            "CONTROL FIRE",
            "आग पर नियंत्रण करें",
            "होलों काबू दो"
        );

        Add(
            "DETECT_GAS",
            "DETECT GAS",
            "गैस का पता लगाएँ",
            "गैस चिन्हाव"
        );

        Add(
            "ISOLATE_AREA",
            "ISOLATE AREA",
            "क्षेत्र को अलग करें",
            "जगह अलग दो"
        );

        Add(
            "EVACUATE",
            "EVACUATE",
            "निकासी करें",
            "बाहरे जाहान"
        );

        Add(
            "SCAN_MINE_AREA",
            "SCAN MINE AREA",
            "खदान क्षेत्र स्कैन करें",
            "खान इलाका स्कैन दो"
        );

        Add(
            "INITIALIZE_AR_SCAN",
            "INITIALIZE AR SURFACE SCAN",
            "AR सतह स्कैन शुरू करें",
            "AR सतह स्कैन एहोब"
        );

        Add(
            "TRAINING_RESULT",
            "TRAINING RESULT IS BASED ON YOUR RESPONSE AND EVACUATION.",
            "प्रशिक्षण परिणाम आपकी प्रतिक्रिया और निकासी पर आधारित है।",
            "सिखना के नतीजा तोर जवाब आर बाहरे जाहान पर निर्भर होय।"
        );

        // =====================================================
        // LANGUAGE
        // =====================================================

        Add(
            "LANGUAGE",
            "LANGUAGE",
            "भाषा",
            "भासा"
        );

        Add(
            "ENGLISH",
            "ENGLISH",
            "अंग्रेज़ी",
            "इंग्रेजी"
        );

        Add(
            "HINDI",
            "हिंदी",
            "हिंदी",
            "हिंदी"
        );

        Add(
            "SANTALI",
            "SANTALI",
            "संताली",
            "संताली"
        );
    }
}