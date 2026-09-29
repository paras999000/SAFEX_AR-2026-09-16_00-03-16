using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SAFEX Gas Training - Buddy System confirmation.
/// Required procedural safety action; not a quiz and not scored.
/// </summary>
public class SAFEXGasBuddyCheck : MonoBehaviour
{
    private Canvas canvas;
    private CanvasGroup group;

    private Text titleText;
    private Text messageText;
    private Text buttonText;
    private Button confirmButton;

    private Action completedCallback;
    private bool visible;
    private bool completed;

    private void Awake()
    {
        CreateUI();
        Hide();
    }

    public void Show(Action onCompleted)
    {
        completedCallback = onCompleted;
        completed = false;

        if (canvas == null)
            CreateUI();

        if (group == null || confirmButton == null)
            return;

        titleText.text = "BUDDY SYSTEM CHECK";
        messageText.text =
            "Confirm your buddy is with you and ready to proceed safely.";
        buttonText.text = "BUDDY CONFIRMED";

        confirmButton.interactable = true;

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        visible = true;
    }

    public void Hide()
    {
        visible = false;
        completedCallback = null;

        if (group == null)
            return;

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    private void OnConfirmPressed()
    {
        if (!visible || completed)
            return;

        completed = true;
        confirmButton.interactable = false;

        Action callback = completedCallback;

        // Hide before invoking the controller so double taps
        // cannot advance the training twice.
        Hide();

        if (callback != null)
            callback();
    }

    private void CreateUI()
    {
        if (canvas != null)
            return;

        GameObject canvasObject =
            new GameObject("SAFEX GAS BUDDY CHECK");

        canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvas.sortingOrder = 125;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1080f, 1920f);

        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        group =
            canvasObject.AddComponent<CanvasGroup>();

        GameObject panel =
            new GameObject("BUDDY CHECK PANEL");

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
            new Vector2(0.08f, 0.33f);

        panelRect.anchorMax =
            new Vector2(0.92f, 0.55f);

        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        titleText =
            CreateText(
                panel.transform,
                "BUDDY SYSTEM CHECK",
                new Vector2(0f, 72f),
                new Vector2(700f, 58f),
                21,
                new Color(0.20f, 1f, 0.45f, 1f)
            );

        messageText =
            CreateText(
                panel.transform,
                "Confirm your buddy is with you and ready to proceed safely.",
                new Vector2(0f, 5f),
                new Vector2(720f, 90f),
                15,
                Color.white
            );

        GameObject buttonObject =
            new GameObject("BUDDY CONFIRMED BUTTON");

        buttonObject.transform.SetParent(
            panel.transform,
            false
        );

        Image buttonImage =
            buttonObject.AddComponent<Image>();

        buttonImage.color =
            new Color(
                0.02f,
                0.85f,
                0.28f,
                1f
            );

        confirmButton =
            buttonObject.AddComponent<Button>();

        confirmButton.targetGraphic =
            buttonImage;

        RectTransform buttonRect =
            buttonObject.GetComponent<RectTransform>();

        buttonRect.anchoredPosition =
            new Vector2(0f, -72f);

        buttonRect.sizeDelta =
            new Vector2(430f, 64f);

        buttonText =
            CreateText(
                buttonObject.transform,
                "BUDDY CONFIRMED",
                Vector2.zero,
                new Vector2(410f, 58f),
                16,
                Color.black
            );

        confirmButton.onClick.AddListener(
            OnConfirmPressed
        );

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        visible = false;
    }

    private Text CreateText(
        Transform parent,
        string value,
        Vector2 position,
        Vector2 size,
        int fontSize,
        Color color)
    {
        GameObject textObject =
            new GameObject("Text");

        textObject.transform.SetParent(
            parent,
            false
        );

        Text text =
            textObject.AddComponent<Text>();

        text.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;

        text.horizontalOverflow =
            HorizontalWrapMode.Wrap;

        text.verticalOverflow =
            VerticalWrapMode.Overflow;

        RectTransform rect =
            text.GetComponent<RectTransform>();

        rect.anchoredPosition =
            position;

        rect.sizeDelta =
            size;

        return text;
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(
                OnConfirmPressed
            );
    }
}
