using UnityEngine;
using UnityEngine.UI;

public class SAFEXLanguageSelector : MonoBehaviour
{
    private GameObject selectorRoot;
    private GameObject dropdownPanel;

    private Text currentLanguageText;

    private readonly Color cyan =
        new Color(0f, 0.88f, 0.82f, 1f);

    private readonly Color white =
        new Color(0.94f, 0.97f, 0.98f, 1f);

    private readonly Color muted =
        new Color(0.48f, 0.56f, 0.61f, 1f);

    private readonly Color buttonNormal =
        new Color(0.01f, 0.025f, 0.032f, 1f);

    private readonly Color buttonHover =
        new Color(0f, 0.18f, 0.17f, 1f);

    // =========================================================
    // BUILD
    // =========================================================

    public void Build(
        Transform parent,
        Vector2 position)
    {
        EnsureLocalizationManager();

        if (selectorRoot != null)
        {
            Destroy(selectorRoot);
        }

        selectorRoot =
            new GameObject("LANGUAGE SELECTOR");

        selectorRoot.transform.SetParent(
            parent,
            false
        );

        RectTransform root =
            selectorRoot.AddComponent<RectTransform>();

        root.anchorMin =
            new Vector2(0.5f, 0.5f);

        root.anchorMax =
            new Vector2(0.5f, 0.5f);

        root.pivot =
            new Vector2(0.5f, 0.5f);

        root.anchoredPosition =
            position;

        root.sizeDelta =
            new Vector2(250f, 90f);

        CreateLabel();
        CreateMainButton();

        if (SAFEXLocalizationManager.Instance != null)
        {
            SAFEXLocalizationManager.Instance.LanguageChanged +=
                OnLanguageChanged;
        }

        RefreshLanguageText();
    }

    // =========================================================
    // LOCALIZATION MANAGER
    // =========================================================

    private void EnsureLocalizationManager()
    {
        if (SAFEXLocalizationManager.Instance != null)
            return;

        SAFEXLocalizationManager manager =
            FindFirstObjectByType<
                SAFEXLocalizationManager
            >();

        if (manager != null)
            return;

        GameObject obj =
            new GameObject(
                "SAFEX Localization Manager"
            );

        obj.AddComponent<
            SAFEXLocalizationManager
        >();
    }

    // =========================================================
    // LABEL
    // =========================================================

    private void CreateLabel()
    {
        GameObject obj =
            new GameObject("LANGUAGE LABEL");

        obj.transform.SetParent(
            selectorRoot.transform,
            false
        );

        RectTransform rect =
            obj.AddComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            new Vector2(0f, 31f);

        rect.sizeDelta =
            new Vector2(240f, 20f);

        Text text =
            obj.AddComponent<Text>();

        text.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.fontSize = 10;
        text.alignment =
            TextAnchor.MiddleRight;

        text.color = muted;
        text.text = "LANGUAGE";

        // VERY IMPORTANT
        text.raycastTarget = false;
    }

    // =========================================================
    // MAIN BUTTON
    // =========================================================

    private void CreateMainButton()
    {
        GameObject obj =
            new GameObject("LANGUAGE MAIN BUTTON");

        obj.transform.SetParent(
            selectorRoot.transform,
            false
        );

        RectTransform rect =
            obj.AddComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            new Vector2(0f, -10f);

        rect.sizeDelta =
            new Vector2(250f, 42f);

        Image background =
            obj.AddComponent<Image>();

        background.color =
            new Color(
                0f,
                0.25f,
                0.23f,
                0.35f
            );

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic =
            background;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            new Color(
                0f,
                0.25f,
                0.23f,
                0.35f
            );

        colors.highlightedColor =
            new Color(
                0f,
                0.45f,
                0.42f,
                0.45f
            );

        colors.pressedColor =
            new Color(
                0f,
                0.65f,
                0.60f,
                0.55f
            );

        button.colors = colors;

        currentLanguageText =
            CreateText(
                obj.transform,
                "ENGLISH",
                new Vector2(-10f, 0f),
                new Vector2(190f, 36f),
                11,
                white,
                TextAnchor.MiddleCenter
            );

        Text arrow =
            CreateText(
                obj.transform,
                "▼",
                new Vector2(105f, 0f),
                new Vector2(25f, 30f),
                9,
                cyan,
                TextAnchor.MiddleCenter
            );

        arrow.raycastTarget = false;

        button.onClick.AddListener(
            ToggleDropdown
        );
    }

    // =========================================================
    // DROPDOWN
    // =========================================================

    private void ToggleDropdown()
    {
        if (dropdownPanel != null)
        {
            CloseDropdown();
        }
        else
        {
            OpenDropdown();
        }
    }

