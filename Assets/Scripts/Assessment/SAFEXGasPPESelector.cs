using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SAFEX Gas PPE selection assessment.
/// No score. No quiz.
/// The trainee must select the required PPE before entering
/// the isolation/contaminated-area response stage.
/// </summary>
public class SAFEXGasPPESelector : MonoBehaviour
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Text statusText;
    private Button confirmButton;

    private readonly HashSet<string> selected =
        new HashSet<string>();

    private Action onCompleted;

    private readonly string[] requiredPPE =
    {
        "RESPIRATORY PROTECTION",
        "SAFETY HELMET"
    };

    private readonly string[] options =
    {
        "RESPIRATORY PROTECTION",
        "SAFETY HELMET",
        "SAFETY GLOVES",
        "SAFETY BOOTS"
    };

    public void Show(Action completedCallback)
    {
        onCompleted = completedCallback;
        selected.Clear();

        if (canvas == null)
            BuildUI();

        UpdateButtons();

        canvas.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    private void BuildUI()
    {
        GameObject canvasObject =
            new GameObject("SAFEX GAS PPE SELECTION");

        canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 130;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        canvasGroup =
            canvasObject.AddComponent<CanvasGroup>();

        GameObject panel =
            new GameObject("PPE PANEL");

        panel.transform.SetParent(
            canvas.transform,
            false
        );

        Image panelImage =
            panel.AddComponent<Image>();

        panelImage.color =
            new Color(
                0.01f,
                0.025f,
                0.03f,
                0.96f
            );

        RectTransform panelRect =
            panel.GetComponent<RectTransform>();

        panelRect.anchorMin =
            new Vector2(0.08f, 0.18f);

        panelRect.anchorMax =
            new Vector2(0.92f, 0.82f);

        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText(
            panel.transform,
            "GAS HAZARD // PPE REQUIRED",
            new Vector2(0f, 245f),
            new Vector2(720f, 65f),
            22,
            new Color(0.2f, 1f, 0.45f, 1f)
        );

        CreateText(
            panel.transform,
            "Select the required protective equipment before entering the response area.",
            new Vector2(0f, 185f),
            new Vector2(650f, 80f),
            14,
            Color.white
        );

        float startY = 85f;

        for (int i = 0; i < options.Length; i++)
        {
            CreatePPEButton(
                panel.transform,
                options[i],
                new Vector2(0f, startY - i * 82f)
            );
        }

        statusText =
            CreateText(
                panel.transform,
                "0 / 2 REQUIRED PPE SELECTED",
                new Vector2(0f, -265f),
                new Vector2(650f, 50f),
                13,
                Color.white
            );

        confirmButton =
            CreateConfirmButton(
                panel.transform,
                new Vector2(0f, -335f)
            );

        confirmButton.onClick.AddListener(
            ConfirmSelection
        );

        Hide();
    }

    private void CreatePPEButton(
        Transform parent,
        string label,
        Vector2 position)
    {
        GameObject obj =
            new GameObject(label);

        obj.transform.SetParent(
            parent,
            false
        );

        Image image =
            obj.AddComponent<Image>();

        image.color =
            new Color(
                0.02f,
                0.12f,
                0.10f,
                1f
            );

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic = image;

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchoredPosition = position;
        rect.sizeDelta =
            new Vector2(620f, 62f);

        CreateText(
            obj.transform,
            label,
            Vector2.zero,
            new Vector2(590f, 55f),
            14,
            Color.white
        );

        button.onClick.AddListener(
            () => TogglePPE(label, image)
        );
    }

    private Button CreateConfirmButton(
        Transform parent,
        Vector2 position)
    {
        GameObject obj =
            new GameObject("CONFIRM PPE");

        obj.transform.SetParent(
            parent,
            false
        );

        Image image =
            obj.AddComponent<Image>();

        image.color =
            new Color(
                0.02f,
                0.85f,
                0.28f,
                1f
            );

        Button button =
            obj.AddComponent<Button>();

        button.targetGraphic = image;

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.anchoredPosition = position;
        rect.sizeDelta =
            new Vector2(420f, 65f);

        CreateText(
            obj.transform,
            "CONFIRM PPE",
            Vector2.zero,
            new Vector2(400f, 58f),
            16,
            Color.black
        );

        return button;
    }

    private Text CreateText(
        Transform parent,
        string value,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color)
    {
        GameObject obj =
            new GameObject("TEXT");

        obj.transform.SetParent(
            parent,
            false
        );

        Text text =
            obj.AddComponent<Text>();

        text.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment =
            TextAnchor.MiddleCenter;

        text.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        text.verticalOverflow =
            VerticalWrapMode.Overflow;

        RectTransform rect =
            text.GetComponent<RectTransform>();

        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        return text;
    }

    private void TogglePPE(
        string label,
        Image image)
    {
        if (selected.Contains(label))
        {
            selected.Remove(label);

            image.color =
                new Color(
                    0.02f,
                    0.12f,
                    0.10f,
                    1f
                );
        }
        else
        {
            selected.Add(label);

            image.color =
                new Color(
                    0.02f,
                    0.55f,
                    0.25f,
                    1f
                );
        }

        UpdateStatus();
    }

    private void UpdateButtons()
    {
        selected.Clear();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        int correctSelected = 0;

        foreach (string required in requiredPPE)
        {
            if (selected.Contains(required))
                correctSelected++;
        }

        if (statusText != null)
        {
            statusText.text =
                correctSelected +
                " / " +
                requiredPPE.Length +
                " REQUIRED PPE SELECTED";
        }
    }

    private void ConfirmSelection()
    {
        foreach (string required in requiredPPE)
        {
            if (!selected.Contains(required))
            {
                if (statusText != null)
                {
                    statusText.text =
                        "REQUIRED PPE MISSING — SELECT ALL REQUIRED PPE";
                }

                return;
            }
        }

        Hide();

        if (onCompleted != null)
            onCompleted();
    }
}