    private void OpenDropdown()
    {
        if (dropdownPanel != null)
            Destroy(dropdownPanel);

        dropdownPanel =
            new GameObject(
                "LANGUAGE DROPDOWN"
            );

        dropdownPanel.transform.SetParent(
            selectorRoot.transform,
            false
        );

        RectTransform rect =
            dropdownPanel.AddComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 1f);

        rect.anchoredPosition =
            new Vector2(0f, -34f);

        rect.sizeDelta =
            new Vector2(250f, 150f);

        Image background =
            dropdownPanel.AddComponent<Image>();

        background.color =
            new Color(
                0.005f,
                0.02f,
                0.025f,
                0.98f
            );

        // Make sure background does NOT block
        // its children.
        background.raycastTarget = false;

        CreateOption(
            "ENGLISH",
            SAFEXLanguage.English,
            45f
        );

        CreateOption(
            "हिंदी",
            SAFEXLanguage.Hindi,
            0f
        );

        CreateOption(
            "SANTALI",
            SAFEXLanguage.Santali,
            -45f
        );
    }

    // =========================================================
    // OPTION BUTTON
    // =========================================================

    private void CreateOption(
        string label,
        SAFEXLanguage language,
        float y)
    {
        GameObject obj =
            new GameObject(
                "LANGUAGE OPTION " +
                language
            );

        obj.transform.SetParent(
            dropdownPanel.transform,
            false
        );

        RectTransform rect =
            obj.AddComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 0.5f);

        rect.anchorMax =
            new Vector2(0.5f, 0.5f);

        rect.pivot =
            new Vector2(0.5f, 0.5f);

        rect.anchoredPosition =
            new Vector2(0f, y);

        rect.sizeDelta =
            new Vector2(225f, 36f);

        Image background =
            obj.AddComponent<Image>();

        background.color =
            buttonNormal;

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic =
            background;

        ColorBlock colors =
            button.colors;

        colors.normalColor =
            buttonNormal;

        colors.highlightedColor =
            buttonHover;

        colors.pressedColor =
            cyan;

        button.colors = colors;

        Text text =
            CreateText(
                obj.transform,
                label,
                Vector2.zero,
                new Vector2(215f, 32f),
                11,
                white,
                TextAnchor.MiddleCenter
            );

        text.raycastTarget = false;

        // =====================================================
        // ACTUAL CLICK
        // =====================================================

        button.onClick.AddListener(
            delegate
            {
                Debug.Log(
                    "SAFEX LANGUAGE CLICKED: " +
                    language
                );

                SelectLanguage(language);
            }
        );
    }

    // =========================================================
    // SELECT LANGUAGE
    // =========================================================

    private void SelectLanguage(
        SAFEXLanguage language)
    {
        Debug.Log(
            "SAFEX SELECT LANGUAGE: " +
            language
        );

        if (SAFEXLocalizationManager.Instance == null)
        {
            Debug.LogError(
                "SAFEX: Localization Manager is NULL."
            );

            return;
        }

        SAFEXLocalizationManager.Instance.SetLanguage(
            language
        );

        RefreshLanguageText();

        CloseDropdown();
    }

    // =========================================================
    // CLOSE
    // =========================================================

    private void CloseDropdown()
    {
        if (dropdownPanel != null)
        {
            Destroy(dropdownPanel);

            dropdownPanel = null;
        }
    }

    // =========================================================
    // REFRESH CURRENT LANGUAGE
    // =========================================================

    private void OnLanguageChanged(
        SAFEXLanguage language)
    {
        Debug.Log(
            "SAFEX LANGUAGE CHANGED: " +
            language
        );

        RefreshLanguageText();
    }

    private void RefreshLanguageText()
    {
        if (currentLanguageText == null)
            return;

        if (SAFEXLocalizationManager.Instance == null)
            return;

        switch (
            SAFEXLocalizationManager.Instance
                .CurrentLanguage)
        {
            case SAFEXLanguage.Hindi:

                currentLanguageText.text =
                    "हिंदी";

                break;

            case SAFEXLanguage.Santali:

                currentLanguageText.text =
                    "SANTALI";

                break;

            default:

                currentLanguageText.text =
                    "ENGLISH";

                break;
        }
    }

    // =========================================================
    // TEXT HELPER
    // =========================================================

    private Text CreateText(
        Transform parent,
        string value,
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

        RectTransform rect =
            obj.AddComponent<RectTransform>();

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

        Text text =
            obj.AddComponent<Text>();

        text.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.fontSize =
            fontSize;

        text.alignment =
            alignment;

        text.color =
            color;

        text.text =
            value;

        text.raycastTarget =
            false;

        return text;
    }
}